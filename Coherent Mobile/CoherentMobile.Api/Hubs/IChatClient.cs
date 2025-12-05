using CoherentMobile.Application.DTOs.Chat;

namespace CoherentMobile.Api.Hubs
{
    /// <summary>
    /// Interface defining methods that can be called on the client
    /// </summary>
    public interface IChatClient
    {
        // Message notifications
        Task ReceiveMessage(ChatMessageDto message);
        Task MessageDelivered(int messageId, DateTime deliveredAt);
        Task MessageRead(int messageId, int userId, string userType, DateTime readAt);
        
        // User status
        Task UserOnline(int userId, string userType);
        Task UserOffline(int userId, string userType, DateTime lastSeenAt);
        Task UserTyping(int conversationId, int userId, string userName, bool isTyping);
        
        // Conversation updates
        Task ConversationUpdated(int conversationId);
        Task UnreadCountUpdated(int conversationId, int unreadCount);
    }
}
