using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace MedicalLabAnalyzer.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private readonly string _connStr;

        public DatabaseService(string dbPath = null)
        {
            _dbPath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
            var dir = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            _connStr = $"Data Source={_dbPath};Version=3;Pooling=True;Max Pool Size=100;";
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
                // execute init script if present
                var initSqlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "init_schema_full.sql");
                if (File.Exists(initSqlPath))
                {
                    using var conn = GetConnection();
                    conn.Open();
                    var sql = File.ReadAllText(initSqlPath);
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IDbConnection GetConnection()
        {
            var conn = new SQLiteConnection(_connStr);
            return conn;
        }
    }
}