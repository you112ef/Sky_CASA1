using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Models;
using System.Linq; // Added for .ToList()
using Dapper; // Added for Dapper
using System.Data; // Added for IDbConnection

namespace MedicalLabAnalyzer.Services
{
    public class PatientService
    {
        private readonly ILogger<PatientService> _logger;
        private readonly IDbConnection _db; // Added for database connection

        public PatientService(ILogger<PatientService> logger = null, IDbConnection db = null)
        {
            _logger = logger;
            _db = db; // Initialize database connection
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            try
            {
                _logger?.LogInformation("Creating new patient: {FirstName} {LastName}", patient.FirstName, patient.LastName);
                
                var sql = @"
                    INSERT INTO Patients (FirstName, LastName, DateOfBirth, Gender, Phone, Email, Address, MedicalHistory, Allergies, EmergencyContact, EmergencyPhone, InsuranceProvider, InsuranceNumber, CreatedAt, UpdatedAt)
                    VALUES (@FirstName, @LastName, @DateOfBirth, @Gender, @Phone, @Email, @Address, @MedicalHistory, @Allergies, @EmergencyContact, @EmergencyPhone, @InsuranceProvider, @InsuranceNumber, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();";
                
                var patientId = await _db.QuerySingleAsync<int>(sql, patient);
                patient.Id = patientId;
                
                _logger?.LogInformation("Patient created successfully with ID: {PatientId}", patientId);
                return patient;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating patient: {FirstName} {LastName}", patient.FirstName, patient.LastName);
                throw;
            }
        }

        public async Task<bool> UpdatePatientAsync(Patient patient)
        {
            try
            {
                _logger?.LogInformation("Updating patient: {PatientId}", patient.Id);
                
                var sql = @"
                    UPDATE Patients 
                    SET FirstName = @FirstName, LastName = @LastName, DateOfBirth = @DateOfBirth, 
                        Gender = @Gender, Phone = @Phone, Email = @Email, Address = @Address, 
                        MedicalHistory = @MedicalHistory, Allergies = @Allergies, 
                        EmergencyContact = @EmergencyContact, EmergencyPhone = @EmergencyPhone, 
                        InsuranceProvider = @InsuranceProvider, InsuranceNumber = @InsuranceNumber, 
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id";
                
                var rowsAffected = await _db.ExecuteAsync(sql, patient);
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("Patient updated successfully: {PatientId}", patient.Id);
                }
                else
                {
                    _logger?.LogWarning("No patient found to update: {PatientId}", patient.Id);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating patient: {PatientId}", patient.Id);
                throw;
            }
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            try
            {
                _logger?.LogInformation("Deleting patient: {PatientId}", patientId);
                
                var sql = "DELETE FROM Patients WHERE Id = @PatientId";
                var rowsAffected = await _db.ExecuteAsync(sql, new { PatientId = patientId });
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("Patient deleted successfully: {PatientId}", patientId);
                }
                else
                {
                    _logger?.LogWarning("No patient found to delete: {PatientId}", patientId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting patient: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            try
            {
                _logger?.LogInformation("Retrieving all patients");
                
                var sql = "SELECT * FROM Patients ORDER BY LastName, FirstName";
                var patients = await _db.QueryAsync<Patient>(sql);
                
                _logger?.LogInformation("Retrieved {Count} patients", patients.Count());
                return patients.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving all patients");
                throw;
            }
        }

        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
            try
            {
                _logger?.LogInformation("Retrieving patient with ID: {PatientId}", patientId);
                
                var sql = "SELECT * FROM Patients WHERE Id = @PatientId";
                var patient = await _db.QueryFirstOrDefaultAsync<Patient>(sql, new { PatientId = patientId });
                
                if (patient != null)
                {
                    _logger?.LogInformation("Patient retrieved successfully: {PatientId}", patientId);
                }
                else
                {
                    _logger?.LogWarning("Patient not found: {PatientId}", patientId);
                }
                
                return patient;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving patient: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
        {
            try
            {
                _logger?.LogInformation("Searching patients with term: {SearchTerm}", searchTerm);
                
                var sql = @"
                    SELECT * FROM Patients 
                    WHERE FirstName LIKE @SearchPattern 
                       OR LastName LIKE @SearchPattern 
                       OR Phone LIKE @SearchPattern 
                       OR Email LIKE @SearchPattern 
                       OR InsuranceNumber LIKE @SearchPattern
                    ORDER BY LastName, FirstName";
                
                var searchPattern = $"%{searchTerm}%";
                var patients = await _db.QueryAsync<Patient>(sql, new { SearchPattern = searchPattern });
                
                _logger?.LogInformation("Found {Count} patients matching search term: {SearchTerm}", patients.Count(), searchTerm);
                return patients.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching patients with term: {SearchTerm}", searchTerm);
                throw;
            }
        }
    }
}