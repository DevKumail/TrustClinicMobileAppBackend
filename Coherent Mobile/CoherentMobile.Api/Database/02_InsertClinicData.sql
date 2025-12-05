-- =============================================
-- Insert Sample Clinic Data
-- Trust Fertility Clinic Information
-- =============================================
USE [CoherentMobApp]
GO

-- =============================================
-- 1. Insert Facility (Clinic) Information
-- =============================================
SET IDENTITY_INSERT MFacility ON
GO

INSERT INTO MFacility (
    FId, FName, LicenceNo, AddressLine1, AddressLine2, City, State, Country,
    Phone1, Phone2, EmailAddress, WebsiteUrl, WhatsappNo,
    About, AboutShort, FacilityImages
)
VALUES (
    1,
    'Trust Fertility Clinic',
    'BMC-AUH-2024',
    'Al Bateen Area',
    'Near Corniche',
    'Abu Dhabi',
    'Abu Dhabi',
    'UAE',
    '+971-2-123-4567',
    '+971-2-123-4568',
    'info@trustfertility.ae',
    'https://www.trustfertility.ae',
    '+971501234567',
    'Trust Fertility Clinic is a state-of-the-art facility designed to support married couples facing fertility challenges with personalized, compassionate care. We offer a comprehensive range of fertility treatments tailored to each patient''s needs. Trust Fertility Clinic aims to provide personalized, holistic, evidence-based integrated healthcare services while utilizing the latest advancements in reproductive medicine and AI-driven technologies, giving each patient the best possible chance on their journey toward parenthood.',
    'State-of-the-art fertility clinic in Abu Dhabi',
    'clinic_image_1.jpg,clinic_image_2.jpg,clinic_image_3.jpg'
);

-- Al Ain Branch
INSERT INTO MFacility (
    FId, FName, LicenceNo, AddressLine1, AddressLine2, City, State, Country,
    Phone1, EmailAddress, WebsiteUrl, About, AboutShort
)
VALUES (
    2,
    'Trust Fertility Clinic - Al Ain',
    'BMC-ALN-2024',
    'Central District',
    'Near Al Ain Mall',
    'Al Ain',
    'Abu Dhabi',
    'UAE',
    '+971-3-789-1234',
    'alain@trustfertility.ae',
    'https://www.trustfertility.ae',
    'Trust Fertility Clinic Al Ain branch provides the same excellence in fertility care.',
    'Fertility clinic in Al Ain'
);

SET IDENTITY_INSERT MFacility OFF
GO

-- =============================================
-- 2. Insert Specialties
-- =============================================
SET IDENTITY_INSERT MSpecility ON
GO

INSERT INTO MSpecility (SPId, SpecilityName, ArSpecilityName, Active, FId)
VALUES 
(1, 'In Vitro Fertilization (IVF)', 'التلقيح الصناعي', 1, 1),
(2, 'Anesthesia', 'التخدير', 1, 1),
(3, 'Obstetrics and Gynecology', 'أمراض النساء والتوليد', 1, 1),
(4, 'Reproductive Endocrinology', 'الغدد الصماء الإنجابية', 1, 1);

SET IDENTITY_INSERT MSpecility OFF
GO

-- =============================================
-- 3. Insert Doctors
-- =============================================
SET IDENTITY_INSERT MDoctors ON
GO

-- Dr. Walid Reda Sayed
INSERT INTO MDoctors (
    DId, DoctorName, ArDoctorName, Title, ArTitle, SPId, YearsOfExperience,
    Nationality, ArNationality, Languages, ArLanguages, DoctorPhotoName,
    About, LicenceNo, Active, Gender
)
VALUES (
    1,
    'Dr. Walid Reda Sayed',
    'د. وليد رضا سيد',
    'Consultant Reproductive Endocrinologist & Infertility (IVF)',
    'استشاري الغدد الصماء الإنجابية والعقم',
    1,
    '30+',
    'Germany',
    'ألمانيا',
    'Arabic,German,English',
    'العربية,الألمانية,الإنجليزية',
    'dr_walid.jpg',
    'Dr. Walid Reda Sayed is an accomplished medical professional with over 30 years of experience in gynecology, obstetrics, and reproductive medicine.

He holds a Bachelor of Medicine (M.B.B.S) from Ein Shams University in Egypt and specialized in gynecological endocrinology and reproductive medicine in Germany.

Dr. Sayed further achieved a doctorate and German Board (Facharzt) certification in gynecology and obstetrics from Düsseldorf, Germany, making him a recognized expert in fertility and reproductive health on an international level.',
    'UAE123',
    1,
    'M'
);

