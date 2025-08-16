using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class LipidProfileTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Basic Lipid Profile
        public double? TotalCholesterol { get; set; } // mg/dL
        public string TotalCholesterolStatus { get; set; } // Normal, Borderline, High
        
        public double? HDL { get; set; } // mg/dL
        public string HDLStatus { get; set; } // Normal, Low, High
        
        public double? LDL { get; set; } // mg/dL
        public string LDLStatus { get; set; } // Normal, Borderline, High, Very High
        
        public double? Triglycerides { get; set; } // mg/dL
        public string TriglyceridesStatus { get; set; } // Normal, Borderline, High, Very High
        
        // Calculated Values
        public double? TotalCholesterolHDL { get; set; } // Ratio
        public string TotalCholesterolHDLStatus { get; set; } // Normal, High
        
        public double? LDLHDL { get; set; } // Ratio
        public string LDLHDLStatus { get; set; } // Normal, High
        
        public double? NonHDLCholesterol { get; set; } // mg/dL
        public string NonHDLCholesterolStatus { get; set; } // Normal, High
        
        // Additional Tests
        public double? VLDL { get; set; } // mg/dL
        public double? ApolipoproteinA1 { get; set; } // mg/dL
        public double? ApolipoproteinB { get; set; } // mg/dL
        public double? ApolipoproteinBA1 { get; set; } // Ratio
        public double? LipoproteinA { get; set; } // mg/dL
        public double? LpPLA2 { get; set; } // ng/mL
        
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