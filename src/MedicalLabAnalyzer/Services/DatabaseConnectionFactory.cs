using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace MedicalLabAnalyzer.Services
{
    public interface IDatabaseConnectionFactory
    {
        IDbConnection CreateConnection();
        string GetConnectionString();
    }

    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseConnectionFactory> _logger;

        public DatabaseConnectionFactory(IConfiguration configuration, ILogger<DatabaseConnectionFactory> logger = null)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = GetConnectionString();
            _logger?.LogDebug("Creating database connection with connection string: {ConnectionString}", connectionString);
            return new SqliteConnection(connectionString);
        }

        public string GetConnectionString()
        {
            var connectionString = _configuration?.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Default connection string - same as in DatabaseService
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
                connectionString = $"Data Source={dbPath}";
                _logger?.LogDebug("Using default connection string: {ConnectionString}", connectionString);
            }
            else
            {
                _logger?.LogDebug("Using configured connection string: {ConnectionString}", connectionString);
            }

            return connectionString;
        }
    }
}