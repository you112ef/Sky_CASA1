using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MedicalLabAnalyzer.Models;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Common.Exceptions;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Common.ErrorHandling;

namespace MedicalLabAnalyzer.Services
{
    public enum Role { Admin, LabTech, Reception }

    public interface IAuthService
    {
        Task<Result<User>> AuthenticateAsync(string username, string password);
        Result<User> Authenticate(string username, string password);
        void Logout();
        User CurrentUser { get; }
        bool HasAccess(Role required);
        Task<Result> ChangePasswordAsync(string username, string oldPassword, string newPassword);
        Task<Result<User>> CreateUserAsync(string username, string password, string fullName, Role role);
        Task<Result> UpdateUserAsync(User user);
        Task<Result> DeleteUserAsync(string username);
        Task<Result> InitializeDefaultUsersAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IDatabaseService _db;
        private readonly ILogger<AuthService> _logger;
        private readonly IErrorHandlerService _errorHandler;
        private User _currentUser;

        public AuthService(IDatabaseService db, ILogger<AuthService> logger, IErrorHandlerService errorHandler = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger;
            _errorHandler = errorHandler;
            
            // Initialize default users asynchronously in background
            Task.Run(async () => await InitializeDefaultUsersAsync());
        }

        public async Task<Result<User>> AuthenticateAsync(string username, string password)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new AuthenticationException("اسم المستخدم مطلوب", username, "LOGIN");
                
                if (string.IsNullOrWhiteSpace(password))
                    throw new AuthenticationException("كلمة المرور مطلوبة", username, "LOGIN");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new AuthenticationException($"خطأ في الاتصال بقاعدة البيانات: {connResult.ErrorMessage}", username, "LOGIN");

                using var conn = connResult.Value;
                
                var user = conn.QuerySingleOrDefault<User>(
                    "SELECT u.*, r.RoleName as RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE Username=@Username", 
                    new { Username = username });
                
                if (user == null)
                {
                    _logger?.LogWarning("Authentication failed - user not found: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger?.LogWarning("Authentication failed - invalid password for user: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                _currentUser = user;
                _logger?.LogInformation("User authenticated successfully: {Username} - {FullName}", username, user.FullName);
                
                return user;
            }, "AuthenticateAsync", new { Username = username }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new AuthenticationException("اسم المستخدم مطلوب", username, "LOGIN");
                
                if (string.IsNullOrWhiteSpace(password))
                    throw new AuthenticationException("كلمة المرور مطلوبة", username, "LOGIN");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new AuthenticationException($"خطأ في الاتصال بقاعدة البيانات: {connResult.ErrorMessage}", username, "LOGIN");

                using var conn = connResult.Value;
                
                var user = conn.QuerySingleOrDefault<User>(
                    "SELECT u.*, r.RoleName as RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE Username=@Username", 
                    new { Username = username });
                
                if (user == null)
                {
                    _logger?.LogWarning("Authentication failed - user not found: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger?.LogWarning("Authentication failed - invalid password for user: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                _currentUser = user;
                _logger?.LogInformation("User authenticated successfully: {Username} - {FullName}", username, user.FullName);
                
                return user;
            }, ex => $"Authentication failed: {ex.Message}", "AUTH_FAILED"));
        }

