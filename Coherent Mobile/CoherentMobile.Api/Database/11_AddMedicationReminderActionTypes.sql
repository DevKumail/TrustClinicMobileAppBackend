USE [CoherentMobApp]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MMedicationReminderActionTypes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[MMedicationReminderActionTypes] (
        [ActionTypeId] INT NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(50) NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1
    )

    PRINT 'Table MMedicationReminderActionTypes created successfully'
END
ELSE
BEGIN
    PRINT 'Table MMedicationReminderActionTypes already exists'
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MMedicationReminderActionTypes WHERE ActionTypeId = 1)
    INSERT INTO dbo.MMedicationReminderActionTypes (ActionTypeId, Name, IsActive) VALUES (1, 'Taken', 1)
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MMedicationReminderActionTypes WHERE ActionTypeId = 2)
    INSERT INTO dbo.MMedicationReminderActionTypes (ActionTypeId, Name, IsActive) VALUES (2, 'NotTaken', 1)
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MMedicationReminderActionTypes WHERE ActionTypeId = 3)
    INSERT INTO dbo.MMedicationReminderActionTypes (ActionTypeId, Name, IsActive) VALUES (3, 'RemindMeLater', 1)
GO

IF COL_LENGTH('dbo.MMedicationReminders', 'LastActionTypeId') IS NULL
BEGIN
    ALTER TABLE dbo.MMedicationReminders
    ADD LastActionTypeId INT NULL;
END
GO

IF COL_LENGTH('dbo.MMedicationReminders', 'LastActionAtUtc') IS NULL
BEGIN
    ALTER TABLE dbo.MMedicationReminders
    ADD LastActionAtUtc DATETIME2 NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_MMedicationReminders_ActionType'
)
BEGIN
    ALTER TABLE dbo.MMedicationReminders
    ADD CONSTRAINT FK_MMedicationReminders_ActionType
        FOREIGN KEY (LastActionTypeId)
        REFERENCES dbo.MMedicationReminderActionTypes(ActionTypeId);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_MMedicationReminders_LastActionType'
      AND object_id = OBJECT_ID('dbo.MMedicationReminders')
)
BEGIN
    CREATE INDEX IX_MMedicationReminders_LastActionType
    ON dbo.MMedicationReminders (LastActionTypeId, LastActionAtUtc);
END
GO
