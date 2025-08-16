using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq; // Added for .ToList()
using MedicalLabAnalyzer.Models; // Added for User model
using MedicalLabAnalyzer.Data; // Added for _db

namespace MedicalLabAnalyzer.Services
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IDbService _db; // Added for database access

        public UserService(ILogger<UserService> logger = null, IDbService db = null)
        {
            _logger = logger;
            _db = db; // Initialize _db
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger?.LogInformation("Retrieving user with ID: {UserId}", userId);
                
                var sql = "SELECT * FROM Users WHERE Id = @UserId";
                var user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
                
                if (user != null)
                {
                    _logger?.LogInformation("User retrieved successfully: {UserId}", userId);
                }
                else
                {
                    _logger?.LogWarning("User not found: {UserId}", userId);
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                _logger?.LogInformation("Retrieving all users");
                
                var sql = "SELECT * FROM Users ORDER BY LastName, FirstName";
                var users = await _db.QueryAsync<User>(sql);
                
                _logger?.LogInformation("Retrieved {Count} users", users.Count());
                return users.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all users");
                throw;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                _logger?.LogInformation("Creating new user: {FirstName} {LastName}", user.FirstName, user.LastName);
                
                var sql = @"
                    INSERT INTO Users (Username, FirstName, LastName, Email, PasswordHash, RoleId, IsActive, CreatedAt, UpdatedAt)
                    VALUES (@Username, @FirstName, @LastName, @Email, @PasswordHash, @RoleId, @IsActive, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";
                
                var userId = await _db.QuerySingleAsync<int>(sql, user);
                user.Id = userId;
                
                _logger?.LogInformation("User created successfully with ID: {UserId}", userId);
                return user;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating user: {FirstName} {LastName}", user.FirstName, user.LastName);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _logger?.LogInformation("Updating user: {UserId}", user.Id);
                
                var sql = @"
                    UPDATE Users 
                    SET Username = @Username, FirstName = @FirstName, LastName = @LastName, 
                        Email = @Email, RoleId = @RoleId, IsActive = @IsActive, UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
                
                var rowsAffected = await _db.ExecuteAsync(sql, user);
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("User updated successfully: {UserId}", user.Id);
                }
                else
                {
                    _logger?.LogWarning("No user found to update: {UserId}", user.Id);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                _logger?.LogInformation("Deleting user: {UserId}", userId);
                
                var sql = "DELETE FROM Users WHERE Id = @UserId";
                var rowsAffected = await _db.ExecuteAsync(sql, new { UserId = userId });
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("User deleted successfully: {UserId}", userId);
                }
                else
                {
                    _logger?.LogWarning("No user found to delete: {UserId}", userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting user: {UserId}", userId);
                throw;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                _logger?.LogInformation("Retrieving user by username: {Username}", username);
                
                var sql = "SELECT * FROM Users WHERE Username = @Username";
                var user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
                
                if (user != null)
                {
                    _logger?.LogInformation("User retrieved successfully by username: {Username}", username);
                }
                else
                {
                    _logger?.LogWarning("User not found by username: {Username}", username);
                }
                
                return user;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving user by username: {Username}", username);
                throw;
            }
        }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLogin { get; set; }
    }
}