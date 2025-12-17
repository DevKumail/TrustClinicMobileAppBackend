using CoherentMobile.Application.DTOs.Chat;

namespace CoherentMobile.Application.Interfaces
{
    public interface IChatService
    {
        // Conversations
        Task<GetConversationsResponseDto> GetUserConversationsAsync(int userId, string userType, int pageNumber = 1, int pageSize = 20);
        Task<ConversationDto?> GetConversationByIdAsync(int conversationId, int userId, string userType);
        Task<(int conversationId, string message)> CreateOrGetConversationAsync(int currentUserId, string currentUserType, CreateConversationRequestDto request);
        
        // Messages
        Task<(List<ChatMessageDto> messages, int totalCount)> GetConversationMessagesAsync(int conversationId, int userId, string userType, int pageNumber = 1, int pageSize = 50);
        Task<ChatMessageDto> SendMessageAsync(int senderId, string senderType, SendMessageRequestDto request);
        Task MarkMessageAsDeliveredAsync(int messageId, int userId, string userType);
        Task MarkMessagesAsReadAsync(int conversationId, int userId, string userType, List<int> messageIds);
        Task DeleteMessageAsync(int messageId, int userId, string userType);
        
        // User Status
        Task UpdateUserStatusAsync(int userId, string userType, bool isOnline, string? connectionId = null, string? deviceType = null);
        Task<bool> GetUserOnlineStatusAsync(int userId, string userType);
    }
}
