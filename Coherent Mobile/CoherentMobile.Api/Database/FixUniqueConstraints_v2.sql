-- =============================================
-- Fix UNIQUE Constraints - Find by COLUMN, not constraint name
-- =============================================

USE CoherentMobApp;
GO

PRINT '========================================';
PRINT 'Starting UNIQUE Constraint Fix...';
PRINT '========================================';
PRINT '';

-- =============================================
-- Drop ALL UNIQUE constraints on Users table
-- =============================================
DECLARE @sql NVARCHAR(MAX) = '';
DECLARE @constraintName NVARCHAR(200);

-- Get all UNIQUE constraints (includes system-generated names)
DECLARE constraint_cursor CURSOR FOR
SELECT kc.name
FROM sys.key_constraints kc
WHERE kc.type = 'UQ' 
AND kc.parent_object_id = OBJECT_ID('Users');

OPEN constraint_cursor;
FETCH NEXT FROM constraint_cursor INTO @constraintName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = 'ALTER TABLE Users DROP CONSTRAINT [' + @constraintName + '];';
    PRINT 'Dropping constraint: ' + @constraintName;
    EXEC sp_executesql @sql;
    
    FETCH NEXT FROM constraint_cursor INTO @constraintName;
END

CLOSE constraint_cursor;
DEALLOCATE constraint_cursor;

PRINT '';
PRINT '✅ All UNIQUE constraints dropped';
PRINT '';
GO

-- =============================================
-- Create Filtered UNIQUE Indexes
-- =============================================

-- Drop existing indexes if they exist (clean slate)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_MRNO' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX UQ_Users_MRNO ON Users;
    PRINT 'Dropped existing index: UQ_Users_MRNO';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_EmiratesId' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX UQ_Users_EmiratesId ON Users;
    PRINT 'Dropped existing index: UQ_Users_EmiratesId';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_PassportNumber' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX UQ_Users_PassportNumber ON Users;
    PRINT 'Dropped existing index: UQ_Users_PassportNumber';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX UQ_Users_Email ON Users;
    PRINT 'Dropped existing index: UQ_Users_Email';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_Users_MobileNumber' AND object_id = OBJECT_ID('Users'))
BEGIN
    DROP INDEX UQ_Users_MobileNumber ON Users;
    PRINT 'Dropped existing index: UQ_Users_MobileNumber';
END

PRINT '';
GO

-- Create new filtered indexes
PRINT 'Creating filtered UNIQUE indexes...';
PRINT '';

-- MRNO: Must be unique (NOT NULL, so normal index is fine)
CREATE UNIQUE INDEX UQ_Users_MRNO ON Users(MRNO);
PRINT '✅ Created UNIQUE index on MRNO';

-- EmiratesId: Unique only for non-NULL values
CREATE UNIQUE INDEX UQ_Users_EmiratesId 
ON Users(EmiratesId) 
WHERE EmiratesId IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on EmiratesId (allows multiple NULLs)';

-- PassportNumber: Unique only for non-NULL values
CREATE UNIQUE INDEX UQ_Users_PassportNumber 
ON Users(PassportNumber) 
WHERE PassportNumber IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on PassportNumber (allows multiple NULLs)';

-- Email: Unique only for non-NULL values
CREATE UNIQUE INDEX UQ_Users_Email 
ON Users(Email) 
WHERE Email IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on Email (allows multiple NULLs)';

-- MobileNumber: Unique only for non-NULL values
CREATE UNIQUE INDEX UQ_Users_MobileNumber 
ON Users(MobileNumber) 
WHERE MobileNumber IS NOT NULL;
PRINT '✅ Created filtered UNIQUE index on MobileNumber (allows multiple NULLs)';

GO

PRINT '';
PRINT '========================================';
PRINT '✅ UNIQUE Constraints Fix COMPLETE!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '- Removed old UNIQUE constraints (including system-generated)';
PRINT '- Created filtered UNIQUE indexes';
PRINT '- Multiple NULL values now allowed';
PRINT '- Duplicate non-NULL values still prevented';
PRINT '';
GO

-- =============================================
-- Verify the indexes
-- =============================================
PRINT 'Current UNIQUE indexes on Users table:';
PRINT '----------------------------------------';

SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    ISNULL(i.filter_definition, 'No Filter') AS FilterDefinition,
    c.name AS ColumnName
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('Users')
AND i.is_unique = 1
AND i.is_primary_key = 0
ORDER BY i.name;
GO

PRINT '';
PRINT '✅ Script execution completed successfully!';
PRINT 'You can now create multiple users with NULL in EmiratesId, PassportNumber, or Email';
GO
