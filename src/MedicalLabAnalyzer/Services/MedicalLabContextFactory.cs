using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Data;
using System;
using System.IO;

namespace MedicalLabAnalyzer.Services
{
    public interface IMedicalLabContextFactory
    {
        MedicalLabContext CreateContext();
    }

    public class MedicalLabContextFactory : IMedicalLabContextFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MedicalLabContextFactory> _logger;

        public MedicalLabContextFactory(IConfiguration configuration, ILogger<MedicalLabContextFactory> logger = null)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public MedicalLabContext CreateContext()
        {
            var connectionString = GetConnectionString();
            
            var options = new DbContextOptionsBuilder<MedicalLabContext>()
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;

            _logger?.LogDebug("Creating MedicalLabContext with connection string: {ConnectionString}", connectionString);
            return new MedicalLabContext(options);
        }

        private string GetConnectionString()
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