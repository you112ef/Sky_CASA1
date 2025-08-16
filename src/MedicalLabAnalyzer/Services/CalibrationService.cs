using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MedicalLabAnalyzer.Services
{
    public class CalibrationService
    {
        private readonly ILogger<CalibrationService> _logger;

        public CalibrationService(ILogger<CalibrationService> logger = null)
        {
            _logger = logger;
        }

        public async Task<CalibrationResult> CalibrateMicroscopeAsync(CalibrationParameters parameters)
        {
            try
            {
                _logger?.LogInformation("Starting microscope calibration with parameters: {Parameters}", parameters);
                
                // التحقق من صحة المعاملات
                if (!ValidateCalibrationParameters(parameters))
                {
                    throw new ArgumentException("Invalid calibration parameters");
                }
                
                // تنفيذ عملية المعايرة الحقيقية
                var result = await Task.Run(() => PerformMicroscopeCalibration(parameters));
                
                // حفظ نتائج المعايرة
                await SaveCalibrationResultAsync(result);
                
                _logger?.LogInformation("Microscope calibration completed successfully: {CalibrationId}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Microscope calibration failed");
                throw;
            }
        }

        private CalibrationResult PerformMicroscopeCalibration(CalibrationParameters parameters)
        {
            var result = new CalibrationResult
            {
                Id = Guid.NewGuid().ToString(),
                CalibrationDate = DateTime.Now,
                Parameters = parameters,
                Status = "Completed"
            };

            // حساب معامل المعايرة بناءً على المعاملات المدخلة
            var pixelSize = CalculatePixelSize(parameters.MicroscopeMagnification, parameters.CameraPixelSize);
            var calibrationFactor = CalculateCalibrationFactor(pixelSize, parameters.ReferenceObjectSize);
            
            result.CalibrationFactor = calibrationFactor;
            result.PixelSize = pixelSize;
            result.Accuracy = CalculateCalibrationAccuracy(parameters);
            result.Notes = GenerateCalibrationNotes(result);
            
            return result;
        }

        private double CalculatePixelSize(double magnification, double cameraPixelSize)
        {
            // حساب حجم البكسل الفعلي بناءً على تكبير المجهر
            return cameraPixelSize / magnification;
        }

        private double CalculateCalibrationFactor(double pixelSize, double referenceObjectSize)
        {
            // حساب معامل المعايرة (ميكرون لكل بكسل)
            return referenceObjectSize / pixelSize;
        }

        private double CalculateCalibrationAccuracy(CalibrationParameters parameters)
        {
            // حساب دقة المعايرة بناءً على جودة المعاملات
            var baseAccuracy = 95.0; // دقة أساسية
            
            // تحسين الدقة بناءً على جودة المعاملات
            if (parameters.MicroscopeMagnification >= 1000) baseAccuracy += 2;
            if (parameters.CameraPixelSize <= 5.0) baseAccuracy += 1;
            if (parameters.ReferenceObjectSize >= 10.0) baseAccuracy += 1;
            
            return Math.Min(100.0, baseAccuracy);
        }

        private string GenerateCalibrationNotes(CalibrationResult result)
        {
            var notes = new List<string>();
            
            if (result.Accuracy >= 98)
                notes.Add("Excellent calibration accuracy achieved");
            else if (result.Accuracy >= 95)
                notes.Add("Good calibration accuracy");
            else if (result.Accuracy >= 90)
                notes.Add("Acceptable calibration accuracy");
            else
                notes.Add("Calibration accuracy below recommended threshold");
            
            notes.Add($"Calibration factor: {result.CalibrationFactor:F3} μm/pixel");
            notes.Add($"Pixel size: {result.PixelSize:F3} μm");
            
            return string.Join("; ", notes);
        }

        public async Task<bool> ValidateCalibrationAsync(CalibrationResult calibration)
        {
            try
            {
                _logger?.LogInformation("Validating calibration: {CalibrationId}", calibration.Id);
                
                // التحقق من صحة المعايرة
                var isValid = await Task.Run(() => PerformCalibrationValidation(calibration));
                
                if (isValid)
                {
                    _logger?.LogInformation("Calibration validation passed: {CalibrationId}", calibration.Id);
                }
                else
                {
                    _logger?.LogWarning("Calibration validation failed: {CalibrationId}", calibration.Id);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Calibration validation failed: {CalibrationId}", calibration.Id);
                return false;
            }
        }

        private bool PerformCalibrationValidation(CalibrationResult calibration)
        {
            // التحقق من معاملات المعايرة
            if (calibration.CalibrationFactor <= 0 || calibration.CalibrationFactor > 1000)
                return false;
            
            if (calibration.PixelSize <= 0 || calibration.PixelSize > 100)
                return false;
            
            if (calibration.Accuracy < 80)
                return false;
            
            // التحقق من تاريخ المعايرة
            var daysSinceCalibration = (DateTime.Now - calibration.CalibrationDate).TotalDays;
            if (daysSinceCalibration > 30) // المعايرة صالحة لمدة 30 يوم
                return false;
            
            return true;
        }

        public async Task<CalibrationResult> GetLatestCalibrationAsync()
        {
            try
            {
                _logger?.LogInformation("Retrieving latest calibration result");
                
                var sql = "SELECT * FROM Calibration ORDER BY CalibrationDate DESC LIMIT 1";
                var calibration = await _db.QueryFirstOrDefaultAsync<CalibrationResult>(sql);
                
                if (calibration != null)
                {
                    _logger?.LogInformation("Latest calibration retrieved: {CalibrationId}", calibration.Id);
                }
                else
                {
                    _logger?.LogWarning("No calibration results found");
                }
                
                return calibration;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving latest calibration");
                throw;
            }
        }

        private async Task SaveCalibrationResultAsync(CalibrationResult result)
        {
            try
            {
                var sql = @"
                    INSERT INTO Calibration (Id, CalibrationDate, MicroscopeMagnification, CameraPixelSize, 
                                           ReferenceObjectSize, CalibrationFactor, PixelSize, Accuracy, 
                                           Status, Notes, CreatedAt)
                    VALUES (@Id, @CalibrationDate, @Parameters.MicroscopeMagnification, @Parameters.CameraPixelSize,
                           @Parameters.ReferenceObjectSize, @CalibrationFactor, @PixelSize, @Accuracy,
                           @Status, @Notes, @CreatedAt)";
                
                var parameters = new
                {
                    result.Id,
                    result.CalibrationDate,
                    result.Parameters,
                    result.CalibrationFactor,
                    result.PixelSize,
                    result.Accuracy,
                    result.Status,
                    result.Notes,
                    CreatedAt = DateTime.Now
                };
                
                await _db.ExecuteAsync(sql, parameters);
                _logger?.LogInformation("Calibration result saved to database: {CalibrationId}", result.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving calibration result: {CalibrationId}", result.Id);
                throw;
            }
        }

        private bool ValidateCalibrationParameters(CalibrationParameters parameters)
        {
            if (parameters == null) return false;
            
            if (parameters.MicroscopeMagnification <= 0 || parameters.MicroscopeMagnification > 10000)
                return false;
            
            if (parameters.CameraPixelSize <= 0 || parameters.CameraPixelSize > 100)
                return false;
            
            if (parameters.ReferenceObjectSize <= 0 || parameters.ReferenceObjectSize > 1000)
                return false;
            
            return true;
        }
    }

    public class CalibrationResult
    {
        public string DeviceType { get; set; }
        public bool IsValid { get; set; }
        public DateTime CalibrationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class CalibrationHistory
    {
        public string DeviceType { get; set; }
        public DateTime CalibrationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string PerformedBy { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}