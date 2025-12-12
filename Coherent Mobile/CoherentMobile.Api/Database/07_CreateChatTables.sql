-- =============================================
-- Chat System Database Schema
-- For Web Portal and Mobile App
-- =============================================

USE [CoherentMobApp]
GO

-- =============================================
-- 1. Conversations Table
-- Stores chat conversations between users
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MConversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MConversations] (
        [ConversationId] INT IDENTITY(1,1) PRIMARY KEY,
        [ConversationType] NVARCHAR(20) NOT NULL, -- 'OneToOne', 'Group', 'Support'
        [Title] NVARCHAR(200) NULL, -- For group chats
        [CreatedBy] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastMessageAt] DATETIME2 NULL,
        [LastMessage] NVARCHAR(500) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsArchived] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [CHK_ConversationType] CHECK ([ConversationType] IN ('OneToOne', 'Group', 'Support'))
    )

    CREATE INDEX [IX_Conversations_CreatedBy] ON [MConversations]([CreatedBy])
    CREATE INDEX [IX_Conversations_LastMessageAt] ON [MConversations]([LastMessageAt] DESC)
    CREATE INDEX [IX_Conversations_IsActive] ON [MConversations]([IsActive]) WHERE [IsActive] = 1

    PRINT 'Table MConversations created successfully'
END
ELSE
BEGIN
    PRINT 'Table MConversations already exists'
END
GO

-- =============================================
-- 2. Conversation Participants Table
-- Links users to conversations
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MConversationParticipants]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MConversationParticipants] (
        [ParticipantId] INT IDENTITY(1,1) PRIMARY KEY,
        [ConversationId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [UserType] NVARCHAR(20) NOT NULL, -- 'Patient', 'Doctor', 'Staff'
        [JoinedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LeftAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsMuted] BIT NOT NULL DEFAULT 0,
        [LastReadMessageId] INT NULL,
        [UnreadCount] INT NOT NULL DEFAULT 0,
        CONSTRAINT [FK_ConversationParticipants_Conversation] 
            FOREIGN KEY ([ConversationId]) REFERENCES [MConversations]([ConversationId]) ON DELETE CASCADE,
        CONSTRAINT [CHK_UserType] CHECK ([UserType] IN ('Patient', 'Doctor', 'Staff'))
    )

    CREATE UNIQUE INDEX [UX_ConversationParticipants] 
        ON [MConversationParticipants]([ConversationId], [UserId], [UserType])
    CREATE INDEX [IX_ConversationParticipants_UserId] ON [MConversationParticipants]([UserId])
    CREATE INDEX [IX_ConversationParticipants_IsActive] ON [MConversationParticipants]([IsActive]) WHERE [IsActive] = 1

    PRINT 'Table MConversationParticipants created successfully'
END
ELSE
BEGIN
    PRINT 'Table MConversationParticipants already exists'
END
GO

-- =============================================
-- 3. Messages Table
-- Stores all chat messages
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MChatMessages] (
        [MessageId] INT IDENTITY(1,1) PRIMARY KEY,
        [ConversationId] INT NOT NULL,
        [SenderId] INT NOT NULL,
        [SenderType] NVARCHAR(20) NOT NULL, -- 'Patient', 'Doctor', 'Staff'
        [MessageType] NVARCHAR(20) NOT NULL DEFAULT 'Text', -- 'Text', 'Image', 'File', 'Voice'
        [Content] NVARCHAR(MAX) NULL, -- Message text
        [FileUrl] NVARCHAR(500) NULL, -- For images/files
        [FileName] NVARCHAR(200) NULL,
        [FileSize] BIGINT NULL,
        [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [IsDelivered] BIT NOT NULL DEFAULT 0,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [ReplyToMessageId] INT NULL, -- For replying to messages
        CONSTRAINT [FK_ChatMessages_Conversation] 
            FOREIGN KEY ([ConversationId]) REFERENCES [MConversations]([ConversationId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_ReplyTo] 
            FOREIGN KEY ([ReplyToMessageId]) REFERENCES [MChatMessages]([MessageId]),
        CONSTRAINT [CHK_SenderType] CHECK ([SenderType] IN ('Patient', 'Doctor', 'Staff')),
        CONSTRAINT [CHK_MessageType] CHECK ([MessageType] IN ('Text', 'Image', 'File', 'Voice', 'Video'))
    )

    CREATE INDEX [IX_ChatMessages_ConversationId] ON [MChatMessages]([ConversationId], [SentAt] DESC)
    CREATE INDEX [IX_ChatMessages_SenderId] ON [MChatMessages]([SenderId])
    CREATE INDEX [IX_ChatMessages_IsDeleted] ON [MChatMessages]([IsDeleted]) WHERE [IsDeleted] = 0

    PRINT 'Table MChatMessages created successfully'
END
ELSE
BEGIN
    PRINT 'Table MChatMessages already exists'
END
GO

-- =============================================
-- 4. Message Receipts Table
-- Tracks who read which message
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MMessageReceipts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MMessageReceipts] (
        [ReceiptId] INT IDENTITY(1,1) PRIMARY KEY,
        [MessageId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [UserType] NVARCHAR(20) NOT NULL,
        [DeliveredAt] DATETIME2 NULL,
        [ReadAt] DATETIME2 NULL,
        CONSTRAINT [FK_MessageReceipts_Message] 
            FOREIGN KEY ([MessageId]) REFERENCES [MChatMessages]([MessageId]) ON DELETE CASCADE,
        CONSTRAINT [CHK_ReceiptUserType] CHECK ([UserType] IN ('Patient', 'Doctor', 'Staff'))
    )

    CREATE UNIQUE INDEX [UX_MessageReceipts] ON [MMessageReceipts]([MessageId], [UserId], [UserType])
    CREATE INDEX [IX_MessageReceipts_UserId] ON [MMessageReceipts]([UserId])

    PRINT 'Table MMessageReceipts created successfully'
END
ELSE
BEGIN
    PRINT 'Table MMessageReceipts already exists'
END
GO

-- =============================================
-- 5. User Status Table
-- Tracks online/offline status
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MUserStatus]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MUserStatus] (
        [StatusId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [UserType] NVARCHAR(20) NOT NULL,
        [IsOnline] BIT NOT NULL DEFAULT 0,
        [LastSeenAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ConnectionId] NVARCHAR(100) NULL, -- SignalR connection ID
        [DeviceType] NVARCHAR(20) NULL, -- 'Web', 'iOS', 'Android'
        CONSTRAINT [CHK_StatusUserType] CHECK ([UserType] IN ('Patient', 'Doctor', 'Staff')),
        CONSTRAINT [CHK_DeviceType] CHECK ([DeviceType] IN ('Web', 'iOS', 'Android', 'Desktop'))
    )

    CREATE UNIQUE INDEX [UX_UserStatus] ON [MUserStatus]([UserId], [UserType])
    CREATE INDEX [IX_UserStatus_IsOnline] ON [MUserStatus]([IsOnline]) WHERE [IsOnline] = 1

    PRINT 'Table MUserStatus created successfully'
END
ELSE
BEGIN
    PRINT 'Table MUserStatus already exists'
END
GO

-- =============================================
-- Stored Procedures
-- =============================================

-- Get User Conversations
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetUserConversations]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[SP_GetUserConversations]
GO

