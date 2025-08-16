using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class UrineTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Physical Properties
        public string Color { get; set; } // Yellow, Dark Yellow, Clear, etc.
        public string Appearance { get; set; } // Clear, Turbid, Cloudy
        public double? SpecificGravity { get; set; }
        public double? pH { get; set; }
        public string Odor { get; set; } // Normal, Foul, Sweet, etc.
        public double? Volume { get; set; } // ml
        
        // Chemical Properties
        public string Glucose { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Protein { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Blood { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Ketones { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Bilirubin { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Urobilinogen { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Nitrites { get; set; } // Negative, Positive
        public string Leukocytes { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string AscorbicAcid { get; set; } // Negative, Positive
        
        // Microscopic Examination
        public int? RBC { get; set; } // per HPF
        public int? WBC { get; set; } // per HPF
        public int? EpithelialCells { get; set; } // per HPF
        public string EpithelialCellType { get; set; } // Squamous, Transitional, Renal
        public int? Casts { get; set; } // per LPF
        public string CastType { get; set; } // Hyaline, Granular, Waxy, etc.
        public int? Crystals { get; set; } // per HPF
        public string CrystalType { get; set; } // Calcium Oxalate, Uric Acid, etc.
        public int? Bacteria { get; set; } // per HPF
        public int? Yeast { get; set; } // per HPF
        public int? Parasites { get; set; } // per HPF
        public string ParasiteType { get; set; }
        public int? Sperm { get; set; } // per HPF
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
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        public string AnalyzedBy { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        
        public string Comments { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Properties
        public virtual Exam Exam { get; set; }
    }
}