using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
    /// <summary>
    /// Service for managing patient data and medical records
    /// </summary>
    public class PatientService
    {
        private readonly IDbConnection _db;
        private readonly ILogger<PatientService> _logger;
        private readonly AuditLogger _auditLogger;

        public PatientService(IDbConnection db, ILogger<PatientService> logger = null, AuditLogger auditLogger = null)
        {
            _db = db;
            _logger = logger;
            _auditLogger = auditLogger;
            InitializeDatabase();
        }

        /// <summary>
        /// Initialize patient tables if they don't exist
        /// </summary>
        private void InitializeDatabase()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Patients (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MedicalRecordNumber TEXT UNIQUE NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    DateOfBirth DATE,
                    Gender TEXT CHECK(Gender IN ('Male', 'Female', 'Other')),
                    PhoneNumber TEXT,
                    Email TEXT,
                    Address TEXT,
                    EmergencyContact TEXT,
                    EmergencyPhone TEXT,
                    BloodType TEXT,
                    Allergies TEXT,
                    MedicalHistory TEXT,
                    Notes TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CreatedBy TEXT,
                    IsActive INTEGER DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Exams (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientId INTEGER NOT NULL,
                    ExamType TEXT NOT NULL,
                    ExamDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Status TEXT DEFAULT 'Pending',
                    TechnicianId TEXT,
                    DoctorId TEXT,
                    Notes TEXT,
                    Results TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (PatientId) REFERENCES Patients(Id)
                );

                CREATE INDEX IF NOT EXISTS idx_patients_mrn ON Patients(MedicalRecordNumber);
                CREATE INDEX IF NOT EXISTS idx_patients_name ON Patients(LastName, FirstName);
                CREATE INDEX IF NOT EXISTS idx_exams_patient ON Exams(PatientId);
                CREATE INDEX IF NOT EXISTS idx_exams_date ON Exams(ExamDate);
            ";

            _db.Execute(sql);
            _logger?.LogInformation("Patient database initialized");
        }

        /// <summary>
        /// Add a new patient
        /// </summary>
        /// <param name="patient">Patient data</param>
        /// <param name="userId">User creating the patient</param>
        /// <returns>Patient ID</returns>
        public int AddPatient(Patient patient, string userId)
        {
            // Validate patient data
            var validation = ValidatePatient(patient);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Patient validation failed: {string.Join(", ", validation.Errors)}");
            }

            // Generate MRN if not provided
            if (string.IsNullOrWhiteSpace(patient.MedicalRecordNumber))
            {
                patient.MedicalRecordNumber = GenerateMRN();
            }

            var sql = @"
                INSERT INTO Patients (
                    MedicalRecordNumber, FirstName, LastName, DateOfBirth, Gender,
                    PhoneNumber, Email, Address, EmergencyContact, EmergencyPhone,
                    BloodType, Allergies, MedicalHistory, Notes, CreatedBy
                ) VALUES (
                    @MedicalRecordNumber, @FirstName, @LastName, @DateOfBirth, @Gender,
                    @PhoneNumber, @Email, @Address, @EmergencyContact, @EmergencyPhone,
                    @BloodType, @Allergies, @MedicalHistory, @Notes, @CreatedBy
                );
                SELECT last_insert_rowid();";

            var id = _db.QuerySingle<int>(sql, patient);
            
            _logger?.LogInformation($"Patient added: {patient.FirstName} {patient.LastName} (MRN: {patient.MedicalRecordNumber})");
            
            _auditLogger?.LogSystemEvent(
                userId: userId,
                userName: "System",
                action: "PATIENT_CREATE",
                category: "PATIENT",
                details: new { PatientId = id, MRN = patient.MedicalRecordNumber, Name = $"{patient.FirstName} {patient.LastName}" }
            );

            return id;
        }

        /// <summary>
        /// Update an existing patient
        /// </summary>
        /// <param name="patient">Updated patient data</param>
        /// <param name="userId">User updating the patient</param>
        /// <returns>True if successful</returns>
        public bool UpdatePatient(Patient patient, string userId)
        {
            if (patient.Id <= 0)
                return false;

            var validation = ValidatePatient(patient);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Patient validation failed: {string.Join(", ", validation.Errors)}");
            }

            var sql = @"
                UPDATE Patients SET 
                    FirstName = @FirstName, LastName = @LastName, DateOfBirth = @DateOfBirth,
                    Gender = @Gender, PhoneNumber = @PhoneNumber, Email = @Email,
                    Address = @Address, EmergencyContact = @EmergencyContact, EmergencyPhone = @EmergencyPhone,
                    BloodType = @BloodType, Allergies = @Allergies, MedicalHistory = @MedicalHistory,
                    Notes = @Notes, UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id";

            var rowsAffected = _db.Execute(sql, patient);
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"Patient updated: {patient.FirstName} {patient.LastName} (ID: {patient.Id})");
                
                _auditLogger?.LogSystemEvent(
                    userId: userId,
                    userName: "System",
                    action: "PATIENT_UPDATE",
                    category: "PATIENT",
                    details: new { PatientId = patient.Id, MRN = patient.MedicalRecordNumber, Name = $"{patient.FirstName} {patient.LastName}" }
                );
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get patient by ID
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <returns>Patient data or null if not found</returns>
        public Patient GetPatientById(int id)
        {
            var sql = "SELECT * FROM Patients WHERE Id = @Id AND IsActive = 1";
            return _db.QueryFirstOrDefault<Patient>(sql, new { Id = id });
        }

        /// <summary>
        /// Get patient by Medical Record Number
        /// </summary>
        /// <param name="mrn">Medical Record Number</param>
        /// <returns>Patient data or null if not found</returns>
        public Patient GetPatientByMRN(string mrn)
        {
            var sql = "SELECT * FROM Patients WHERE MedicalRecordNumber = @MRN AND IsActive = 1";
            return _db.QueryFirstOrDefault<Patient>(sql, new { MRN = mrn });
        }

        /// <summary>
        /// Search patients by name
        /// </summary>
        /// <param name="searchTerm">Search term (first name, last name, or MRN)</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>List of matching patients</returns>
        public List<Patient> SearchPatients(string searchTerm, int limit = 50)
        {
            var sql = @"
                SELECT * FROM Patients 
                WHERE IsActive = 1 
                AND (FirstName LIKE @Search OR LastName LIKE @Search OR MedicalRecordNumber LIKE @Search)
                ORDER BY LastName, FirstName
                LIMIT @Limit";

            var searchPattern = $"%{searchTerm}%";
            return _db.Query<Patient>(sql, new { Search = searchPattern, Limit = limit }).ToList();
        }

        /// <summary>
        /// Get all patients with pagination
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of patients per page</param>
        /// <returns>Paginated patient list</returns>
        public (List<Patient> Patients, int TotalCount) GetPatients(int page = 1, int pageSize = 20)
        {
            var offset = (page - 1) * pageSize;
            
            var countSql = "SELECT COUNT(*) FROM Patients WHERE IsActive = 1";
            var totalCount = _db.QuerySingle<int>(countSql);
            
            var sql = @"
                SELECT * FROM Patients 
                WHERE IsActive = 1 
                ORDER BY LastName, FirstName
                LIMIT @PageSize OFFSET @Offset";

            var patients = _db.Query<Patient>(sql, new { PageSize = pageSize, Offset = offset }).ToList();
            
            return (patients, totalCount);
        }

        /// <summary>
        /// Soft delete a patient (mark as inactive)
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="userId">User performing the deletion</param>
        /// <returns>True if successful</returns>
        public bool DeletePatient(int id, string userId)
        {
            var sql = "UPDATE Patients SET IsActive = 0, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @Id";
            var rowsAffected = _db.Execute(sql, new { Id = id });
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"Patient deleted (ID: {id})");
                
                _auditLogger?.LogSystemEvent(
                    userId: userId,
                    userName: "System",
                    action: "PATIENT_DELETE",
                    category: "PATIENT",
                    details: new { PatientId = id }
                );
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Add an exam for a patient
        /// </summary>
        /// <param name="exam">Exam data</param>
        /// <param name="userId">User creating the exam</param>
        /// <returns>Exam ID</returns>
        public int AddExam(Exam exam, string userId)
        {
            var sql = @"
                INSERT INTO Exams (
                    PatientId, ExamType, ExamDate, Status, TechnicianId, DoctorId, Notes
                ) VALUES (
                    @PatientId, @ExamType, @ExamDate, @Status, @TechnicianId, @DoctorId, @Notes
                );
                SELECT last_insert_rowid();";

            var id = _db.QuerySingle<int>(sql, exam);
            
            _logger?.LogInformation($"Exam added: {exam.ExamType} for patient {exam.PatientId}");
            
            _auditLogger?.LogSystemEvent(
                userId: userId,
                userName: "System",
                action: "EXAM_CREATE",
                category: "EXAM",
                details: new { ExamId = id, PatientId = exam.PatientId, ExamType = exam.ExamType }
            );

            return id;
        }

        /// <summary>
        /// Get exams for a patient
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <returns>List of exams</returns>
        public List<Exam> GetPatientExams(int patientId)
        {
            var sql = @"
                SELECT * FROM Exams 
                WHERE PatientId = @PatientId 
                ORDER BY ExamDate DESC";

            return _db.Query<Exam>(sql, new { PatientId = patientId }).ToList();
        }

        /// <summary>
        /// Update exam results
        /// </summary>
        /// <param name="examId">Exam ID</param>
        /// <param name="results">Exam results</param>
        /// <param name="status">Exam status</param>
        /// <param name="userId">User updating the exam</param>
        /// <returns>True if successful</returns>
        public bool UpdateExamResults(int examId, string results, string status, string userId)
        {
            var sql = @"
                UPDATE Exams 
                SET Results = @Results, Status = @Status, UpdatedAt = CURRENT_TIMESTAMP
                WHERE Id = @Id";

            var rowsAffected = _db.Execute(sql, new { Id = examId, Results = results, Status = status });
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"Exam results updated (ID: {examId})");
                
                _auditLogger?.LogSystemEvent(
                    userId: userId,
                    userName: "System",
                    action: "EXAM_UPDATE",
                    category: "EXAM",
                    details: new { ExamId = examId, Status = status }
                );
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get patient statistics
        /// </summary>
        /// <returns>Patient statistics</returns>
        public PatientStatistics GetStatistics()
        {
            var sql = @"
                SELECT 
                    COUNT(*) as TotalPatients,
                    COUNT(CASE WHEN Gender = 'Male' THEN 1 END) as MalePatients,
                    COUNT(CASE WHEN Gender = 'Female' THEN 1 END) as FemalePatients,
                    COUNT(CASE WHEN DateOfBirth >= date('now', '-18 years') THEN 1 END) as AdultPatients,
                    COUNT(CASE WHEN DateOfBirth < date('now', '-18 years') THEN 1 END) as MinorPatients,
                    MIN(CreatedAt) as FirstPatient,
                    MAX(CreatedAt) as LastPatient
                FROM Patients 
                WHERE IsActive = 1";

            return _db.QueryFirstOrDefault<PatientStatistics>(sql) ?? new PatientStatistics();
        }

        /// <summary>
        /// Validate patient data
        /// </summary>
        /// <param name="patient">Patient to validate</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidatePatient(Patient patient)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            if (string.IsNullOrWhiteSpace(patient.FirstName))
            {
                result.IsValid = false;
                result.Errors.Add("First name is required");
            }

            if (string.IsNullOrWhiteSpace(patient.LastName))
            {
                result.IsValid = false;
                result.Errors.Add("Last name is required");
            }

            if (patient.DateOfBirth.HasValue && patient.DateOfBirth.Value > DateTime.Now)
            {
                result.IsValid = false;
                result.Errors.Add("Date of birth cannot be in the future");
            }

            if (!string.IsNullOrWhiteSpace(patient.Email) && !IsValidEmail(patient.Email))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid email format");
            }

            if (!string.IsNullOrWhiteSpace(patient.PhoneNumber) && !IsValidPhone(patient.PhoneNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Invalid phone number format");
            }

            return result;
        }

        /// <summary>
        /// Generate a unique Medical Record Number
        /// </summary>
        /// <returns>Generated MRN</returns>
        private string GenerateMRN()
        {
            var year = DateTime.Now.Year;
            var sql = "SELECT COUNT(*) FROM Patients WHERE MedicalRecordNumber LIKE @Pattern";
            var count = _db.QuerySingle<int>(sql, new { Pattern = $"{year}%" });
            
            return $"{year}{count + 1:D6}";
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

        /// <summary>
        /// Validate phone number format
        /// </summary>
        private bool IsValidPhone(string phone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\+]?[0-9\s\-\(\)]{7,15}$");
        }
    }

    /// <summary>
    /// Patient statistics
    /// </summary>
    public class PatientStatistics
    {
        public int TotalPatients { get; set; }
        public int MalePatients { get; set; }
        public int FemalePatients { get; set; }
        public int AdultPatients { get; set; }
        public int MinorPatients { get; set; }
        public DateTime? FirstPatient { get; set; }
        public DateTime? LastPatient { get; set; }
    }
}