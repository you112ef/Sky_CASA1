using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using MedicalLabAnalyzer.Data;
using MedicalLabAnalyzer.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MedicalLabAnalyzer.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly MedicalLabContext _context;
        private readonly Dictionary<string, string> _fallbackUsers;

        public AuthService(ILogger<AuthService> logger = null, MedicalLabContext context = null)
        {
            _logger = logger;
            _context = context;
            
            // SECURITY WARNING: Fallback users with default passwords for emergency access only
            // These should be changed immediately after first login in production
            _fallbackUsers = new Dictionary<string, string>
            {
                { "admin", BCrypt.Net.BCrypt.HashPassword("Admin@2024!") },
                { "emergency", BCrypt.Net.BCrypt.HashPassword("Emergency@2024!") }
            };
            
            _logger?.LogWarning("AuthService initialized with fallback users. Change default passwords immediately in production!");
        }

        public bool AuthenticateUser(string username, string password)
        {
            try
            {
                _logger?.LogInformation("Attempting authentication for user: {Username}", username);

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger?.LogWarning("Authentication failed: Empty username or password");
                    return false;
                }

                // First, try to authenticate against the database
                if (_context != null)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
                    if (user != null)
                    {
                        // Check if account is locked
                        if (user.IsAccountLocked())
                        {
                            _logger?.LogWarning("Authentication failed: Account is locked for user: {Username}", username);
                            return false;
                        }
                        
                        if (user.VerifyPassword(password))
                        {
                            RecordLoginAttempt(username, true);
                            _logger?.LogInformation("Database authentication successful for user: {Username}", username);
                            return true;
                        }
                        else
                        {
                            RecordLoginAttempt(username, false);
                            _logger?.LogWarning("Authentication failed: Invalid password for user: {Username}", username);
                            return false;
                        }
                    }
                }

                // Fallback to emergency users (only if database is unavailable)
                if (_fallbackUsers.ContainsKey(username))
                {
                    string hashedPassword = _fallbackUsers[username];
                    bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);

                    if (isValid)
                    {
                        _logger?.LogWarning("EMERGENCY: Fallback authentication successful for user: {Username}. Change password immediately!", username);
                    }
                    else
                    {
                        _logger?.LogWarning("Authentication failed: Invalid password for user: {Username}", username);
                    }

                    return isValid;
                }

                _logger?.LogWarning("Authentication failed: User not found: {Username}", username);
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during authentication for user: {Username}", username);
                
                // In case of database error, try fallback authentication
                if (_fallbackUsers.ContainsKey(username))
                {
                    string hashedPassword = _fallbackUsers[username];
                    bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                    if (isValid)
                    {
                        _logger?.LogCritical("CRITICAL: Database error - using emergency authentication for: {Username}", username);
                    }
                    return isValid;
                }
                
                return false;
            }
        }

        public bool CreateUser(string username, string password, string role = "User", string fullName = "", string email = "", string department = "")
        {
            try
            {
                _logger?.LogInformation("Creating new user: {Username} with role: {Role}", username, role);

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger?.LogWarning("User creation failed: Empty username or password");
                    return false;
                }

                // Validate password strength
                if (!ValidatePasswordStrength(password))
                {
                    _logger?.LogWarning("User creation failed: Password does not meet strength requirements for user: {Username}", username);
                    return false;
                }

                if (_context != null)
                {
                    // Check if user already exists in database
                    if (_context.Users.Any(u => u.Username == username))
                    {
                        _logger?.LogWarning("User creation failed: User already exists in database: {Username}", username);
                        return false;
                    }

                    var newUser = new User
                    {
                        Username = username,
                        FullName = fullName,
                        Email = email,
                        Role = role,
                        Department = department,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    newUser.SetPassword(password);
                    
                    _context.Users.Add(newUser);
                    _context.SaveChanges();

                    _logger?.LogInformation("User created successfully in database: {Username}", username);
                    return true;
                }
                else
                {
                    _logger?.LogError("Cannot create user: Database context not available");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating user: {Username}", username);
                return false;
            }
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                _logger?.LogInformation("Changing password for user: {Username}", username);

                // Validate new password strength
                if (!ValidatePasswordStrength(newPassword))
                {
                    _logger?.LogWarning("Password change failed: New password does not meet strength requirements for user: {Username}", username);
                    return false;
                }

                if (!AuthenticateUser(username, oldPassword))
                {
                    _logger?.LogWarning("Password change failed: Invalid old password for user: {Username}", username);
                    return false;
                }

                if (_context != null)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
                    if (user != null)
                    {
                        user.SetPassword(newPassword);
                        user.UpdatedAt = DateTime.UtcNow;
                        _context.SaveChanges();
                        
                        _logger?.LogInformation("Password changed successfully in database for user: {Username}", username);
                        return true;
                    }
                }

                // Handle fallback users (emergency access)
                if (_fallbackUsers.ContainsKey(username))
                {
                    _logger?.LogWarning("SECURITY WARNING: Cannot change password for emergency user: {Username}. Use database user instead.", username);
                    return false;
                }

                _logger?.LogWarning("Password change failed: User not found: {Username}", username);
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error changing password for user: {Username}", username);
                return false;
            }
        }

        public bool DeleteUser(string username, string adminUsername, string adminPassword)
        {
            try
            {
                _logger?.LogInformation("Deleting user: {Username}", username);

                // Verify admin authentication
                if (!AuthenticateUser(adminUsername, adminPassword))
                {
                    _logger?.LogWarning("User deletion failed: Invalid admin credentials");
                    return false;
                }

                if (_context != null)
                {
                    // Check if admin has permission
                    var adminUser = _context.Users.FirstOrDefault(u => u.Username == adminUsername && u.IsActive);
                    if (adminUser == null || adminUser.Role != "Admin")
                    {
                        _logger?.LogWarning("User deletion failed: Insufficient permissions for user: {AdminUser}", adminUsername);
                        return false;
                    }

                    var userToDelete = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (userToDelete == null)
                    {
                        _logger?.LogWarning("User deletion failed: User not found in database: {Username}", username);
                        return false;
                    }

                    // Soft delete - don't actually remove, just deactivate
                    userToDelete.IsActive = false;
                    userToDelete.UpdatedAt = DateTime.UtcNow;
                    _context.SaveChanges();

                    _logger?.LogInformation("User deactivated successfully: {Username}", username);
                    return true;
                }
                else
                {
                    _logger?.LogError("Cannot delete user: Database context not available");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting user: {Username}", username);
                return false;
            }
        }

        public List<string> GetAllUsers()
        {
            try
            {
                if (_context != null)
                {
                    return _context.Users.Where(u => u.IsActive).Select(u => u.Username).ToList();
                }
                else
                {
                    return new List<string>(_fallbackUsers.Keys);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving users");
                return new List<string>(_fallbackUsers.Keys);
            }
        }

        public bool UserExists(string username)
        {
            try
            {
                if (_context != null)
                {
                    return _context.Users.Any(u => u.Username == username && u.IsActive);
                }
                else
                {
                    return _fallbackUsers.ContainsKey(username);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if user exists: {Username}", username);
                return _fallbackUsers.ContainsKey(username);
            }
        }

        public User GetUserInfo(string username)
        {
            try
            {
                if (_context != null)
                {
                    return _context.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving user info: {Username}", username);
                return null;
            }
        }

        public bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Minimum requirements
            bool hasMinLength = password.Length >= 8;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            // Log validation results for debugging
            _logger?.LogDebug("Password validation - Length: {HasMinLength}, Upper: {HasUpper}, Lower: {HasLower}, Digit: {HasDigit}, Special: {HasSpecial}", 
                hasMinLength, hasUpper, hasLower, hasDigit, hasSpecial);

            return hasMinLength && hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public void RecordLoginAttempt(string username, bool success, string ipAddress = "")
        {
            try
            {
                _logger?.LogInformation("Recording login attempt for user: {Username}, Success: {Success}, IP: {IpAddress}", 
                    username, success, ipAddress);

                if (_context != null)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        if (success)
                        {
                            user.RecordSuccessfulLogin();
                        }
                        else
                        {
                            user.RecordFailedLogin();
                        }
                        
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording login attempt for user: {Username}", username);
            }
        }

        public bool IsAccountLocked(string username)
        {
            try
            {
                if (_context != null)
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        return user.IsAccountLocked();
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking if account is locked: {Username}", username);
                return false;
            }
        }

        public bool UnlockAccount(string username, string adminUsername, string adminPassword)
        {
            try
            {
                _logger?.LogInformation("Attempting to unlock account: {Username} by admin: {AdminUsername}", username, adminUsername);

                // Verify admin authentication
                if (!AuthenticateUser(adminUsername, adminPassword))
                {
                    _logger?.LogWarning("Account unlock failed: Invalid admin credentials for: {AdminUsername}", adminUsername);
                    return false;
                }

                if (_context != null)
                {
                    // Check if admin has permission
                    var adminUser = _context.Users.FirstOrDefault(u => u.Username == adminUsername && u.IsActive);
                    if (adminUser == null || adminUser.Role != "Admin")
                    {
                        _logger?.LogWarning("Account unlock failed: Insufficient permissions for user: {AdminUsername}", adminUsername);
                        return false;
                    }

                    var userToUnlock = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (userToUnlock == null)
                    {
                        _logger?.LogWarning("Account unlock failed: User not found: {Username}", username);
                        return false;
                    }

                    userToUnlock.UnlockAccount();
                    _context.SaveChanges();

                    _logger?.LogInformation("Account unlocked successfully: {Username} by admin: {AdminUsername}", username, adminUsername);
                    return true;
                }
                else
                {
                    _logger?.LogError("Cannot unlock account: Database context not available");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error unlocking account: {Username}", username);
                return false;
            }
        }
    }
}