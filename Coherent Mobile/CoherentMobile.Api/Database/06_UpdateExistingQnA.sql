-- =============================================
-- UPDATE Existing Q&A Data (Alternative approach)
-- Use this if you want to replace the existing 3 Q&A items
-- =============================================
USE [CoherentMobApp]
GO

-- =============================================
-- Update Service 1 (Fertility Assessment) Q&A Items
-- =============================================

-- Clear existing sub-services for Fertility Assessment
DELETE FROM MSubServices WHERE SId = 1;
GO

-- Insert all Q&A items in correct order
SET IDENTITY_INSERT MSubServices ON
GO

INSERT INTO MSubServices (
    SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
    DisplayOrder, Active, FId, SId
)
VALUES 
-- Q&A 1: Medical and Fertility History Review
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
-- Q&A 2: Hormonal Testing
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
-- Q&A 3: Semen Analysis
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
),
-- Q&A 4: Imaging and Ultrasound Scans
(
    7,
    'Imaging and Ultrasound Scans',
    'التصوير والموجات فوق الصوتية',
    'Comprehensive imaging services including:

• Transvaginal Ultrasound: Evaluates the ovaries, uterine structure, and endometrial thickness.

• 3D Transvaginal Ultrasound: Evaluates the uterus and endometrial lining for any abnormalities. It is specifically useful for women with repeated pregnancy loss or implantation failure.

• Hysterosalpingo-Contrast Sonography (HyCoSy): A less invasive alternative to Hystrosalpingography evaluating tubal patency.

• Pelvic MRI (if necessary): Provides a detailed view of reproductive organs in complex cases.',
    'خدمات التصوير الشاملة.',
    4,
    1,
    1,
    1
),
-- Q&A 5: Advanced Genetic Screening
(
    8,
    'Advanced Genetic Screening',
    'الفحص الجيني المتقدم',
    'Comprehensive genetic screening services including:

• Genetic Carrier Screening: Identifies inherited genetic conditions that could be passed on to offspring.

• Karyotyping: Analyzes chromosomal structure to detect abnormalities.

• Preimplantation Genetic Testing (PGT): Screens embryos for genetic conditions before implantation.

• Advanced genetic testing for male infertility and ovarian insufficiency.',
    'خدمات الفحص الجيني الشاملة.',
    5,
    1,
    1,
    1
);

SET IDENTITY_INSERT MSubServices OFF
GO

PRINT '================================';
PRINT 'Fertility Assessment Q&A updated!';
PRINT 'Total Q&A items: 5';
PRINT '================================';
GO
