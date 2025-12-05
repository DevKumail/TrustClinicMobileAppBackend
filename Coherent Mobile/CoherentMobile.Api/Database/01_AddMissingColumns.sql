-- =============================================
-- Add Missing Columns for Image Support
-- =============================================
USE [CoherentMobApp]
GO

-- Add image columns to MFacility table for facility images
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MFacility') AND name = 'FacilityImages')
BEGIN
    ALTER TABLE MFacility
    ADD FacilityImages NVARCHAR(MAX) NULL; -- Comma-separated image file names
END
GO

-- Add IconImageName to MServices for service icons
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MServices') AND name = 'IconImageName')
BEGIN
    ALTER TABLE MServices
    ADD IconImageName NVARCHAR(250) NULL;
END
GO

-- Add ArAbout (Arabic) to MFacility if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MFacility') AND name = 'ArAbout')
BEGIN
    ALTER TABLE MFacility
    ADD ArAbout NVARCHAR(MAX) NULL;
END
GO

-- Add ArAboutShort (Arabic) to MFacility if not exists
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MFacility') AND name = 'ArAboutShort')
BEGIN
    ALTER TABLE MFacility
    ADD ArAboutShort NVARCHAR(500) NULL;
END
GO

PRINT 'Missing columns added successfully!'
