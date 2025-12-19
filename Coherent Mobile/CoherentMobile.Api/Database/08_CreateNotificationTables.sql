-- =============================================
-- Notifications Database Schema
-- =============================================

USE [CoherentMobApp]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MNotifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MNotifications] (
        [NotificationId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [UserType] NVARCHAR(20) NOT NULL,
        [NotificationType] NVARCHAR(50) NOT NULL,
        [Title] NVARCHAR(200) NULL,
        [Body] NVARCHAR(1000) NULL,
        [DataJson] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [DeliveredAt] DATETIME2 NULL,
        [ReadAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        CONSTRAINT [CHK_NotificationUserType] CHECK ([UserType] IN ('Patient', 'Doctor', 'Staff'))
    )

    CREATE INDEX [IX_Notifications_User_CreatedAt]
        ON [dbo].[MNotifications]([UserId], [UserType], [CreatedAt] DESC)

    CREATE INDEX [IX_Notifications_User_Unread]
        ON [dbo].[MNotifications]([UserId], [UserType], [ReadAt])
        WHERE [ReadAt] IS NULL AND [IsDeleted] = 0

    PRINT 'Table MNotifications created successfully'
END
ELSE
BEGIN
    PRINT 'Table MNotifications already exists'
END
GO
