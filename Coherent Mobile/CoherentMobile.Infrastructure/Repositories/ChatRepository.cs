using CoherentMobile.Domain.Entities;
using CoherentMobile.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;

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

        public async Task<IEnumerable<int>> GetActiveThreadIdsAsync(TimeSpan withinPeriod)
        {
            using var connection = CreateConnection();
            var cutoff = DateTime.UtcNow.Subtract(withinPeriod);
            var sql = @"
                SELECT DISTINCT c.ConversationId 
                FROM MConversations c
                WHERE c.IsActive = 1 
                  AND (c.LastMessageAt IS NULL OR c.LastMessageAt >= @Cutoff)
                ORDER BY c.ConversationId";
            return await connection.QueryAsync<int>(sql, new { Cutoff = cutoff });
        }

        public async Task<bool> CrmMessageExistsAsync(string crmMessageId)
        {
            if (string.IsNullOrWhiteSpace(crmMessageId))
                return false;
                
            using var connection = CreateConnection();
            var sql = @"SELECT CAST(CASE WHEN EXISTS (
                SELECT 1 FROM MChatMessages WHERE CrmMessageId = @CrmMessageId
            ) THEN 1 ELSE 0 END AS BIT)";
            return await connection.ExecuteScalarAsync<bool>(sql, new { CrmMessageId = crmMessageId });
        }

        public async Task<int> SaveCrmMessageAsync(ChatMessage message, string crmMessageId)
        {
            using var connection = CreateConnection();
            
            // Check if already exists (idempotency)
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT MessageId FROM MChatMessages WHERE CrmMessageId = @CrmMessageId",
                new { CrmMessageId = crmMessageId });
            
            if (existingId.HasValue)
                return existingId.Value;
            
            var sql = @"
                INSERT INTO MChatMessages (ConversationId, SenderId, SenderType, MessageType, Content, FileUrl, FileName, FileSize, SentAt, IsDelivered, IsRead, ReplyToMessageId, CrmMessageId)
                VALUES (@ConversationId, @SenderId, @SenderType, @MessageType, @Content, @FileUrl, @FileName, @FileSize, @SentAt, 0, 0, @ReplyToMessageId, @CrmMessageId);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                message.ConversationId,
                message.SenderId,
                message.SenderType,
                message.MessageType,
                message.Content,
                message.FileUrl,
                message.FileName,
                message.FileSize,
                message.SentAt,
                message.ReplyToMessageId,
                CrmMessageId = crmMessageId
            });
        }

        public async Task<List<ChatBroadcastChannelListItem>> GetBroadcastChannelsForStaffAsync(string staffType, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(staffType))
                throw new ArgumentException("staffType is required", nameof(staffType));

            if (limit <= 0)
                limit = 50;

            limit = Math.Min(limit, 200);

            var channelTitle = $"{staffType} Channel";

            using var connection = CreateConnection();

            var sql = @"
                SELECT TOP (@Limit)
                    c.ConversationId,
                    c.Title AS ChannelTitle,
                    c.LastMessageAt,
                    c.LastMessage AS LastMessagePreview,
                    u.MRNO AS PatientMrNo,
                    COALESCE(NULLIF(u.FullName, ''), NULLIF(u.MRNO, ''), CAST(u.Id AS NVARCHAR(50))) AS PatientName,
                    (
                        SELECT COUNT(1)
                        FROM dbo.MChatMessages m
                        WHERE m.ConversationId = c.ConversationId
                          AND m.SenderType = 'Patient'
                          AND m.IsRead = 0
                          AND m.IsDeleted = 0
                    ) AS UnreadCount
                FROM dbo.MConversations c
                INNER JOIN dbo.MConversationParticipants cp
                    ON cp.ConversationId = c.ConversationId
                   AND cp.UserType = 'Patient'
                INNER JOIN dbo.Users u
                    ON u.Id = cp.UserId
                WHERE c.ConversationType = 'Broadcast'
                  AND c.IsActive = 1
                  AND (
                        c.Title = @ChannelTitle
                        OR c.Title = @ChannelTitleAlt
                      )
                ORDER BY COALESCE(c.LastMessageAt, '1900-01-01') DESC, c.ConversationId DESC";

            var rows = await connection.QueryAsync<ChatBroadcastChannelListItem>(sql, new
            {
                ChannelTitle = channelTitle,
                ChannelTitleAlt = $"{staffType} Support Channel",
                Limit = limit
            });

            return rows.ToList();
        }

        public async Task<(int totalUnreadCount, int channelsWithUnread)> GetBroadcastUnreadSummaryForStaffAsync(string staffType)
        {
            if (string.IsNullOrWhiteSpace(staffType))
                throw new ArgumentException("staffType is required", nameof(staffType));

            var channelTitle = $"{staffType} Channel";

            using var connection = CreateConnection();

            var sql = @"
                SELECT
                    SUM(x.UnreadCount) AS TotalUnreadCount,
                    SUM(CASE WHEN x.UnreadCount > 0 THEN 1 ELSE 0 END) AS ChannelsWithUnread
                FROM (
                    SELECT
                        c.ConversationId,
                        (
                            SELECT COUNT(1)
                            FROM dbo.MChatMessages m
                            WHERE m.ConversationId = c.ConversationId
                              AND m.SenderType = 'Patient'
                              AND m.IsRead = 0
                              AND m.IsDeleted = 0
                        ) AS UnreadCount
                    FROM dbo.MConversations c
                    WHERE c.ConversationType = 'Support'
                      AND c.IsActive = 1
                      AND (
                            c.Title = @ChannelTitle
                            OR c.Title = @ChannelTitleAlt
                          )
                ) x";

            var result = await connection.QueryFirstOrDefaultAsync(sql, new
            {
                ChannelTitle = channelTitle,
                ChannelTitleAlt = $"{staffType} Support Channel"
            });

            var totalUnread = result?.TotalUnreadCount == null ? 0 : (int)result.TotalUnreadCount;
            var channelsWithUnread = result?.ChannelsWithUnread == null ? 0 : (int)result.ChannelsWithUnread;
            return (totalUnread, channelsWithUnread);
        }

        public async Task<int> MarkBroadcastChannelAsReadForStaffAsync(int conversationId)
        {
            if (conversationId <= 0)
                throw new ArgumentException("conversationId is required", nameof(conversationId));

            using var connection = CreateConnection();

            var sql = @"
                UPDATE dbo.MChatMessages
                SET IsRead = 1
                WHERE ConversationId = @ConversationId
                  AND SenderType = 'Patient'
                  AND IsRead = 0
                  AND IsDeleted = 0";

            var affected = await connection.ExecuteAsync(sql, new { ConversationId = conversationId });
            return affected;
        }

        // ══════════════════════════════════════════════════════════════
        //  CRM V2 direct-DB methods (replaces API proxy to Web Backend)
        // ══════════════════════════════════════════════════════════════

        public async Task<int?> ResolvePatientIdAsync(string mrNo)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT TOP 1 Id FROM dbo.Users WHERE MRNO = @MRNO AND IsDeleted = 0",
                new { MRNO = mrNo });
        }

        public async Task<int?> ResolveDoctorIdAsync(string licenceNo)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT TOP 1 DId FROM dbo.MDoctors WHERE LicenceNo = @LicenceNo",
                new { LicenceNo = licenceNo });
        }

        public async Task<int> ResolveSenderIdAsync(string senderType, string? mrNo, string? licenceNo, long? empId)
        {
            if (string.Equals(senderType, "Patient", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(mrNo))
                    throw new ArgumentException("senderMrNo is required for Patient");
                var id = await ResolvePatientIdAsync(mrNo);
                return id ?? throw new InvalidOperationException($"Patient not found for MRNO {mrNo}");
            }

            if (string.Equals(senderType, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(licenceNo))
                    throw new ArgumentException("senderDoctorLicenseNo is required for Doctor");
                var id = await ResolveDoctorIdAsync(licenceNo);
                return id ?? throw new InvalidOperationException($"Doctor not found for LicenceNo {licenceNo}");
            }

            if (string.Equals(senderType, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                if (!empId.HasValue)
                    throw new ArgumentException("senderEmpId is required for Staff");
                return (int)empId.Value;
            }

            throw new ArgumentException($"Unsupported senderType: {senderType}");
        }

        public async Task<int> CrmGetOrCreateThreadAsync(string patientMrNo, string doctorLicenseNo)
        {
            var patientId = await ResolvePatientIdAsync(patientMrNo)
                ?? throw new InvalidOperationException($"Patient not found for MRNO {patientMrNo}");

            var doctorId = await ResolveDoctorIdAsync(doctorLicenseNo)
                ?? throw new InvalidOperationException($"Doctor not found for LicenceNo {doctorLicenseNo}");

            using var connection = CreateConnection();
            var conversationId = await connection.QueryFirstOrDefaultAsync<int?>(
                "EXEC dbo.SP_CreateOrGetConversation @User1Id, @User1Type, @User2Id, @User2Type",
                new { User1Id = patientId, User1Type = "Patient", User2Id = doctorId, User2Type = "Doctor" });

            if (conversationId == null || conversationId.Value <= 0)
                throw new InvalidOperationException("Failed to create or get conversation");

            return conversationId.Value;
        }

        public async Task<(int messageId, bool isDuplicate)> CrmInsertMessageAsync(
            int conversationId, int senderId, string senderType, string messageType,
            string? content, string? fileUrl, string? fileName, long? fileSize,
            DateTime sentAt, Guid clientMessageId)
        {
            using var connection = CreateConnection();

            // Idempotency check
            var existingId = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT TOP 1 MessageId FROM dbo.MChatMessages WHERE ConversationId = @ConversationId AND ClientMessageId = @ClientMessageId",
                new { ConversationId = conversationId, ClientMessageId = clientMessageId });

            if (existingId.HasValue)
                return (existingId.Value, true);

            var sql = @"
                INSERT INTO dbo.MChatMessages
                    (ConversationId, SenderId, SenderType, MessageType, Content, FileUrl, FileName, FileSize, SentAt, IsDelivered, IsRead, IsDeleted, ClientMessageId)
                VALUES
                    (@ConversationId, @SenderId, @SenderType, @MessageType, @Content, @FileUrl, @FileName, @FileSize, @SentAt, 0, 0, 0, @ClientMessageId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var messageId = await connection.QuerySingleAsync<int>(sql, new
            {
                ConversationId = conversationId,
                SenderId = senderId,
                SenderType = senderType,
                MessageType = messageType,
                Content = content,
                FileUrl = fileUrl,
                FileName = fileName,
                FileSize = fileSize,
                SentAt = sentAt,
                ClientMessageId = clientMessageId
            });

            // Update conversation last message
            var lastMsg = !string.IsNullOrWhiteSpace(content)
                ? (content.Length > 500 ? content.Substring(0, 500) : content)
                : !string.IsNullOrWhiteSpace(fileName)
                    ? (fileName.Length > 500 ? fileName.Substring(0, 500) : fileName)
                    : messageType;

            await connection.ExecuteAsync(
                "UPDATE dbo.MConversations SET LastMessageAt = @Now, LastMessage = @LastMessage WHERE ConversationId = @ConversationId",
                new { Now = DateTime.UtcNow, LastMessage = lastMsg, ConversationId = conversationId });

            return (messageId, false);
        }

        public async Task<List<CrmMessageUpdateRow>> CrmGetDoctorToPatientUpdatesAsync(DateTime sinceUtc, int limit)
        {
            if (limit <= 0) limit = 100;

            using var connection = CreateConnection();

            var sql = @"
                SELECT TOP (@Limit)
                    c.ConversationId,
                    m.MessageId,
                    m.SentAt,
                    d.LicenceNo AS DoctorLicenseNo,
                    u.MRNO AS PatientMrNo,
                    m.MessageType,
                    m.Content,
                    m.FileUrl,
                    m.FileName,
                    m.FileSize
                FROM dbo.MChatMessages m
                INNER JOIN dbo.MConversations c ON c.ConversationId = m.ConversationId
                INNER JOIN dbo.MConversationParticipants pDoctor ON pDoctor.ConversationId = c.ConversationId AND pDoctor.UserType = 'Doctor'
                INNER JOIN dbo.MConversationParticipants pPatient ON pPatient.ConversationId = c.ConversationId AND pPatient.UserType = 'Patient'
                INNER JOIN dbo.MDoctors d ON d.DId = pDoctor.UserId
                INNER JOIN dbo.Users u ON u.Id = pPatient.UserId
                WHERE m.SenderType = 'Doctor'
                  AND m.SentAt > @SinceUtc
                  AND m.IsDeleted = 0
                ORDER BY m.SentAt ASC";

            var rows = await connection.QueryAsync<CrmMessageUpdateRow>(sql, new { SinceUtc = sinceUtc, Limit = limit });
            return rows.ToList();
        }

        public async Task<List<CrmConversationRow>> CrmGetPatientConversationsAsync(string patientMrNo, int limit)
        {
            if (limit <= 0) limit = 50;
            limit = Math.Min(limit, 200);

            using var connection = CreateConnection();

            var sql = @"
                SELECT TOP (@Limit)
                    c.ConversationId,
                    c.LastMessageAt,
                    c.LastMessage AS LastMessagePreview,
                    d.LicenceNo AS DoctorLicenseNo,
                    d.DoctorName,
                    d.Title AS DoctorTitle,
                    d.DoctorPhotoName,
                    (
                        SELECT COUNT(1)
                        FROM dbo.MChatMessages m
                        WHERE m.ConversationId = c.ConversationId
                          AND m.SenderType = 'Doctor'
                          AND m.IsRead = 0
                          AND m.IsDeleted = 0
                    ) AS UnreadCount
                FROM dbo.MConversations c
                INNER JOIN dbo.MConversationParticipants pPatient
                    ON pPatient.ConversationId = c.ConversationId
                   AND pPatient.UserType = 'Patient'
                INNER JOIN dbo.Users u
                    ON u.Id = pPatient.UserId
                INNER JOIN dbo.MConversationParticipants pDoctor
                    ON pDoctor.ConversationId = c.ConversationId
                   AND pDoctor.UserType = 'Doctor'
                INNER JOIN dbo.MDoctors d
                    ON d.DId = pDoctor.UserId
                WHERE u.MRNO = @PatientMrNo
                ORDER BY COALESCE(c.LastMessageAt, '1900-01-01') DESC, c.ConversationId DESC";

            var rows = await connection.QueryAsync<CrmConversationRow>(sql, new { PatientMrNo = patientMrNo, Limit = limit });
            return rows.ToList();
        }

        public async Task<(int conversationId, string channelTitle)> CrmGetOrCreateBroadcastChannelAsync(int patientUserId, string staffType)
        {
            using var connection = CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "EXEC dbo.SP_CreateOrGetBroadcastChannel @PatientUserId, @StaffType",
                new { PatientUserId = patientUserId, StaffType = staffType });

            if (result == null || (int)result.ConversationId <= 0)
                throw new InvalidOperationException("Failed to create or get broadcast channel");

            return ((int)result.ConversationId, (string)(result.ChannelTitle ?? $"{staffType} Support Channel"));
        }

        public async Task<List<CrmThreadMessageRow>> CrmGetThreadMessagesAsync(int conversationId, int take)
        {
            if (take <= 0) take = 50;

            using var connection = CreateConnection();

            var sql = @"
                SELECT TOP (@Take)
                    m.MessageId,
                    m.SenderType,
                    m.MessageType,
                    m.Content,
                    m.FileUrl,
                    m.FileName,
                    m.FileSize,
                    m.SentAt
                FROM dbo.MChatMessages m
                WHERE m.ConversationId = @ConversationId
                  AND m.IsDeleted = 0
                ORDER BY m.SentAt DESC, m.MessageId DESC";

            var rows = await connection.QueryAsync<CrmThreadMessageRow>(sql, new { ConversationId = conversationId, Take = take });
            // Return ascending for UI rendering
            return rows.OrderBy(x => x.SentAt).ThenBy(x => x.MessageId).ToList();
        }
    }
}
