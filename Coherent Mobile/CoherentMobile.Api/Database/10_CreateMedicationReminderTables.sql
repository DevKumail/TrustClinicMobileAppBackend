USE [CoherentMobApp]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MMedicationReminders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MMedicationReminders] (
        [MedicationReminderId] INT IDENTITY(1,1) PRIMARY KEY,
        [UserId] INT NOT NULL,
        [UserType] NVARCHAR(20) NOT NULL,
        [MedicationName] NVARCHAR(200) NULL,
        [Dosage] NVARCHAR(200) NULL,
        [Title] NVARCHAR(200) NULL,
        [Body] NVARCHAR(1000) NULL,
        [DataJson] NVARCHAR(MAX) NULL,
        [NextTriggerAtUtc] DATETIME2 NOT NULL,
        [RepeatIntervalMinutes] INT NULL,
        [LastTriggeredAtUtc] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAtUtc] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAtUtc] DATETIME2 NULL,
        CONSTRAINT [CHK_MedicationReminderUserType] CHECK ([UserType] IN ('Patient', 'Doctor', 'Staff'))
    )

    CREATE INDEX [IX_MedicationReminders_User_Next]
        ON [dbo].[MMedicationReminders]([UserId], [UserType], [IsActive], [NextTriggerAtUtc])

    CREATE INDEX [IX_MedicationReminders_Due]
        ON [dbo].[MMedicationReminders]([IsActive], [NextTriggerAtUtc])
        WHERE [IsActive] = 1

    PRINT 'Table MMedicationReminders created successfully'
END
ELSE
BEGIN
    PRINT 'Table MMedicationReminders already exists'
END
GO
