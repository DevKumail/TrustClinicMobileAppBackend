USE [CoherentMobApp]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MDeviceTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MDeviceTokens] (
        [DeviceTokenId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [UserType] NVARCHAR(20) NOT NULL,
        [Token] NVARCHAR(512) NOT NULL,
        [Platform] NVARCHAR(20) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [CHK_DeviceTokenUserType] CHECK ([UserType] IN ('Patient', 'Doctor', 'Staff'))
    )

    CREATE UNIQUE INDEX [UX_DeviceTokens_User_Token]
        ON [dbo].[MDeviceTokens]([UserId], [UserType], [Token])

    CREATE INDEX [IX_DeviceTokens_User_Active]
        ON [dbo].[MDeviceTokens]([UserId], [UserType], [IsActive])
        WHERE [IsActive] = 1

    PRINT 'Table MDeviceTokens created successfully'
END
ELSE
BEGIN
    PRINT 'Table MDeviceTokens already exists'
END
GO
