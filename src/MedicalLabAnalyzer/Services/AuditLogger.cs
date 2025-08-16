using System;
using System.Data.SQLite;

namespace MedicalLabAnalyzer.Services
{
    public static class AuditLogger
    {
        private static string _dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
        private static string _connStr => $"Data Source={_dbPath};Version=3;";

        public static void Log(string action, string description)
        {
            try
            {
                using var conn = new SQLiteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO AuditLogs (Action, Description, CreatedAt) VALUES (@Action,@Description,@CreatedAt)";
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // swallow to avoid breaking main flow; optionally write to file
            }
        }
    }
}