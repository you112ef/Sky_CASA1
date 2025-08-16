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
    public class UrineAnalyzer
    {
        private readonly IDbConnection _db;
        private readonly ILogger<UrineAnalyzer> _logger;
        private readonly AuditLogger _auditLogger;

        public UrineAnalyzer(IDbConnection db, ILogger<UrineAnalyzer> logger = null, AuditLogger auditLogger = null)
        {
            _db = db;
            _logger = logger;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// تحليل نتائج البول وتحديد الحالة
        /// </summary>
        public UrineTestResult AnalyzeUrineResults(Dictionary<string, object> values, int examId, string userId, string userName)
        {
            try
            {
                var result = new UrineTestResult
                {
                    ExamId = examId,
                    Color = values.GetValueOrDefault("Color", "Yellow").ToString(),
                    Turbidity = values.GetValueOrDefault("Turbidity", "Clear").ToString(),
                    pH = ParseDoubleValue(values, "pH"),
                    SpecificGravity = ParseDoubleValue(values, "SpecificGravity"),
                    Protein = values.GetValueOrDefault("Protein", "Negative").ToString(),
                    Glucose = values.GetValueOrDefault("Glucose", "Negative").ToString(),
                    Ketones = values.GetValueOrDefault("Ketones", "Negative").ToString(),
                    Blood = values.GetValueOrDefault("Blood", "Negative").ToString(),
                    LeukocyteEsterase = values.GetValueOrDefault("LeukocyteEsterase", "Negative").ToString(),
                    Nitrite = values.GetValueOrDefault("Nitrite", "Negative").ToString(),
                    MicroscopyNotes = values.GetValueOrDefault("MicroscopyNotes", "").ToString(),
                    TestDate = DateTime.Now
                };

                // التحقق من القيم المرجعية
                result.ValidateResults();

                // حفظ النتائج في قاعدة البيانات
                SaveUrineResult(result);

                // تسجيل في AuditLog
                _auditLogger?.LogSystemEvent(userId, userName, "Urine_Analysis_Completed", "Urine", 
                    new { ExamId = examId, Status = result.Status, AbnormalCount = result.GetAbnormalValues().Count });

                _logger?.LogInformation("Urine analysis completed for exam {ExamId} with status {Status}", examId, result.Status);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing urine results for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// تحليل البول من ملف Excel أو CSV
        /// </summary>
        public async Task<UrineTestResult> AnalyzeUrineFromFileAsync(string filePath, int examId, string userId, string userName)
        {
            try
            {
                var values = await ParseUrineFileAsync(filePath);
                return AnalyzeUrineResults(values, examId, userId, userName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing urine from file {FilePath} for exam {ExamId}", filePath, examId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على نتائج البول من قاعدة البيانات
        /// </summary>
        public async Task<UrineTestResult> GetUrineResultAsync(int examId)
        {
            try
            {
                var sql = "SELECT * FROM Urine_Results WHERE ExamId = @ExamId";
                return await _db.QueryFirstOrDefaultAsync<UrineTestResult>(sql, new { ExamId = examId });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving urine result for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على جميع نتائج البول لمريض معين
        /// </summary>
        public async Task<List<UrineTestResult>> GetPatientUrineResultsAsync(int patientId)
        {
            try
            {
                var sql = @"
                    SELECT u.* FROM Urine_Results u
                    JOIN Exams e ON u.ExamId = e.ExamId
                    WHERE e.PatientId = @PatientId
                    ORDER BY u.CreatedAt DESC";
                
                var results = await _db.QueryAsync<UrineTestResult>(sql, new { PatientId = patientId });
                return results.AsList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving urine results for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// تحديث نتائج البول
        /// </summary>
        public async Task<bool> UpdateUrineResultAsync(UrineTestResult result, string userId, string userName)
        {
            try
            {
                result.ValidateResults();
                
                var sql = @"
                    UPDATE Urine_Results SET
                        Color = @Color, Turbidity = @Turbidity, pH = @pH, SpecificGravity = @SpecificGravity,
                        Protein = @Protein, Glucose = @Glucose, Ketones = @Ketones, Blood = @Blood,
                        LeukocyteEsterase = @LeukocyteEsterase, Nitrite = @Nitrite, MicroscopyNotes = @MicroscopyNotes,
                        CreatedAt = @TestDate
                    WHERE ExamId = @ExamId";
                
                var rowsAffected = await _db.ExecuteAsync(sql, result);
                
                if (rowsAffected > 0)
                {
                    _auditLogger?.LogSystemEvent(userId, userName, "Urine_Result_Updated", "Urine", 
                        new { ExamId = result.ExamId, Status = result.Status });
                    
                    _logger?.LogInformation("Urine result updated for exam {ExamId}", result.ExamId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating urine result for exam {ExamId}", result.ExamId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات البول
        /// </summary>
        public async Task<UrineStatistics> GetUrineStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
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
                        AVG(SpecificGravity) as AvgSpecificGravity
                    FROM Urine_Results {whereClause}";

                var stats = await _db.QueryFirstAsync<UrineStatistics>(sql, parameters);
                return stats;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving urine statistics");
                throw;
            }
        }

        /// <summary>
        /// تحليل الأنماط المرضية
        /// </summary>
        public UrinePatternAnalysis AnalyzePatterns(UrineTestResult result)
        {
            var analysis = new UrinePatternAnalysis();
            
            // UTI Analysis
            if (result.LeukocyteEsterase.ToLower() == "positive" || 
                result.Nitrite.ToLower() == "positive" ||
                result.Blood.ToLower() == "positive")
            {
                analysis.HasUTI = true;
                analysis.UTISeverity = DetermineUTISeverity(result);
            }
            
            // Hematuria Analysis
            if (result.Blood.ToLower() == "trace")
                analysis.HasHematuria = true;
            else if (result.Blood.ToLower() == "positive")
                analysis.HasHematuria = true;
            
            // Proteinuria Analysis
            if (result.Protein.ToLower() == "trace")
                analysis.HasProteinuria = true;
            else if (result.Protein.ToLower() == "positive")
                analysis.HasProteinuria = true;
            else if (result.Protein.ToLower() == "++" || result.Protein.ToLower() == "+++")
                analysis.HasProteinuria = true;
            
            // Diabetes Analysis
            if (result.Glucose.ToLower() == "positive" || result.Glucose.ToLower() == "trace")
            {
                analysis.HasGlucosuria = true;
            }
            
            // Ketosis Analysis
            if (result.Ketones.ToLower() == "positive" || result.Ketones.ToLower() == "trace")
            {
                analysis.HasKetonuria = true;
            }
            
            return analysis;
        }

        /// <summary>
        /// التحقق من القيم الحرجة
        /// </summary>
        public List<string> CheckCriticalValues(UrineTestResult result)
        {
            var criticalValues = new List<string>();
            
            // pH Critical Values
            if (result.pH < 4.0)
                criticalValues.Add($"pH critically low: {result.pH:F1} (Critical: <4.0)");
            else if (result.pH > 9.0)
                criticalValues.Add($"pH critically high: {result.pH:F1} (Critical: >9.0)");
            
            // Specific Gravity Critical Values
            if (result.SpecificGravity < 1.001)
                criticalValues.Add($"Specific Gravity critically low: {result.SpecificGravity:F3} (Critical: <1.001)");
            else if (result.SpecificGravity > 1.040)
                criticalValues.Add($"Specific Gravity critically high: {result.SpecificGravity:F3} (Critical: >1.040)");
            
            // Microscopic Critical Values
            if (result.Blood.ToLower() == "positive")
            {
                if (result.RBC > 10)
                    criticalValues.Add($"RBC critically high: {result.RBC}/HPF (Critical: >10)");
                if (result.WBC > 100)
                    criticalValues.Add($"WBC critically high: {result.WBC}/HPF (Critical: >100)");
            }
            
            // Chemical Critical Values
            if (result.Protein.ToLower() == "positive" || result.Protein.ToLower() == "trace")
            {
                if (result.Protein.ToLower() == "++" || result.Protein.ToLower() == "+++")
                    criticalValues.Add("Protein: ++ or +++ (Critical: Severe proteinuria)");
                else
                    criticalValues.Add("Protein: Positive (Critical: Moderate proteinuria)");
            }
            
            if (result.Glucose.ToLower() == "positive" || result.Glucose.ToLower() == "trace")
            {
                if (result.Glucose.ToLower() == "++" || result.Glucose.ToLower() == "+++")
                    criticalValues.Add("Glucose: ++ or +++ (Critical: Severe glycosuria)");
                else
                    criticalValues.Add("Glucose: Positive (Critical: Moderate glycosuria)");
            }
            
            if (result.Blood.ToLower() == "positive")
            {
                if (result.Blood.ToLower() == "++" || result.Blood.ToLower() == "+++")
                    criticalValues.Add("Blood: ++ or +++ (Critical: Severe hematuria)");
                else
                    criticalValues.Add("Blood: Positive (Critical: Moderate hematuria)");
            }
            
            return criticalValues;
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

        private void SaveUrineResult(UrineTestResult result)
        {
            var sql = @"
                INSERT INTO Urine_Results (ExamId, Color, Turbidity, pH, SpecificGravity, 
                    Protein, Glucose, Ketones, Blood, LeukocyteEsterase, Nitrite, MicroscopyNotes, CreatedAt)
                VALUES (@ExamId, @Color, @Turbidity, @pH, @SpecificGravity, 
                    @Protein, @Glucose, @Ketones, @Blood, @LeukocyteEsterase, @Nitrite, @MicroscopyNotes, @TestDate)";
            
            _db.Execute(sql, result);
        }

        private async Task<Dictionary<string, object>> ParseUrineFileAsync(string filePath)
        {
            // هذا مثال مبسط - في التطبيق الحقيقي ستحتاج لاستخدام مكتبة لقراءة Excel/CSV
            var values = new Dictionary<string, object>();
            
            // قراءة الملف واستخراج القيم
            // يمكن استخدام EPPlus لقراءة Excel أو CsvHelper لقراءة CSV
            
            return values;
        }

        private string DetermineUTISeverity(UrineTestResult result)
        {
            var positiveCount = 0;
            if (result.LeukocyteEsterase.ToLower() == "positive") positiveCount++;
            if (result.Nitrite.ToLower() == "positive") positiveCount++;
            if (result.Blood.ToLower() == "positive") positiveCount++;
            
            return positiveCount switch
            {
                1 => "Mild",
                2 => "Moderate",
                >= 3 => "Severe",
                _ => "None"
            };
        }

        private string DetermineHematuriaType(UrineTestResult result)
        {
            if (result.Blood.ToLower() == "trace")
                return "Microscopic";
            else if (result.Blood.ToLower() == "positive")
                return "Macroscopic";
            else
                return "None";
        }

        private string DetermineProteinuriaSeverity(UrineTestResult result)
        {
            if (result.Protein.ToLower() == "trace")
                return "Mild";
            else if (result.Protein.ToLower() == "positive")
                return "Moderate";
            else if (result.Protein.ToLower() == "++" || result.Protein.ToLower() == "+++")
                return "Severe";
            else
                return "None";
        }

        #endregion
    }

    #region Data Classes

    public class UrineStatistics
    {
        public int TotalTests { get; set; }
        public int NormalTests { get; set; }
        public int AbnormalTests { get; set; }
        public int CriticalTests { get; set; }
        public double AvgpH { get; set; }
        public double AvgSpecificGravity { get; set; }
        
        public double NormalPercentage => TotalTests > 0 ? (double)NormalTests / TotalTests * 100 : 0;
        public double AbnormalPercentage => TotalTests > 0 ? (double)AbnormalTests / TotalTests * 100 : 0;
        public double CriticalPercentage => TotalTests > 0 ? (double)CriticalTests / TotalTests * 100 : 0;
    }

    public class UrinePatternAnalysis
    {
        public bool HasUTI { get; set; }
        public string UTISeverity { get; set; }
        public bool HasHematuria { get; set; }
        public string HematuriaType { get; set; }
        public bool HasProteinuria { get; set; }
        public string ProteinuriaSeverity { get; set; }
        public bool HasGlucosuria { get; set; }
        public bool HasKetonuria { get; set; }
        
        public List<string> GetPatterns()
        {
            var patterns = new List<string>();
            
            if (HasUTI)
                patterns.Add($"UTI: {UTISeverity}");
            
            if (HasHematuria)
                patterns.Add($"Hematuria: {HematuriaType}");
            
            if (HasProteinuria)
                patterns.Add($"Proteinuria: {ProteinuriaSeverity}");
            
            if (HasGlucosuria)
                patterns.Add("Glucosuria");
            
            if (HasKetonuria)
                patterns.Add("Ketonuria");
            
            return patterns;
        }
    }

    #endregion
}