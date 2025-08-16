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
    /// Service for managing microscope calibration data
    /// </summary>
    public class CalibrationService
    {
        private readonly IDbConnection _db;
        private readonly ILogger<CalibrationService> _logger;

        public CalibrationService(IDbConnection db, ILogger<CalibrationService> logger = null)
        {
            _db = db;
            _logger = logger;
            InitializeDatabase();
        }

        /// <summary>
        /// Initialize calibration table if it doesn't exist
        /// </summary>
        private void InitializeDatabase()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Calibrations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    MicronsPerPixel REAL NOT NULL,
                    FPS REAL NOT NULL,
                    Objective TEXT,
                    Magnification INTEGER,
                    CameraModel TEXT,
                    CreatedBy TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    IsActive INTEGER DEFAULT 1,
                    Notes TEXT
                )";

            _db.Execute(sql);
            _logger?.LogInformation("Calibration database initialized");
        }

        /// <summary>
        /// Save a new calibration
        /// </summary>
        /// <param name="calibration">Calibration data</param>
        /// <returns>Calibration ID</returns>
        public int SaveCalibration(CalibrationData calibration)
        {
            // Deactivate all existing calibrations if this is set as active
            if (calibration.IsActive)
            {
                var deactivateSql = "UPDATE Calibrations SET IsActive = 0";
                _db.Execute(deactivateSql);
            }

            var sql = @"
                INSERT INTO Calibrations (Name, MicronsPerPixel, FPS, Objective, Magnification, CameraModel, CreatedBy, IsActive, Notes)
                VALUES (@Name, @MicronsPerPixel, @FPS, @Objective, @Magnification, @CameraModel, @CreatedBy, @IsActive, @Notes);
                SELECT last_insert_rowid();";

            var id = _db.QuerySingle<int>(sql, calibration);
            
            _logger?.LogInformation($"Calibration saved: {calibration.Name} (ID: {id})");
            return id;
        }

        /// <summary>
        /// Get the most recent active calibration
        /// </summary>
        /// <returns>Latest calibration or null if none exists</returns>
        public CalibrationData GetLatestCalibration()
        {
            var sql = @"
                SELECT * FROM Calibrations 
                WHERE IsActive = 1 
                ORDER BY CreatedAt DESC 
                LIMIT 1";

            var result = _db.QueryFirstOrDefault<CalibrationData>(sql);
            
            if (result != null)
            {
                _logger?.LogInformation($"Retrieved calibration: {result.Name} (MicronsPerPixel: {result.MicronsPerPixel})");
            }
            else
            {
                _logger?.LogWarning("No active calibration found");
            }
            
            return result;
        }

        /// <summary>
        /// Get calibration by ID
        /// </summary>
        /// <param name="id">Calibration ID</param>
        /// <returns>Calibration data or null if not found</returns>
        public CalibrationData GetCalibrationById(int id)
        {
            var sql = "SELECT * FROM Calibrations WHERE Id = @Id";
            return _db.QueryFirstOrDefault<CalibrationData>(sql, new { Id = id });
        }

        /// <summary>
        /// Get all calibrations
        /// </summary>
        /// <returns>List of all calibrations</returns>
        public List<CalibrationData> GetAllCalibrations()
        {
            var sql = "SELECT * FROM Calibrations ORDER BY CreatedAt DESC";
            return _db.Query<CalibrationData>(sql).ToList();
        }

        /// <summary>
        /// Update an existing calibration
        /// </summary>
        /// <param name="calibration">Updated calibration data</param>
        /// <returns>True if successful</returns>
        public bool UpdateCalibration(CalibrationData calibration)
        {
            if (calibration.Id <= 0)
                return false;

            // Deactivate all other calibrations if this is set as active
            if (calibration.IsActive)
            {
                var deactivateSql = "UPDATE Calibrations SET IsActive = 0 WHERE Id != @Id";
                _db.Execute(deactivateSql, new { calibration.Id });
            }

            var sql = @"
                UPDATE Calibrations 
                SET Name = @Name, MicronsPerPixel = @MicronsPerPixel, FPS = @FPS, 
                    Objective = @Objective, Magnification = @Magnification, 
                    CameraModel = @CameraModel, IsActive = @IsActive, Notes = @Notes
                WHERE Id = @Id";

            var rowsAffected = _db.Execute(sql, calibration);
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"Calibration updated: {calibration.Name} (ID: {calibration.Id})");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Delete a calibration
        /// </summary>
        /// <param name="id">Calibration ID</param>
        /// <returns>True if successful</returns>
        public bool DeleteCalibration(int id)
        {
            var sql = "DELETE FROM Calibrations WHERE Id = @Id";
            var rowsAffected = _db.Execute(sql, new { Id = id });
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"Calibration deleted (ID: {id})");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Set a calibration as active
        /// </summary>
        /// <param name="id">Calibration ID</param>
        /// <returns>True if successful</returns>
        public bool SetActiveCalibration(int id)
        {
            // First deactivate all calibrations
            var deactivateSql = "UPDATE Calibrations SET IsActive = 0";
            _db.Execute(deactivateSql);

            // Then activate the specified one
            var activateSql = "UPDATE Calibrations SET IsActive = 1 WHERE Id = @Id";
            var rowsAffected = _db.Execute(activateSql, new { Id = id });
            
            if (rowsAffected > 0)
            {
                _logger?.LogInformation($"Calibration activated (ID: {id})");
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Validate calibration data
        /// </summary>
        /// <param name="calibration">Calibration to validate</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidateCalibration(CalibrationData calibration)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            if (string.IsNullOrWhiteSpace(calibration.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Calibration name is required");
            }

            if (calibration.MicronsPerPixel <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Microns per pixel must be greater than 0");
            }

            if (calibration.FPS <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Frame rate must be greater than 0");
            }

            if (calibration.MicronsPerPixel > 100)
            {
                result.IsValid = false;
                result.Errors.Add("Microns per pixel seems too high (>100). Please check calibration.");
            }

            if (calibration.FPS > 1000)
            {
                result.IsValid = false;
                result.Errors.Add("Frame rate seems too high (>1000 FPS). Please check calibration.");
            }

            return result;
        }

        /// <summary>
        /// Calculate microns per pixel from calibration slide measurement
        /// </summary>
        /// <param name="knownDistanceMicrons">Known distance on calibration slide (µm)</param>
        /// <param name="measuredPixels">Measured distance in pixels</param>
        /// <returns>Microns per pixel</returns>
        public double CalculateMicronsPerPixel(double knownDistanceMicrons, double measuredPixels)
        {
            if (measuredPixels <= 0)
                throw new ArgumentException("Measured pixels must be greater than 0");

            return knownDistanceMicrons / measuredPixels;
        }

        /// <summary>
        /// Get calibration statistics
        /// </summary>
        /// <returns>Calibration statistics</returns>
        public CalibrationStatistics GetStatistics()
        {
            var sql = @"
                SELECT 
                    COUNT(*) as TotalCalibrations,
                    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCalibrations,
                    AVG(MicronsPerPixel) as AverageMicronsPerPixel,
                    AVG(FPS) as AverageFPS,
                    MIN(CreatedAt) as FirstCalibration,
                    MAX(CreatedAt) as LastCalibration
                FROM Calibrations";

            return _db.QueryFirstOrDefault<CalibrationStatistics>(sql) ?? new CalibrationStatistics();
        }
    }

    /// <summary>
    /// Calibration data model
    /// </summary>
    public class CalibrationData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double MicronsPerPixel { get; set; }
        public double FPS { get; set; }
        public string Objective { get; set; } = "";
        public int Magnification { get; set; }
        public string CameraModel { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; } = "";

        public CalibrationData()
        {
            CreatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Calibration statistics
    /// </summary>
    public class CalibrationStatistics
    {
        public int TotalCalibrations { get; set; }
        public int ActiveCalibrations { get; set; }
        public double AverageMicronsPerPixel { get; set; }
        public double AverageFPS { get; set; }
        public DateTime? FirstCalibration { get; set; }
        public DateTime? LastCalibration { get; set; }
    }
>>>>>>> release/v1.0.0
}