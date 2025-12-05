-- =============================================
-- Insert Facility Timings - WORKAROUND for broken FK constraint
-- Works with the current (incorrect) constraint
-- =============================================
USE [CoherentMobApp]
GO

-- Clear existing timings
DELETE FROM MFacilityTimings WHERE FId = 1;
GO

-- Since FK constraint requires FTId to match FId in MFacility,
-- we can only insert ONE record per facility (FTId must match FId)
-- This is a limitation of the current schema design

-- Insert consolidated timing for Facility 1
IF NOT EXISTS (SELECT 1 FROM MFacilityTimings WHERE FTId = 1)
BEGIN
    INSERT INTO MFacilityTimings (FTId, Day, ArDay, TimeFrom, TimeTo, FId, Active)
    VALUES (1, 'Sun-Thu: 8AM-8PM, Fri: Closed, Sat: 10AM-6PM', 
            'الأحد-الخميس: 8ص-8م، الجمعة: مغلق، السبت: 10ص-6م', 
            '08:00', '20:00', 1, 1);
    PRINT 'Timing inserted for Facility 1';
END
ELSE
BEGIN
    PRINT 'Timing already exists for Facility 1';
END
GO

-- Insert consolidated timing for Facility 2 (if needed)
IF NOT EXISTS (SELECT 1 FROM MFacilityTimings WHERE FTId = 2)
BEGIN
    INSERT INTO MFacilityTimings (FTId, Day, ArDay, TimeFrom, TimeTo, FId, Active)
    VALUES (2, 'Sun-Thu: 9AM-6PM, Fri: Closed, Sat: 10AM-4PM', 
            'الأحد-الخميس: 9ص-6م، الجمعة: مغلق، السبت: 10ص-4م', 
            '09:00', '18:00', 2, 1);
    PRINT 'Timing inserted for Facility 2';
END
ELSE
BEGIN
    PRINT 'Timing already exists for Facility 2';
END
GO

PRINT 'Facility timings inserted (workaround applied)';
GO
