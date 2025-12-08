using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoherentMobile.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// Get user's conversations
        /// </summary>
        [HttpGet("conversations")]
        [ProducesResponseType(typeof(GetConversationsResponseDto), 200)]
        public async Task<IActionResult> GetConversations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var (userId, userType) = GetCurrentUser();
                if (userId == 0) return Unauthorized();

                var result = await _chatService.GetUserConversationsAsync(userId, userType, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new { message = "An error occurred while retrieving conversations" });
            }
        }

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
        /// Get user's online status
        /// </summary>
        [HttpGet("users/{userId}/status")]
        public async Task<IActionResult> GetUserStatus(int userId, [FromQuery] string userType)
        {
            try
            {
                var isOnline = await _chatService.GetUserOnlineStatusAsync(userId, userType);
                return Ok(new { userId, userType, isOnline });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user status");
                return StatusCode(500, new { message = "An error occurred while checking user status" });
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
