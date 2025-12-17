using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CoherentMobile.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var userType = GetUserType();

            if (userId.HasValue && !string.IsNullOrEmpty(userType))
            {
                var connectionId = Context.ConnectionId;
                await _chatService.UpdateUserStatusAsync(userId.Value, userType, true, connectionId);
                
                // Notify other users
                await Clients.Others.UserOnline(userId.Value, userType);
                
                _logger.LogInformation("User {UserId} ({UserType}) connected with connection {ConnectionId}", userId, userType, connectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var userType = GetUserType();

            if (userId.HasValue && !string.IsNullOrEmpty(userType))
            {
                var lastSeen = DateTime.UtcNow;
                await _chatService.UpdateUserStatusAsync(userId.Value, userType, false);
                
                // Notify other users
                await Clients.Others.UserOffline(userId.Value, userType, lastSeen);
                
                _logger.LogInformation("User {UserId} ({UserType}) disconnected", userId, userType);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = GetUserId();
            var userType = GetUserType();

            if (!userId.HasValue || string.IsNullOrEmpty(userType))
            {
                return;
            }

            var conversation = await _chatService.GetConversationByIdAsync(conversationId, userId.Value, userType);
            if (conversation == null)
            {
                throw new HubException("You are not allowed to join this conversation");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation("Connection {ConnectionId} joined conversation {ConversationId}", Context.ConnectionId, conversationId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogInformation("Connection {ConnectionId} left conversation {ConversationId}", Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(SendMessageRequestDto request)
        {
            try
            {
                var userId = GetUserId();
                var userType = GetUserType();

                if (!userId.HasValue || string.IsNullOrEmpty(userType))
                {
                    _logger.LogWarning("Unauthorized message send attempt");
                    return;
                }

                // Send message using service
                var message = await _chatService.SendMessageAsync(userId.Value, userType, request);

                // Broadcast to conversation group
                await Clients.Group($"conversation_{request.ConversationId}").ReceiveMessage(message);

                _logger.LogInformation("Message {MessageId} sent to conversation {ConversationId}", message.MessageId, request.ConversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message via SignalR");
                throw new HubException("Failed to send message");
            }
        }

        public async Task UpdateTypingStatus(int conversationId, bool isTyping)
        {
            var userId = GetUserId();
            var userName = GetUserName();
            var userType = GetUserType();

            if (userId.HasValue && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userType))
            {
                var conversation = await _chatService.GetConversationByIdAsync(conversationId, userId.Value, userType);
                if (conversation == null)
                {
                    return;
                }

                await Clients.OthersInGroup($"conversation_{conversationId}")
                    .UserTyping(conversationId, userId.Value, userName, isTyping);
            }
        }

        public async Task MarkAsDelivered(int conversationId, List<int> messageIds)
        {
            try
            {
                var userId = GetUserId();
                var userType = GetUserType();

                if (!userId.HasValue || string.IsNullOrEmpty(userType))
                {
                    return;
                }

                var conversation = await _chatService.GetConversationByIdAsync(conversationId, userId.Value, userType);
                if (conversation == null)
                {
                    return;
                }

                var deliveredAt = DateTime.UtcNow;
                foreach (var messageId in messageIds)
                {
                    await _chatService.MarkMessageAsDeliveredAsync(messageId, userId.Value, userType);
                    await Clients.OthersInGroup($"conversation_{conversationId}")
                        .MessageDelivered(messageId, deliveredAt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as delivered");
                throw new HubException("Failed to mark messages as delivered");
            }
        }

        public async Task MarkAsRead(int conversationId, List<int> messageIds)
        {
            try
            {
                var userId = GetUserId();
                var userType = GetUserType();

                if (!userId.HasValue || string.IsNullOrEmpty(userType))
                {
                    return;
                }

                var conversation = await _chatService.GetConversationByIdAsync(conversationId, userId.Value, userType);
                if (conversation == null)
                {
                    return;
                }

                await _chatService.MarkMessagesAsReadAsync(conversationId, userId.Value, userType, messageIds);

                // Notify other participants
                var readAt = DateTime.UtcNow;
                foreach (var messageId in messageIds)
                {
                    await Clients.OthersInGroup($"conversation_{conversationId}")
                        .MessageRead(messageId, userId.Value, userType, readAt);
                }

                _logger.LogInformation("User {UserId} marked {Count} messages as read", userId, messageIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                throw new HubException("Failed to mark messages as read");
            }
        }

        // Helper methods to get user info from JWT claims
        private int? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return null;
        }

        private string? GetUserType()
        {
            return Context.User?.FindFirst("UserType")?.Value;
        }

        private string? GetUserName()
        {
            return Context.User?.FindFirst(ClaimTypes.Name)?.Value 
                ?? Context.User?.FindFirst("FullName")?.Value;
        }
    }
}
