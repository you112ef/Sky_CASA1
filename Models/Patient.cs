using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FileNumber { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; }
        
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [StringLength(100)]
        public string Email { get; set; }
        
        [StringLength(200)]
        public string Address { get; set; }
        
        [StringLength(50)]
        public string Nationality { get; set; }
        
        [StringLength(20)]
        public string BloodType { get; set; }
        
        [StringLength(500)]
        public string MedicalHistory { get; set; }
        
        [StringLength(500)]
        public string Allergies { get; set; }
        
        [StringLength(500)]
        public string CurrentMedications { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        public string EmergencyContact { get; set; }
        
        [StringLength(20)]
        public string EmergencyPhone { get; set; }
        
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }
}