        public Result<User> Authenticate(string username, string password)
        {
            return _errorHandler?.Handle(() =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new AuthenticationException("اسم المستخدم مطلوب", username, "LOGIN");
                
                if (string.IsNullOrWhiteSpace(password))
                    throw new AuthenticationException("كلمة المرور مطلوبة", username, "LOGIN");

                var connResult = _db.GetConnection();
                if (connResult.IsFailure)
                    throw new AuthenticationException($"خطأ في الاتصال بقاعدة البيانات: {connResult.ErrorMessage}", username, "LOGIN");

                using var conn = connResult.Value;
                
                var user = conn.QuerySingleOrDefault<User>(
                    "SELECT u.*, r.RoleName as RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE Username=@Username", 
                    new { Username = username });
                
                if (user == null)
                {
                    _logger?.LogWarning("Authentication failed - user not found: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger?.LogWarning("Authentication failed - invalid password for user: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                _currentUser = user;
                _logger?.LogInformation("User authenticated successfully: {Username} - {FullName}", username, user.FullName);
                
                return user;
            }, "Authenticate", new { Username = username }) ??
            Result.TryCatch(() =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new AuthenticationException("اسم المستخدم مطلوب", username, "LOGIN");
                
                if (string.IsNullOrWhiteSpace(password))
                    throw new AuthenticationException("كلمة المرور مطلوبة", username, "LOGIN");

                var connResult = _db.GetConnection();
                if (connResult.IsFailure)
                    throw new AuthenticationException($"خطأ في الاتصال بقاعدة البيانات: {connResult.ErrorMessage}", username, "LOGIN");

                using var conn = connResult.Value;
                
                var user = conn.QuerySingleOrDefault<User>(
                    "SELECT u.*, r.RoleName as RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE Username=@Username", 
                    new { Username = username });
                
                if (user == null)
                {
                    _logger?.LogWarning("Authentication failed - user not found: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger?.LogWarning("Authentication failed - invalid password for user: {Username}", username);
                    throw new AuthenticationException("اسم المستخدم أو كلمة المرور غير صحيحة", username, "LOGIN");
                }
                
                _currentUser = user;
                _logger?.LogInformation("User authenticated successfully: {Username} - {FullName}", username, user.FullName);
                
                return user;
            }, ex => $"Authentication failed: {ex.Message}", "AUTH_FAILED");
        }

        public void Logout() 
        {
            var username = _currentUser?.Username;
            _currentUser = null;
            _logger?.LogInformation("User logged out: {Username}", username);
        }

        public User CurrentUser => _currentUser;

        public bool HasAccess(Role required)
        {
            try
            {
                if (_currentUser == null) 
                {
                    _logger?.LogWarning("Access denied - no authenticated user for role: {RequiredRole}", required);
                    return false;
                }
                
                var roleName = _currentUser.RoleName ?? GetRoleNameById(_currentUser.RoleId);
                if (string.IsNullOrEmpty(roleName))
                {
                    _logger?.LogWarning("Access denied - unable to determine role for user: {Username}", _currentUser.Username);
                    return false;
                }
                
                // Admin has access to everything
                if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return true;
                
                var hasAccess = required switch
                {
                    Role.Admin => roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                    Role.LabTech => roleName.Equals("LabTech", StringComparison.OrdinalIgnoreCase) || 
                                   roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                    Role.Reception => roleName.Equals("Reception", StringComparison.OrdinalIgnoreCase) || 
                                     roleName.Equals("LabTech", StringComparison.OrdinalIgnoreCase) || 
                                     roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
                
                if (!hasAccess)
                {
                    _logger?.LogWarning("Access denied - user {Username} with role {UserRole} does not have {RequiredRole} access", 
                        _currentUser.Username, roleName, required);
                }
                
                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking access for user {Username} and role {RequiredRole}", 
                    _currentUser?.Username, required);
                return false;
            }
        }

        private string GetRoleNameById(int roleId)
        {
            try
            {
                var connResult = _db.GetConnection();
                if (connResult.IsFailure)
                {
                    _logger?.LogError("Failed to get database connection for role lookup: {Error}", connResult.ErrorMessage);
                    return null;
                }

                using var conn = connResult.Value;
                return conn.QuerySingleOrDefault<string>("SELECT RoleName FROM Roles WHERE RoleId=@RoleId", new { RoleId = roleId });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting role name for RoleId: {RoleId}", roleId);
                return null;
            }
        }

        public async Task<Result> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new AuthenticationException("اسم المستخدم مطلوب", username, "CHANGE_PASSWORD");
                
                if (string.IsNullOrWhiteSpace(oldPassword))
                    throw new AuthenticationException("كلمة المرور القديمة مطلوبة", username, "CHANGE_PASSWORD");
                
                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new AuthenticationException("كلمة المرور الجديدة مطلوبة", username, "CHANGE_PASSWORD");
                
                if (newPassword.Length < 6)
                    throw new ValidationException("كلمة المرور يجب أن تكون 6 أحرف على الأقل", new[] { "Password too short" }, "PASSWORD");

                // First authenticate with old password
                var authResult = await AuthenticateAsync(username, oldPassword);
                if (authResult.IsFailure)
                    throw new AuthenticationException("كلمة المرور القديمة غير صحيحة", username, "CHANGE_PASSWORD");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "CHANGE_PASSWORD");

