using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace MedicalLabAnalyzer.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = "User"; // Admin, Doctor, Technician, User

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Exam> AssignedExams { get; set; } = new List<Exam>();

        // Computed properties
        [NotMapped]
        public bool IsAdmin => Role == "Admin";

        [NotMapped]
        public bool IsDoctor => Role == "Doctor";

        [NotMapped]
        public bool IsTechnician => Role == "Technician";

        // Security methods
        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty");

            PasswordHash = HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return PasswordHash == HashPassword(password);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public bool HasPermission(string permission)
        {
            return Role switch
            {
                "Admin" => true,
                "Doctor" => permission is "ViewPatients" or "ViewExams" or "EditExams" or "ViewReports",
                "Technician" => permission is "ViewPatients" or "ViewExams" or "EditExams" or "VideoAnalysis",
                "User" => permission is "ViewPatients" or "ViewExams",
                _ => false
            };
        }

        // Validation methods
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(PasswordHash) &&
                   (Role == "Admin" || Role == "Doctor" || Role == "Technician" || Role == "User");
        }

        public override string ToString()
        {
            return $"{FullName} ({Username}) - {Role}";
        }
    }
}