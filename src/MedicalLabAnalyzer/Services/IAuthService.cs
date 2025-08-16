using MedicalLabAnalyzer.Models;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plain text password</param>
        /// <returns>True if authentication successful</returns>
        bool AuthenticateUser(string username, string password);
        
        /// <summary>
        /// Create a new user account
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plain text password</param>
        /// <param name="role">User role</param>
        /// <param name="fullName">Full name</param>
        /// <param name="email">Email address</param>
        /// <param name="department">Department</param>
        /// <returns>True if user created successfully</returns>
        bool CreateUser(string username, string password, string role = "User", string fullName = "", string email = "", string department = "");
        
        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="oldPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password changed successfully</returns>
        bool ChangePassword(string username, string oldPassword, string newPassword);
        
        /// <summary>
        /// Delete/Deactivate a user (soft delete)
        /// </summary>
        /// <param name="username">Username to delete</param>
        /// <param name="adminUsername">Admin username performing the action</param>
        /// <param name="adminPassword">Admin password</param>
        /// <returns>True if user deactivated successfully</returns>
        bool DeleteUser(string username, string adminUsername, string adminPassword);
        
        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns>List of usernames</returns>
        List<string> GetAllUsers();
        
        /// <summary>
        /// Check if a user exists and is active
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if user exists and is active</returns>
        bool UserExists(string username);
        
        /// <summary>
        /// Get detailed user information
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User object or null if not found</returns>
        User GetUserInfo(string username);
        
        /// <summary>
        /// Validate password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets requirements</returns>
        bool ValidatePasswordStrength(string password);
        
        /// <summary>
        /// Record login attempt
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="success">Whether login was successful</param>
        /// <param name="ipAddress">IP address of login attempt</param>
        void RecordLoginAttempt(string username, bool success, string ipAddress = "");
        
        /// <summary>
        /// Check if user account is locked
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>True if account is locked</returns>
        bool IsAccountLocked(string username);
        
        /// <summary>
        /// Unlock user account
        /// </summary>
        /// <param name="username">Username to unlock</param>
        /// <param name="adminUsername">Admin performing the action</param>
        /// <param name="adminPassword">Admin password</param>
        /// <returns>True if account unlocked successfully</returns>
        bool UnlockAccount(string username, string adminUsername, string adminPassword);
    }
}