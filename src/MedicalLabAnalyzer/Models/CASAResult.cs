using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    public class CASAResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Basic Parameters
        [Range(0.1, 10.0, ErrorMessage = "حجم العينة يجب أن يكون بين 0.1 و 10.0 مل")]
        [Column(TypeName = "decimal(8,2)")]
        public double? Volume { get; set; } // ml
        
        [Range(0, 300, ErrorMessage = "التركيز يجب أن يكون بين 0 و 300 مليون/مل")]
        [Column(TypeName = "decimal(10,2)")]
        public double? Concentration { get; set; } // million/ml
        
        [Range(0, 1000, ErrorMessage = "عدد الحيوانات المنوية الكلي يجب أن يكون بين 0 و 1000 مليون")]
        [Column(TypeName = "decimal(12,2)")]
        public double? TotalSpermCount { get; set; } // million
        
        [Range(0, 100, ErrorMessage = "نسبة الحركة يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? Motility { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة الحركة التقدمية يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? ProgressiveMotility { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة الحركة غير التقدمية يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? NonProgressiveMotility { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة الحيوانات المنوية غير المتحركة يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? Immotile { get; set; } // %
        
        // Morphology
        [Range(0, 100, ErrorMessage = "نسبة الأشكال الطبيعية يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? NormalForms { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة الأشكال غير الطبيعية يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? AbnormalForms { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة تشوهات الرأس يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? HeadAbnormalities { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة تشوهات الجزء الأوسط يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? MidpieceAbnormalities { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة تشوهات الذيل يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? TailAbnormalities { get; set; } // %
        
        // Vitality
        [Range(0, 100, ErrorMessage = "نسبة الحيوية يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? Vitality { get; set; } // %
        
        [Range(0, 100, ErrorMessage = "نسبة الحيوانات المنوية الميتة يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? DeadSperm { get; set; } // %
        
        // Agglutination
        [StringLength(20, ErrorMessage = "وصف التلاصق يجب ألا يتجاوز 20 حرف")]
        [RegularExpression("^(None|Mild|Moderate|Severe|لا يوجد|خفيف|متوسط|شديد)$", ErrorMessage = "قيمة التلاصق غير صحيحة")]
        public string Agglutination { get; set; } // None, Mild, Moderate, Severe
        
        // Viscosity
        [StringLength(20, ErrorMessage = "وصف اللزوجة يجب ألا يتجاوز 20 حرف")]
        [RegularExpression("^(Normal|Increased|Decreased|طبيعي|مرتفع|منخفض)$", ErrorMessage = "قيمة اللزوجة غير صحيحة")]
        public string Viscosity { get; set; } // Normal, Increased, Decreased
        
        // pH
        [Range(6.0, 8.5, ErrorMessage = "قيمة الأس الهيدروجيني يجب أن تكون بين 6.0 و 8.5")]
        [Column(TypeName = "decimal(3,1)")]
        public double? pH { get; set; }
        
        // Liquefaction Time
        [Range(5, 120, ErrorMessage = "وقت السيولة يجب أن يكون بين 5 و 120 دقيقة")]
        public int? LiquefactionTime { get; set; } // minutes
        
        // Color
        [StringLength(20, ErrorMessage = "وصف اللون يجب ألا يتجاوز 20 حرف")]
        [RegularExpression("^(Normal|Yellow|Brown|Red|Clear|طبيعي|أصفر|بني|أحمر|شفاف)$", ErrorMessage = "لون العينة غير صحيح")]
        public string Color { get; set; } // Normal, Yellow, Brown, Red
        
        // Appearance
        [StringLength(20, ErrorMessage = "وصف المظهر يجب ألا يتجاوز 20 حرف")]
        [RegularExpression("^(Normal|Turbid|Clear|طبيعي|عكر|شفاف)$", ErrorMessage = "مظهر العينة غير صحيح")]
        public string Appearance { get; set; } // Normal, Turbid, Clear
        
        // Round Cells
        [Range(0, 50, ErrorMessage = "عدد الخلايا المستديرة يجب أن يكون بين 0 و 50 مليون/مل")]
        [Column(TypeName = "decimal(8,2)")]
        public double? RoundCells { get; set; } // million/ml
        
        // Leukocytes
        [Range(0, 10, ErrorMessage = "عدد كريات الدم البيضاء يجب أن يكون بين 0 و 10 مليون/مل")]
        [Column(TypeName = "decimal(8,2)")]
        public double? Leukocytes { get; set; } // million/ml
        
        // MAR Test
        [Range(0, 100, ErrorMessage = "نتيجة فحص MAR يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? MARTest { get; set; } // %
        
        // Immunobead Test
        [Range(0, 100, ErrorMessage = "نتيجة فحص Immunobead يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? ImmunobeadTest { get; set; } // %
        
        // DNA Fragmentation
        [Range(0, 100, ErrorMessage = "نسبة تفتت الحمض النووي يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? DNAFragmentation { get; set; } // %
        
        // Acrosome Reaction
        [Range(0, 100, ErrorMessage = "نسبة تفاعل الأكروزوم يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? AcrosomeReaction { get; set; } // %
        
        // Hyperactivation
        [Range(0, 100, ErrorMessage = "نسبة فرط النشاط يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? Hyperactivation { get; set; } // %
        
        // Computer Analysis Results
        [Range(0, 200, ErrorMessage = "متوسط سرعة المسار يجب أن يكون بين 0 و 200 ميكرون/ثانية")]
        [Column(TypeName = "decimal(8,2)")]
        public double? VAP { get; set; } // Average Path Velocity
        
        [Range(0, 200, ErrorMessage = "سرعة الخط المستقيم يجب أن تكون بين 0 و 200 ميكرون/ثانية")]
        [Column(TypeName = "decimal(8,2)")]
        public double? VSL { get; set; } // Straight Line Velocity
        
        [Range(0, 300, ErrorMessage = "السرعة المنحنية يجب أن تكون بين 0 و 300 ميكرون/ثانية")]
        [Column(TypeName = "decimal(8,2)")]
        public double? VCL { get; set; } // Curvilinear Velocity
        
        [Range(0, 20, ErrorMessage = "سعة الإزاحة الجانبية للرأس يجب أن تكون بين 0 و 20 ميكرون")]
        [Column(TypeName = "decimal(6,2)")]
        public double? ALH { get; set; } // Amplitude of Lateral Head Displacement
        
        [Range(0, 50, ErrorMessage = "تكرار ضربات العبور يجب أن يكون بين 0 و 50 هرتز")]
        [Column(TypeName = "decimal(6,2)")]
        public double? BCF { get; set; } // Beat Cross Frequency
        
        [Range(0, 100, ErrorMessage = "الاستقامة يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? STR { get; set; } // Straightness
        
        [Range(0, 100, ErrorMessage = "الخطية يجب أن تكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? LIN { get; set; } // Linearity
        
        [Range(0, 100, ErrorMessage = "التذبذب يجب أن يكون بين 0 و 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public double? WOB { get; set; } // Wobble
        
        // Quality Control
        [Required(ErrorMessage = "حالة مراقبة الجودة مطلوبة")]
        public bool IsQualityControlPassed { get; set; } = false;
        
        [StringLength(500, ErrorMessage = "ملاحظات مراقبة الجودة يجب ألا تتجاوز 500 حرف")]
        public string QualityControlNotes { get; set; }
        
        // Results Interpretation
        [StringLength(50, ErrorMessage = "تفسير النتائج يجب ألا يتجاوز 50 حرف")]
        [RegularExpression("^(Normal|Abnormal|Borderline|طبيعي|غير طبيعي|حدي)$", ErrorMessage = "تفسير النتائج غير صحيح")]
        public string Interpretation { get; set; } // Normal, Abnormal, Borderline
        
        [StringLength(1000, ErrorMessage = "الأهمية السريرية يجب ألا تتجاوز 1000 حرف")]
        public string ClinicalSignificance { get; set; }
        
        [StringLength(1000, ErrorMessage = "التوصيات يجب ألا تتجاوز 1000 حرف")]
        public string Recommendations { get; set; }
        
        // Technical Details
        [StringLength(500, ErrorMessage = "مسار ملف الفيديو يجب ألا يتجاوز 500 حرف")]
        public string VideoFilePath { get; set; }
        
        [Range(1, 10000, ErrorMessage = "عدد الإطارات يجب أن يكون بين 1 و 10000")]
        public int? FrameCount { get; set; }
        
        [Range(1.0, 120.0, ErrorMessage = "معدل الإطارات يجب أن يكون بين 1.0 و 120.0 إطار/ثانية")]
        [Column(TypeName = "decimal(5,1)")]
        public double? FrameRate { get; set; }
        
        [Range(1, 3600, ErrorMessage = "مدة التحليل يجب أن تكون بين 1 و 3600 ثانية")]
        public int? AnalysisDuration { get; set; } // seconds
        
        [Range(0, 10000, ErrorMessage = "عدد الحيوانات المنوية المتتبعة يجب أن يكون بين 0 و 10000")]
        public int? TrackedSpermCount { get; set; }
        
        // Calibration Data
        public double? CalibrationFactor { get; set; }
        public string CalibrationMethod { get; set; }
        public DateTime? CalibrationDate { get; set; }
        
        // Analysis Parameters
        public double? MinimumTrackLength { get; set; }
        public double? MinimumVelocity { get; set; }
        public double? MaximumVelocity { get; set; }
        public double? MinimumProgressiveVelocity { get; set; }
        
        // Results
        [Required(ErrorMessage = "تاريخ التحليل مطلوب")]
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "اسم المحلل مطلوب")]
        [StringLength(100, ErrorMessage = "اسم المحلل يجب ألا يتجاوز 100 حرف")]
        public string AnalyzedBy { get; set; }
        
        [StringLength(100, ErrorMessage = "اسم المراجع يجب ألا يتجاوز 100 حرف")]
        public string VerifiedBy { get; set; }
        
        public DateTime? VerificationDate { get; set; }
        
        [StringLength(1000, ErrorMessage = "التعليقات يجب ألا تتجاوز 1000 حرف")]
        public string Comments { get; set; }
        
        [Required(ErrorMessage = "تاريخ الإنشاء مطلوب")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Properties
        public virtual Exam Exam { get; set; }
    }
}