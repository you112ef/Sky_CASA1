using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Data
{
    public class DatabaseSeeder
    {
        private readonly MedicalLabContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(MedicalLabContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogWarning("DEVELOPMENT ONLY: Starting database seeding with test data...");
                _logger.LogWarning("WARNING: This seeder contains fake patient data and default passwords.");
                _logger.LogWarning("DO NOT USE IN PRODUCTION without changing all passwords and removing test data!");

                // Check if data already exists
                if (_context.Users.Any() || _context.Patients.Any() || _context.Exams.Any())
                {
                    _logger.LogInformation("Database already contains data, skipping seeding.");
                    return;
                }

                await SeedUsersAsync();
                await SeedPatientsAsync();
                await SeedExamsAsync();

                await _context.SaveChangesAsync();
                _logger.LogWarning("Database seeding completed. REMEMBER: Change all default passwords before production use!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database seeding");
                throw;
            }
        }

        private async Task SeedUsersAsync()
        {
            _logger.LogWarning("DEVELOPMENT ONLY: Seeding test users with default passwords...");

            var users = new[]
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@medicallab.com",
                    FullName = "System Administrator",
                    Role = "Admin",
                    Department = "IT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "doctor1",
                    Email = "doctor1@medicallab.com",
                    FullName = "Dr. أحمد محمد",
                    Role = "Doctor",
                    Department = "Cardiology",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "doctor2",
                    Email = "doctor2@medicallab.com",
                    FullName = "Dr. فاطمة علي",
                    Role = "Doctor",
                    Department = "Neurology",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "technician1",
                    Email = "tech1@medicallab.com",
                    FullName = "محمد أحمد",
                    Role = "Technician",
                    Department = "Laboratory",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "technician2",
                    Email = "tech2@medicallab.com",
                    FullName = "سارة محمد",
                    Role = "Technician",
                    Department = "Radiology",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Set passwords for all users - THESE ARE DEFAULT PASSWORDS FOR DEVELOPMENT ONLY!
            foreach (var user in users)
            {
                if (user.Username == "admin")
                {
                    user.SetPassword("Admin@123"); // CHANGE THIS IN PRODUCTION!
                    _logger.LogWarning("Created admin user with default password: Admin@123 - CHANGE IMMEDIATELY!");
                }
                else
                {
                    user.SetPassword("Password@123"); // CHANGE THIS IN PRODUCTION!
                }
            }

            await _context.Users.AddRangeAsync(users);
            _logger.LogWarning("Seeded {Count} test users with DEFAULT PASSWORDS - Change in production!", users.Length);
        }

        private async Task SeedPatientsAsync()
        {
            _logger.LogWarning("DEVELOPMENT ONLY: Seeding FAKE patient data - Remove in production!");

            var patients = new[]
            {
                new Patient
                {
                    FirstName = "أحمد",
                    LastName = "محمد",
                    NationalId = "1234567890",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Gender = "Male",
                    Phone = "+966501234567",
                    Email = "ahmed.mohamed@email.com",
                    Address = "الرياض، المملكة العربية السعودية",
                    MedicalHistory = "No significant medical history",
                    Allergies = "None known",
                    CurrentMedications = "None",
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    FirstName = "فاطمة",
                    LastName = "علي",
                    NationalId = "0987654321",
                    DateOfBirth = new DateTime(1990, 8, 22),
                    Gender = "Female",
                    Phone = "+966509876543",
                    Email = "fatima.ali@email.com",
                    Address = "جدة، المملكة العربية السعودية",
                    MedicalHistory = "Hypertension, controlled with medication",
                    Allergies = "Penicillin",
                    CurrentMedications = "Lisinopril 10mg daily",
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    FirstName = "عمر",
                    LastName = "خالد",
                    NationalId = "1122334455",
                    DateOfBirth = new DateTime(1978, 12, 3),
                    Gender = "Male",
                    Phone = "+966507654321",
                    Email = "omar.khalid@email.com",
                    Address = "الدمام، المملكة العربية السعودية",
                    MedicalHistory = "Diabetes Type 2, diagnosed 2015",
                    Allergies = "None known",
                    CurrentMedications = "Metformin 500mg twice daily",
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    FirstName = "نور",
                    LastName = "أحمد",
                    NationalId = "5566778899",
                    DateOfBirth = new DateTime(1995, 3, 18),
                    Gender = "Female",
                    Phone = "+966501112233",
                    Email = "noor.ahmed@email.com",
                    Address = "مكة المكرمة، المملكة العربية السعودية",
                    MedicalHistory = "Asthma, mild",
                    Allergies = "Dust, pollen",
                    CurrentMedications = "Albuterol inhaler as needed",
                    CreatedAt = DateTime.UtcNow
                },
                new Patient
                {
                    FirstName = "خالد",
                    LastName = "محمد",
                    NationalId = "9988776655",
                    DateOfBirth = new DateTime(1982, 7, 25),
                    Gender = "Male",
                    Phone = "+966504445556",
                    Email = "khalid.mohamed@email.com",
                    Address = "المدينة المنورة، المملكة العربية السعودية",
                    MedicalHistory = "No significant medical history",
                    Allergies = "None known",
                    CurrentMedications = "None",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.Patients.AddRangeAsync(patients);
            _logger.LogWarning("Seeded {Count} FAKE patients - This is test data only! Remove before production!", patients.Length);
        }

        private async Task SeedExamsAsync()
        {
            _logger.LogWarning("DEVELOPMENT ONLY: Seeding fake exam data - Remove in production!");

            var patients = _context.Patients.ToList();
            var users = _context.Users.ToList();

            var examTypes = new[]
            {
                "Blood Test",
                "Urine Analysis",
                "X-Ray",
                "Ultrasound",
                "ECG",
                "Endoscopy",
                "Biopsy",
                "MRI Scan",
                "CT Scan",
                "Blood Pressure Monitoring"
            };

            var examNames = new[]
            {
                "Complete Blood Count (CBC)",
                "Comprehensive Metabolic Panel",
                "Lipid Panel",
                "Thyroid Function Test",
                "Urinalysis",
                "Chest X-Ray",
                "Abdominal Ultrasound",
                "Electrocardiogram",
                "Upper Endoscopy",
                "24-Hour Blood Pressure Monitor"
            };

            var random = new Random();
            var exams = new List<Exam>();

            for (int i = 0; i < 20; i++)
            {
                var patient = patients[random.Next(patients.Count)];
                var examType = examTypes[random.Next(examTypes.Length)];
                var examName = examNames[random.Next(examNames.Length)];
                var status = random.Next(4) switch
                {
                    0 => "Pending",
                    1 => "In Progress",
                    2 => "Completed",
                    _ => "Cancelled"
                };

                var exam = new Exam
                {
                    PatientId = patient.Id,
                    ExamType = examType,
                    ExamName = examName,
                    Description = $"Routine {examType.ToLower()} for {patient.FullName}",
                    ExamDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                    Status = status,
                    Price = random.Next(50, 500),
                    Technician = users.FirstOrDefault(u => u.Role == "Technician")?.Username ?? "admin",
                    Doctor = users.FirstOrDefault(u => u.Role == "Doctor")?.Username ?? "admin",
                    CreatedAt = DateTime.UtcNow
                };

                if (status == "Completed")
                {
                    exam.CompletedAt = exam.CreatedAt.AddHours(random.Next(1, 24));
                    exam.Results = $"Normal results for {examName}. All values within expected ranges.";
                    exam.Notes = "Patient tolerated procedure well. No complications.";
                }

                exams.Add(exam);
            }

            await _context.Exams.AddRangeAsync(exams);
            _logger.LogWarning("Seeded {Count} FAKE exams - This is test data only! Remove before production!", exams.Count);
        }

        public async Task ClearAllDataAsync()
        {
            try
            {
                _logger.LogWarning("Clearing all database data...");

                _context.Exams.RemoveRange(_context.Exams);
                _context.Patients.RemoveRange(_context.Patients);
                _context.Users.RemoveRange(_context.Users);

                await _context.SaveChangesAsync();
                _logger.LogInformation("All database data cleared successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing database data");
                throw;
            }
        }

        public async Task ResetToSeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Resetting database to seed data...");

                await ClearAllDataAsync();
                await SeedAsync();

                _logger.LogInformation("Database reset to seed data completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting database to seed data");
                throw;
            }
        }
    }
}