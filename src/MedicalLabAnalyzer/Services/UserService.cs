using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using MedicalLabAnalyzer.Models;
using BCrypt.Net;

namespace MedicalLabAnalyzer.Services
{
    /// <summary>
    /// Service for managing users and authentication
    /// </summary>
    public class UserService
    {
        private readonly IDbConnection _db;
        private readonly ILogger<UserService> _logger;
        private readonly AuditLogger _auditLogger;

        public UserService(IDbConnection db, ILogger<UserService> logger = null, AuditLogger auditLogger = null)
        {
            _db = db;
            _logger = logger;
            _auditLogger = auditLogger;
            InitializeDatabase();
        }

        /// <summary>
        /// Initialize user tables if they don't exist
        /// </summary>
        private void InitializeDatabase()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    Email TEXT UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Role TEXT NOT NULL CHECK(Role IN ('Admin', 'Doctor', 'Technician', 'Receptionist')),
                    Department TEXT,
                    PhoneNumber TEXT,
                    IsActive INTEGER DEFAULT 1,
                    LastLogin DATETIME,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy TEXT,
                    PasswordChangedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                CREATE TABLE IF NOT EXISTS UserSessions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    SessionToken TEXT UNIQUE NOT NULL,
                    LoginTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    LastActivity DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IPAddress TEXT,
                    UserAgent TEXT,
                    IsActive INTEGER DEFAULT 1,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username);
                CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
                CREATE INDEX IF NOT EXISTS idx_users_role ON Users(Role);
                CREATE INDEX IF NOT EXISTS idx_sessions_token ON UserSessions(SessionToken);
                CREATE INDEX IF NOT EXISTS idx_sessions_user ON UserSessions(UserId);
            ";

            _db.Execute(sql);
            
            // Create default admin user if no users exist
            CreateDefaultAdmin();
            
