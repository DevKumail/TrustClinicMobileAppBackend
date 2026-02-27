using CoherentMobile.Application.DTOs.Chat;

namespace CoherentMobile.Application.Interfaces
{
    /// <summary>
    /// Abstraction for broadcasting messages to connected mobile clients via the ChatHub.
    /// Implemented in the Api layer where IHubContext is available.
    /// </summary>
    public interface IChatHubNotifier
    {
        Task SendMessageToConversationAsync(int conversationId, ChatMessageDto message);
        Task NotifyConversationUpdatedAsync(int conversationId);
    }
}
