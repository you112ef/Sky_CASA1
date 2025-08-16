using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;
using System.Linq;

namespace MedicalLabAnalyzer.Services
{
    public class StoolAnalyzer
    {
        private readonly IDbConnection _db;
        private readonly ILogger<StoolAnalyzer> _logger;
        private readonly AuditLogger _auditLogger;

        public StoolAnalyzer(IDbConnection db, ILogger<StoolAnalyzer> logger = null, AuditLogger auditLogger = null)
        {
            _db = db;
            _logger = logger;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// تحليل نتائج البراز وتحديد الحالة
        /// </summary>
        public StoolTestResult AnalyzeStoolResults(Dictionary<string, object> values, int examId, string userId, string userName)
        {
            try
            {
                var result = new StoolTestResult
                {
                    ExamId = examId,
                    Color = values.GetValueOrDefault("Color", "Brown").ToString(),
                    Consistency = values.GetValueOrDefault("Consistency", "Formed").ToString(),
                    Shape = values.GetValueOrDefault("Shape", "Normal").ToString(),
                    Weight = ParseDoubleValue(values, "Weight"),
                    Odor = values.GetValueOrDefault("Odor", "Normal").ToString(),
                    pH = ParseDoubleValue(values, "pH"),
                    OccultBlood = values.GetValueOrDefault("OccultBlood", "Negative").ToString(),
                    ReducingSubstances = values.GetValueOrDefault("ReducingSubstances", "Negative").ToString(),
                    FatContent = values.GetValueOrDefault("FatContent", "Normal").ToString(),
                    Mucus = values.GetValueOrDefault("Mucus", "None").ToString(),
                    UndigestedFood = values.GetValueOrDefault("UndigestedFood", "None").ToString(),
                    MuscleFiber = values.GetValueOrDefault("MuscleFiber", "None").ToString(),
                    Starch = values.GetValueOrDefault("Starch", "None").ToString(),
                    FatGlobules = values.GetValueOrDefault("FatGlobules", "None").ToString(),
                    Parasites = values.GetValueOrDefault("Parasites", "None").ToString(),
                    Ova = values.GetValueOrDefault("Ova", "None").ToString(),
                    CalprotectinValue = ParseDoubleValue(values, "CalprotectinValue"),
                    LactoferrinValue = ParseDoubleValue(values, "LactoferrinValue"),
                    Alpha1AntitrypsinValue = ParseDoubleValue(values, "Alpha1AntitrypsinValue"),
                    TestDate = DateTime.Now
                };

                // التحقق من القيم المرجعية
                result.ValidateResults();

                // حفظ النتائج في قاعدة البيانات
                SaveStoolResult(result);

                // تسجيل في AuditLog
                _auditLogger?.LogSystemEvent(userId, userName, "Stool_Analysis_Completed", "Stool", 
                    new { ExamId = examId, Status = result.Status, AbnormalCount = result.GetAbnormalValues().Count });

                _logger?.LogInformation("Stool analysis completed for exam {ExamId} with status {Status}", examId, result.Status);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing stool results for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// تحليل البراز من ملف Excel أو CSV
        /// </summary>
        public async Task<StoolTestResult> AnalyzeStoolFromFileAsync(string filePath, int examId, string userId, string userName)
        {
            try
            {
                var values = await ParseStoolFileAsync(filePath);
                return AnalyzeStoolResults(values, examId, userId, userName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing stool from file {FilePath} for exam {ExamId}", filePath, examId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على نتائج البراز من قاعدة البيانات
        /// </summary>
        public async Task<StoolTestResult> GetStoolResultAsync(int examId)
        {
            try
            {
                var sql = "SELECT * FROM Stool_Results WHERE ExamId = @ExamId";
                return await _db.QueryFirstOrDefaultAsync<StoolTestResult>(sql, new { ExamId = examId });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving stool result for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على جميع نتائج البراز لمريض معين
        /// </summary>
        public async Task<List<StoolTestResult>> GetPatientStoolResultsAsync(int patientId)
        {
            try
            {
                var sql = @"
                    SELECT s.* FROM Stool_Results s
                    JOIN Exams e ON s.ExamId = e.ExamId
                    WHERE e.PatientId = @PatientId
                    ORDER BY s.CreatedAt DESC";
                
                var results = await _db.QueryAsync<StoolTestResult>(sql, new { PatientId = patientId });
                return results.AsList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving stool results for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// تحديث نتائج البراز
        /// </summary>
        public async Task<bool> UpdateStoolResultAsync(StoolTestResult result, string userId, string userName)
        {
            try
            {
                result.ValidateResults();
                
                var sql = @"
                    UPDATE Stool_Results SET
                        Color = @Color, Consistency = @Consistency, Shape = @Shape, Weight = @Weight,
                        Odor = @Odor, pH = @pH, OccultBlood = @OccultBlood, ReducingSubstances = @ReducingSubstances,
                        FatContent = @FatContent, Mucus = @Mucus, UndigestedFood = @UndigestedFood,
                        MuscleFiber = @MuscleFiber, Starch = @Starch, FatGlobules = @FatGlobules,
                        Parasites = @Parasites, Ova = @Ova, CalprotectinValue = @CalprotectinValue,
                        LactoferrinValue = @LactoferrinValue, Alpha1AntitrypsinValue = @Alpha1AntitrypsinValue,
                        CreatedAt = @TestDate
                    WHERE ExamId = @ExamId";
                
                var rowsAffected = await _db.ExecuteAsync(sql, result);
                
                if (rowsAffected > 0)
                {
                    _auditLogger?.LogSystemEvent(userId, userName, "Stool_Result_Updated", "Stool", 
                        new { ExamId = result.ExamId, Status = result.Status });
                    
                    _logger?.LogInformation("Stool result updated for exam {ExamId}", result.ExamId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating stool result for exam {ExamId}", result.ExamId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات البراز
        /// </summary>
        public async Task<StoolStatistics> GetStoolStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var whereClause = "";
                var parameters = new DynamicParameters();
                
                if (startDate.HasValue && endDate.HasValue)
                {
                    whereClause = "WHERE CreatedAt BETWEEN @StartDate AND @EndDate";
                    parameters.Add("@StartDate", startDate.Value);
                    parameters.Add("@EndDate", endDate.Value);
                }

                var sql = $@"
                    SELECT 
                        COUNT(*) as TotalTests,
                        COUNT(CASE WHEN Status = 'Normal' THEN 1 END) as NormalTests,
                        COUNT(CASE WHEN Status = 'Abnormal' THEN 1 END) as AbnormalTests,
                        COUNT(CASE WHEN Status = 'Critical' THEN 1 END) as CriticalTests,
                        AVG(pH) as AvgpH,
                        AVG(Weight) as AvgWeight
                    FROM Stool_Results {whereClause}";

                var stats = await _db.QueryFirstAsync<StoolStatistics>(sql, parameters);
                return stats;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving stool statistics");
                throw;
            }
        }

        /// <summary>
        /// تحليل الأنماط المرضية
        /// </summary>
        public StoolPatternAnalysis AnalyzePatterns(StoolTestResult result)
        {
            var analysis = new StoolPatternAnalysis();
            
            // Inflammatory Bowel Disease Analysis
            if (result.CalprotectinValue > 50 || result.LactoferrinValue > 7.25)
            {
                analysis.HasIBD = true;
                analysis.IBDSeverity = DetermineIBDSeverity(result);
            }
            
            // Malabsorption Analysis
            if (result.FatContent.ToLower() == "increased" || 
                result.UndigestedFood.ToLower() == "present" ||
                result.MuscleFiber.ToLower() == "present")
            {
                analysis.HasMalabsorption = true;
                analysis.MalabsorptionType = DetermineMalabsorptionType(result);
            }
            
            // Parasitic Infection Analysis
            if (result.Parasites.ToLower() != "none" || result.Ova.ToLower() != "none")
            {
                analysis.HasParasiticInfection = true;
                analysis.ParasiteType = result.Parasites;
            }
            
            // Bleeding Analysis
            if (result.OccultBlood.ToLower() == "positive")
            {
                analysis.HasBleeding = true;
                analysis.BleedingType = "Occult";
            }
            
            // Steatorrhea Analysis
            if (result.FatContent.ToLower() == "increased" || result.FatGlobules.ToLower() == "present")
            {
                analysis.HasSteatorrhea = true;
            }
            
            return analysis;
        }

        #region Private Methods

        private double ParseDoubleValue(Dictionary<string, object> values, string key)
        {
            if (values.TryGetValue(key, out var value))
            {
                if (value is double doubleValue)
                    return doubleValue;
                if (value is string stringValue && double.TryParse(stringValue, out double parsed))
                    return parsed;
                if (value is int intValue)
                    return intValue;
            }
            return 0.0;
        }

        private void SaveStoolResult(StoolTestResult result)
        {
            var sql = @"
                INSERT INTO Stool_Results (ExamId, Color, Consistency, Shape, Weight, Odor, pH, 
                    OccultBlood, ReducingSubstances, FatContent, Mucus, UndigestedFood, MuscleFiber,
                    Starch, FatGlobules, Parasites, Ova, CalprotectinValue, LactoferrinValue,
                    Alpha1AntitrypsinValue, CreatedAt)
                VALUES (@ExamId, @Color, @Consistency, @Shape, @Weight, @Odor, @pH, 
                    @OccultBlood, @ReducingSubstances, @FatContent, @Mucus, @UndigestedFood, @MuscleFiber,
                    @Starch, @FatGlobules, @Parasites, @Ova, @CalprotectinValue, @LactoferrinValue,
                    @Alpha1AntitrypsinValue, @TestDate)";
            
            _db.Execute(sql, result);
        }

        private async Task<Dictionary<string, object>> ParseStoolFileAsync(string filePath)
        {
            // هذا مثال مبسط - في التطبيق الحقيقي ستحتاج لاستخدام مكتبة لقراءة Excel/CSV
            var values = new Dictionary<string, object>();
            
            // قراءة الملف واستخراج القيم
            // يمكن استخدام EPPlus لقراءة Excel أو CsvHelper لقراءة CSV
            
            return values;
        }

        private string DetermineIBDSeverity(StoolTestResult result)
        {
            if (result.CalprotectinValue > 200 || result.LactoferrinValue > 15)
                return "Severe";
            else if (result.CalprotectinValue > 100 || result.LactoferrinValue > 10)
                return "Moderate";
            else
                return "Mild";
        }

        private string DetermineMalabsorptionType(StoolTestResult result)
        {
            if (result.FatContent.ToLower() == "increased")
                return "Fat Malabsorption";
            else if (result.UndigestedFood.ToLower() == "present")
                return "General Malabsorption";
            else if (result.MuscleFiber.ToLower() == "present")
                return "Protein Malabsorption";
            else
                return "Unknown";
        }

        #endregion
    }

    #region Data Classes

    public class StoolStatistics
    {
        public int TotalTests { get; set; }
        public int NormalTests { get; set; }
        public int AbnormalTests { get; set; }
        public int CriticalTests { get; set; }
        public double AvgpH { get; set; }
        public double AvgWeight { get; set; }
        
        public double NormalPercentage => TotalTests > 0 ? (double)NormalTests / TotalTests * 100 : 0;
        public double AbnormalPercentage => TotalTests > 0 ? (double)AbnormalTests / TotalTests * 100 : 0;
        public double CriticalPercentage => TotalTests > 0 ? (double)CriticalTests / TotalTests * 100 : 0;
    }

    public class StoolPatternAnalysis
    {
        public bool HasIBD { get; set; }
        public string IBDSeverity { get; set; }
        public bool HasMalabsorption { get; set; }
        public string MalabsorptionType { get; set; }
        public bool HasParasiticInfection { get; set; }
        public string ParasiteType { get; set; }
        public bool HasBleeding { get; set; }
        public string BleedingType { get; set; }
        public bool HasSteatorrhea { get; set; }
        
        public List<string> GetPatterns()
        {
            var patterns = new List<string>();
            
            if (HasIBD)
                patterns.Add($"IBD: {IBDSeverity}");
            
            if (HasMalabsorption)
                patterns.Add($"Malabsorption: {MalabsorptionType}");
            
            if (HasParasiticInfection)
                patterns.Add($"Parasitic Infection: {ParasiteType}");
            
            if (HasBleeding)
                patterns.Add($"Bleeding: {BleedingType}");
            
            if (HasSteatorrhea)
                patterns.Add("Steatorrhea");
            
            return patterns;
        }
    }

    #endregion
}