            _logger?.LogInformation("User database initialized");
        }

        /// <summary>
        /// Create default admin user if no users exist
        /// </summary>
        private void CreateDefaultAdmin()
        {
            var userCount = _db.QuerySingle<int>("SELECT COUNT(*) FROM Users");
            if (userCount == 0)
            {
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@medicallabsolutions.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    Department = "IT",
                    IsActive = true
                };

                AddUser(adminUser, "admin123", "system");
                _logger?.LogInformation("Default admin user created");
            }
        }

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <param name="user">User data</param>
        /// <param name="password">Plain text password</param>
        /// <param name="createdBy">User creating this account</param>
        /// <returns>User ID</returns>
        public int AddUser(User user, string password, string createdBy)
        {
            // Validate user data
            var validation = ValidateUser(user);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"User validation failed: {string.Join(", ", validation.Errors)}");
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long");
            }

            // Check if username already exists
            if (UserExists(user.Username))
            {
                throw new ArgumentException("Username already exists");
            }

            // Check if email already exists
            if (!string.IsNullOrWhiteSpace(user.Email) && EmailExists(user.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var sql = @"
                INSERT INTO Users (
                    Username, Email, PasswordHash, FirstName, LastName, Role, 
                    Department, PhoneNumber, CreatedBy
                ) VALUES (
                    @Username, @Email, @PasswordHash, @FirstName, @LastName, @Role,
                    @Department, @PhoneNumber, @CreatedBy
                );
                SELECT last_insert_rowid();";

            var parameters = new
            {
                user.Username,
                user.Email,
                PasswordHash = passwordHash,
                user.FirstName,
                user.LastName,
                user.Role,
                user.Department,
                user.PhoneNumber,
                CreatedBy = createdBy
            };

            var id = _db.QuerySingle<int>(sql, parameters);
            
            _logger?.LogInformation($"User added: {user.Username} ({user.Role})");
            
            _auditLogger?.LogSystemEvent(
                userId: createdBy,
                userName: "System",
                action: "USER_CREATE",
                category: "USER",
                details: new { UserId = id, Username = user.Username, Role = user.Role }
            );

            return id;
        }

        /// <summary>
        /// Authenticate user login
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plain text password</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <param name="userAgent">Client user agent</param>
        /// <returns>Authentication result</returns>
        public AuthenticationResult AuthenticateUser(string username, string password, string ipAddress = null, string userAgent = null)
        {
            var user = GetUserByUsername(username);
            if (user == null || !user.IsActive)
            {
                _auditLogger?.LogSecurityEvent(
                    userId: "unknown",
                    userName: username,
                    action: "LOGIN_FAILED",
                    details: new { Username = username, Reason = "Invalid username or inactive account", IPAddress = ipAddress }
                );
                return new AuthenticationResult { Success = false, Message = "Invalid username or password" };
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _auditLogger?.LogSecurityEvent(
                    userId: user.Id.ToString(),
                    userName: username,
                    action: "LOGIN_FAILED",
                    details: new { Username = username, Reason = "Invalid password", IPAddress = ipAddress }
                );
                return new AuthenticationResult { Success = false, Message = "Invalid username or password" };
            }

            // Update last login
            UpdateLastLogin(user.Id);

            // Create session
            var sessionToken = CreateSession(user.Id, ipAddress, userAgent);

            _auditLogger?.LogSecurityEvent(
                userId: user.Id.ToString(),
                userName: username,
                action: "LOGIN_SUCCESS",
                details: new { Username = username, IPAddress = ipAddress, SessionToken = sessionToken }
            );

            return new AuthenticationResult
            {
                Success = true,
                User = user,
                SessionToken = sessionToken,
                Message = "Login successful"
            };
        }

        /// <summary>
        /// Validate session token
        /// </summary>
        /// <param name="sessionToken">Session token</param>
        /// <returns>User if valid, null otherwise</returns>
        public User ValidateSession(string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
                return null;

            var sql = @"
                SELECT u.* FROM Users u
                INNER JOIN UserSessions s ON u.Id = s.UserId
                WHERE s.SessionToken = @SessionToken 
                AND s.IsActive = 1 
                AND u.IsActive = 1
                AND s.LastActivity > datetime('now', '-8 hours')";

            var user = _db.QueryFirstOrDefault<User>(sql, new { SessionToken = sessionToken });

            if (user != null)
            {
                // Update last activity
                UpdateSessionActivity(sessionToken);
            }

            return user;
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="sessionToken">Session token</param>
        /// <param name="userId">User ID</param>
        /// <returns>True if successful</returns>
        public bool LogoutUser(string sessionToken, string userId)
        {
            var sql = "UPDATE UserSessions SET IsActive = 0 WHERE SessionToken = @SessionToken";
            var rowsAffected = _db.Execute(sql, new { SessionToken = sessionToken });

            if (rowsAffected > 0)
            {
                _auditLogger?.LogSecurityEvent(
                    userId: userId,
                    userName: "System",
                    action: "LOGOUT",
                    details: new { SessionToken = sessionToken }
                );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if successful</returns>
        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = GetUserById(userId);
            if (user == null)
                return false;

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return false;

            // Validate new password
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                throw new ArgumentException("New password must be at least 8 characters long");

            // Hash new password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            var sql = @"
                UPDATE Users 
                SET PasswordHash = @PasswordHash, PasswordChangedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id";

            var rowsAffected = _db.Execute(sql, new { PasswordHash = newPasswordHash, Id = userId });

            if (rowsAffected > 0)
            {
                _auditLogger?.LogSecurityEvent(
                    userId: userId.ToString(),
                    userName: user.Username,
                    action: "PASSWORD_CHANGE",
                    details: new { UserId = userId }
                );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reset user password (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newPassword">New password</param>
        /// <param name="adminUserId">Admin user ID</param>
        /// <returns>True if successful</returns>
        public bool ResetPassword(int userId, string newPassword, string adminUserId)
        {
            var user = GetUserById(userId);
            if (user == null)
                return false;

            // Validate new password
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                throw new ArgumentException("New password must be at least 8 characters long");

            // Hash new password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            var sql = @"
                UPDATE Users 
                SET PasswordHash = @PasswordHash, PasswordChangedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id";

            var rowsAffected = _db.Execute(sql, new { PasswordHash = newPasswordHash, Id = userId });

            if (rowsAffected > 0)
            {
                _auditLogger?.LogSecurityEvent(
                    userId: adminUserId,
                    userName: "Admin",
                    action: "PASSWORD_RESET",
                    details: new { TargetUserId = userId, TargetUsername = user.Username }
                );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User data or null if not found</returns>
        public User GetUserById(int id)
        {
            var sql = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
            return _db.QueryFirstOrDefault<User>(sql, new { Id = id });
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User data or null if not found</returns>
        public User GetUserByUsername(string username)
        {
            var sql = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
            return _db.QueryFirstOrDefault<User>(sql, new { Username = username });
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of users per page</param>
        /// <returns>Paginated user list</returns>
        public (List<User> Users, int TotalCount) GetUsers(int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;
            
            var countSql = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
            var totalCount = _db.QuerySingle<int>(countSql);
            
            var sql = @"
                SELECT * FROM Users 
                WHERE IsActive = 1 
                ORDER BY LastName, FirstName
                LIMIT @PageSize OFFSET @Offset";

            var users = _db.Query<User>(sql, new { PageSize = pageSize, Offset = offset }).ToList();
            
            return (users, totalCount);
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="user">Updated user data</param>
        /// <param name="updatedBy">User performing the update</param>
        /// <returns>True if successful</returns>
        public bool UpdateUser(User user, string updatedBy)
        {
            if (user.Id <= 0)
                return false;

            var validation = ValidateUser(user);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"User validation failed: {string.Join(", ", validation.Errors)}");
            }

            var sql = @"
                UPDATE Users SET 
                    FirstName = @FirstName, LastName = @LastName, Email = @Email,
                    Role = @Role, Department = @Department, PhoneNumber = @PhoneNumber,
                    UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id";

            var rowsAffected = _db.Execute(sql, user);
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"User updated: {user.Username} (ID: {user.Id})");
                
                _auditLogger?.LogSystemEvent(
                    userId: updatedBy,
                    userName: "System",
                    action: "USER_UPDATE",
                    category: "USER",
                    details: new { UserId = user.Id, Username = user.Username, Role = user.Role }
                );
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Deactivate user account
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="deactivatedBy">User performing the deactivation</param>
        /// <returns>True if successful</returns>
        public bool DeactivateUser(int userId, string deactivatedBy)
        {
            var sql = "UPDATE Users SET IsActive = 0, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            var rowsAffected = _db.Execute(sql, new { Id = userId });
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"User deactivated (ID: {userId})");
                
                _auditLogger?.LogSystemEvent(
                    userId: deactivatedBy,
                    userName: "System",
                    action: "USER_DEACTIVATE",
                    category: "USER",
                    details: new { UserId = userId }
                );
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get user statistics
        /// </summary>
        /// <returns>User statistics</returns>
        public UserStatistics GetStatistics()
        {
            var sql = @"
                SELECT 
                    COUNT(*) as TotalUsers,
                    COUNT(CASE WHEN Role = 'Admin' THEN 1 END) as AdminUsers,
                    COUNT(CASE WHEN Role = 'Doctor' THEN 1 END) as DoctorUsers,
                    COUNT(CASE WHEN Role = 'Technician' THEN 1 END) as TechnicianUsers,
                    COUNT(CASE WHEN Role = 'Receptionist' THEN 1 END) as ReceptionistUsers,
                    COUNT(CASE WHEN LastLogin > datetime('now', '-7 days') THEN 1 END) as ActiveUsers,
                    MIN(CreatedAt) as FirstUser,
                    MAX(CreatedAt) as LastUser
                FROM Users 
                WHERE IsActive = 1";

            return _db.QueryFirstOrDefault<UserStatistics>(sql) ?? new UserStatistics();
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        private bool UserExists(string username)
        {
            var sql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            return _db.QuerySingle<int>(sql, new { Username = username }) > 0;
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        private bool EmailExists(string email)
        {
            var sql = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            return _db.QuerySingle<int>(sql, new { Email = email }) > 0;
        }

        /// <summary>
        /// Update last login time
        /// </summary>
        private void UpdateLastLogin(int userId)
        {
            var sql = "UPDATE Users SET LastLogin = CURRENT_TIMESTAMP WHERE Id = @Id";
            _db.Execute(sql, new { Id = userId });
        }

        /// <summary>
        /// Create user session
        /// </summary>
        private string CreateSession(int userId, string ipAddress, string userAgent)
        {
            var sessionToken = Guid.NewGuid().ToString();
            
            var sql = @"
                INSERT INTO UserSessions (UserId, SessionToken, IPAddress, UserAgent)
                VALUES (@UserId, @SessionToken, @IPAddress, @UserAgent)";

            _db.Execute(sql, new { UserId = userId, SessionToken = sessionToken, IPAddress = ipAddress, UserAgent = userAgent });
            
            return sessionToken;
        }

        /// <summary>
        /// Update session activity
        /// </summary>
        private void UpdateSessionActivity(string sessionToken)
        {
            var sql = "UPDATE UserSessions SET LastActivity = CURRENT_TIMESTAMP WHERE SessionToken = @SessionToken";
            _db.Execute(sql, new { SessionToken = sessionToken });
        }

        /// <summary>
        /// Validate user data
        /// </summary>
        private ValidationResult ValidateUser(User user)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                result.IsValid = false;
                result.Errors.Add("Username is required");
            }

            if (string.IsNullOrWhiteSpace(user.FirstName))
            {
                result.IsValid = false;
                result.Errors.Add("First name is required");
            }

            if (string.IsNullOrWhiteSpace(user.LastName))
            {
                result.IsValid = false;
                result.Errors.Add("Last name is required");
            }

            if (string.IsNullOrWhiteSpace(user.Role))
            {
                result.IsValid = false;
                result.Errors.Add("Role is required");
            }

            if (!string.IsNullOrWhiteSpace(user.Email) && !IsValidEmail(user.Email))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid email format");
            }

            return result;
        }

        /// <summary>
        /// Validate email format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Authentication result
    /// </summary>
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public User User { get; set; }
        public string SessionToken { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// User statistics
    /// </summary>
    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int DoctorUsers { get; set; }
        public int TechnicianUsers { get; set; }
        public int ReceptionistUsers { get; set; }
        public int ActiveUsers { get; set; }
        public DateTime? FirstUser { get; set; }
        public DateTime? LastUser { get; set; }
    }
}