-- Dr. Muna Amam
INSERT INTO MDoctors (
    DId, DoctorName, ArDoctorName, Title, ArTitle, SPId, YearsOfExperience,
    Nationality, ArNationality, Languages, ArLanguages, DoctorPhotoName,
    About, LicenceNo, Active, Gender
)
VALUES (
    2,
    'Dr. Muna Amam',
    'د. منى عمام',
    'Specialist Anesthesia',
    'أخصائي تخدير',
    2,
    '17+',
    'Syria',
    'سوريا',
    'Arabic,English',
    'العربية,الإنجليزية',
    'dr_muna.jpg',
    'Dr. Muna Amam a highly experienced Specialist in Anesthesia with over 17 years of clinical experience across leading healthcare institutions in the Middle East. Currently practicing at BMC Trust Fertility Clinic, she is recognized for her expertise in managing complex and high-risk cases, particularly in obstetrics and gynecology.

Dr. Muna holds a Master''s degree in Anesthesia and Resuscitation from Damascus University and is licensed to practice in Syria, the UAE, and under HAAD (Abu Dhabi).

Dr. Muna has served in prominent hospitals including Al-Mowasat Hospital in Syria and Royal Hospital in Oman, where she provided critical care and performed a wide range of anesthetic procedures, including general, epidural, and spinal anesthesia.

Her proficiency extends beyond the operating room, with advanced skills in resuscitation and invasive procedures such as central venous and arterial cannulation. she has contributed to surgical teams across multiple specialties, including general surgery, orthopedics, pediatrics, neurosurgery, and cardiothoracic surgery.

With a strong commitment to patient safety and clinical excellence, Dr. Muna continues to be a trusted and valued member of the medical community.',
    'UAE456',
    1,
    'F'
);

-- Dr. Nayrouz Gezaf
INSERT INTO MDoctors (
    DId, DoctorName, ArDoctorName, Title, ArTitle, SPId, YearsOfExperience,
    Nationality, ArNationality, Languages, ArLanguages, DoctorPhotoName,
    About, LicenceNo, Active, Gender
)
VALUES (
    3,
    'Dr. Nayrouz Gezaf',
    'د. نيروز جزاف',
    'Specialist Reproductive Medicine and Infertility, Obstetrics & Gynecology',
    'أخصائي الطب الإنجابي والعقم وأمراض النساء والتوليد',
    3,
    '17+',
    'Egypt',
    'مصر',
    'Arabic,English',
    'العربية,الإنجليزية',
    'dr_nayrouz.jpg',
    'Dr. Nayrouz Gezaf is a highly skilled and experienced Specialist in Obstetrics, Gynecology, and Infertility with over 17 years of hands-on experience.

She has demonstrated her expertise through successfully managing and handling a wide range of complex cases, including performing over 750 operations, 2,400 ovum pickups, 800 embryo transfers, and supervising 900 deliveries.

Dr. Gezaf holds a Master''s degree in Obstetrics and Gynecology from Ain Shams University, Cairo, and a Professional Diploma in Reproductive Medicine and Surgery from Birmingham Women''s and Children''s Hospital in the UK.

She is also certified in Aesthetic Gynecology, reflecting her diverse skill set in the field.

With international experience across varied medical settings, Dr. Gezaf is recognized for her excellence in managing critical cases, delivering compassionate patient care, and her commitment to women''s health.',
    'UAE789',
    1,
    'F'
);

SET IDENTITY_INSERT MDoctors OFF
GO

-- =============================================
-- 4. Map Doctors to Facilities
-- =============================================
SET IDENTITY_INSERT MDoctorFacilities ON
GO

INSERT INTO MDoctorFacilities (Id, FId, DId)
VALUES 
(1, 1, 1), -- Dr. Walid at Abu Dhabi
(2, 1, 2), -- Dr. Muna at Abu Dhabi
(3, 1, 3), -- Dr. Nayrouz at Abu Dhabi
(4, 2, 1); -- Dr. Walid also at Al Ain

SET IDENTITY_INSERT MDoctorFacilities OFF
GO

-- =============================================
-- 5. Insert Services
-- =============================================
SET IDENTITY_INSERT MServices ON
GO

INSERT INTO MServices (
    SId, FId, ServiceTitle, ArServiceTitle, ServiceIntro, ArServiceIntro,
    Active, DisplayOrder, DisplayImageName, IconImageName
)
VALUES 
(
    1,
    1,
    'Fertility Assessment',
    'تقييم الخصوبة',
    'Comprehensive fertility evaluation and testing',
    'تقييم واختبار شامل للخصوبة',
    1,
    1,
    'service1.jpg',
    'fertility_assessment_icon.png'
),
(
    2,
    1,
    'Assisted Reproductive',
    'الإنجاب المساعد',
    'Advanced fertility treatments including IVF and ICSI',
    'علاجات الخصوبة المتقدمة بما في ذلك التلقيح الصناعي والحقن المجهري',
    1,
    2,
    'service2.jpg',
    'assisted_reproductive_icon.png'
);

SET IDENTITY_INSERT MServices OFF
GO

-- =============================================
-- 6. Insert Sub-Services (Q&A)
-- =============================================
SET IDENTITY_INSERT MSubServices ON
GO

