using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MedicalLabAnalyzer.Services
{
    public class MedicalLabDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MedicalLabDbContext> _logger;

        public MedicalLabDbContext(DbContextOptions<MedicalLabDbContext> options, 
            IConfiguration configuration, 
            ILogger<MedicalLabDbContext> logger) : base(options)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // DbSets
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Exam> Exams { get; set; } = null!;
        public DbSet<CASAResult> CASAResults { get; set; } = null!;
        public DbSet<CBCTestResult> CBCTestResults { get; set; } = null!;
        public DbSet<UrineTestResult> UrineTestResults { get; set; } = null!;
        public DbSet<StoolTestResult> StoolTestResults { get; set; } = null!;
        public DbSet<GlucoseTestResult> GlucoseTestResults { get; set; } = null!;
        public DbSet<LipidProfileTestResult> LipidProfileTestResults { get; set; } = null!;
        public DbSet<LiverFunctionTestResult> LiverFunctionTestResults { get; set; } = null!;
        public DbSet<KidneyFunctionTestResult> KidneyFunctionTestResults { get; set; } = null!;
        public DbSet<CRPTestResult> CRPTestResults { get; set; } = null!;
        public DbSet<ThyroidTestResult> ThyroidTestResults { get; set; } = null!;
        public DbSet<ElectrolytesTestResult> ElectrolytesTestResults { get; set; } = null!;
        public DbSet<CoagulationTestResult> CoagulationTestResults { get; set; } = null!;
        public DbSet<VitaminTestResult> VitaminTestResults { get; set; } = null!;
        public DbSet<HormoneTestResult> HormoneTestResults { get; set; } = null!;
        public DbSet<MicrobiologyTestResult> MicrobiologyTestResults { get; set; } = null!;
        public DbSet<PCRTestResult> PCRTestResults { get; set; } = null!;
        public DbSet<SerologyTestResult> SerologyTestResults { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Default connection string
                    var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
                    connectionString = $"Data Source={dbPath}";
                }

                optionsBuilder.UseSqlite(connectionString);
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NationalId).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NationalId).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
            });

            // Configure Exam
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExamType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.Exams)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CBC Test Result
            modelBuilder.Entity<CBCTestResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Exam)
                    .WithOne(ex => ex.CBCTestResult)
                    .HasForeignKey<CBCTestResult>(e => e.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CASA Result
            modelBuilder.Entity<CASAResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Exam)
                    .WithOne(ex => ex.CASAResult)
                    .HasForeignKey<CASAResult>(e => e.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure other test results similarly...
            ConfigureTestResult<UrineTestResult>(modelBuilder);
            ConfigureTestResult<StoolTestResult>(modelBuilder);
            ConfigureTestResult<GlucoseTestResult>(modelBuilder);
            ConfigureTestResult<LipidProfileTestResult>(modelBuilder);
            ConfigureTestResult<LiverFunctionTestResult>(modelBuilder);
            ConfigureTestResult<KidneyFunctionTestResult>(modelBuilder);
            ConfigureTestResult<CRPTestResult>(modelBuilder);
            ConfigureTestResult<ThyroidTestResult>(modelBuilder);
            ConfigureTestResult<ElectrolytesTestResult>(modelBuilder);
            ConfigureTestResult<CoagulationTestResult>(modelBuilder);
            ConfigureTestResult<VitaminTestResult>(modelBuilder);
            ConfigureTestResult<HormoneTestResult>(modelBuilder);
            ConfigureTestResult<MicrobiologyTestResult>(modelBuilder);
            ConfigureTestResult<PCRTestResult>(modelBuilder);
            ConfigureTestResult<SerologyTestResult>(modelBuilder);
        }

        private void ConfigureTestResult<T>(ModelBuilder modelBuilder) where T : class
        {
            modelBuilder.Entity<T>(entity =>
            {
                var examIdProperty = entity.Metadata.FindProperty("ExamId");
                if (examIdProperty != null)
                {
                    entity.HasKey("Id");
                    entity.HasOne("Exam")
                        .WithOne()
                        .HasForeignKey(typeof(T), "ExamId")
                        .OnDelete(DeleteBehavior.Cascade);
                }
            });
        }
    }

    public class DatabaseService
    {
        private readonly MedicalLabDbContext _context;
        private readonly ILogger<DatabaseService> _logger;
        private readonly IConfiguration _configuration;

        public DatabaseService(MedicalLabDbContext context, ILogger<DatabaseService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Initializing database...");

                // Ensure database directory exists
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
                if (!Directory.Exists(dbPath))
                {
                    Directory.CreateDirectory(dbPath);
                    _logger.LogInformation("Created database directory: {DbPath}", dbPath);
                }

                // Create database if it doesn't exist
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database created successfully");

                // Apply any pending migrations
                if ((await _context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Database migrations applied successfully");
                }

                // Seed initial data if database is empty
                if (!await _context.Patients.AnyAsync())
                {
                    await SeedInitialDataAsync();
                    _logger.LogInformation("Initial data seeded successfully");
                }

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                throw;
            }
        }

        private async Task SeedInitialDataAsync()
        {
            try
            {
                // Add sample patients
                var patients = new List<Patient>
                {
                    new Patient
                    {
                        FirstName = "أحمد",
                        LastName = "محمد",
                        NationalId = "1234567890",
                        DateOfBirth = new DateTime(1985, 5, 15),
                        Gender = "ذكر",
                        PhoneNumber = "0501234567",
                        Email = "ahmed@example.com",
                        Address = "الرياض، المملكة العربية السعودية",
                        BloodType = "A+",
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    },
                    new Patient
                    {
                        FirstName = "فاطمة",
                        LastName = "علي",
                        NationalId = "0987654321",
                        DateOfBirth = new DateTime(1990, 8, 22),
                        Gender = "أنثى",
                        PhoneNumber = "0509876543",
                        Email = "fatima@example.com",
                        Address = "جدة، المملكة العربية السعودية",
                        BloodType = "O+",
                        CreatedBy = "System",
                        UpdatedBy = "System"
                    }
                };

                await _context.Patients.AddRangeAsync(patients);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Seeded {Count} sample patients", patients.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding initial data");
                throw;
            }
        }

        public async Task<bool> BackupDatabaseAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation("Creating database backup to: {BackupPath}", backupPath);

                // Ensure backup directory exists
                var backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Close all connections
                await _context.Database.CloseConnectionAsync();

                // Copy database file
                var dbPath = _context.Database.GetConnectionString();
                if (dbPath != null && dbPath.Contains("Data Source="))
                {
                    var sourcePath = dbPath.Replace("Data Source=", "").Trim();
                    if (File.Exists(sourcePath))
                    {
                        File.Copy(sourcePath, backupPath, true);
                        _logger.LogInformation("Database backup created successfully");
                        return true;
                    }
                }

                _logger.LogWarning("Could not create database backup - source file not found");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database backup");
                return false;
            }
        }

        public async Task<bool> RestoreDatabaseAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation("Restoring database from: {BackupPath}", backupPath);

                if (!File.Exists(backupPath))
                {
                    _logger.LogError("Backup file not found: {BackupPath}", backupPath);
                    return false;
                }

                // Close all connections
                await _context.Database.CloseConnectionAsync();

                // Get current database path
                var dbPath = _context.Database.GetConnectionString();
                if (dbPath != null && dbPath.Contains("Data Source="))
                {
                    var targetPath = dbPath.Replace("Data Source=", "").Trim();
                    
                    // Copy backup to current database location
                    File.Copy(backupPath, targetPath, true);
                    
                    _logger.LogInformation("Database restored successfully");
                    return true;
                }

                _logger.LogWarning("Could not restore database - target path not found");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring database");
                return false;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}