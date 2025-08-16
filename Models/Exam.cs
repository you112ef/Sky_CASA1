using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ExamNumber { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ExamType { get; set; }
        
        [StringLength(100)]
        public string ExamSubType { get; set; }
        
        public DateTime ExamDate { get; set; } = DateTime.Now;
        
        public DateTime? SampleCollectionDate { get; set; }
        
        public DateTime? SampleReceivedDate { get; set; }
        
        public DateTime? AnalysisDate { get; set; }
        
        public DateTime? ReportDate { get; set; }
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
        
        [StringLength(500)]
        public string ClinicalNotes { get; set; }
        
        [StringLength(500)]
        public string SpecialInstructions { get; set; }
        
        [StringLength(100)]
        public string RequestingPhysician { get; set; }
        
        [StringLength(100)]
        public string Department { get; set; }
        
        [StringLength(100)]
        public string Ward { get; set; }
        
        public decimal? Cost { get; set; }
        
        public bool IsPaid { get; set; } = false;
        
        public bool IsUrgent { get; set; } = false;
        
        [StringLength(100)]
        public string PerformedBy { get; set; }
        
        [StringLength(100)]
        public string VerifiedBy { get; set; }
        
        public DateTime? VerificationDate { get; set; }
        
        [StringLength(500)]
        public string Comments { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedDate { get; set; }
        
        [StringLength(50)]
        public string CreatedBy { get; set; }
        
        [StringLength(50)]
        public string ModifiedBy { get; set; }
        
        // Navigation Properties
        public virtual Patient Patient { get; set; }
    }
}