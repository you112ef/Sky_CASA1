using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedicalLabAnalyzer.Common.Validation;

namespace MedicalLabAnalyzer.Models
{
    /// <summary>
    /// Patient model with comprehensive medical validation
    /// نموذج المريض مع التحقق الطبي الشامل
    /// </summary>
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        
        [StringLength(20, MinimumLength = 3)]
        [Display(Name = "رقم الملف الطبي - Medical Record Number")]
        public string MRN { get; set; }
        
        [RequiredBilingual]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "الاسم الأول - First Name")]
        public string FirstName { get; set; }
        
        [RequiredBilingual]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "الاسم الأخير - Last Name")]
        public string LastName { get; set; }
        
        /// <summary>
        /// Full name computed from FirstName and LastName
        /// الاسم الكامل محسوب من الاسم الأول والأخير
        /// </summary>
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
        
        [MedicalGender]
        [Display(Name = "الجنس - Gender")]
        public string Gender { get; set; }
        
        [Required]
        [Display(Name = "تاريخ الميلاد - Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        
        /// <summary>
        /// Age computed from DateOfBirth
        /// العمر محسوب من تاريخ الميلاد
        /// </summary>
        [NotMapped]
        [MedicalAge(0, 150)]
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth.Date > today.AddYears(-age))
                    age--;
                return age;
            }
        }
        
        /// <summary>
        /// Date of Birth as string for compatibility (deprecated)
        /// تاريخ الميلاد كنص للتوافق (مهمل)
        /// </summary>
        [Obsolete("Use DateOfBirth property instead")]
        public string DOB 
        { 
            get => DateOfBirth.ToString("yyyy-MM-dd");
            set => DateOfBirth = DateTime.TryParse(value, out var date) ? date : default;
        }
        
        [SaudiPhone]
        [Display(Name = "رقم الهاتف - Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني - Email Address")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        
        [SaudiNationalId]
        [Display(Name = "رقم الهوية الوطنية - National ID")]
        public string NationalId { get; set; }
        
        [StringLength(200)]
        [Display(Name = "العنوان - Address")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        
        [StringLength(500)]
        [Display(Name = "ملاحظات - Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }
        
        [Display(Name = "تاريخ الإنشاء - Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "تاريخ التحديث - Updated Date")]
        public DateTime? UpdatedDate { get; set; }
        
        [Display(Name = "المستخدم المنشئ - Created By")]
        public string CreatedBy { get; set; }
        
        [Display(Name = "المستخدم المحدث - Updated By")]
        public string UpdatedBy { get; set; }
        
        [Display(Name = "نشط - Is Active")]
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Emergency contact information
        /// معلومات جهة الاتصال في حالات الطوارئ
        /// </summary>
        [StringLength(100)]
        [Display(Name = "اسم جهة الاتصال للطوارئ - Emergency Contact Name")]
        public string EmergencyContactName { get; set; }
        
        [SaudiPhone]
        [Display(Name = "هاتف جهة الاتصال للطوارئ - Emergency Contact Phone")]
        public string EmergencyContactPhone { get; set; }
        
        [StringLength(50)]
        [Display(Name = "علاقة جهة الاتصال للطوارئ - Emergency Contact Relationship")]
        public string EmergencyContactRelationship { get; set; }
        
        /// <summary>
        /// Medical history flags
        /// أعلام التاريخ الطبي
        /// </summary>
        [Display(Name = "لديه تاريخ مرضي - Has Medical History")]
        public bool HasMedicalHistory { get; set; }
        
        [Display(Name = "لديه حساسية - Has Allergies")]
        public bool HasAllergies { get; set; }
        
        [StringLength(500)]
        [Display(Name = "تفاصيل الحساسية - Allergy Details")]
        [DataType(DataType.MultilineText)]
        public string AllergyDetails { get; set; }
        
        [Display(Name = "يتناول أدوية - Takes Medications")]
        public bool TakesMedications { get; set; }
        
        [StringLength(500)]
        [Display(Name = "تفاصيل الأدوية - Medication Details")]
        [DataType(DataType.MultilineText)]
        public string MedicationDetails { get; set; }
        
        /// <summary>
        /// Insurance information
        /// معلومات التأمين
        /// </summary>
        [StringLength(100)]
        [Display(Name = "شركة التأمين - Insurance Company")]
        public string InsuranceCompany { get; set; }
        
        [StringLength(50)]
        [Display(Name = "رقم بوليصة التأمين - Insurance Policy Number")]
        public string InsurancePolicyNumber { get; set; }
        
        [Display(Name = "تاريخ انتهاء التأمين - Insurance Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? InsuranceExpiryDate { get; set; }
        
        /// <summary>
        /// Navigation properties for related exams
        /// خصائص التنقل للفحوصات المرتبطة
        /// </summary>
        public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
        
        /// <summary>
        /// Get gender display name in Arabic and English
        /// احصل على اسم الجنس باللغة العربية والإنجليزية
        /// </summary>
        public string GenderDisplay => Gender?.ToUpper() switch
        {
            "M" or "MALE" => "ذكر - Male",
            "F" or "FEMALE" => "أنثى - Female",
            "ذكر" => "ذكر - Male",
            "أنثى" => "أنثى - Female",
            _ => Gender
        };
        
        /// <summary>
        /// Get formatted full name with MRN
        /// احصل على الاسم الكامل المنسق مع رقم الملف الطبي
        /// </summary>
        public string DisplayName => string.IsNullOrWhiteSpace(MRN) ? FullName : $"{FullName} ({MRN})";
        
        /// <summary>
        /// Check if patient data is complete for medical exams
        /// تحقق من اكتمال بيانات المريض للفحوصات الطبية
        /// </summary>
        public bool IsDataComplete => 
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(Gender) &&
            DateOfBirth != default &&
            Age >= 0 && Age <= 150;
            
        /// <summary>
        /// Get patient summary for reports
        /// احصل على ملخص المريض للتقارير
        /// </summary>
        public string GetSummary()
        {
            return $"{DisplayName}, {GenderDisplay}, العمر: {Age} سنة - Age: {Age} years";
        }
        
        public override string ToString()
        {
            return DisplayName;
        }
    }
}