CREATE PROCEDURE [dbo].[SP_GetUserConversations]
    @UserId INT,
    @UserType NVARCHAR(20),
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.[ConversationId],
        c.[ConversationType],
        c.[Title],
        c.[LastMessageAt],
        c.[LastMessage],
        cp.[UnreadCount],
        cp.[IsMuted],
        cp.[LastReadMessageId],
        -- Get other participant info (for one-to-one chats)
        (SELECT TOP 1 [UserId] FROM [MConversationParticipants] 
         WHERE [ConversationId] = c.[ConversationId] 
         AND NOT ([UserId] = @UserId AND [UserType] = @UserType)) AS [OtherUserId],
        (SELECT TOP 1 [UserType] FROM [MConversationParticipants] 
         WHERE [ConversationId] = c.[ConversationId] 
         AND NOT ([UserId] = @UserId AND [UserType] = @UserType)) AS [OtherUserType]
    FROM [MConversations] c
    INNER JOIN [MConversationParticipants] cp 
        ON c.[ConversationId] = cp.[ConversationId]
    WHERE cp.[UserId] = @UserId 
        AND cp.[UserType] = @UserType
        AND cp.[IsActive] = 1
        AND c.[IsActive] = 1
    ORDER BY c.[LastMessageAt] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
END
GO

-- Get Conversation Messages
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_GetConversationMessages]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[SP_GetConversationMessages]
GO

CREATE PROCEDURE [dbo].[SP_GetConversationMessages]
    @ConversationId INT,
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        [MessageId],
        [SenderId],
        [SenderType],
        [MessageType],
        [Content],
        [FileUrl],
        [FileName],
        [FileSize],
        [SentAt],
        [IsDelivered],
        [IsRead],
        [ReplyToMessageId]
    FROM [MChatMessages]
    WHERE [ConversationId] = @ConversationId
        AND [IsDeleted] = 0
    ORDER BY [SentAt] DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY
END
GO

-- Create or Get One-to-One Conversation
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SP_CreateOrGetConversation]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[SP_CreateOrGetConversation]
GO

CREATE PROCEDURE [dbo].[SP_CreateOrGetConversation]
    @User1Id INT,
    @User1Type NVARCHAR(20),
    @User2Id INT,
    @User2Type NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ConversationId INT

    -- Check if conversation already exists
    SELECT TOP 1 @ConversationId = cp1.[ConversationId]
    FROM [MConversationParticipants] cp1
    INNER JOIN [MConversationParticipants] cp2 
        ON cp1.[ConversationId] = cp2.[ConversationId]
    INNER JOIN [MConversations] c 
        ON cp1.[ConversationId] = c.[ConversationId]
    WHERE cp1.[UserId] = @User1Id 
        AND cp1.[UserType] = @User1Type
        AND cp2.[UserId] = @User2Id 
        AND cp2.[UserType] = @User2Type
        AND c.[ConversationType] = 'OneToOne'
        AND c.[IsActive] = 1

    IF @ConversationId IS NULL
    BEGIN
        -- Create new conversation
        INSERT INTO [MConversations] ([ConversationType], [CreatedBy])
        VALUES ('OneToOne', @User1Id)
        
        SET @ConversationId = SCOPE_IDENTITY()

        -- Add participants
        INSERT INTO [MConversationParticipants] ([ConversationId], [UserId], [UserType])
        VALUES 
            (@ConversationId, @User1Id, @User1Type),
            (@ConversationId, @User2Id, @User2Type)
    END

    SELECT @ConversationId AS ConversationId
END
GO

PRINT 'Chat database schema created successfully!'
PRINT 'Tables: MConversations, MConversationParticipants, MChatMessages, MMessageReceipts, MUserStatus'
PRINT 'Stored Procedures: SP_GetUserConversations, SP_GetConversationMessages, SP_CreateOrGetConversation'
