using Microsoft.EntityFrameworkCore;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Data
{
    public class MedicalLabContext : DbContext
    {
        public MedicalLabContext(DbContextOptions<MedicalLabContext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Patient entity
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NationalId).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NationalId).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DateOfBirth).IsRequired();
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.MedicalHistory).HasMaxLength(500);
                entity.Property(e => e.Allergies).HasMaxLength(500);
                entity.Property(e => e.CurrentMedications).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Exam entity
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExamType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ExamName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ExamDate).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.Results).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.VideoPath).HasMaxLength(200);
                entity.Property(e => e.ReportPath).HasMaxLength(200);
                entity.Property(e => e.Technician).HasMaxLength(100);
                entity.Property(e => e.Doctor).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                // Configure relationship with Patient
                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.Exams)
                      .HasForeignKey(e => e.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed admin user
            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@medicallab.com",
                FullName = "System Administrator",
                Role = "Admin",
                Department = "IT",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            adminUser.SetPassword("Admin@123");

            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed sample patients
            var samplePatients = new[]
            {
                new Patient
                {
                    Id = 1,
                    FirstName = "أحمد",
                    LastName = "محمد",
                    NationalId = "1234567890",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Gender = "Male",
                    Phone = "+966501234567",
                    Email = "ahmed.mohamed@email.com",
                    Address = "الرياض، المملكة العربية السعودية",
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    Id = 2,
                    FirstName = "فاطمة",
                    LastName = "علي",
                    NationalId = "0987654321",
                    DateOfBirth = new DateTime(1990, 8, 22),
                    Gender = "Female",
                    Phone = "+966509876543",
                    Email = "fatima.ali@email.com",
                    Address = "جدة، المملكة العربية السعودية",
                    CreatedAt = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<Patient>().HasData(samplePatients);

            // Seed sample exams
            var sampleExams = new[]
            {
                new Exam
                {
                    Id = 1,
                    PatientId = 1,
                    ExamType = "Blood Test",
                    ExamName = "Complete Blood Count (CBC)",
                    Description = "Routine blood analysis",
                    ExamDate = DateTime.Now.AddDays(-5),
                    Status = "Completed",
                    Results = "All values within normal range",
                    Price = 150.00m,
                    Technician = "admin",
                    Doctor = "Dr. محمد",
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Exam
                {
                    Id = 2,
                    PatientId = 2,
                    ExamType = "Urine Analysis",
                    ExamName = "Urinalysis",
                    Description = "Urine sample analysis",
                    ExamDate = DateTime.Now.AddDays(-2),
                    Status = "In Progress",
                    Price = 80.00m,
                    Technician = "admin",
                    Doctor = "Dr. أحمد",
                    CreatedAt = DateTime.UtcNow
                }
            };

            modelBuilder.Entity<Exam>().HasData(sampleExams);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update UpdatedAt timestamp for modified entities
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Patient || e.Entity is Exam || e.Entity is User)
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Patient patient)
                {
                    patient.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Exam exam)
                {
                    exam.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is User user)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}