using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoherentMobile.Api.Controllers
{
    /// <summary>
    /// Internal endpoint for cross-backend real-time chat notification.
    /// Called by Web Backend after saving a message so Mobile Backend's ChatHub
    /// can broadcast to connected clients (both Flutter app and Angular CRM UI).
    /// Secured by API key — not exposed to end users.
    /// </summary>
    [Route("api/internal/chat-notify")]
    [ApiController]
    public class InternalChatNotifyController : ControllerBase
    {
        private readonly IChatHubNotifier _chatHubNotifier;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InternalChatNotifyController> _logger;

        public InternalChatNotifyController(
            IChatHubNotifier chatHubNotifier,
            IConfiguration configuration,
            ILogger<InternalChatNotifyController> logger)
        {
            _chatHubNotifier = chatHubNotifier;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("broadcast")]
        public async Task<IActionResult> BroadcastMessage([FromBody] InternalChatBroadcastRequest request)
        {
            // Validate API key
            var apiKey = Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
            var expectedKey = _configuration["CrmSignalR:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey != expectedKey)
            {
                _logger.LogWarning("Internal chat-notify rejected — invalid API key");
                return Unauthorized();
            }

            if (request.ConversationId <= 0 || request.MessageId <= 0)
                return BadRequest(new { message = "conversationId and messageId are required" });

            try
            {
                var msgDto = new ChatMessageDto
                {
                    MessageId = request.MessageId,
                    ConversationId = request.ConversationId,
                    SenderId = request.SenderId,
                    SenderType = request.SenderType ?? "",
                    SenderName = request.SenderName ?? "",
                    MessageType = request.MessageType ?? "Text",
                    Content = request.Content,
                    FileUrl = request.FileUrl,
                    FileName = request.FileName,
                    FileSize = request.FileSize,
                    SentAt = request.SentAt ?? DateTime.UtcNow,
                    CrmMessageId = request.CrmMessageId,
                    CrmThreadId = request.CrmThreadId
                };

                await _chatHubNotifier.SendMessageToConversationAsync(request.ConversationId, msgDto);

                _logger.LogInformation(
                    "[InternalChatNotify] Broadcast message {MessageId} to conversation_{ConversationId}",
                    request.MessageId, request.ConversationId);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[InternalChatNotify] Failed to broadcast message {MessageId}", request.MessageId);
                return StatusCode(500, new { message = "Failed to broadcast" });
            }
        }
    }

    public class InternalChatBroadcastRequest
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string? SenderType { get; set; }
        public string? SenderName { get; set; }
        public string? MessageType { get; set; }
        public string? Content { get; set; }
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime? SentAt { get; set; }
        public string? CrmMessageId { get; set; }
        public string? CrmThreadId { get; set; }
    }
}
