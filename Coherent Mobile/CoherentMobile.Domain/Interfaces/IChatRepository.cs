using CoherentMobile.Domain.Entities;

namespace CoherentMobile.Domain.Interfaces
{
    public interface IChatRepository
    {
        // Conversations
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId, string userType, int pageNumber, int pageSize);
        Task<Conversation?> GetConversationByIdAsync(int conversationId);
        Task<int> CreateConversationAsync(string conversationType, int createdBy, string? title = null);
        Task<int?> GetOrCreateOneToOneConversationAsync(int user1Id, string user1Type, int user2Id, string user2Type);
        Task UpdateConversationLastMessageAsync(int conversationId, string lastMessage, DateTime lastMessageAt);
        
        // Participants
        Task AddParticipantAsync(int conversationId, int userId, string userType);
        Task<bool> IsParticipantAsync(int conversationId, int userId, string userType);
        Task UpdateUnreadCountAsync(int conversationId, int userId, string userType, int count);
        Task ResetUnreadCountAsync(int conversationId, int userId, string userType);
        Task<ConversationParticipant?> GetParticipantAsync(int conversationId, int userId, string userType);
        
        // Messages
        Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(int conversationId, int pageNumber, int pageSize);
        Task<ChatMessage?> GetMessageByIdAsync(int messageId);
        Task<int> SendMessageAsync(ChatMessage message);
        Task MarkMessageAsDeliveredAsync(int messageId);
        Task MarkMessageAsReadAsync(int messageId);
        Task MarkMessagesAsReadAsync(int conversationId, int userId, string userType, List<int> messageIds);
        Task DeleteMessageAsync(int messageId, int userId, string userType);
        
        // User Status
        Task UpdateUserStatusAsync(int userId, string userType, bool isOnline, string? connectionId = null, string? deviceType = null);
        Task<bool> GetUserOnlineStatusAsync(int userId, string userType);
        Task<DateTime?> GetUserLastSeenAsync(int userId, string userType);
        
        // Active threads for SignalR subscription
        Task<IEnumerable<int>> GetActiveThreadIdsAsync(TimeSpan withinPeriod);
        
        // CRM message deduplication
        Task<bool> CrmMessageExistsAsync(string crmMessageId);
        Task<int> SaveCrmMessageAsync(ChatMessage message, string crmMessageId);
    }
}
