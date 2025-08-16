using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class CASAResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Basic Parameters
        public double? Volume { get; set; } // ml
        public double? Concentration { get; set; } // million/ml
        public double? TotalSpermCount { get; set; } // million
        public double? Motility { get; set; } // %
        public double? ProgressiveMotility { get; set; } // %
        public double? NonProgressiveMotility { get; set; } // %
        public double? Immotile { get; set; } // %
        
        // Morphology
        public double? NormalForms { get; set; } // %
        public double? AbnormalForms { get; set; } // %
        public double? HeadAbnormalities { get; set; } // %
        public double? MidpieceAbnormalities { get; set; } // %
        public double? TailAbnormalities { get; set; } // %
        
        // Vitality
        public double? Vitality { get; set; } // %
        public double? DeadSperm { get; set; } // %
        
        // Agglutination
        public string Agglutination { get; set; } // None, Mild, Moderate, Severe
        
        // Viscosity
        public string Viscosity { get; set; } // Normal, Increased, Decreased
        
        // pH
        public double? pH { get; set; }
        
        // Liquefaction Time
        public int? LiquefactionTime { get; set; } // minutes
        
        // Color
        public string Color { get; set; } // Normal, Yellow, Brown, Red
        
        // Appearance
        public string Appearance { get; set; } // Normal, Turbid, Clear
        
        // Round Cells
        public double? RoundCells { get; set; } // million/ml
        
        // Leukocytes
        public double? Leukocytes { get; set; } // million/ml
        
        // MAR Test
        public double? MARTest { get; set; } // %
        
        // Immunobead Test
        public double? ImmunobeadTest { get; set; } // %
        
        // DNA Fragmentation
        public double? DNAFragmentation { get; set; } // %
        
        // Acrosome Reaction
        public double? AcrosomeReaction { get; set; } // %
        
        // Hyperactivation
        public double? Hyperactivation { get; set; } // %
        
        // Computer Analysis Results
        public double? VAP { get; set; } // Average Path Velocity
        public double? VSL { get; set; } // Straight Line Velocity
        public double? VCL { get; set; } // Curvilinear Velocity
        public double? ALH { get; set; } // Amplitude of Lateral Head Displacement
        public double? BCF { get; set; } // Beat Cross Frequency
        public double? STR { get; set; } // Straightness
        public double? LIN { get; set; } // Linearity
        public double? WOB { get; set; } // Wobble
        
        // Quality Control
        public bool IsQualityControlPassed { get; set; } = false;
        public string QualityControlNotes { get; set; }
        
        // Results Interpretation
        public string Interpretation { get; set; } // Normal, Abnormal, Borderline
        public string ClinicalSignificance { get; set; }
        public string Recommendations { get; set; }
        
        // Technical Details
        public string VideoFilePath { get; set; }
        public int? FrameCount { get; set; }
        public double? FrameRate { get; set; }
        public int? AnalysisDuration { get; set; } // seconds
        public int? TrackedSpermCount { get; set; }
        
        // Calibration Data
        public double? CalibrationFactor { get; set; }
        public string CalibrationMethod { get; set; }
        public DateTime? CalibrationDate { get; set; }
        
        // Analysis Parameters
        public double? MinimumTrackLength { get; set; }
        public double? MinimumVelocity { get; set; }
        public double? MaximumVelocity { get; set; }
        public double? MinimumProgressiveVelocity { get; set; }
        
        // Results
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