                using var conn = connResult.Value;
                
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var rowsAffected = conn.Execute(
                    "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = @Username",
                    new { PasswordHash = newPasswordHash, Username = username });
                
                if (rowsAffected == 0)
                    throw new AuthenticationException("فشل في تحديث كلمة المرور", username, "CHANGE_PASSWORD");
                
                _logger?.LogInformation("Password changed successfully for user: {Username}", username);
            }, "ChangePasswordAsync", new { Username = username }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new AuthenticationException("اسم المستخدم مطلوب", username, "CHANGE_PASSWORD");
                
                if (string.IsNullOrWhiteSpace(oldPassword))
                    throw new AuthenticationException("كلمة المرور القديمة مطلوبة", username, "CHANGE_PASSWORD");
                
                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new AuthenticationException("كلمة المرور الجديدة مطلوبة", username, "CHANGE_PASSWORD");
                
                if (newPassword.Length < 6)
                    throw new ValidationException("كلمة المرور يجب أن تكون 6 أحرف على الأقل", new[] { "Password too short" }, "PASSWORD");

                // First authenticate with old password
                var authResult = await AuthenticateAsync(username, oldPassword);
                if (authResult.IsFailure)
                    throw new AuthenticationException("كلمة المرور القديمة غير صحيحة", username, "CHANGE_PASSWORD");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "CHANGE_PASSWORD");

                using var conn = connResult.Value;
                
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var rowsAffected = conn.Execute(
                    "UPDATE Users SET PasswordHash = @PasswordHash WHERE Username = @Username",
                    new { PasswordHash = newPasswordHash, Username = username });
                
                if (rowsAffected == 0)
                    throw new AuthenticationException("فشل في تحديث كلمة المرور", username, "CHANGE_PASSWORD");
                
                _logger?.LogInformation("Password changed successfully for user: {Username}", username);
            }, ex => $"Failed to change password: {ex.Message}", "CHANGE_PASSWORD_FAILED"));
        }

        public async Task<Result<User>> CreateUserAsync(string username, string password, string fullName, Role role)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ValidationException("اسم المستخدم مطلوب", new[] { "Username is required" }, "USER");
                
                if (string.IsNullOrWhiteSpace(password))
                    throw new ValidationException("كلمة المرور مطلوبة", new[] { "Password is required" }, "USER");
                
                if (string.IsNullOrWhiteSpace(fullName))
                    throw new ValidationException("الاسم الكامل مطلوب", new[] { "Full name is required" }, "USER");
                
                if (password.Length < 6)
                    throw new ValidationException("كلمة المرور يجب أن تكون 6 أحرف على الأقل", new[] { "Password too short" }, "USER");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "CREATE_USER");

                using var conn = connResult.Value;
                
                // Check if user already exists
                var existingUser = conn.QuerySingleOrDefault<User>(
                    "SELECT * FROM Users WHERE Username = @Username", 
                    new { Username = username });
                
                if (existingUser != null)
                    throw new ValidationException($"المستخدم {username} موجود بالفعل", new[] { "Username already exists" }, "USER");
                
                var roleId = (int)role + 1; // Assuming roles start from 1
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                
                var userId = conn.QuerySingle<int>(
                    "INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username, @PasswordHash, @FullName, @RoleId); SELECT last_insert_rowid();",
                    new { Username = username, PasswordHash = passwordHash, FullName = fullName, RoleId = roleId });
                
                var newUser = conn.QuerySingle<User>(
                    "SELECT u.*, r.RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE u.UserId = @UserId",
                    new { UserId = userId });
                
                _logger?.LogInformation("User created successfully: {Username} - {FullName} with role {Role}", username, fullName, role);
                
                return newUser;
            }, "CreateUserAsync", new { Username = username, Role = role }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ValidationException("اسم المستخدم مطلوب", new[] { "Username is required" }, "USER");
                
                if (string.IsNullOrWhiteSpace(password))
                    throw new ValidationException("كلمة المرور مطلوبة", new[] { "Password is required" }, "USER");
                
                if (string.IsNullOrWhiteSpace(fullName))
                    throw new ValidationException("الاسم الكامل مطلوب", new[] { "Full name is required" }, "USER");
                
                if (password.Length < 6)
                    throw new ValidationException("كلمة المرور يجب أن تكون 6 أحرف على الأقل", new[] { "Password too short" }, "USER");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "CREATE_USER");

                using var conn = connResult.Value;
                
                // Check if user already exists
                var existingUser = conn.QuerySingleOrDefault<User>(
                    "SELECT * FROM Users WHERE Username = @Username", 
                    new { Username = username });
                
                if (existingUser != null)
                    throw new ValidationException($"المستخدم {username} موجود بالفعل", new[] { "Username already exists" }, "USER");
                
                var roleId = (int)role + 1; // Assuming roles start from 1
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                
                var userId = conn.QuerySingle<int>(
                    "INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username, @PasswordHash, @FullName, @RoleId); SELECT last_insert_rowid();",
                    new { Username = username, PasswordHash = passwordHash, FullName = fullName, RoleId = roleId });
                
                var newUser = conn.QuerySingle<User>(
                    "SELECT u.*, r.RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE u.UserId = @UserId",
                    new { UserId = userId });
                
                _logger?.LogInformation("User created successfully: {Username} - {FullName} with role {Role}", username, fullName, role);
                
                return newUser;
            }, ex => $"Failed to create user: {ex.Message}", "CREATE_USER_FAILED"));
        }

        public async Task<Result> UpdateUserAsync(User user)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                
                if (string.IsNullOrWhiteSpace(user.Username))
                    throw new ValidationException("اسم المستخدم مطلوب", new[] { "Username is required" }, "USER");
                
                if (string.IsNullOrWhiteSpace(user.FullName))
                    throw new ValidationException("الاسم الكامل مطلوب", new[] { "Full name is required" }, "USER");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "UPDATE_USER");

                using var conn = connResult.Value;
                
                var rowsAffected = conn.Execute(
                    "UPDATE Users SET FullName = @FullName, RoleId = @RoleId WHERE Username = @Username",
                    new { FullName = user.FullName, RoleId = user.RoleId, Username = user.Username });
                
                if (rowsAffected == 0)
                    throw new ValidationException($"المستخدم {user.Username} غير موجود", new[] { "User not found" }, "USER");
                
                _logger?.LogInformation("User updated successfully: {Username} - {FullName}", user.Username, user.FullName);
            }, "UpdateUserAsync", new { Username = user?.Username }) ??
            Result.TryCatchAsync(async () =>
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user), "User cannot be null");
                
                if (string.IsNullOrWhiteSpace(user.Username))
                    throw new ValidationException("اسم المستخدم مطلوب", new[] { "Username is required" }, "USER");
                
                if (string.IsNullOrWhiteSpace(user.FullName))
                    throw new ValidationException("الاسم الكامل مطلوب", new[] { "Full name is required" }, "USER");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "UPDATE_USER");

                using var conn = connResult.Value;
                
                var rowsAffected = conn.Execute(
                    "UPDATE Users SET FullName = @FullName, RoleId = @RoleId WHERE Username = @Username",
                    new { FullName = user.FullName, RoleId = user.RoleId, Username = user.Username });
                
                if (rowsAffected == 0)
                    throw new ValidationException($"المستخدم {user.Username} غير موجود", new[] { "User not found" }, "USER");
                
                _logger?.LogInformation("User updated successfully: {Username} - {FullName}", user.Username, user.FullName);
            }, ex => $"Failed to update user: {ex.Message}", "UPDATE_USER_FAILED"));
        }

        public async Task<Result> DeleteUserAsync(string username)
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("اسم المستخدم مطلوب", nameof(username));
                
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    throw new ValidationException("لا يمكن حذف المستخدم الإداري الرئيسي", new[] { "Cannot delete admin user" }, "USER");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "DELETE_USER");

                using var conn = connResult.Value;
                
                var rowsAffected = conn.Execute(
                    "DELETE FROM Users WHERE Username = @Username",
                    new { Username = username });
                
                if (rowsAffected == 0)
                    throw new ValidationException($"المستخدم {username} غير موجود", new[] { "User not found" }, "USER");
                
                _logger?.LogInformation("User deleted successfully: {Username}", username);
            }, "DeleteUserAsync", new { Username = username }) ??
            Result.TryCatchAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("اسم المستخدم مطلوب", nameof(username));
                
                if (username.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    throw new ValidationException("لا يمكن حذف المستخدم الإداري الرئيسي", new[] { "Cannot delete admin user" }, "USER");

                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                    throw new DatabaseException(connResult.ErrorMessage, "DELETE_USER");

                using var conn = connResult.Value;
                
                var rowsAffected = conn.Execute(
                    "DELETE FROM Users WHERE Username = @Username",
                    new { Username = username });
                
                if (rowsAffected == 0)
                    throw new ValidationException($"المستخدم {username} غير موجود", new[] { "User not found" }, "USER");
                
                _logger?.LogInformation("User deleted successfully: {Username}", username);
            }, ex => $"Failed to delete user: {ex.Message}", "DELETE_USER_FAILED"));
        }

        public async Task<Result> InitializeDefaultUsersAsync()
        {
            return await (_errorHandler?.HandleAsync(async () =>
            {
                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                {
                    _logger?.LogError("Failed to get database connection for user initialization: {Error}", connResult.ErrorMessage);
                    throw new DatabaseException(connResult.ErrorMessage, "INIT_USERS");
                }

                using var conn = connResult.Value;
                
                // Create admin user if not exists
                var admin = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='admin'");
                if (admin == null)
                {
                    var pwHash = BCrypt.Net.BCrypt.HashPassword("MedLab2024!");
                    conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                        new { Username = "admin", PasswordHash = pwHash, FullName = "System Administrator - مدير النظام", RoleId = 1 });
                    _logger.LogInformation("Created default admin user: admin / MedLab2024!");
                }
                
                // Create lab technician user
                var labTech = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='lab'");
                if (labTech == null)
                {
                    var pwHash = BCrypt.Net.BCrypt.HashPassword("Lab123!");
                    conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                        new { Username = "lab", PasswordHash = pwHash, FullName = "Lab Technician - فني المختبر", RoleId = 2 });
                    _logger.LogInformation("Created lab technician user: lab / Lab123!");
                }
                
                // Create reception user
                var reception = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='reception'");
                if (reception == null)
                {
                    var pwHash = BCrypt.Net.BCrypt.HashPassword("Reception123!");
                    conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                        new { Username = "reception", PasswordHash = pwHash, FullName = "Reception Staff - موظف الاستقبال", RoleId = 3 });
                    _logger.LogInformation("Created reception user: reception / Reception123!");
                }
                
                _logger?.LogInformation("Default users initialization completed");
            }, "InitializeDefaultUsersAsync") ??
            Result.TryCatchAsync(async () =>
            {
                var connResult = await _db.GetConnectionAsync();
                if (connResult.IsFailure)
                {
                    _logger?.LogError("Failed to get database connection for user initialization: {Error}", connResult.ErrorMessage);
                    throw new DatabaseException(connResult.ErrorMessage, "INIT_USERS");
                }

                using var conn = connResult.Value;
                
                // Create admin user if not exists
                var admin = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='admin'");
                if (admin == null)
                {
                    var pwHash = BCrypt.Net.BCrypt.HashPassword("MedLab2024!");
                    conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                        new { Username = "admin", PasswordHash = pwHash, FullName = "System Administrator - مدير النظام", RoleId = 1 });
                    _logger.LogInformation("Created default admin user: admin / MedLab2024!");
                }
                
                // Create lab technician user
                var labTech = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='lab'");
                if (labTech == null)
                {
                    var pwHash = BCrypt.Net.BCrypt.HashPassword("Lab123!");
                    conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                        new { Username = "lab", PasswordHash = pwHash, FullName = "Lab Technician - فني المختبر", RoleId = 2 });
                    _logger.LogInformation("Created lab technician user: lab / Lab123!");
                }
                
                // Create reception user
                var reception = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='reception'");
                if (reception == null)
                {
                    var pwHash = BCrypt.Net.BCrypt.HashPassword("Reception123!");
                    conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                        new { Username = "reception", PasswordHash = pwHash, FullName = "Reception Staff - موظف الاستقبال", RoleId = 3 });
                    _logger.LogInformation("Created reception user: reception / Reception123!");
                }
                
                _logger?.LogInformation("Default users initialization completed");
            }, ex => $"Failed to initialize default users: {ex.Message}", "INIT_USERS_FAILED"));
        }
    }
}