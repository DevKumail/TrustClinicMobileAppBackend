-- =============================================
-- VERIFY DATA EXISTS, IF NOT, RE-INSERT
-- =============================================
USE [CoherentMobApp]
GO

-- Check if data exists
DECLARE @DoctorCount INT = (SELECT COUNT(*) FROM MDoctors WHERE DId IN (1,2,3));
DECLARE @ServiceCount INT = (SELECT COUNT(*) FROM MServices WHERE SId IN (1,2));

PRINT 'Doctor Count: ' + CAST(@DoctorCount AS VARCHAR);
PRINT 'Service Count: ' + CAST(@ServiceCount AS VARCHAR);

-- If no data, insert it
IF @DoctorCount = 0 OR @ServiceCount = 0
BEGIN
    PRINT '';
    PRINT '===== DATA MISSING - RE-INSERTING =====';
    
    -- Re-run the insert script
    -- Make sure you have FacilityId = 1 in MFacility first
    IF NOT EXISTS (SELECT 1 FROM MFacility WHERE FId = 1)
    BEGIN
        PRINT 'ERROR: Facility with FId = 1 does not exist!';
        PRINT 'Please run 02_InsertClinicData.sql first!';
    END
    ELSE
    BEGIN
        PRINT 'Facility exists. Proceeding with data insert...';
        
        -- Insert Specialities if not exist
        IF NOT EXISTS (SELECT 1 FROM MSpecility WHERE SPId IN (1,2,3,4))
        BEGIN
            SET IDENTITY_INSERT MSpecility ON;
            
            INSERT INTO MSpecility (SPId, SpecilityName, ArSpecilityName, Active, FId)
            VALUES 
            (1, 'In Vitro Fertilization (IVF)', N'التلقيح الصناعي', 1, 1),
            (2, 'Anesthesia', N'التخدير', 1, 1),
            (3, 'Obstetrics and Gynecology', N'أمراض النساء والتوليد', 1, 1),
            (4, 'Reproductive Endocrinology', N'الغدد الصماء الإنجابية', 1, 1);
            
            SET IDENTITY_INSERT MSpecility OFF;
            PRINT 'Specialities inserted!';
        END
        
        -- Insert Doctors if not exist
        IF NOT EXISTS (SELECT 1 FROM MDoctors WHERE DId IN (1,2,3))
        BEGIN
            SET IDENTITY_INSERT MDoctors ON;
            
            INSERT INTO MDoctors (DId, DoctorName, Title, SPId, YearsOfExperience, Nationality, Languages, DoctorPhotoName, About, LicenceNo, Active, Gender)
            VALUES 
            (1, 'Dr. Walid Reda Sayed', 'Consultant Reproductive Endocrinologist & Infertility (IVF)', 1, '30+', 'Germany', 'Arabic,German,English', 'dr_walid.jpg', 
             'Dr. Walid Reda Sayed is an accomplished medical professional with over 30 years of experience in gynecology, obstetrics, and reproductive medicine.', 
             'UAE123', 1, 'M'),
            (2, 'Dr. Muna Amam', 'Specialist Anesthesia', 2, '17+', 'Syria', 'Arabic,English', 'dr_muna.jpg',
             'Dr. Muna Amam a highly experienced Specialist in Anesthesia with over 17 years of clinical experience.',
             'UAE456', 1, 'F'),
            (3, 'Dr. Nayrouz Gezaf', 'Specialist Reproductive Medicine and Infertility, Obstetrics & Gynecology', 3, '17+', 'Egypt', 'Arabic,English', 'dr_nayrouz.jpg',
             'Dr. Nayrouz Gezaf is a highly skilled and experienced Specialist in Obstetrics, Gynecology, and Infertility with over 17 years of hands-on experience.',
             'UAE789', 1, 'F');
            
            SET IDENTITY_INSERT MDoctors OFF;
            PRINT 'Doctors inserted!';
        END
        
        -- Insert Doctor-Facility mappings
        DELETE FROM MDoctorFacilities WHERE FId = 1;
        
        SET IDENTITY_INSERT MDoctorFacilities ON;
        INSERT INTO MDoctorFacilities (Id, FId, DId)
        VALUES (1, 1, 1), (2, 1, 2), (3, 1, 3);
        SET IDENTITY_INSERT MDoctorFacilities OFF;
        PRINT 'Doctor-Facility mappings inserted!';
        
        -- Insert Services if not exist
        IF NOT EXISTS (SELECT 1 FROM MServices WHERE SId IN (1,2))
        BEGIN
            SET IDENTITY_INSERT MServices ON;
            
            INSERT INTO MServices (SId, FId, ServiceTitle, ArServiceTitle, ServiceIntro, ArServiceIntro, Active, DisplayOrder, DisplayImageName, IconImageName)
            VALUES 
            (1, 1, 'Fertility Assessment', N'تقييم الخصوبة', 'Comprehensive fertility evaluation and testing', N'تقييم واختبار شامل للخصوبة', 1, 1, 'service1.jpg', 'fertility_assessment_icon.png'),
            (2, 1, 'Assisted Reproductive', N'الإنجاب المساعد', 'Advanced fertility treatments including IVF and ICSI', N'علاجات الخصوبة المتقدمة', 1, 2, 'service2.jpg', 'assisted_reproductive_icon.png');
            
            SET IDENTITY_INSERT MServices OFF;
            PRINT 'Services inserted!';
        END
        
        -- Insert Sub-Services (Q&A)
        DELETE FROM MSubServices WHERE SId IN (1, 2);
        
        SET IDENTITY_INSERT MSubServices ON;
        
        -- Fertility Assessment Q&A
        INSERT INTO MSubServices (SSId, SubServiceTitle, Details, DisplayOrder, Active, FId, SId)
        VALUES 
        (1, 'Medical and Fertility History Review', 
         'A fertility consultation involves a detailed discussion with our specialists to understand your medical background, lifestyle, and any past fertility treatments.', 
         1, 1, 1, 1),
        (2, 'Hormonal Testing', 
         'Hormonal testing helps evaluate ovarian function and reproductive health. Blood tests measure hormone levels such as FSH, LH, AMH, prolactin, thyroid hormones, estrogen, progesterone, and testosterone.', 
         2, 1, 1, 1),
        (3, 'Semen Analysis', 
         'Male fertility is assessed through a semen analysis, which evaluates sperm count, motility, morphology, and volume.', 
         3, 1, 1, 1),
        
        -- Assisted Reproductive Q&A
        (4, 'In Vitro Fertilization (IVF)', 
         'IVF is a highly effective fertility treatment where eggs are retrieved, fertilized in the lab, and the best embryos are transferred into the uterus.', 
         1, 1, 1, 2),
        (5, 'Intracytoplasmic Sperm Injection (ICSI)', 
         'ICSI is an advanced fertilization technique used in cases of severe male infertility. A single sperm is injected directly into the egg.', 
         2, 1, 1, 2),
        (6, 'Embryo Transfer', 
         'The final step of IVF where selected embryos are carefully placed into the uterus to achieve pregnancy.', 
         3, 1, 1, 2);
        
        SET IDENTITY_INSERT MSubServices OFF;
        PRINT 'Sub-Services (Q&A) inserted!';
    END
