using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CoherentMobile.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly string _connectionString;

        public ChatRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        // Conversations
        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId, string userType, int pageNumber, int pageSize)
        {
            using var connection = CreateConnection();
            var parameters = new { UserId = userId, UserType = userType, PageNumber = pageNumber, PageSize = pageSize };
            return await connection.QueryAsync<Conversation>("SP_GetUserConversations", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT * FROM MConversations WHERE ConversationId = @ConversationId";
            return await connection.QueryFirstOrDefaultAsync<Conversation>(sql, new { ConversationId = conversationId });
        }

        public async Task<int> CreateConversationAsync(string conversationType, int createdBy, string? title = null)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO MConversations (ConversationType, Title, CreatedBy, CreatedAt, IsActive)
                VALUES (@ConversationType, @Title, @CreatedBy, GETUTCDATE(), 1);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            return await connection.ExecuteScalarAsync<int>(sql, new { ConversationType = conversationType, Title = title, CreatedBy = createdBy });
        }

        public async Task<int?> GetOrCreateOneToOneConversationAsync(int user1Id, string user1Type, int user2Id, string user2Type)
        {
            using var connection = CreateConnection();
            var parameters = new { User1Id = user1Id, User1Type = user1Type, User2Id = user2Id, User2Type = user2Type };
            var result = await connection.QueryFirstOrDefaultAsync<int?>("SP_CreateOrGetConversation", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task UpdateConversationLastMessageAsync(int conversationId, string lastMessage, DateTime lastMessageAt)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE MConversations 
                SET LastMessage = @LastMessage, LastMessageAt = @LastMessageAt
                WHERE ConversationId = @ConversationId";
            
            await connection.ExecuteAsync(sql, new { ConversationId = conversationId, LastMessage = lastMessage, LastMessageAt = lastMessageAt });
        }

        // Participants
        public async Task AddParticipantAsync(int conversationId, int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM MConversationParticipants 
                              WHERE ConversationId = @ConversationId AND UserId = @UserId AND UserType = @UserType)
                BEGIN
                    INSERT INTO MConversationParticipants (ConversationId, UserId, UserType, JoinedAt, IsActive, UnreadCount)
                    VALUES (@ConversationId, @UserId, @UserType, GETUTCDATE(), 1, 0)
                END";
            
            await connection.ExecuteAsync(sql, new { ConversationId = conversationId, UserId = userId, UserType = userType });
        }

        public async Task<bool> IsParticipantAsync(int conversationId, int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1 FROM MConversationParticipants 
                    WHERE ConversationId = @ConversationId AND UserId = @UserId AND UserType = @UserType AND IsActive = 1
                ) THEN 1 ELSE 0 END AS BIT)";
            
            return await connection.ExecuteScalarAsync<bool>(sql, new { ConversationId = conversationId, UserId = userId, UserType = userType });
        }

        public async Task UpdateUnreadCountAsync(int conversationId, int userId, string userType, int count)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE MConversationParticipants 
                SET UnreadCount = @Count
                WHERE ConversationId = @ConversationId AND UserId = @UserId AND UserType = @UserType";
            
            await connection.ExecuteAsync(sql, new { ConversationId = conversationId, UserId = userId, UserType = userType, Count = count });
        }

        public async Task ResetUnreadCountAsync(int conversationId, int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE MConversationParticipants 
                SET UnreadCount = 0
                WHERE ConversationId = @ConversationId AND UserId = @UserId AND UserType = @UserType";
            
            await connection.ExecuteAsync(sql, new { ConversationId = conversationId, UserId = userId, UserType = userType });
        }

        public async Task<ConversationParticipant?> GetParticipantAsync(int conversationId, int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"
                SELECT * FROM MConversationParticipants 
                WHERE ConversationId = @ConversationId AND UserId = @UserId AND UserType = @UserType";
            
            return await connection.QueryFirstOrDefaultAsync<ConversationParticipant>(sql, new { ConversationId = conversationId, UserId = userId, UserType = userType });
        }

        // Messages
        public async Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(int conversationId, int pageNumber, int pageSize)
        {
            using var connection = CreateConnection();
            var parameters = new { ConversationId = conversationId, PageNumber = pageNumber, PageSize = pageSize };
            return await connection.QueryAsync<ChatMessage>("SP_GetConversationMessages", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<ChatMessage?> GetMessageByIdAsync(int messageId)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT * FROM MChatMessages WHERE MessageId = @MessageId AND IsDeleted = 0";
            return await connection.QueryFirstOrDefaultAsync<ChatMessage>(sql, new { MessageId = messageId });
        }

        public async Task<int> SendMessageAsync(ChatMessage message)
        {
            using var connection = CreateConnection();
            var sql = @"
                INSERT INTO MChatMessages (ConversationId, SenderId, SenderType, MessageType, Content, FileUrl, FileName, FileSize, SentAt, IsDelivered, IsRead, ReplyToMessageId)
                VALUES (@ConversationId, @SenderId, @SenderType, @MessageType, @Content, @FileUrl, @FileName, @FileSize, @SentAt, 0, 0, @ReplyToMessageId);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            return await connection.ExecuteScalarAsync<int>(sql, message);
        }

        public async Task MarkMessageAsDeliveredAsync(int messageId)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE MChatMessages SET IsDelivered = 1 WHERE MessageId = @MessageId";
            await connection.ExecuteAsync(sql, new { MessageId = messageId });
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            using var connection = CreateConnection();
            var sql = @"UPDATE MChatMessages SET IsRead = 1 WHERE MessageId = @MessageId";
            await connection.ExecuteAsync(sql, new { MessageId = messageId });
        }

        public async Task MarkMessagesAsReadAsync(int conversationId, int userId, string userType, List<int> messageIds)
        {
            using var connection = CreateConnection();
            
            // Mark messages as read
            var sql = @"UPDATE MChatMessages SET IsRead = 1 WHERE MessageId IN @MessageIds";
            await connection.ExecuteAsync(sql, new { MessageIds = messageIds });

            // Update unread count
            await ResetUnreadCountAsync(conversationId, userId, userType);
        }

        public async Task DeleteMessageAsync(int messageId, int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"
                UPDATE MChatMessages 
                SET IsDeleted = 1, DeletedAt = GETUTCDATE()
                WHERE MessageId = @MessageId AND SenderId = @UserId AND SenderType = @UserType";
            
            await connection.ExecuteAsync(sql, new { MessageId = messageId, UserId = userId, UserType = userType });
        }

        // User Status
        public async Task UpdateUserStatusAsync(int userId, string userType, bool isOnline, string? connectionId = null, string? deviceType = null)
        {
            using var connection = CreateConnection();
            var sql = @"
                MERGE MUserStatus AS target
                USING (SELECT @UserId AS UserId, @UserType AS UserType) AS source
                ON (target.UserId = source.UserId AND target.UserType = source.UserType)
                WHEN MATCHED THEN
                    UPDATE SET IsOnline = @IsOnline, LastSeen = GETUTCDATE()
                WHEN NOT MATCHED THEN
                    INSERT (UserId, UserType, IsOnline, LastSeen)
                    VALUES (@UserId, @UserType, @IsOnline, GETUTCDATE());";
            
            await connection.ExecuteAsync(sql, new { UserId = userId, UserType = userType, IsOnline = isOnline, ConnectionId = connectionId, DeviceType = deviceType });
        }

        public async Task<bool> GetUserOnlineStatusAsync(int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ISNULL(IsOnline, 0) FROM MUserStatus WHERE UserId = @UserId AND UserType = @UserType";
            return await connection.ExecuteScalarAsync<bool>(sql, new { UserId = userId, UserType = userType });
        }

        public async Task<DateTime?> GetUserLastSeenAsync(int userId, string userType)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT LastSeen FROM MUserStatus WHERE UserId = @UserId AND UserType = @UserType";
            return await connection.ExecuteScalarAsync<DateTime?>(sql, new { UserId = userId, UserType = userType });
        }
    }
}
