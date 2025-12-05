-- =============================================
-- Insert Additional Q&A Data for Fertility Assessment Service
-- Based on clinic information images
-- =============================================
USE [CoherentMobApp]
GO

-- =============================================
-- Service 1: Fertility Assessment - Additional Q&A Items
-- =============================================

-- Get the next available SSId (assuming we already have SSId 1-6)
DECLARE @NextSSId INT = 7;

-- Check if these Q&A items already exist
IF NOT EXISTS (SELECT 1 FROM MSubServices WHERE SubServiceTitle = 'Medical and Fertility History Review')
BEGIN
    INSERT INTO MSubServices (
        SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
        DisplayOrder, Active, FId, SId
    )
    VALUES (
        @NextSSId,
        'Medical and Fertility History Review',
        N'مراجعة التاريخ الطبي والخصوبة',
        'A fertility consultation involves a detailed discussion with our specialists to understand your medical background, lifestyle, and any past fertility treatments. We assess menstrual cycles, past pregnancies, contraceptive history, and potential genetic, environmental, or lifestyle factors affecting fertility.',
        N'تتضمن استشارة الخصوبة مناقشة مفصلة مع متخصصينا لفهم خلفيتك الطبية ونمط حياتك وأي علاجات خصوبة سابقة. نقوم بتقييم الدورات الشهرية والحمل السابق وتاريخ وسائل منع الحمل والعوامل الوراثية والبيئية أو نمط الحياة المحتملة التي تؤثر على الخصوبة.',
        1,
        1,
        1,
        1
    );
    PRINT 'Inserted: Medical and Fertility History Review';
END

SET @NextSSId = @NextSSId + 1;

IF NOT EXISTS (SELECT 1 FROM MSubServices WHERE SubServiceTitle = 'Hormonal Testing')
BEGIN
    INSERT INTO MSubServices (
        SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
        DisplayOrder, Active, FId, SId
    )
    VALUES (
        @NextSSId,
        'Hormonal Testing',
        N'فحص الهرمونات',
        'Hormonal testing helps evaluate ovarian function and reproductive health. Blood tests measure hormone levels such as FSH, LH, AMH, prolactin, thyroid hormones, estrogen, progesterone, and testosterone. These results guide personalized fertility treatments and help assess ovarian reserve and ovulation quality.',
        N'يساعد فحص الهرمونات في تقييم وظيفة المبيض والصحة الإنجابية. تقيس اختبارات الدم مستويات الهرمونات مثل FSH و LH و AMH والبرولاكتين وهرمونات الغدة الدرقية والإستروجين والبروجسترون والتستوستيرون. توجه هذه النتائج علاجات الخصوبة الشخصية وتساعد في تقييم احتياطي المبيض وجودة الإباضة.',
        2,
        1,
        1,
        1
    );
    PRINT 'Inserted: Hormonal Testing';
END

SET @NextSSId = @NextSSId + 1;

IF NOT EXISTS (SELECT 1 FROM MSubServices WHERE SubServiceTitle = 'Semen Analysis')
BEGIN
    INSERT INTO MSubServices (
        SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
        DisplayOrder, Active, FId, SId
    )
    VALUES (
        @NextSSId,
        'Semen Analysis',
        N'تحليل السائل المنوي',
        'Male fertility is assessed through a semen analysis, which evaluates sperm count, motility, morphology, and volume. Additional testing, such as sperm DNA fragmentation, can be conducted to determine genetic integrity and overall sperm health.',
        N'يتم تقييم خصوبة الذكور من خلال تحليل السائل المنوي، والذي يقيم عدد الحيوانات المنوية وحركتها وشكلها وحجمها. يمكن إجراء اختبارات إضافية، مثل تجزئة الحمض النووي للحيوانات المنوية، لتحديد السلامة الوراثية وصحة الحيوانات المنوية بشكل عام.',
        3,
        1,
        1,
        1
    );
    PRINT 'Inserted: Semen Analysis';
END

SET @NextSSId = @NextSSId + 1;

IF NOT EXISTS (SELECT 1 FROM MSubServices WHERE SubServiceTitle = 'Imaging and Ultrasound Scans')
BEGIN
    INSERT INTO MSubServices (
        SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
        DisplayOrder, Active, FId, SId
    )
    VALUES (
        @NextSSId,
        'Imaging and Ultrasound Scans',
        N'التصوير والموجات فوق الصوتية',
        'Comprehensive imaging services including:

• Transvaginal Ultrasound: Evaluates the ovaries, uterine structure, and endometrial thickness.

• 3D Transvaginal Ultrasound: Evaluates the uterus and endometrial lining for any abnormalities. It is specifically useful for women with repeated pregnancy loss or implantation failure.

• Hysterosalpingo-Contrast Sonography (HyCoSy): A less invasive alternative to Hystrosalpingography evaluating tubal patency.

• Pelvic MRI (if necessary): Provides a detailed view of reproductive organs in complex cases.',
        N'خدمات التصوير الشاملة بما في ذلك:

• الموجات فوق الصوتية عبر المهبل: تقييم المبيضين وبنية الرحم وسمك بطانة الرحم.

• الموجات فوق الصوتية ثلاثية الأبعاد عبر المهبل: تقييم الرحم وبطانة الرحم لأي تشوهات. مفيد بشكل خاص للنساء اللواتي يعانين من فقدان الحمل المتكرر أو فشل الزرع.

• تصوير الرحم والبوق بالموجات فوق الصوتية: بديل أقل توغلاً لتقييم سالكية قناة فالوب.

• التصوير بالرنين المغناطيسي للحوض: يوفر رؤية مفصلة للأعضاء التناسلية في الحالات المعقدة.',
        4,
        1,
        1,
        1
    );
    PRINT 'Inserted: Imaging and Ultrasound Scans';
END

SET @NextSSId = @NextSSId + 1;

IF NOT EXISTS (SELECT 1 FROM MSubServices WHERE SubServiceTitle = 'Advanced Genetic Screening')
BEGIN
    INSERT INTO MSubServices (
        SSId, SubServiceTitle, ArSubServiceTitle, Details, ArDetails,
        DisplayOrder, Active, FId, SId
    )
    VALUES (
        @NextSSId,
        'Advanced Genetic Screening',
        N'الفحص الجيني المتقدم',
        'Comprehensive genetic screening services including:

• Genetic Carrier Screening: Identifies inherited genetic conditions that could be passed on to offspring.

• Karyotyping: Analyzes chromosomal structure to detect abnormalities.

• Preimplantation Genetic Testing (PGT): Screens embryos for genetic conditions before implantation.

• Advanced genetic testing for male infertility and ovarian insufficiency.',
        N'خدمات الفحص الجيني الشاملة بما في ذلك:

• فحص الناقل الجيني: يحدد الحالات الوراثية الموروثة التي يمكن نقلها إلى النسل.

• التنميط النووي: يحلل البنية الكروموسومية للكشف عن التشوهات.

• الاختبار الجيني قبل الزرع (PGT): فحص الأجنة للحالات الوراثية قبل الزرع.

• الاختبار الجيني المتقدم للعقم عند الذكور وقصور المبيض.',
        5,
        1,
        1,
        1
    );
    PRINT 'Inserted: Advanced Genetic Screening';
END

GO

PRINT '================================';
PRINT 'Additional Q&A data inserted successfully!';
PRINT '5 new sub-services added to Fertility Assessment';
PRINT '================================';
GO