END
ELSE
BEGIN
    PRINT '';
    PRINT '===== DATA ALREADY EXISTS =====';
    PRINT 'Running fixes to ensure data is accessible...';
    
    -- Ensure Active flags
    UPDATE MDoctors SET Active = 1 WHERE DId IN (1,2,3);
    UPDATE MServices SET Active = 1 WHERE SId IN (1,2);
    UPDATE MSubServices SET Active = 1 WHERE SId IN (1,2);
    
    -- Ensure FId is correct
    UPDATE MServices SET FId = 1 WHERE SId IN (1,2);
    UPDATE MSubServices SET FId = 1 WHERE SId IN (1,2);
    
    PRINT 'Data fixed!';
END
GO

-- Final verification
PRINT '';
PRINT '===== FINAL VERIFICATION =====';

SELECT 'Doctors for API' AS QueryType, COUNT(*) AS Count
FROM MDoctors d
INNER JOIN MDoctorFacilities df ON d.DId = df.DId
WHERE df.FId = 1 AND d.Active = 1

UNION ALL

SELECT 'Services for API', COUNT(*)
FROM MServices
WHERE FId = 1 AND Active = 1

UNION ALL

SELECT 'Q&A for API', COUNT(*)
FROM MSubServices
WHERE FId = 1 AND Active = 1;

PRINT '';
PRINT 'If counts are > 0, API should work!';
PRINT 'Now restart your API and test: GET /api/guest/clinic-info';
GO
