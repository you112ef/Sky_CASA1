using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class CBCTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Red Blood Cells (RBC)
        public double? RBC { get; set; } // million/μL
        public double? Hemoglobin { get; set; } // g/dL
        public double? Hematocrit { get; set; } // %
        public double? MCV { get; set; } // fL
        public double? MCH { get; set; } // pg
        public double? MCHC { get; set; } // g/dL
        public double? RDW { get; set; } // %
        
        // White Blood Cells (WBC)
        public double? WBC { get; set; } // thousand/μL
        public double? Neutrophils { get; set; } // %
        public double? Lymphocytes { get; set; } // %
        public double? Monocytes { get; set; } // %
        public double? Eosinophils { get; set; } // %
        public double? Basophils { get; set; } // %
        public double? NeutrophilsAbsolute { get; set; } // cells/μL
        public double? LymphocytesAbsolute { get; set; } // cells/μL
        public double? MonocytesAbsolute { get; set; } // cells/μL
        public double? EosinophilsAbsolute { get; set; } // cells/μL
        public double? BasophilsAbsolute { get; set; } // cells/μL
        
        // Platelets
        public double? Platelets { get; set; } // thousand/μL
        public double? MPV { get; set; } // fL
        public double? PDW { get; set; } // %
        public double? PCT { get; set; } // %
        
        // Additional Parameters
        public double? Reticulocytes { get; set; } // %
        public double? ReticulocyteCount { get; set; } // cells/μL
        public double? ESR { get; set; } // mm/hr
        public double? CRP { get; set; } // mg/L
        
        // Blood Film
        public string RBCMorphology { get; set; } // Normal, Anisocytosis, Poikilocytosis, etc.
        public string WBCMorphology { get; set; } // Normal, Left Shift, Right Shift, etc.
        public string PlateletMorphology { get; set; } // Normal, Giant, Small, etc.
        public string Parasites { get; set; } // None, Malaria, etc.
        public string Inclusions { get; set; } // None, Howell-Jolly bodies, etc.
        
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