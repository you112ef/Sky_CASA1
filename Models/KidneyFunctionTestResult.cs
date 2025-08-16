using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class KidneyFunctionTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Kidney Function Tests
        public double? Urea { get; set; } // mg/dL
        public string UreaStatus { get; set; } // Normal, Elevated, High
        
        public double? Creatinine { get; set; } // mg/dL
        public string CreatinineStatus { get; set; } // Normal, Elevated, High
        
        public double? UricAcid { get; set; } // mg/dL
        public string UricAcidStatus { get; set; } // Normal, Elevated, High
        
        public double? CystatinC { get; set; } // mg/L
        public string CystatinCStatus { get; set; } // Normal, Elevated, High
        
        // Calculated Values
        public double? eGFR { get; set; } // mL/min/1.73m²
        public string eGFRStatus { get; set; } // Normal, Mild, Moderate, Severe, Kidney Failure
        
        public double? BUNCreatinineRatio { get; set; } // Ratio
        public string BUNCreatinineRatioStatus { get; set; } // Normal, Elevated, High
        
        // Electrolytes
        public double? Sodium { get; set; } // mEq/L
        public string SodiumStatus { get; set; } // Normal, Low, High
        
        public double? Potassium { get; set; } // mEq/L
        public string PotassiumStatus { get; set; } // Normal, Low, High
        
        public double? Chloride { get; set; } // mEq/L
        public string ChlorideStatus { get; set; } // Normal, Low, High
        
        public double? Bicarbonate { get; set; } // mEq/L
        public string BicarbonateStatus { get; set; } // Normal, Low, High
        
        // Additional Tests
        public double? Calcium { get; set; } // mg/dL
        public string CalciumStatus { get; set; } // Normal, Low, High
        
        public double? Phosphorus { get; set; } // mg/dL
        public string PhosphorusStatus { get; set; } // Normal, Low, High
        
        public double? Magnesium { get; set; } // mg/dL
        public string MagnesiumStatus { get; set; } // Normal, Low, High
        
        public double? Microalbumin { get; set; } // mg/L
        public string MicroalbuminStatus { get; set; } // Normal, Elevated, High
        
        public double? AlbuminCreatinineRatio { get; set; } // mg/g
        public string AlbuminCreatinineRatioStatus { get; set; } // Normal, Elevated, High
        
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