-- Fertility Assessment Sub-Services
INSERT INTO MSubServices (
    SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
    DisplayOrder, Active, FId, SId
)
VALUES 
(
    1,
    'Medical and Fertility History Review',
    'مراجعة التاريخ الطبي والخصوبة',
    'A fertility consultation involves a detailed discussion with our specialists to understand your medical background, lifestyle, and any past fertility treatments. We assess menstrual cycles, past pregnancies, contraceptive history, and potential genetic, environmental, or lifestyle factors affecting fertility.',
    'تتضمن استشارة الخصوبة مناقشة مفصلة مع متخصصينا لفهم خلفيتك الطبية ونمط حياتك وأي علاجات خصوبة سابقة.',
    1,
    1,
    1,
    1
),
(
    2,
    'Hormonal Testing',
    'فحص الهرمونات',
    'Hormonal testing helps evaluate ovarian function and reproductive health. Blood tests measure hormone levels such as FSH, LH, AMH, prolactin, thyroid hormones, estrogen, progesterone, and testosterone. These results guide personalized fertility treatments and help assess ovarian reserve and ovulation quality.',
    'يساعد فحص الهرمونات في تقييم وظيفة المبيض والصحة الإنجابية.',
    2,
    1,
    1,
    1
),
(
    3,
    'Semen Analysis',
    'تحليل السائل المنوي',
    'Male fertility is assessed through a semen analysis, which evaluates sperm count, motility, morphology, and volume. Additional testing, such as sperm DNA fragmentation, can be conducted to determine genetic integrity and overall sperm health.',
    'يتم تقييم خصوبة الذكور من خلال تحليل السائل المنوي.',
    3,
    1,
    1,
    1
);

-- Assisted Reproductive Sub-Services
INSERT INTO MSubServices (
    SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
    DisplayOrder, Active, FId, SId
)
VALUES 
(
    4,
    'In Vitro Fertilization (IVF)',
    'التلقيح الصناعي',
    'Explanation: IVF is a highly effective fertility treatment where eggs are retrieved, fertilized in the lab, and the best embryos are transferred into the uterus.


Step-by-Step: 


1. Ovarian Stimulation: Hormonal injections are administered to stimulate egg production.

2. Monitoring: Ultrasounds and blood tests track follicle development.

3. Trigger Injection: A final hormone injection triggers ovulation.

4. Egg Retrieval: A minor, ultrasound-guided procedure under sedation to collect eggs.',
    'التلقيح الصناعي هو علاج فعال للخصوبة.',
    1,
    1,
    1,
    2
),
(
    5,
    'Intracytoplasmic Sperm Injection (ICSI)',
    'الحقن المجهري',
    'Explanation: ICSI is an advanced fertilization technique used in cases of severe male infertility. Instead of allowing sperm to fertilize an egg naturally, a single sperm is injected directly into the egg.


Step-by-Step: 


Follows the same steps as IVF, with the additional step of direct sperm injection into the egg to improve fertilization success.

1. Ovulation Tracking: The woman''s ovulation cycle is monitored through ultrasound and hormonal tests.

2. Sperm Collection & Preparation: A semen sample is processed to isolate the healthiest sperm.

3. Insemination: A thin catheter is used to insert sperm directly into the uterus.

4. Pregnancy Test: A blood test confirms pregnancy after two weeks.',
    'الحقن المجهري هو تقنية متقدمة للتخصيب.',
    2,
    1,
    1,
    2
),
(
    6,
    'Embryo Transfer',
    'نقل الأجنة',
    'The final step of IVF where selected embryos are carefully placed into the uterus to achieve pregnancy.',
    'الخطوة الأخيرة من التلقيح الصناعي حيث يتم وضع الأجنة المختارة بعناية في الرحم.',
    3,
    1,
    1,
    2
);

SET IDENTITY_INSERT MSubServices OFF
GO

-- =============================================
-- 7. Insert Facility Timings
-- Note: FK constraint requires FTId to match existing FId from MFacility
-- Since we have FId = 1 and 2, we can only use FTId = 1 or 2
-- =============================================

-- Timings for Facility 1 (Abu Dhabi)
IF NOT EXISTS (SELECT 1 FROM MFacilityTimings WHERE FTId = 1)
BEGIN
    INSERT INTO MFacilityTimings (FTId, Day, ArDay, TimeFrom, TimeTo, FId, Active)
    VALUES (1, 'Sunday-Thursday', 'الأحد-الخميس', '08:00', '20:00', 1, 1);
END

-- Timings for Facility 2 (Al Ain)
IF NOT EXISTS (SELECT 1 FROM MFacilityTimings WHERE FTId = 2)
BEGIN
    INSERT INTO MFacilityTimings (FTId, Day, ArDay, TimeFrom, TimeTo, FId, Active)
    VALUES (2, 'Sunday-Thursday', 'الأحد-الخميس', '09:00', '18:00', 2, 1);
END

GO

PRINT 'Sample clinic data inserted successfully!'
PRINT 'Facility: Trust Fertility Clinic'
PRINT 'Doctors: 3 doctors added'
PRINT 'Services: 2 services with sub-services added'
GO
