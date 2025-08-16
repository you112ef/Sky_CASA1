using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    [Table("CBCTestResults")]
    public class CBCTestResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExamId { get; set; }

        // Red Blood Cells (RBC)
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RBC { get; set; } // 10^12/L
        public string? RBCStatus { get; set; } // Normal, Low, High
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RBCMin { get; set; } = 4.5m; // Male: 4.5-5.5, Female: 4.0-5.0
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RBCMax { get; set; } = 5.5m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Hemoglobin { get; set; } // g/dL
        public string? HemoglobinStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HemoglobinMin { get; set; } = 13.5m; // Male: 13.5-17.5, Female: 12.0-15.5
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HemoglobinMax { get; set; } = 17.5m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Hematocrit { get; set; } // %
        public string? HematocritStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HematocritMin { get; set; } = 41.0m; // Male: 41-50, Female: 36-46
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HematocritMax { get; set; } = 50.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCV { get; set; } // fL
        public string? MCVStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCVMin { get; set; } = 80.0m; // 80-100
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCVMax { get; set; } = 100.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCH { get; set; } // pg
        public string? MCHStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHMin { get; set; } = 27.0m; // 27-32
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHMax { get; set; } = 32.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHC { get; set; } // g/dL
        public string? MCHCStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHCMin { get; set; } = 32.0m; // 32-36
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHCMax { get; set; } = 36.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? RDW { get; set; } // %
        public string? RDWStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RDWMin { get; set; } = 11.5m; // 11.5-14.5
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RDWMax { get; set; } = 14.5m;

        // White Blood Cells (WBC)
        [Column(TypeName = "decimal(8,2)")]
        public decimal? WBC { get; set; } // 10^9/L
        public string? WBCStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? WBCMin { get; set; } = 4.0m; // 4.0-11.0
        [Column(TypeName = "decimal(8,2)")]
        public decimal? WBCMax { get; set; } = 11.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Neutrophils { get; set; } // %
        public string? NeutrophilsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? NeutrophilsMin { get; set; } = 40.0m; // 40-70
        [Column(TypeName = "decimal(8,2)")]
        public decimal? NeutrophilsMax { get; set; } = 70.0m;
        [Column(TypeName = "decimal(8,2)")]
        public decimal? NeutrophilsAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Lymphocytes { get; set; } // %
        public string? LymphocytesStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? LymphocytesMin { get; set; } = 20.0m; // 20-40
        [Column(TypeName = "decimal(8,2)")]
        public decimal? LymphocytesMax { get; set; } = 40.0m;
        [Column(TypeName = "decimal(8,2)")]
        public decimal? LymphocytesAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Monocytes { get; set; } // %
        public string? MonocytesStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MonocytesMin { get; set; } = 2.0m; // 2-8
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MonocytesMax { get; set; } = 8.0m;
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MonocytesAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Eosinophils { get; set; } // %
        public string? EosinophilsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EosinophilsMin { get; set; } = 1.0m; // 1-4
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EosinophilsMax { get; set; } = 4.0m;
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EosinophilsAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Basophils { get; set; } // %
        public string? BasophilsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? BasophilsMin { get; set; } = 0.5m; // 0.5-1
        [Column(TypeName = "decimal(8,2)")]
        public decimal? BasophilsMax { get; set; } = 1.0m;
        [Column(TypeName = "decimal(8,2)")]
        public decimal? BasophilsAbsolute { get; set; } // 10^9/L

        // Platelets
        [Column(TypeName = "decimal(8,2)")]
        public decimal? Platelets { get; set; } // 10^9/L
        public string? PlateletsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PlateletsMin { get; set; } = 150.0m; // 150-450
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PlateletsMax { get; set; } = 450.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MPV { get; set; } // fL
        public string? MPVStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MPVMin { get; set; } = 7.5m; // 7.5-11.5
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MPVMax { get; set; } = 11.5m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? PDW { get; set; } // %
        public string? PDWStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PDWMin { get; set; } = 10.0m; // 10-17
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PDWMax { get; set; } = 17.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? PCT { get; set; } // %
        public string? PCTStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PCTMin { get; set; } = 0.1m; // 0.1-0.3
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PCTMax { get; set; } = 0.3m;

        // Additional Parameters
        [Column(TypeName = "decimal(8,2)")]
        public decimal? Reticulocytes { get; set; } // %
        public string? ReticulocytesStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ReticulocytesMin { get; set; } = 0.5m; // 0.5-2.5
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ReticulocytesMax { get; set; } = 2.5m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ESR { get; set; } // mm/hr
        public string? ESRStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ESRMin { get; set; } = 0.0m; // Male: 0-15, Female: 0-20
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ESRMax { get; set; } = 20.0m;

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CRP { get; set; } // mg/L
        public string? CRPStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? CRPMin { get; set; } = 0.0m; // 0-3
        [Column(TypeName = "decimal(8,2)")]
        public decimal? CRPMax { get; set; } = 3.0m;

        // Blood Film Morphology
        [StringLength(500)]
        public string? RBCMorphology { get; set; }
        [StringLength(500)]
        public string? WBCMorphology { get; set; }
        [StringLength(500)]
        public string? PlateletMorphology { get; set; }
        [StringLength(500)]
        public string? AbnormalCells { get; set; }

        // Quality Control
        [StringLength(100)]
        public string? QCStatus { get; set; } = "Passed";
        [StringLength(500)]
        public string? QCComments { get; set; }

        // Interpretation
        [StringLength(1000)]
        public string? Interpretation { get; set; }
        [StringLength(100)]
        public string? OverallStatus { get; set; } // Normal, Abnormal, Critical

        // Analysis Details
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string AnalyzedBy { get; set; } = string.Empty;
        public DateTime VerifiedDate { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string VerifiedBy { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;
        [StringLength(50)]
        public string UpdatedBy { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; } = null!;

        // Methods
        public void AnalyzeResults()
        {
            // RBC Analysis
            if (RBC.HasValue)
            {
                RBCStatus = RBC.Value < RBCMin ? "Low" : RBC.Value > RBCMax ? "High" : "Normal";
            }

            // Hemoglobin Analysis
            if (Hemoglobin.HasValue)
            {
                HemoglobinStatus = Hemoglobin.Value < HemoglobinMin ? "Low" : Hemoglobin.Value > HemoglobinMax ? "High" : "Normal";
            }

            // WBC Analysis
            if (WBC.HasValue)
            {
                WBCStatus = WBC.Value < WBCMin ? "Low" : WBC.Value > WBCMax ? "High" : "Normal";
            }

            // Platelets Analysis
            if (Platelets.HasValue)
            {
                PlateletsStatus = Platelets.Value < PlateletsMin ? "Low" : Platelets.Value > PlateletsMax ? "High" : "Normal";
            }

            // Calculate absolute counts
            if (WBC.HasValue && Neutrophils.HasValue)
                NeutrophilsAbsolute = WBC.Value * Neutrophils.Value / 100;

            if (WBC.HasValue && Lymphocytes.HasValue)
                LymphocytesAbsolute = WBC.Value * Lymphocytes.Value / 100;

            if (WBC.HasValue && Monocytes.HasValue)
                MonocytesAbsolute = WBC.Value * Monocytes.Value / 100;

            if (WBC.HasValue && Eosinophils.HasValue)
                EosinophilsAbsolute = WBC.Value * Eosinophils.Value / 100;

            if (WBC.HasValue && Basophils.HasValue)
                BasophilsAbsolute = WBC.Value * Basophils.Value / 100;

            // Determine overall status
            var abnormalCount = new[] { RBCStatus, HemoglobinStatus, WBCStatus, PlateletsStatus }
                .Count(s => s == "Low" || s == "High");

            OverallStatus = abnormalCount switch
            {
                0 => "Normal",
                1 => "Abnormal",
                _ => "Critical"
            };
        }

        public override string ToString()
        {
            return $"CBC - {Exam?.Patient?.FullName} - {AnalysisDate:dd/MM/yyyy}";
        }
    }
}