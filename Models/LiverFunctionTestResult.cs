using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class LiverFunctionTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Liver Enzymes
        public double? ALT { get; set; } // U/L
        public string ALTStatus { get; set; } // Normal, Elevated, High
        
        public double? AST { get; set; } // U/L
        public string ASTStatus { get; set; } // Normal, Elevated, High
        
        public double? ALP { get; set; } // U/L
        public string ALPStatus { get; set; } // Normal, Elevated, High
        
        public double? GGT { get; set; } // U/L
        public string GGTStatus { get; set; } // Normal, Elevated, High
        
        public double? LDH { get; set; } // U/L
        public string LDHStatus { get; set; } // Normal, Elevated, High
        
        // Bilirubin
        public double? TotalBilirubin { get; set; } // mg/dL
        public string TotalBilirubinStatus { get; set; } // Normal, Elevated, High
        
        public double? DirectBilirubin { get; set; } // mg/dL
        public string DirectBilirubinStatus { get; set; } // Normal, Elevated, High
        
        public double? IndirectBilirubin { get; set; } // mg/dL
        public string IndirectBilirubinStatus { get; set; } // Normal, Elevated, High
        
        // Proteins
        public double? TotalProtein { get; set; } // g/dL
        public string TotalProteinStatus { get; set; } // Normal, Low, High
        
        public double? Albumin { get; set; } // g/dL
        public string AlbuminStatus { get; set; } // Normal, Low, High
        
        public double? Globulin { get; set; } // g/dL
        public string GlobulinStatus { get; set; } // Normal, Low, High
        
        public double? AlbuminGlobulinRatio { get; set; } // Ratio
        public string AlbuminGlobulinRatioStatus { get; set; } // Normal, Low, High
        
        // Additional Tests
        public double? Ammonia { get; set; } // μg/dL
        public string AmmoniaStatus { get; set; } // Normal, Elevated, High
        
        public double? AlphaFetoprotein { get; set; } // ng/mL
        public string AlphaFetoproteinStatus { get; set; } // Normal, Elevated, High
        
        public double? ProthrombinTime { get; set; } // seconds
        public string ProthrombinTimeStatus { get; set; } // Normal, Prolonged
        
        public double? INR { get; set; } // International Normalized Ratio
        public string INRStatus { get; set; } // Normal, Elevated, High
        
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