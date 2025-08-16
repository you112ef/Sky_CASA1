using System;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string InsuranceProvider { get; set; }
        public string InsuranceNumber { get; set; }
        public string MedicalHistory { get; set; }
        public string Allergies { get; set; }
        public string BloodType { get; set; }
        public string RhFactor { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string MaritalStatus { get; set; }
        public string Occupation { get; set; }
        public string Employer { get; set; }
        public string ReferredBy { get; set; }
        public string PrimaryCarePhysician { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Notes { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public int Age => DateTime.Now.Year - DateOfBirth.Year - (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    }

    public class PatientContact
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string ContactType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Relationship { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool IsPrimaryContact { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class PatientMedicalHistory
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Condition { get; set; }
        public string Diagnosis { get; set; }
        public DateTime DiagnosisDate { get; set; }
        public string Treatment { get; set; }
        public string Physician { get; set; }
        public string Hospital { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PatientAllergy
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Allergen { get; set; }
        public string AllergenType { get; set; }
        public string Reaction { get; set; }
        public string Severity { get; set; }
        public DateTime OnsetDate { get; set; }
        public string Treatment { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PatientMedication
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public string Route { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PrescribingPhysician { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PatientVitalSigns
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime RecordedAt { get; set; }
        public double? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? BloodPressureSystolic { get; set; }
        public int? BloodPressureDiastolic { get; set; }
        public int? RespiratoryRate { get; set; }
        public double? OxygenSaturation { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public double? Bmi { get; set; }
        public string Notes { get; set; }
        public string RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PatientInsurance
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string InsuranceProvider { get; set; }
        public string PolicyNumber { get; set; }
        public string GroupNumber { get; set; }
        public string SubscriberName { get; set; }
        public string SubscriberId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CoverageType { get; set; }
        public double? CopayAmount { get; set; }
        public double? DeductibleAmount { get; set; }
        public double? CoinsurancePercentage { get; set; }
        public string PrimaryCarePhysician { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PatientDocument
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string DocumentType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public DateTime DocumentDate { get; set; }
        public string UploadedBy { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PatientNote
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string NoteType { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class PatientAppointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string AppointmentType { get; set; }
        public string Reason { get; set; }
        public string Physician { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public int Duration { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string ConfirmedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Patient Patient { get; set; }
    }

    public class PatientSearchResult
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string NationalId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string InsuranceProvider { get; set; }
        public DateTime LastVisit { get; set; }
        public string Status { get; set; }
        public double RelevanceScore { get; set; }
        public List<string> MatchedFields { get; set; } = new List<string>();
    }
}