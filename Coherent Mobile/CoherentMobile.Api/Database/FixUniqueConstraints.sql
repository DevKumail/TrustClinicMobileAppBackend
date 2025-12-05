-- =============================================
-- Fix UNIQUE Constraints to Allow Multiple NULLs
-- =============================================

USE CoherentMobApp;
GO

-- Drop existing UNIQUE constraints
DECLARE @ConstraintName NVARCHAR(200);
DECLARE @SQL NVARCHAR(MAX);

-- Find and drop EmiratesId UNIQUE constraint
SELECT @ConstraintName = name
FROM sys.key_constraints
WHERE type = 'UQ'
AND parent_object_id = OBJECT_ID('Users')
AND name LIKE '%EmiratesId%';

IF @ConstraintName IS NOT NULL
BEGIN
    SET @SQL = 'ALTER TABLE Users DROP CONSTRAINT ' + @ConstraintName;
    EXEC sp_executesql @SQL;
    PRINT 'Dropped EmiratesId UNIQUE constraint: ' + @ConstraintName;
END

-- Find and drop PassportNumber UNIQUE constraint
SELECT @ConstraintName = name
FROM sys.key_constraints
WHERE type = 'UQ'
AND parent_object_id = OBJECT_ID('Users')
AND name LIKE '%PassportNumber%';

IF @ConstraintName IS NOT NULL
BEGIN
    SET @SQL = 'ALTER TABLE Users DROP CONSTRAINT ' + @ConstraintName;
    EXEC sp_executesql @SQL;
    PRINT 'Dropped PassportNumber UNIQUE constraint: ' + @ConstraintName;
END

-- Find and drop Email UNIQUE constraint (if exists)
SELECT @ConstraintName = name
FROM sys.key_constraints
WHERE type = 'UQ'
AND parent_object_id = OBJECT_ID('Users')
AND name LIKE '%Email%';

IF @ConstraintName IS NOT NULL
BEGIN
    SET @SQL = 'ALTER TABLE Users DROP CONSTRAINT ' + @ConstraintName;
    EXEC sp_executesql @SQL;
    PRINT 'Dropped Email UNIQUE constraint: ' + @ConstraintName;
END

-- Find and drop MRNO UNIQUE constraint (if exists)
SELECT @ConstraintName = name
FROM sys.key_constraints
WHERE type = 'UQ'
AND parent_object_id = OBJECT_ID('Users')
AND name LIKE '%MRNO%';

IF @ConstraintName IS NOT NULL
BEGIN
    SET @SQL = 'ALTER TABLE Users DROP CONSTRAINT ' + @ConstraintName;
    EXEC sp_executesql @SQL;
    PRINT 'Dropped MRNO UNIQUE constraint: ' + @ConstraintName;
END
GO

-- =============================================
-- Create Filtered UNIQUE Indexes (Allow Multiple NULLs)
-- =============================================

-- MRNO: Must be unique (NOT NULL, so normal index is fine)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_MRNO' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX UQ_Users_MRNO ON Users(MRNO);
    PRINT 'Created UNIQUE index on MRNO';
END

-- EmiratesId: Unique only for non-NULL values
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_EmiratesId' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX UQ_Users_EmiratesId 
    ON Users(EmiratesId) 
    WHERE EmiratesId IS NOT NULL;
    PRINT 'Created filtered UNIQUE index on EmiratesId';
END

-- PassportNumber: Unique only for non-NULL values
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_PassportNumber' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX UQ_Users_PassportNumber 
    ON Users(PassportNumber) 
    WHERE PassportNumber IS NOT NULL;
    PRINT 'Created filtered UNIQUE index on PassportNumber';
END

-- Email: Unique only for non-NULL values (optional)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX UQ_Users_Email 
    ON Users(Email) 
    WHERE Email IS NOT NULL;
    PRINT 'Created filtered UNIQUE index on Email';
END

-- MobileNumber: Unique only for non-NULL values
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_MobileNumber' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX UQ_Users_MobileNumber 
    ON Users(MobileNumber) 
    WHERE MobileNumber IS NOT NULL;
    PRINT 'Created filtered UNIQUE index on MobileNumber';
END
GO

PRINT '✅ UNIQUE constraints fixed successfully!';
PRINT 'Now multiple users can have NULL in EmiratesId, PassportNumber, or Email';
GO

-- Verify the indexes
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.filter_definition AS FilterDefinition,
    c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('Users')
AND i.is_unique = 1
ORDER BY i.name;
GO
