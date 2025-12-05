-- =============================================
-- Fix MFacilityTimings Foreign Key Constraint
-- The current constraint is backwards - it should use FId, not FTId
-- =============================================
USE [CoherentMobApp]
GO

-- Step 1: Drop the incorrect constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MFacilityTimings_MFacility')
BEGIN
    ALTER TABLE MFacilityTimings 
    DROP CONSTRAINT FK_MFacilityTimings_MFacility;
    PRINT 'Dropped incorrect FK constraint';
END
GO

-- Step 2: Add the correct constraint (FId should reference MFacility.FId, not FTId)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MFacilityTimings_MFacility_Corrected')
BEGIN
    ALTER TABLE MFacilityTimings
    ADD CONSTRAINT FK_MFacilityTimings_MFacility_Corrected
    FOREIGN KEY (FId) REFERENCES MFacility(FId);
    PRINT 'Added correct FK constraint on FId column';
END
GO

-- Step 3: Make FTId auto-increment if not already
-- (This requires recreating the table or using IDENTITY column)
-- For now, we'll just document this

PRINT 'Foreign key constraint fixed!';
PRINT 'FTId is now independent (primary key only)';
PRINT 'FId now correctly references MFacility.FId';
GO

-- Step 4: Now you can insert timings normally
-- Example:
/*
INSERT INTO MFacilityTimings (FTId, Day, ArDay, TimeFrom, TimeTo, FId, Active)
VALUES 
(1, 'Sunday', 'الأحد', '08:00', '20:00', 1, 1),
(2, 'Monday', 'الإثنين', '08:00', '20:00', 1, 1),
(3, 'Tuesday', 'الثلاثاء', '08:00', '20:00', 1, 1),
(4, 'Wednesday', 'الأربعاء', '08:00', '20:00', 1, 1),
(5, 'Thursday', 'الخميس', '08:00', '20:00', 1, 1),
(6, 'Friday', 'الجمعة', 'Closed', 'Closed', 1, 1),
(7, 'Saturday', 'السبت', '10:00', '18:00', 1, 1);
*/
