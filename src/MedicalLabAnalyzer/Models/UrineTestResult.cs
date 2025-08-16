using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    public class UrineTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Physical Properties
        [StringLength(30, ErrorMessage = "وصع لون البول يجب ألا يتجاوز 30 حرف")]
        [RegularExpression("^(Yellow|Dark Yellow|Clear|Amber|Red|Brown|أصفر|أصفر غامق|شفاف|عنبري|أحمر|بني)$", ErrorMessage = "لون البول غير صحيح")]
        public string Color { get; set; } // Yellow, Dark Yellow, Clear, etc.
        
        [StringLength(30, ErrorMessage = "مظهر البول يجب ألا يتجاوز 30 حرف")]
        [RegularExpression("^(Clear|Turbid|Cloudy|Hazy|شفاف|عكر|غائم|ضبابي)$", ErrorMessage = "مظهر البول غير صحيح")]
        public string Appearance { get; set; } // Clear, Turbid, Cloudy
        
        [Range(1.000, 1.030, ErrorMessage = "الوزن النوعي يجب أن يكون بين 1.000 و 1.030")]
        [Column(TypeName = "decimal(5,3)")]
        public double? SpecificGravity { get; set; }
        
        [Range(4.5, 8.5, ErrorMessage = "قيمة الأس الهيدروجيني يجب أن تكون بين 4.5 و 8.5")]
        [Column(TypeName = "decimal(3,1)")]
        public double? pH { get; set; }
        
        [StringLength(30, ErrorMessage = "وصف رائحة البول يجب ألا يتجاوز 30 حرف")]
        [RegularExpression("^(Normal|Foul|Sweet|Fruity|طبيعي|فاسد|حلو|فواكهي)$", ErrorMessage = "رائحة البول غير صحيحة")]
        public string Odor { get; set; } // Normal, Foul, Sweet, etc.
        
        [Range(10, 2000, ErrorMessage = "حجم البول يجب أن يكون بين 10 و 2000 مل")]
        [Column(TypeName = "decimal(8,2)")]
        public double? Volume { get; set; } // ml
        
        // Chemical Properties
        [StringLength(10, ErrorMessage = "نتيجة فحص الجلوكوز يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص الجلوكوز غير صحيحة")]
        public string Glucose { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص البروتين يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص البروتين غير صحيحة")]
        public string Protein { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص الدم يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص الدم غير صحيحة")]
        public string Blood { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص الكيتونات يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص الكيتونات غير صحيحة")]
        public string Ketones { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص البيليروبين يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص البيليروبين غير صحيحة")]
        public string Bilirubin { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص اليوروبيلينوجين يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص اليوروبيلينوجين غير صحيحة")]
        public string Urobilinogen { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص النترايت يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Positive|سالب|موجب)$", ErrorMessage = "نتيجة فحص النترايت غير صحيحة")]
        public string Nitrites { get; set; } // Negative, Positive
        
        [StringLength(10, ErrorMessage = "نتيجة فحص كريات الدم البيضاء يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Trace|1\\+|2\\+|3\\+|4\\+|سالب|أثر)$", ErrorMessage = "نتيجة فحص كريات الدم البيضاء غير صحيحة")]
        public string Leukocytes { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        [StringLength(10, ErrorMessage = "نتيجة فحص حمض الأسكوربيك يجب ألا تتجاوز 10 أحرف")]
        [RegularExpression("^(Negative|Positive|سالب|موجب)$", ErrorMessage = "نتيجة فحص حمض الأسكوربيك غير صحيحة")]
        public string AscorbicAcid { get; set; } // Negative, Positive
        
        // Microscopic Examination
        [Range(0, 100, ErrorMessage = "عدد كريات الدم الحمراء يجب أن يكون بين 0 و 100 في المجال عالي القوة")]
        public int? RBC { get; set; } // per HPF
        
        [Range(0, 200, ErrorMessage = "عدد كريات الدم البيضاء يجب أن يكون بين 0 و 200 في المجال عالي القوة")]
        public int? WBC { get; set; } // per HPF
        
        [Range(0, 50, ErrorMessage = "عدد الخلايا الظهارية يجب أن يكون بين 0 و 50 في المجال عالي القوة")]
        public int? EpithelialCells { get; set; } // per HPF
        
        [StringLength(20, ErrorMessage = "نوع الخلايا الظهارية يجب ألا يتجاوز 20 حرف")]
        public string EpithelialCellType { get; set; } // Squamous, Transitional, Renal
        
        [Range(0, 20, ErrorMessage = "عدد الاسطوانات يجب أن يكون بين 0 و 20 في المجال منخفض القوة")]
        public int? Casts { get; set; } // per LPF
        
        [StringLength(30, ErrorMessage = "نوع الاسطوانة يجب ألا يتجاوز 30 حرف")]
        public string CastType { get; set; } // Hyaline, Granular, Waxy, etc.
        
        [Range(0, 50, ErrorMessage = "عدد البلورات يجب أن يكون بين 0 و 50 في المجال عالي القوة")]
        public int? Crystals { get; set; } // per HPF
        
        [StringLength(50, ErrorMessage = "نوع البلورات يجب ألا يتجاوز 50 حرف")]
        public string CrystalType { get; set; } // Calcium Oxalate, Uric Acid, etc.
        
        [Range(0, 100, ErrorMessage = "عدد البكتيريا يجب أن يكون بين 0 و 100 في المجال عالي القوة")]
        public int? Bacteria { get; set; } // per HPF
        
        [Range(0, 50, ErrorMessage = "عدد الخمائر يجب أن يكون بين 0 و 50 في المجال عالي القوة")]
        public int? Yeast { get; set; } // per HPF
        
        [Range(0, 20, ErrorMessage = "عدد الطفيليات يجب أن يكون بين 0 و 20 في المجال عالي القوة")]
        public int? Parasites { get; set; } // per HPF
        
        [StringLength(50, ErrorMessage = "نوع الطفيلي يجب ألا يتجاوز 50 حرف")]
        public string ParasiteType { get; set; }
        
        [Range(0, 10, ErrorMessage = "عدد الحيوانات المنوية يجب أن يكون بين 0 و 10 في المجال عالي القوة")]
        public int? Sperm { get; set; } // per HPF
        
        [Range(0, 20, ErrorMessage = "عدد المخاط يجب أن يكون بين 0 و 20 في المجال عالي القوة")]
        public int? Mucus { get; set; } // per HPF
        
        // Additional Tests
        public double? Albumin { get; set; } // mg/dL
        public double? Creatinine { get; set; } // mg/dL
        public double? Microalbumin { get; set; } // mg/L
        public double? AlbuminCreatinineRatio { get; set; } // mg/g
        public double? Sodium { get; set; } // mEq/L
        public double? Potassium { get; set; } // mEq/L
        public double? Chloride { get; set; } // mEq/L
        public double? Calcium { get; set; } // mg/dL
        public double? Phosphorus { get; set; } // mg/dL
        
        // Culture and Sensitivity
        public string CultureResult { get; set; } // No Growth, Mixed Flora, etc.
        public string Organism { get; set; }
        public string Sensitivity { get; set; } // Sensitive, Resistant, Intermediate
        
        // Quality Control
        public bool IsQualityControlPassed { get; set; } = false;
        public string QualityControlNotes { get; set; }
        
        // Results Interpretation
        public string Interpretation { get; set; } // Normal, Abnormal, Borderline
        public string ClinicalSignificance { get; set; }
        public string Recommendations { get; set; }
        
        // Analysis Details
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