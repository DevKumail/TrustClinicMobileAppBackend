-- =============================================
-- FIX: Ensure all data is properly configured
-- =============================================
USE [CoherentMobApp]
GO

PRINT '===== FIXING DATA ISSUES =====';

-- Step 1: Ensure Active flags are set to 1
PRINT 'Step 1: Setting Active flags...';
UPDATE MDoctors SET Active = 1 WHERE Active IS NULL OR Active = 0;
UPDATE MServices SET Active = 1 WHERE Active IS NULL OR Active = 0;
UPDATE MSubServices SET Active = 1 WHERE Active IS NULL OR Active = 0;
PRINT 'Active flags updated!';
GO

-- Step 2: Ensure all services belong to Facility 1
PRINT '';
PRINT 'Step 2: Updating service FacilityId...';
UPDATE MServices SET FId = 1 WHERE FId IS NULL OR FId = 0;
UPDATE MSubServices SET FId = 1 WHERE FId IS NULL OR FId = 0;
PRINT 'FacilityId updated for services!';
GO

-- Step 3: Verify Doctor-Facility mappings exist
PRINT '';
PRINT 'Step 3: Checking Doctor-Facility mappings...';

-- Check if mappings exist
IF NOT EXISTS (SELECT 1 FROM MDoctorFacilities WHERE FId = 1 AND DId = 1)
BEGIN
    -- Get the next ID
    DECLARE @NextId INT = ISNULL((SELECT MAX(Id) FROM MDoctorFacilities), 0) + 1;
    
    -- Delete any incorrect mappings
    DELETE FROM MDoctorFacilities WHERE FId = 1;
    
    -- Insert correct mappings
    SET IDENTITY_INSERT MDoctorFacilities ON;
    
    INSERT INTO MDoctorFacilities (Id, FId, DId)
    SELECT @NextId + 0, 1, DId FROM MDoctors WHERE DId = 1 AND NOT EXISTS (SELECT 1 FROM MDoctorFacilities WHERE FId = 1 AND DId = 1);
    
    INSERT INTO MDoctorFacilities (Id, FId, DId)
    SELECT @NextId + 1, 1, DId FROM MDoctors WHERE DId = 2 AND NOT EXISTS (SELECT 1 FROM MDoctorFacilities WHERE FId = 1 AND DId = 2);
    
    INSERT INTO MDoctorFacilities (Id, FId, DId)
    SELECT @NextId + 2, 1, DId FROM MDoctors WHERE DId = 3 AND NOT EXISTS (SELECT 1 FROM MDoctorFacilities WHERE FId = 1 AND DId = 3);
    
    SET IDENTITY_INSERT MDoctorFacilities OFF;
    
    PRINT 'Doctor-Facility mappings created!';
END
ELSE
BEGIN
    PRINT 'Doctor-Facility mappings already exist!';
END
GO

-- Step 4: Verify data counts
PRINT '';
PRINT '===== VERIFICATION =====';
PRINT 'Facility Count:';
SELECT COUNT(*) AS FacilityCount FROM MFacility WHERE FId = 1;

PRINT '';
PRINT 'Active Doctors for Facility 1:';
SELECT COUNT(*) AS DoctorCount 
FROM MDoctors d
INNER JOIN MDoctorFacilities df ON d.DId = df.DId
WHERE df.FId = 1 AND d.Active = 1;

PRINT '';
PRINT 'Active Services for Facility 1:';
SELECT COUNT(*) AS ServiceCount 
FROM MServices 
WHERE FId = 1 AND Active = 1;

PRINT '';
PRINT 'Active Sub-Services (Q&A) for Facility 1:';
SELECT COUNT(*) AS QnaCount 
FROM MSubServices 
WHERE FId = 1 AND Active = 1;

PRINT '';
PRINT '===== FIX COMPLETED =====';
PRINT 'Now test the API again: GET /api/guest/clinic-info';
GO
