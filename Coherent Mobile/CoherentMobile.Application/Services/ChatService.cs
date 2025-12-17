using CoherentMobile.Application.DTOs.Chat;
using CoherentMobile.Application.Interfaces;
using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoherentMobile.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepository,
            IPatientRepository patientRepository,
            ILogger<ChatService> logger)
        {
            _chatRepo = chatRepository;
            _patientRepo = patientRepository;
            _logger = logger;
        }

        public async Task<GetConversationsResponseDto> GetUserConversationsAsync(int userId, string userType, int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                var conversations = await _chatRepo.GetUserConversationsAsync(userId, userType, pageNumber, pageSize);
                var conversationDtos = new List<ConversationDto>();

                foreach (var conv in conversations)
                {
                    var dto = new ConversationDto
                    {
                        ConversationId = conv.ConversationId,
                        ConversationType = conv.ConversationType,
                        Title = conv.Title,
                        LastMessageAt = conv.LastMessageAt,
                        LastMessage = conv.LastMessage
                    };

                    // Get participant info if needed
                    // TODO: Add logic to fetch other user's name, avatar, online status

                    conversationDtos.Add(dto);
                }

                return new GetConversationsResponseDto
                {
                    Conversations = conversationDtos,
                    TotalCount = conversationDtos.Count,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ConversationDto?> GetConversationByIdAsync(int conversationId, int userId, string userType)
        {
            try
            {
                // Check if user is participant
                var isParticipant = await _chatRepo.IsParticipantAsync(conversationId, userId, userType);
                if (!isParticipant)
                {
                    _logger.LogWarning("User {UserId} tried to access conversation {ConversationId} without permission", userId, conversationId);
                    return null;
                }

                var conversation = await _chatRepo.GetConversationByIdAsync(conversationId);
                if (conversation == null) return null;

                return new ConversationDto
                {
                    ConversationId = conversation.ConversationId,
                    ConversationType = conversation.ConversationType,
                    Title = conversation.Title,
                    LastMessageAt = conversation.LastMessageAt,
                    LastMessage = conversation.LastMessage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<(int conversationId, string message)> CreateOrGetConversationAsync(int currentUserId, string currentUserType, CreateConversationRequestDto request)
        {
            try
            {
                // Get or create one-to-one conversation
                var conversationId = await _chatRepo.GetOrCreateOneToOneConversationAsync(
                    currentUserId, currentUserType,
                    request.OtherUserId, request.OtherUserType);

                if (conversationId == null)
                {
                    return (0, "Failed to create conversation");
                }

                // Send initial message if provided
                if (!string.IsNullOrWhiteSpace(request.InitialMessage))
                {
                    var message = new ChatMessage
                    {
                        ConversationId = conversationId.Value,
                        SenderId = currentUserId,
                        SenderType = currentUserType,
                        MessageType = "Text",
                        Content = request.InitialMessage,
                        SentAt = DateTime.UtcNow
                    };

                    await _chatRepo.SendMessageAsync(message);
                    await _chatRepo.UpdateConversationLastMessageAsync(conversationId.Value, request.InitialMessage, DateTime.UtcNow);
                }

                return (conversationId.Value, "Conversation created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                throw;
            }
        }

        public async Task<(List<ChatMessageDto> messages, int totalCount)> GetConversationMessagesAsync(int conversationId, int userId, string userType, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                // Check if user is participant
                var isParticipant = await _chatRepo.IsParticipantAsync(conversationId, userId, userType);
                if (!isParticipant)
                {
                    _logger.LogWarning("User {UserId} tried to access messages in conversation {ConversationId} without permission", userId, conversationId);
                    return (new List<ChatMessageDto>(), 0);
                }

                var messages = await _chatRepo.GetConversationMessagesAsync(conversationId, pageNumber, pageSize);
                var messageDtos = messages.Select(m => new ChatMessageDto
                {
                    MessageId = m.MessageId,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderType = m.SenderType,
                    MessageType = m.MessageType,
                    Content = m.Content,
                    FileUrl = m.FileUrl,
                    FileName = m.FileName,
                    FileSize = m.FileSize,
                    SentAt = m.SentAt,
                    IsDelivered = m.IsDelivered,
                    IsRead = m.IsRead,
                    ReplyToMessageId = m.ReplyToMessageId
                }).ToList();

                return (messageDtos, messageDtos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<ChatMessageDto> SendMessageAsync(int senderId, string senderType, SendMessageRequestDto request)
        {
            try
            {
                // Check if user is participant
                var isParticipant = await _chatRepo.IsParticipantAsync(request.ConversationId, senderId, senderType);
                if (!isParticipant)
                {
                    throw new UnauthorizedAccessException("You are not a participant in this conversation");
                }

                var message = new ChatMessage
                {
                    ConversationId = request.ConversationId,
                    SenderId = senderId,
                    SenderType = senderType,
                    MessageType = request.MessageType,
                    Content = request.Content,
                    FileUrl = request.FileUrl,
                    FileName = request.FileName,
                    FileSize = request.FileSize,
                    SentAt = DateTime.UtcNow,
                    ReplyToMessageId = request.ReplyToMessageId
                };

                var messageId = await _chatRepo.SendMessageAsync(message);
                message.MessageId = messageId;

                // Update conversation last message
                var lastMessage = request.MessageType == "Text" 
                    ? request.Content 
                    : $"[{request.MessageType}]";
                    
                await _chatRepo.UpdateConversationLastMessageAsync(request.ConversationId, lastMessage ?? "", DateTime.UtcNow);

                // TODO: Increment unread count for other participants
                // TODO: Send SignalR notification

                return new ChatMessageDto
                {
                    MessageId = message.MessageId,
                    ConversationId = message.ConversationId,
                    SenderId = message.SenderId,
                    SenderType = message.SenderType,
                    MessageType = message.MessageType,
                    Content = message.Content,
                    FileUrl = message.FileUrl,
                    FileName = message.FileName,
                    FileSize = message.FileSize,
                    SentAt = message.SentAt,
                    IsDelivered = false,
                    IsRead = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw;
            }
        }

        public async Task MarkMessageAsDeliveredAsync(int messageId, int userId, string userType)
        {
            try
            {
                var message = await _chatRepo.GetMessageByIdAsync(messageId);
                if (message == null)
                {
                    return;
                }

                var isParticipant = await _chatRepo.IsParticipantAsync(message.ConversationId, userId, userType);
                if (!isParticipant)
                {
                    throw new UnauthorizedAccessException("You are not a participant in this conversation");
                }

                if (message.SenderId == userId && string.Equals(message.SenderType, userType, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                await _chatRepo.MarkMessageAsDeliveredAsync(messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as delivered", messageId);
                throw;
            }
        }

        public async Task MarkMessagesAsReadAsync(int conversationId, int userId, string userType, List<int> messageIds)
        {
            try
            {
                await _chatRepo.MarkMessagesAsReadAsync(conversationId, userId, userType, messageIds);
                _logger.LogInformation("User {UserId} marked {Count} messages as read in conversation {ConversationId}", userId, messageIds.Count, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read");
                throw;
            }
        }

        public async Task DeleteMessageAsync(int messageId, int userId, string userType)
        {
            try
            {
                await _chatRepo.DeleteMessageAsync(messageId, userId, userType);
                _logger.LogInformation("User {UserId} deleted message {MessageId}", userId, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                throw;
            }
        }

        public async Task UpdateUserStatusAsync(int userId, string userType, bool isOnline, string? connectionId = null, string? deviceType = null)
        {
            try
            {
                await _chatRepo.UpdateUserStatusAsync(userId, userType, isOnline, connectionId, deviceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status for {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> GetUserOnlineStatusAsync(int userId, string userType)
        {
            try
            {
                return await _chatRepo.GetUserOnlineStatusAsync(userId, userType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting online status for user {UserId}", userId);
                return false;
            }
        }
    }
}
