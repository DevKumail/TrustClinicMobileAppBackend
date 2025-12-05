-- =============================================
-- DEBUG: Check if data exists in tables
-- =============================================
USE [CoherentMobApp]
GO

PRINT '===== CHECKING FACILITY DATA =====';
SELECT FId, FName, City, Country FROM MFacility;
GO

PRINT '';
PRINT '===== CHECKING DOCTORS DATA =====';
SELECT DId, DoctorName, SPId, Active FROM MDoctors;
GO

PRINT '';
PRINT '===== CHECKING DOCTOR-FACILITY MAPPING =====';
SELECT df.Id, df.FId, df.DId, d.DoctorName, f.FName
FROM MDoctorFacilities df
LEFT JOIN MDoctors d ON df.DId = d.DId
LEFT JOIN MFacility f ON df.FId = f.FId;
GO

PRINT '';
PRINT '===== CHECKING SERVICES DATA =====';
SELECT SId, FId, ServiceTitle, Active, DisplayOrder FROM MServices;
GO

PRINT '';
PRINT '===== CHECKING SUB-SERVICES (Q&A) DATA =====';
SELECT SSId, SId, SubServiceTitle, Active, DisplayOrder FROM MSubServices
ORDER BY SId, DisplayOrder;
GO

PRINT '';
PRINT '===== CHECKING WHAT API WILL QUERY =====';
PRINT 'Doctors for Facility 1 (what API queries):';
SELECT d.DId, d.DoctorName, d.Title, d.Active
FROM MDoctors d
INNER JOIN MDoctorFacilities df ON d.DId = df.DId
WHERE df.FId = 1 AND d.Active = 1;
GO

PRINT '';
PRINT 'Services for Facility 1 (what API queries):';
SELECT SId, FId, ServiceTitle, Active, DisplayOrder
FROM MServices
WHERE FId = 1 AND Active = 1
ORDER BY DisplayOrder;
GO

PRINT '';
PRINT '===== DIAGNOSIS =====';
PRINT 'If any of the above queries return 0 rows, that is the problem!';
GO
