using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class GlucoseTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Fasting Glucose
        public double? FastingGlucose { get; set; } // mg/dL
        public string FastingGlucoseStatus { get; set; } // Normal, Prediabetes, Diabetes
        
        // Random Glucose
        public double? RandomGlucose { get; set; } // mg/dL
        public string RandomGlucoseStatus { get; set; } // Normal, Elevated, High
        
        // Postprandial Glucose (2 hours after meal)
        public double? PostprandialGlucose { get; set; } // mg/dL
        public string PostprandialGlucoseStatus { get; set; } // Normal, Elevated, High
        
        // Oral Glucose Tolerance Test (OGTT)
        public double? OGTTFasting { get; set; } // mg/dL
        public double? OGTT1Hour { get; set; } // mg/dL
        public double? OGTT2Hour { get; set; } // mg/dL
        public string OGTTResult { get; set; } // Normal, Impaired, Diabetes
        
        // HbA1c (Glycated Hemoglobin)
        public double? HbA1c { get; set; } // %
        public string HbA1cStatus { get; set; } // Normal, Prediabetes, Diabetes
        
        // Fructosamine
        public double? Fructosamine { get; set; } // μmol/L
        public string FructosamineStatus { get; set; } // Normal, Elevated
        
        // Additional Tests
        public double? Insulin { get; set; } // μIU/mL
        public double? CPeptide { get; set; } // ng/mL
        public double? Ketones { get; set; } // mmol/L
        public string KetonesStatus { get; set; } // Normal, Elevated, High
        
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