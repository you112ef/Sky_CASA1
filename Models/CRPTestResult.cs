using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class CRPTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // C-Reactive Protein
        public double? CRP { get; set; } // mg/L
        public string CRPStatus { get; set; } // Normal, Elevated, High
        
        // High Sensitivity CRP
        public double? HsCRP { get; set; } // mg/L
        public string HsCRPStatus { get; set; } // Low Risk, Average Risk, High Risk
        
        // Additional Inflammatory Markers
        public double? ESR { get; set; } // mm/hr
        public string ESRStatus { get; set; } // Normal, Elevated, High
        
        public double? Fibrinogen { get; set; } // mg/dL
        public string FibrinogenStatus { get; set; } // Normal, Elevated, High
        
        public double? Ferritin { get; set; } // ng/mL
        public string FerritinStatus { get; set; } // Normal, Low, High
        
        public double? Procalcitonin { get; set; } // ng/mL
        public string ProcalcitoninStatus { get; set; } // Normal, Elevated, High
        
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