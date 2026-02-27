using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace CoherentMobile.Api.Hubs
{
    /// <summary>
    /// Implementation of IChatHubNotifier that broadcasts messages to connected
    /// mobile clients via the Mobile Backend's ChatHub.
    /// </summary>
    public class ChatHubNotifier : IChatHubNotifier
    {
        private readonly IHubContext<ChatHub, IChatClient> _hubContext;
        private readonly ILogger<ChatHubNotifier> _logger;

        public ChatHubNotifier(
            IHubContext<ChatHub, IChatClient> hubContext,
            ILogger<ChatHubNotifier> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendMessageToConversationAsync(int conversationId, ChatMessageDto message)
        {
            var groupName = $"conversation_{conversationId}";
            await _hubContext.Clients.Group(groupName).ReceiveMessage(message);
            _logger.LogInformation(
                "Broadcasted message {MessageId} to ChatHub group {Group}",
                message.MessageId, groupName);
        }

        public async Task NotifyConversationUpdatedAsync(int conversationId)
        {
            var groupName = $"conversation_{conversationId}";
            await _hubContext.Clients.Group(groupName).ConversationUpdated(conversationId);
        }
    }
}
