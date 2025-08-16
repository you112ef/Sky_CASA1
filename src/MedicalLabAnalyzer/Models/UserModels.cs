using System;
using System.Collections.Generic;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; }
        
        [StringLength(100)]
        public string FirstName { get; set; }
        
        [StringLength(100)]
        public string LastName { get; set; }
        
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [StringLength(50)]
        public string Role { get; set; } = "User";
        
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }
        
        [StringLength(100)]
        public string Department { get; set; }
        
        [StringLength(100)]
        public string Position { get; set; }
        
        [StringLength(50)]
        public string EmployeeId { get; set; }
        
        public string ProfileImagePath { get; set; }
        public Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();
        public List<string> Permissions { get; set; } = new List<string>();
        
        // Security fields
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? AccountLockedAt { get; set; }
        public DateTime? AccountLockedUntil { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public bool RequirePasswordChange { get; set; } = false;
        
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        // Password methods
        public void SetPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");
                
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, 12); // Use cost factor 12 for security
            PasswordChangedAt = DateTime.UtcNow;
            RequirePasswordChange = false;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(PasswordHash))
                return false;
                
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
            }
            catch
            {
                return false;
            }
        }
        
        public bool IsAccountLocked()
        {
            return AccountLockedUntil.HasValue && AccountLockedUntil > DateTime.UtcNow;
        }
        
        public void LockAccount(int lockoutMinutes = 30, string reason = "Too many failed login attempts")
        {
            AccountLockedAt = DateTime.UtcNow;
            AccountLockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void UnlockAccount()
        {
            AccountLockedAt = null;
            AccountLockedUntil = null;
            FailedLoginAttempts = 0;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;
            UpdatedAt = DateTime.UtcNow;
            
            // Lock account after 5 failed attempts
            if (FailedLoginAttempts >= 5)
            {
                LockAccount();
            }
        }
        
        public void RecordSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public int Priority { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
    }

    public class UserSession
    {
        public string SessionId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public Dictionary<string, object> SessionData { get; set; } = new Dictionary<string, object>();
    }

    public class UserActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Module { get; set; }
        public string Result { get; set; }
        public int Duration { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class UserProfile
    {
        public int UserId { get; set; }
        public string ProfileImagePath { get; set; }
        public string Bio { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public List<string> Certifications { get; set; } = new List<string>();
        public Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();
        public DateTime LastUpdated { get; set; }
    }

    public class UserPermission
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Module { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Priority { get; set; }
        public string Category { get; set; }
    }

    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
        public Role Role { get; set; }
        public User User { get; set; }
    }

    public class UserGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public List<User> Members { get; set; } = new List<User>();
        public List<string> Permissions { get; set; } = new List<string>();
        public string Category { get; set; }
        public int MaxMembers { get; set; }
        public string Color { get; set; }
        public string Icon { get; set; }
    }

    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
        public string ActionUrl { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public DateTime? ExpiresAt { get; set; }
    }

    public class UserPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public List<string> ValidValues { get; set; } = new List<string>();
    }

    public class UserSecurity
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime PasswordChangedAt { get; set; }
        public DateTime? PasswordExpiresAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LastFailedLoginAt { get; set; }
        public DateTime? AccountLockedAt { get; set; }
        public DateTime? AccountLockedUntil { get; set; }
        public string LockReason { get; set; }
        public bool RequirePasswordChange { get; set; }
        public List<string> SecurityQuestions { get; set; } = new List<string>();
        public List<string> SecurityAnswers { get; set; } = new List<string>();
        public List<string> TwoFactorMethods { get; set; } = new List<string>();
        public bool TwoFactorEnabled { get; set; }
        public string BackupCodes { get; set; }
        public DateTime LastSecurityReview { get; set; }
    }
}