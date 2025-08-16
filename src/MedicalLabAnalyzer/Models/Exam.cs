using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string ExamType { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ExamName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ExamDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Cancelled

        [StringLength(1000)]
        public string? Results { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(200)]
        public string? VideoPath { get; set; }

        [StringLength(200)]
        public string? ReportPath { get; set; }

        public decimal? Price { get; set; }

        [StringLength(100)]
        public string? Technician { get; set; }

        [StringLength(100)]
        public string? Doctor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        // Computed properties
        [NotMapped]
        public string PatientName => Patient?.FullName ?? "Unknown";

        [NotMapped]
        public bool IsCompleted => Status == "Completed";

        [NotMapped]
        public bool HasVideo => !string.IsNullOrEmpty(VideoPath);

        [NotMapped]
        public bool HasReport => !string.IsNullOrEmpty(ReportPath);

        // Validation methods
        public bool IsValid()
        {
            return PatientId > 0 &&
                   !string.IsNullOrWhiteSpace(ExamType) &&
                   !string.IsNullOrWhiteSpace(ExamName) &&
                   ExamDate <= DateTime.Now;
        }

        public void MarkAsCompleted()
        {
            Status = "Completed";
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
            
            if (newStatus == "Completed" && !CompletedAt.HasValue)
            {
                CompletedAt = DateTime.UtcNow;
            }
        }

        public override string ToString()
        {
            return $"{ExamName} - {PatientName} ({ExamDate:yyyy-MM-dd})";
        }
    }
}