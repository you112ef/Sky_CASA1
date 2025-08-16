using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
        public decimal? RBCMin { get; set; } // Age and gender specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RBCMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Hemoglobin { get; set; } // g/dL
        public string? HemoglobinStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HemoglobinMin { get; set; } // Age and gender specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HemoglobinMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Hematocrit { get; set; } // %
        public string? HematocritStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HematocritMin { get; set; } // Age and gender specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? HematocritMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCV { get; set; } // fL
        public string? MCVStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCVMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCVMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCH { get; set; } // pg
        public string? MCHStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHC { get; set; } // g/dL
        public string? MCHCStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHCMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MCHCMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? RDW { get; set; } // %
        public string? RDWStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RDWMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? RDWMax { get; set; }

        // White Blood Cells (WBC)
        [Column(TypeName = "decimal(8,2)")]
        public decimal? WBC { get; set; } // 10^9/L
        public string? WBCStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? WBCMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? WBCMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Neutrophils { get; set; } // %
        public string? NeutrophilsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? NeutrophilsMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? NeutrophilsMax { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? NeutrophilsAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Lymphocytes { get; set; } // %
        public string? LymphocytesStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? LymphocytesMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? LymphocytesMax { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? LymphocytesAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Monocytes { get; set; } // %
        public string? MonocytesStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MonocytesMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MonocytesMax { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MonocytesAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Eosinophils { get; set; } // %
        public string? EosinophilsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EosinophilsMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EosinophilsMax { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EosinophilsAbsolute { get; set; } // 10^9/L

        [Column(TypeName = "decimal(8,2)")]
        public decimal? Basophils { get; set; } // %
        public string? BasophilsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? BasophilsMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? BasophilsMax { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? BasophilsAbsolute { get; set; } // 10^9/L

        // Platelets
        [Column(TypeName = "decimal(8,2)")]
        public decimal? Platelets { get; set; } // 10^9/L
        public string? PlateletsStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PlateletsMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PlateletsMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? MPV { get; set; } // fL
        public string? MPVStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MPVMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? MPVMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? PDW { get; set; } // %
        public string? PDWStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PDWMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PDWMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? PCT { get; set; } // %
        public string? PCTStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PCTMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? PCTMax { get; set; }

        // Additional Parameters
        [Column(TypeName = "decimal(8,2)")]
        public decimal? Reticulocytes { get; set; } // %
        public string? ReticulocytesStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ReticulocytesMin { get; set; } // Age specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ReticulocytesMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ESR { get; set; } // mm/hr
        public string? ESRStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ESRMin { get; set; } // Age and gender specific, set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ESRMax { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? CRP { get; set; } // mg/L
        public string? CRPStatus { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal? CRPMin { get; set; } // Set by MedicalReferenceService
        [Column(TypeName = "decimal(8,2)")]
        public decimal? CRPMax { get; set; }

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
            // Only analyze if reference ranges are set (should be set by MedicalReferenceService)
            if (!RBCMin.HasValue || !RBCMax.HasValue) return;

            // RBC Analysis
            if (RBC.HasValue && RBCMin.HasValue && RBCMax.HasValue)
            {
                RBCStatus = RBC.Value < RBCMin.Value ? "Low" : RBC.Value > RBCMax.Value ? "High" : "Normal";
            }

            // Hemoglobin Analysis
            if (Hemoglobin.HasValue && HemoglobinMin.HasValue && HemoglobinMax.HasValue)
            {
                HemoglobinStatus = Hemoglobin.Value < HemoglobinMin.Value ? "Low" : Hemoglobin.Value > HemoglobinMax.Value ? "High" : "Normal";
            }

            // Hematocrit Analysis
            if (Hematocrit.HasValue && HematocritMin.HasValue && HematocritMax.HasValue)
            {
                HematocritStatus = Hematocrit.Value < HematocritMin.Value ? "Low" : Hematocrit.Value > HematocritMax.Value ? "High" : "Normal";
            }

            // MCV Analysis
            if (MCV.HasValue && MCVMin.HasValue && MCVMax.HasValue)
            {
                MCVStatus = MCV.Value < MCVMin.Value ? "Low" : MCV.Value > MCVMax.Value ? "High" : "Normal";
            }

            // MCH Analysis
            if (MCH.HasValue && MCHMin.HasValue && MCHMax.HasValue)
            {
                MCHStatus = MCH.Value < MCHMin.Value ? "Low" : MCH.Value > MCHMax.Value ? "High" : "Normal";
            }

            // MCHC Analysis
            if (MCHC.HasValue && MCHCMin.HasValue && MCHCMax.HasValue)
            {
                MCHCStatus = MCHC.Value < MCHCMin.Value ? "Low" : MCHC.Value > MCHCMax.Value ? "High" : "Normal";
            }

            // RDW Analysis
            if (RDW.HasValue && RDWMin.HasValue && RDWMax.HasValue)
            {
                RDWStatus = RDW.Value < RDWMin.Value ? "Low" : RDW.Value > RDWMax.Value ? "High" : "Normal";
            }

            // WBC Analysis
            if (WBC.HasValue && WBCMin.HasValue && WBCMax.HasValue)
            {
                WBCStatus = WBC.Value < WBCMin.Value ? "Low" : WBC.Value > WBCMax.Value ? "High" : "Normal";
            }

            // Neutrophils Analysis
            if (Neutrophils.HasValue && NeutrophilsMin.HasValue && NeutrophilsMax.HasValue)
            {
                NeutrophilsStatus = Neutrophils.Value < NeutrophilsMin.Value ? "Low" : Neutrophils.Value > NeutrophilsMax.Value ? "High" : "Normal";
            }

            // Lymphocytes Analysis
            if (Lymphocytes.HasValue && LymphocytesMin.HasValue && LymphocytesMax.HasValue)
            {
                LymphocytesStatus = Lymphocytes.Value < LymphocytesMin.Value ? "Low" : Lymphocytes.Value > LymphocytesMax.Value ? "High" : "Normal";
            }

            // Monocytes Analysis
            if (Monocytes.HasValue && MonocytesMin.HasValue && MonocytesMax.HasValue)
            {
                MonocytesStatus = Monocytes.Value < MonocytesMin.Value ? "Low" : Monocytes.Value > MonocytesMax.Value ? "High" : "Normal";
            }

            // Eosinophils Analysis
            if (Eosinophils.HasValue && EosinophilsMin.HasValue && EosinophilsMax.HasValue)
            {
                EosinophilsStatus = Eosinophils.Value < EosinophilsMin.Value ? "Low" : Eosinophils.Value > EosinophilsMax.Value ? "High" : "Normal";
            }

            // Basophils Analysis
            if (Basophils.HasValue && BasophilsMin.HasValue && BasophilsMax.HasValue)
            {
                BasophilsStatus = Basophils.Value < BasophilsMin.Value ? "Low" : Basophils.Value > BasophilsMax.Value ? "High" : "Normal";
            }

            // Platelets Analysis
            if (Platelets.HasValue && PlateletsMin.HasValue && PlateletsMax.HasValue)
            {
                PlateletsStatus = Platelets.Value < PlateletsMin.Value ? "Low" : Platelets.Value > PlateletsMax.Value ? "High" : "Normal";
            }

            // MPV Analysis
            if (MPV.HasValue && MPVMin.HasValue && MPVMax.HasValue)
            {
                MPVStatus = MPV.Value < MPVMin.Value ? "Low" : MPV.Value > MPVMax.Value ? "High" : "Normal";
            }

            // PDW Analysis
            if (PDW.HasValue && PDWMin.HasValue && PDWMax.HasValue)
            {
                PDWStatus = PDW.Value < PDWMin.Value ? "Low" : PDW.Value > PDWMax.Value ? "High" : "Normal";
            }

            // PCT Analysis
            if (PCT.HasValue && PCTMin.HasValue && PCTMax.HasValue)
            {
                PCTStatus = PCT.Value < PCTMin.Value ? "Low" : PCT.Value > PCTMax.Value ? "High" : "Normal";
            }

            // Reticulocytes Analysis
            if (Reticulocytes.HasValue && ReticulocytesMin.HasValue && ReticulocytesMax.HasValue)
            {
                ReticulocytesStatus = Reticulocytes.Value < ReticulocytesMin.Value ? "Low" : Reticulocytes.Value > ReticulocytesMax.Value ? "High" : "Normal";
            }

            // ESR Analysis
            if (ESR.HasValue && ESRMin.HasValue && ESRMax.HasValue)
            {
                ESRStatus = ESR.Value < ESRMin.Value ? "Low" : ESR.Value > ESRMax.Value ? "High" : "Normal";
            }

            // CRP Analysis
            if (CRP.HasValue && CRPMin.HasValue && CRPMax.HasValue)
            {
                CRPStatus = CRP.Value < CRPMin.Value ? "Low" : CRP.Value > CRPMax.Value ? "High" : "Normal";
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

            // Determine overall status based on critical parameters
            var statusList = new[] { RBCStatus, HemoglobinStatus, HematocritStatus, WBCStatus, PlateletsStatus };
            var abnormalCount = statusList.Count(s => s == "Low" || s == "High");
            var criticalCount = statusList.Count(s => s == "High" && IsCriticalHigh()) + 
                               statusList.Count(s => s == "Low" && IsCriticalLow());

            OverallStatus = criticalCount > 0 ? "Critical" : abnormalCount switch
            {
                0 => "Normal",
                >= 1 and <= 2 => "Abnormal",
                _ => "Critical"
            };
        }

        private bool IsCriticalHigh()
        {
            // Define critical high values that require immediate attention
            return (WBC.HasValue && WBC.Value > 30) || // Severe leukocytosis
                   (Platelets.HasValue && Platelets.Value > 1000) || // Severe thrombocytosis
                   (ESR.HasValue && ESR.Value > 100) || // Very high ESR
                   (CRP.HasValue && CRP.Value > 50); // Very high CRP
        }

        private bool IsCriticalLow()
        {
            // Define critical low values that require immediate attention
            return (Hemoglobin.HasValue && Hemoglobin.Value < 8) || // Severe anemia
                   (WBC.HasValue && WBC.Value < 2) || // Severe leukopenia
                   (Platelets.HasValue && Platelets.Value < 50); // Severe thrombocytopenia
        }

        public override string ToString()
        {
            return $"CBC - {Exam?.Patient?.FullName} - {AnalysisDate:dd/MM/yyyy}";
        }
    }
}