using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace MedicalLabAnalyzer.Services
{
    public class AuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly Dictionary<string, string> _users;

        public AuthService(ILogger<AuthService> logger = null)
        {
            _logger = logger;
            
            // Initialize with default users (in production, this would come from database)
            _users = new Dictionary<string, string>
            {
                { "admin", BCrypt.Net.BCrypt.HashPassword("admin123") },
                { "doctor", BCrypt.Net.BCrypt.HashPassword("doctor123") },
                { "technician", BCrypt.Net.BCrypt.HashPassword("tech123") }
            };
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

                if (!_users.ContainsKey(username))
                {
                    _logger?.LogWarning("Authentication failed: User not found: {Username}", username);
                    return false;
                }

                string hashedPassword = _users[username];
                bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);

                if (isValid)
                {
                    _logger?.LogInformation("Authentication successful for user: {Username}", username);
                }
                else
                {
                    _logger?.LogWarning("Authentication failed: Invalid password for user: {Username}", username);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during authentication for user: {Username}", username);
                return false;
            }
        }

        public bool CreateUser(string username, string password, string role = "User")
        {
            try
            {
                _logger?.LogInformation("Creating new user: {Username} with role: {Role}", username, role);

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _logger?.LogWarning("User creation failed: Empty username or password");
                    return false;
                }

                if (_users.ContainsKey(username))
                {
                    _logger?.LogWarning("User creation failed: User already exists: {Username}", username);
                    return false;
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                _users[username] = hashedPassword;

                _logger?.LogInformation("User created successfully: {Username}", username);
                return true;
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

                if (!AuthenticateUser(username, oldPassword))
                {
                    _logger?.LogWarning("Password change failed: Invalid old password for user: {Username}", username);
                    return false;
                }

                string hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _users[username] = hashedNewPassword;

                _logger?.LogInformation("Password changed successfully for user: {Username}", username);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error changing password for user: {Username}", username);
                return false;
            }
        }

        public bool DeleteUser(string username, string adminPassword)
        {
            try
            {
                _logger?.LogInformation("Deleting user: {Username}", username);

                // Verify admin password
                if (!AuthenticateUser("admin", adminPassword))
                {
                    _logger?.LogWarning("User deletion failed: Invalid admin password");
                    return false;
                }

                if (!_users.ContainsKey(username))
                {
                    _logger?.LogWarning("User deletion failed: User not found: {Username}", username);
                    return false;
                }

                _users.Remove(username);
                _logger?.LogInformation("User deleted successfully: {Username}", username);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting user: {Username}", username);
                return false;
            }
        }

        public List<string> GetAllUsers()
        {
            return new List<string>(_users.Keys);
        }

        public bool UserExists(string username)
        {
            return _users.ContainsKey(username);
        }
    }
}