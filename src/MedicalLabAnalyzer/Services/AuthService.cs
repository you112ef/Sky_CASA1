using System;
using System.Linq;
using Dapper;
using MedicalLabAnalyzer.Models;
using System.Security.Cryptography;

namespace MedicalLabAnalyzer.Services
{
    public enum Role { Admin, LabTech, Reception }

    public class AuthService
    {
        private readonly DatabaseService _db;
        private User _currentUser;
        public AuthService(DatabaseService db)
        {
            _db = db;
            CreateInitialAdminIfNotExists();
        }

        public User Authenticate(string username, string password)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var user = conn.QuerySingleOrDefault<User>("SELECT u.*, r.RoleName as RoleName FROM Users u JOIN Roles r ON u.RoleId = r.RoleId WHERE Username=@Username", new { Username = username });
            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;
            _currentUser = user;
            return user;
        }

        public void Logout() => _currentUser = null;

        public User CurrentUser => _currentUser;

        public bool HasAccess(Role required)
        {
            if (_currentUser == null) return false;
            var roleName = _currentUser.RoleName ?? GetRoleNameById(_currentUser.RoleId);
            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)) return true;
            if (required == Role.Admin) return roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            if (required == Role.LabTech) return roleName.Equals("LabTech", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            if (required == Role.Reception) return roleName.Equals("Reception", StringComparison.OrdinalIgnoreCase) || roleName.Equals("LabTech", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private string GetRoleNameById(int roleId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            return conn.QuerySingleOrDefault<string>("SELECT RoleName FROM Roles WHERE RoleId=@RoleId", new { RoleId = roleId });
        }

        private void CreateInitialAdminIfNotExists()
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var admin = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Username='admin'");
            if (admin == null)
            {
                var pwHash = BCrypt.Net.BCrypt.HashPassword("admin123"); // force change
                conn.Execute("INSERT INTO Users (Username, PasswordHash, FullName, RoleId) VALUES (@Username,@PasswordHash,@FullName,@RoleId)",
                    new { Username = "admin", PasswordHash = pwHash, FullName = "المسؤول", RoleId = 1 });
                AuditLogger.Log("Init", "Created default admin user");
            }
        }
    }
}