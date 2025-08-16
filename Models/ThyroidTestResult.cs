using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class ThyroidTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Thyroid Function Tests
        public double? TSH { get; set; } // μIU/mL
        public string TSHStatus { get; set; } // Normal, Low, High
        
        public double? FT4 { get; set; } // ng/dL
        public string FT4Status { get; set; } // Normal, Low, High
        
        public double? FT3 { get; set; } // pg/mL
        public string FT3Status { get; set; } // Normal, Low, High
        
        public double? TotalT4 { get; set; } // μg/dL
        public string TotalT4Status { get; set; } // Normal, Low, High
        
        public double? TotalT3 { get; set; } // ng/dL
        public string TotalT3Status { get; set; } // Normal, Low, High
        
        public double? ReverseT3 { get; set; } // ng/dL
        public string ReverseT3Status { get; set; } // Normal, Low, High
        
        // Thyroid Antibodies
        public double? AntiTPO { get; set; } // IU/mL
        public string AntiTPOStatus { get; set; } // Normal, Elevated, High
        
        public double? AntiTG { get; set; } // IU/mL
        public string AntiTGStatus { get; set; } // Normal, Elevated, High
        
        public double? TRAb { get; set; } // IU/L
        public string TRAbStatus { get; set; } // Normal, Elevated, High
        
        // Additional Tests
        public double? Thyroglobulin { get; set; } // ng/mL
        public string ThyroglobulinStatus { get; set; } // Normal, Elevated, High
        
        public double? Calcitonin { get; set; } // pg/mL
        public string CalcitoninStatus { get; set; } // Normal, Elevated, High
        
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