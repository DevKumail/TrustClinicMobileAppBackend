-- =============================================
-- Coherent Mobile Health Database Schema
-- SQL Server Database Creation Script
-- =============================================

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CoherentHealthDb')
BEGIN
    CREATE DATABASE CoherentHealthDb;
END
GO

USE CoherentHealthDb;
GO

-- =============================================
-- Users Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(500) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        DateOfBirth DATE NULL,
        Gender NVARCHAR(10) NOT NULL,
        IsEmailVerified BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        LastLoginAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        INDEX IX_Users_Email (Email),
        INDEX IX_Users_IsActive (IsActive),
        INDEX IX_Users_IsDeleted (IsDeleted)
    );
END
GO

-- =============================================
-- HealthRecords Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HealthRecords')
BEGIN
    CREATE TABLE HealthRecords (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        RecordType NVARCHAR(50) NOT NULL,
        Value NVARCHAR(100) NOT NULL,
        Unit NVARCHAR(20) NOT NULL,
        RecordedAt DATETIME2 NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        
        CONSTRAINT FK_HealthRecords_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE,
        
        INDEX IX_HealthRecords_UserId (UserId),
        INDEX IX_HealthRecords_RecordType (RecordType),
        INDEX IX_HealthRecords_RecordedAt (RecordedAt),
        INDEX IX_HealthRecords_IsDeleted (IsDeleted)
    );
END
GO

-- =============================================
-- Sample Data (Optional - for testing)
-- =============================================
-- Uncomment to insert sample data

/*
-- Sample User (Password: SecureP@ssw0rd!)
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, PhoneNumber, Gender, IsEmailVerified, IsActive)
VALUES (
    NEWID(),
    'john.doe@example.com',
    'U2VjdXJlUEBzc3cwcmQh', -- Base64 encoded (simplified for demo, use BCrypt in production)
    'John',
    'Doe',
    '+1234567890',
    'Male',
    1,
    1
);

-- Sample Health Records
DECLARE @userId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'john.doe@example.com');

INSERT INTO HealthRecords (Id, UserId, RecordType, Value, Unit, RecordedAt)
VALUES 
    (NEWID(), @userId, 'BloodPressure', '120/80', 'mmHg', GETUTCDATE()),
    (NEWID(), @userId, 'HeartRate', '72', 'bpm', GETUTCDATE()),
    (NEWID(), @userId, 'Weight', '75.5', 'kg', GETUTCDATE()),
    (NEWID(), @userId, 'Temperature', '36.6', '°C', GETUTCDATE());
*/

-- =============================================
-- Stored Procedures (Optional - for advanced scenarios)
-- =============================================

-- Get User Health Summary
CREATE OR ALTER PROCEDURE sp_GetUserHealthSummary
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SELECT 
        RecordType,
        COUNT(*) AS RecordCount,
        MIN(RecordedAt) AS FirstRecordDate,
        MAX(RecordedAt) AS LastRecordDate
    FROM HealthRecords
    WHERE UserId = @UserId AND IsDeleted = 0
    GROUP BY RecordType
    ORDER BY RecordType;
END
GO

-- Get Recent Health Records
CREATE OR ALTER PROCEDURE sp_GetRecentHealthRecords
    @UserId UNIQUEIDENTIFIER,
    @Days INT = 7
AS
BEGIN
    SELECT TOP 100
        Id,
        RecordType,
        Value,
        Unit,
        RecordedAt,
        Notes,
        CreatedAt
    FROM HealthRecords
    WHERE UserId = @UserId 
        AND IsDeleted = 0
        AND RecordedAt >= DATEADD(DAY, -@Days, GETUTCDATE())
    ORDER BY RecordedAt DESC;
END
GO

-- =============================================
-- Views (Optional)
-- =============================================

-- Active Users View
CREATE OR ALTER VIEW vw_ActiveUsers
AS
SELECT 
    Id,
    Email,
    FirstName,
    LastName,
    PhoneNumber,
    Gender,
    IsEmailVerified,
    LastLoginAt,
    CreatedAt
FROM Users
WHERE IsActive = 1 AND IsDeleted = 0;
GO

-- Health Records Summary View
CREATE OR ALTER VIEW vw_HealthRecordsSummary
AS
SELECT 
    u.Id AS UserId,
    u.FirstName + ' ' + u.LastName AS FullName,
    u.Email,
    hr.RecordType,
    COUNT(hr.Id) AS TotalRecords,
    MAX(hr.RecordedAt) AS LastRecordDate
FROM Users u
LEFT JOIN HealthRecords hr ON u.Id = hr.UserId AND hr.IsDeleted = 0
WHERE u.IsDeleted = 0
GROUP BY u.Id, u.FirstName, u.LastName, u.Email, hr.RecordType;
GO

-- =============================================
-- Grant Permissions (if using specific SQL user)
-- =============================================
-- GRANT SELECT, INSERT, UPDATE, DELETE ON Users TO [YourDatabaseUser];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON HealthRecords TO [YourDatabaseUser];
-- GRANT EXECUTE ON sp_GetUserHealthSummary TO [YourDatabaseUser];
-- GRANT EXECUTE ON sp_GetRecentHealthRecords TO [YourDatabaseUser];

PRINT 'Database schema created successfully!';
GO
