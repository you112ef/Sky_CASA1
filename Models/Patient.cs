using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    [Table("Patients")]
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(20)]
        public string NationalId { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty; // ذكر، أنثى

        [StringLength(100)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        [StringLength(100)]
        public string? MedicalHistory { get; set; }

        [StringLength(100)]
        public string? Allergies { get; set; }

        [StringLength(100)]
        public string? CurrentMedications { get; set; }

        [StringLength(50)]
        public string? BloodType { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-

        [StringLength(50)]
        public string? InsuranceProvider { get; set; }

        [StringLength(50)]
        public string? InsuranceNumber { get; set; }

        [StringLength(100)]
        public string? ReferringDoctor { get; set; }

        [StringLength(100)]
        public string? ReferringClinic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [StringLength(50)]
        public string UpdatedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

        // Calculated Properties
        [NotMapped]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [NotMapped]
        public string FullNameWithId => $"{FullName} - {NationalId}";

        // Methods
        public override string ToString()
        {
            return FullNameWithId;
        }
    }
}