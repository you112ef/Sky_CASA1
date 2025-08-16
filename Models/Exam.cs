using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    [Table("Exams")]
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; } = string.Empty; // CASA, CBC, Urine, Stool, etc.

        [Required]
        public DateTime ExamDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime SampleCollectionDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime SampleReceivedDate { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled

        [StringLength(50)]
        public string? SampleId { get; set; }

        [StringLength(100)]
        public string? SampleType { get; set; }

        [StringLength(100)]
        public string? SampleCondition { get; set; }

        [StringLength(500)]
        public string? ClinicalNotes { get; set; }

        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        [StringLength(100)]
        public string? RequestingDoctor { get; set; }

        [StringLength(100)]
        public string? RequestingClinic { get; set; }

        [StringLength(100)]
        public string? Priority { get; set; } = "Normal"; // Normal, Urgent, Critical

        public DateTime? CompletedDate { get; set; }

        [StringLength(50)]
        public string? CompletedBy { get; set; }

        [StringLength(500)]
        public string? Results { get; set; }

        [StringLength(500)]
        public string? Interpretation { get; set; }

        [StringLength(100)]
        public string? ReportPath { get; set; }

        public bool IsReportGenerated { get; set; } = false;

        public DateTime ReportGeneratedDate { get; set; }

        [StringLength(50)]
        public string ReportGeneratedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(50)]
        public string UpdatedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        public virtual CASAResult? CASAResult { get; set; }
        public virtual CBCTestResult? CBCTestResult { get; set; }
        public virtual UrineTestResult? UrineTestResult { get; set; }
        public virtual StoolTestResult? StoolTestResult { get; set; }
        public virtual GlucoseTestResult? GlucoseTestResult { get; set; }
        public virtual LipidProfileTestResult? LipidProfileTestResult { get; set; }
        public virtual LiverFunctionTestResult? LiverFunctionTestResult { get; set; }
        public virtual KidneyFunctionTestResult? KidneyFunctionTestResult { get; set; }
        public virtual CRPTestResult? CRPTestResult { get; set; }
        public virtual ThyroidTestResult? ThyroidTestResult { get; set; }
        public virtual ElectrolytesTestResult? ElectrolytesTestResult { get; set; }
        public virtual CoagulationTestResult? CoagulationTestResult { get; set; }
        public virtual VitaminTestResult? VitaminTestResult { get; set; }
        public virtual HormoneTestResult? HormoneTestResult { get; set; }
        public virtual MicrobiologyTestResult? MicrobiologyTestResult { get; set; }
        public virtual PCRTestResult? PCRTestResult { get; set; }
        public virtual SerologyTestResult? SerologyTestResult { get; set; }

        // Calculated Properties
        [NotMapped]
        public string ExamSummary => $"{ExamType} - {Patient?.FullName} - {ExamDate:dd/MM/yyyy}";

        [NotMapped]
        public string StatusDisplay
        {
            get
            {
                return Status switch
                {
                    "Pending" => "في الانتظار",
                    "InProgress" => "قيد التنفيذ",
                    "Completed" => "مكتمل",
                    "Cancelled" => "ملغي",
                    _ => Status
                };
            }
        }

        [NotMapped]
        public string PriorityDisplay
        {
            get
            {
                return Priority switch
                {
                    "Normal" => "عادي",
                    "Urgent" => "عاجل",
                    "Critical" => "حرج",
                    _ => Priority
                };
            }
        }

        // Methods
        public override string ToString()
        {
            return ExamSummary;
        }
    }
}