using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Interfaces;
using CoherentMobile.ExternalIntegration.Interfaces;
using CoherentMobile.ExternalIntegration.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace CoherentMobile.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ICrmChatApiClient _crmChatApiClient;
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ICrmChatApiClient crmChatApiClient, IChatRepository chatRepository, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _crmChatApiClient = crmChatApiClient;
            _chatRepository = chatRepository;
            _logger = logger;
        }

        [HttpPost("/api/v2/chat/threads/get-or-create")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CrmGetOrCreateThreadResponse), 200)]
        public async Task<IActionResult> CrmGetOrCreateThread([FromBody] CrmGetOrCreateThreadRequest request)
        {
            try
            {
                var result = await _crmChatApiClient.GetOrCreateThreadAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat get-or-create thread");
                return StatusCode(500, new { message = "An error occurred while calling CRM chat" });
            }
        }

        [HttpPost("/api/v2/chat/messages")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CrmSendMessageResponse), 200)]
        public async Task<IActionResult> CrmSendMessage([FromBody] CrmSendMessageRequest request)
        {
            try
            {
                var result = await _crmChatApiClient.SendMessageAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat send message");
                return StatusCode(500, new { message = "An error occurred while calling CRM chat" });
            }
        }

        [HttpGet("/api/v2/chat/messages/updates")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<CrmMessageUpdateEvent>), 200)]
        public async Task<IActionResult> CrmGetMessageUpdates([FromQuery] DateTime since, [FromQuery] int limit = 100)
        {
            try
            {
                var result = await _crmChatApiClient.GetMessageUpdatesAsync(since, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat message updates");
                return StatusCode(500, new { message = "An error occurred while calling CRM chat" });
            }
        }

        [HttpGet("/api/v2/chat/conversations")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CrmConversationListResponse), 200)]
        public async Task<IActionResult> CrmGetConversations([FromQuery] string patientMrNo, [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(patientMrNo))
                {
                    return BadRequest(new { message = "patientMrNo is required" });
                }

                var result = await _crmChatApiClient.GetConversationsAsync(patientMrNo, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat conversations");
                return StatusCode(500, new { message = "An error occurred while calling CRM chat" });
            }
        }

        #region Broadcast Channel APIs (Patient -> Staff)

        [HttpPost("/api/v2/chat/broadcast-channels/get-or-create")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CrmGetOrCreateBroadcastChannelResponse), 200)]
        public async Task<IActionResult> CrmGetOrCreateBroadcastChannel([FromBody] CrmGetOrCreateBroadcastChannelRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PatientMrNo))
                {
                    return BadRequest(new { message = "patientMrNo is required" });
                }

                if (string.IsNullOrWhiteSpace(request.StaffType))
                {
                    return BadRequest(new { message = "staffType is required (Nurse, Receptionist, or IVFLab)" });
                }

                var result = await _crmChatApiClient.GetOrCreateBroadcastChannelAsync(request);

                if (result.ConversationId == null && !string.IsNullOrWhiteSpace(result.CrmThreadId))
                {
                    var idPart = result.CrmThreadId.StartsWith("CRM-TH-", StringComparison.OrdinalIgnoreCase)
                        ? result.CrmThreadId.Substring("CRM-TH-".Length)
                        : result.CrmThreadId;

                    if (int.TryParse(idPart, out var conversationId) && conversationId > 0)
                    {
                        result.ConversationId = conversationId;
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat get-or-create broadcast channel");
                return StatusCode(500, new { message = "An error occurred while creating broadcast channel" });
            }
        }

        [HttpGet("/api/v2/chat/broadcast-channels")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<CrmBroadcastChannelItem>), 200)]
        public async Task<IActionResult> CrmGetBroadcastChannels([FromQuery] string staffType, [FromQuery] int limit = 50)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(staffType))
                {
                    return BadRequest(new { message = "staffType is required" });
                }

                var rows = await _chatRepository.GetBroadcastChannelsForStaffAsync(staffType, limit);
                var result = rows.Select(x => new CrmBroadcastChannelItem
                {
                    ConversationId = x.ConversationId,
                    CrmThreadId = $"CRM-TH-{x.ConversationId}",
                    PatientMrNo = x.PatientMrNo,
                    PatientName = x.PatientName,
                    StaffType = staffType,
                    LastMessageContent = x.LastMessagePreview,
                    LastMessageAt = x.LastMessageAt,
                    UnreadCount = x.UnreadCount
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat broadcast channels");
                return StatusCode(500, new { message = "An error occurred while getting broadcast channels" });
            }
        }

        [HttpGet("/api/v2/chat/broadcast-channels/unread-summary")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CrmStaffUnreadSummary), 200)]
        public async Task<IActionResult> CrmGetStaffUnreadSummary([FromQuery] string staffType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(staffType))
                {
                    return BadRequest(new { message = "staffType is required" });
                }

                var (totalUnreadCount, channelsWithUnread) = await _chatRepository.GetBroadcastUnreadSummaryForStaffAsync(staffType);
                var result = new CrmStaffUnreadSummary
                {
                    StaffType = staffType,
                    TotalUnreadCount = totalUnreadCount,
                    ChannelsWithUnread = channelsWithUnread
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat staff unread summary");
                return StatusCode(500, new { message = "An error occurred while getting unread summary" });
            }
        }

        [HttpGet("/api/v2/chat/threads/{crmThreadId}/messages")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<CrmThreadMessage>), 200)]
        public async Task<IActionResult> CrmGetThreadMessages([FromRoute] string crmThreadId, [FromQuery] int take = 50)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(crmThreadId))
                {
                    return BadRequest(new { message = "crmThreadId is required" });
                }

                var result = await _crmChatApiClient.GetThreadMessagesAsync(crmThreadId, take);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat thread messages for {CrmThreadId}", crmThreadId);
                return StatusCode(500, new { message = "An error occurred while getting thread messages" });
            }
        }

        [HttpPost("/api/v2/chat/broadcast-channels/{crmThreadId}/mark-read")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CrmMarkReadResponse), 200)]
        public async Task<IActionResult> CrmMarkBroadcastChannelRead(
            [FromRoute] string crmThreadId,
            [FromQuery] long empId,
            [FromQuery] string staffType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(crmThreadId))
                {
                    return BadRequest(new { message = "crmThreadId is required" });
                }

                if (empId <= 0)
                {
                    return BadRequest(new { message = "empId is required" });
                }

                if (string.IsNullOrWhiteSpace(staffType))
                {
                    return BadRequest(new { message = "staffType is required" });
                }

                var idPart = crmThreadId.StartsWith("CRM-TH-", StringComparison.OrdinalIgnoreCase)
                    ? crmThreadId.Substring("CRM-TH-".Length)
                    : crmThreadId;

                if (!int.TryParse(idPart, out var conversationId) || conversationId <= 0)
                {
                    return BadRequest(new { message = "Invalid crmThreadId" });
                }

                var messagesMarked = await _chatRepository.MarkBroadcastChannelAsReadForStaffAsync(conversationId);
                return Ok(new CrmMarkReadResponse
                {
                    Success = true,
                    MessagesMarked = messagesMarked
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling CRM chat mark broadcast channel read for {CrmThreadId}", crmThreadId);
                return StatusCode(500, new { message = "An error occurred while marking channel as read" });
            }
        }

        #endregion

        /// <summary>
        /// Create or get existing one-to-one conversation
        /// </summary>
        [HttpPost("conversations")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequestDto request)
        {
            try
            {
                var (userId, userType) = GetCurrentUser();
                if (userId == 0) return Unauthorized();

                var (conversationId, message) = await _chatService.CreateOrGetConversationAsync(userId, userType, request);
                
                if (conversationId == 0)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { conversationId, message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return StatusCode(500, new { message = "An error occurred while creating conversation" });
            }
        }

        /// <summary>
        /// Get messages for a conversation
        /// </summary>
        [HttpGet("conversations/{conversationId}/messages")]
        [ProducesResponseType(typeof(List<ChatMessageDto>), 200)]
        public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var (userId, userType) = GetCurrentUser();
                if (userId == 0) return Unauthorized();

                var (messages, totalCount) = await _chatService.GetConversationMessagesAsync(conversationId, userId, userType, pageNumber, pageSize);

                return Ok(new
                {
                    messages,
                    totalCount,
                    pageNumber,
                    pageSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, new { message = "An error occurred while retrieving messages" });
            }
        }

        /// <summary>
        /// Send a message (also available via SignalR)
        /// </summary>
        [HttpPost("messages")]
        [ProducesResponseType(typeof(ChatMessageDto), 200)]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
        {
            try
            {
                var (userId, userType) = GetCurrentUser();
                if (userId == 0) return Unauthorized();

                var message = await _chatService.SendMessageAsync(userId, userType, request);
                return Ok(message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized message send attempt");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { message = "An error occurred while sending message" });
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        [HttpPost("conversations/{conversationId}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int conversationId, [FromBody] List<int> messageIds)
        {
            try
            {
                var (userId, userType) = GetCurrentUser();
                if (userId == 0) return Unauthorized();

                await _chatService.MarkMessagesAsReadAsync(conversationId, userId, userType, messageIds);
                return Ok(new { success = true, message = "Messages marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                return StatusCode(500, new { message = "An error occurred while marking messages as read" });
            }
        }

        /// <summary>
        /// Delete a message
        /// </summary>
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var (userId, userType) = GetCurrentUser();
                if (userId == 0) return Unauthorized();

                await _chatService.DeleteMessageAsync(messageId, userId, userType);
                return Ok(new { success = true, message = "Message deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                return StatusCode(500, new { message = "An error occurred while deleting message" });
            }
        }

        /// <summary>
        /// Upload file for chat (images, documents)
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }

                // Validate file size (10MB max)
                const long maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = "File size exceeds 10MB limit" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".txt" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { message = "File type not allowed" });
                }

                // TODO: Implement actual file upload to cloud storage (Azure Blob, AWS S3, etc.)
                // For now, save locally
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadPath = Path.Combine("uploads", "chat");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/chat/{fileName}";

                return Ok(new
                {
                    fileUrl,
                    fileName = file.FileName,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { message = "An error occurred while uploading file" });
            }
        }

        // Helper method to get current user from JWT token
        private (int userId, string userType) GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value ?? "Patient";

            if (int.TryParse(userIdClaim, out int userId))
            {
                return (userId, userType);
            }

            return (0, userType);
        }
    }
}
