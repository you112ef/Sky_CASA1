using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class ElectrolytesTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Basic Electrolytes
        public double? Sodium { get; set; } // mEq/L
        public string SodiumStatus { get; set; } // Normal, Low, High
        
        public double? Potassium { get; set; } // mEq/L
        public string PotassiumStatus { get; set; } // Normal, Low, High
        
        public double? Chloride { get; set; } // mEq/L
        public string ChlorideStatus { get; set; } // Normal, Low, High
        
        public double? Bicarbonate { get; set; } // mEq/L
        public string BicarbonateStatus { get; set; } // Normal, Low, High
        
        // Additional Electrolytes
        public double? Calcium { get; set; } // mg/dL
        public string CalciumStatus { get; set; } // Normal, Low, High
        
        public double? IonizedCalcium { get; set; } // mg/dL
        public string IonizedCalciumStatus { get; set; } // Normal, Low, High
        
        public double? Phosphorus { get; set; } // mg/dL
        public string PhosphorusStatus { get; set; } // Normal, Low, High
        
        public double? Magnesium { get; set; } // mg/dL
        public string MagnesiumStatus { get; set; } // Normal, Low, High
        
        // Calculated Values
        public double? AnionGap { get; set; } // mEq/L
        public string AnionGapStatus { get; set; } // Normal, Elevated, High
        
        public double? Osmolality { get; set; } // mOsm/kg
        public string OsmolalityStatus { get; set; } // Normal, Low, High
        
        public double? CalculatedOsmolality { get; set; } // mOsm/kg
        public string CalculatedOsmolalityStatus { get; set; } // Normal, Low, High
        
        public double? OsmolarGap { get; set; } // mOsm/kg
        public string OsmolarGapStatus { get; set; } // Normal, Elevated, High
        
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