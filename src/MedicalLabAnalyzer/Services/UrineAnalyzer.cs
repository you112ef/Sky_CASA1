using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;

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
                    Color = values.GetValueOrDefault("Color", "أصفر").ToString(),
                    pH = Convert.ToDouble(values.GetValueOrDefault("pH", 6.0)),
                    SpecificGravity = Convert.ToDouble(values.GetValueOrDefault("SpecificGravity", 1.020)),
                    Appearance = values.GetValueOrDefault("Appearance", "صافي").ToString(),
                    Protein = values.GetValueOrDefault("Protein", "Negative").ToString(),
                    Glucose = values.GetValueOrDefault("Glucose", "Negative").ToString(),
                    Ketones = values.GetValueOrDefault("Ketones", "Negative").ToString(),
                    Blood = values.GetValueOrDefault("Blood", "Negative").ToString(),
                    Leukocytes = values.GetValueOrDefault("Leukocytes", "Negative").ToString(),
                    Nitrites = values.GetValueOrDefault("Nitrites", "Negative").ToString(),
                    Bilirubin = values.GetValueOrDefault("Bilirubin", "Negative").ToString(),
                    Urobilinogen = values.GetValueOrDefault("Urobilinogen", "Negative").ToString(),
                    RBC = Convert.ToInt32(values.GetValueOrDefault("RBC", 0)),
                    WBC = Convert.ToInt32(values.GetValueOrDefault("WBC", 0)),
                    EpithelialCells = Convert.ToInt32(values.GetValueOrDefault("EpithelialCells", 0)),
                    Casts = values.GetValueOrDefault("Casts", "None").ToString(),
                    CastsCount = Convert.ToInt32(values.GetValueOrDefault("CastsCount", 0)),
                    Crystals = values.GetValueOrDefault("Crystals", "None").ToString(),
                    CrystalsCount = Convert.ToInt32(values.GetValueOrDefault("CrystalsCount", 0)),
                    Bacteria = values.GetValueOrDefault("Bacteria", "None").ToString(),
                    Yeast = values.GetValueOrDefault("Yeast", "None").ToString(),
                    Parasites = values.GetValueOrDefault("Parasites", "None").ToString(),
                    TestDate = DateTime.Now
                };

                // التحقق من القيم المرجعية
                result.ValidateResults();

                // حفظ النتائج في قاعدة البيانات
                SaveUrineResult(result);

                // تسجيل في AuditLog
                _auditLogger?.LogSystemEvent(userId, userName, "Urine_Analysis_Completed", "Urine", 
                    new { ExamId = examId, Status = result.Status, HasUTI = result.HasUTI() });

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
                var sql = "SELECT * FROM UrineTestResults WHERE ExamId = @ExamId";
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
                    SELECT u.* FROM UrineTestResults u
                    JOIN Exams e ON u.ExamId = e.Id
                    WHERE e.PatientId = @PatientId
                    ORDER BY u.TestDate DESC";
                
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
                    UPDATE UrineTestResults SET
                        Color = @Color, pH = @pH, SpecificGravity = @SpecificGravity, Appearance = @Appearance,
                        Protein = @Protein, Glucose = @Glucose, Ketones = @Ketones, Blood = @Blood,
                        Leukocytes = @Leukocytes, Nitrites = @Nitrites, Bilirubin = @Bilirubin, Urobilinogen = @Urobilinogen,
                        RBC = @RBC, WBC = @WBC, EpithelialCells = @EpithelialCells, Casts = @Casts, CastsCount = @CastsCount,
                        Crystals = @Crystals, CrystalsCount = @CrystalsCount, Bacteria = @Bacteria, Yeast = @Yeast,
                        Parasites = @Parasites, Status = @Status, Notes = @Notes, TestDate = @TestDate
                    WHERE Id = @Id";
                
                var rowsAffected = await _db.ExecuteAsync(sql, result);
                
                if (rowsAffected > 0)
                {
                    _auditLogger?.LogSystemEvent(userId, userName, "Urine_Result_Updated", "Urine", 
                        new { ResultId = result.Id, ExamId = result.ExamId, Status = result.Status });
                    
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
                    whereClause = "WHERE TestDate BETWEEN @StartDate AND @EndDate";
                    parameters.Add("@StartDate", startDate.Value);
                    parameters.Add("@EndDate", endDate.Value);
                }

                var sql = $@"
                    SELECT 
                        COUNT(*) as TotalTests,
                        COUNT(CASE WHEN Status = 'Normal' THEN 1 END) as NormalTests,
                        COUNT(CASE WHEN Status = 'Abnormal' THEN 1 END) as AbnormalTests,
                        COUNT(CASE WHEN Status = 'Critical' THEN 1 END) as CriticalTests,
                        COUNT(CASE WHEN Protein != 'Negative' THEN 1 END) as ProteinuriaCount,
                        COUNT(CASE WHEN Glucose != 'Negative' THEN 1 END) as GlycosuriaCount,
                        COUNT(CASE WHEN Blood != 'Negative' THEN 1 END) as HematuriaCount,
                        COUNT(CASE WHEN Leukocytes != 'Negative' OR Nitrites = 'Positive' THEN 1 END) as UTICount,
                        AVG(pH) as AvgpH,
                        AVG(SpecificGravity) as AvgSpecificGravity
                    FROM UrineTestResults {whereClause}";

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
        /// تحليل الأنماط المرضية في البول
        /// </summary>
        public UrinePatternAnalysis AnalyzePatterns(UrineTestResult result)
        {
            var analysis = new UrinePatternAnalysis();
            
            // UTI Analysis
            if (result.HasUTI())
            {
                analysis.HasUTI = true;
                analysis.UTISeverity = DetermineUTISeverity(result);
            }
            
            // Hematuria Analysis
            if (result.HasHematuria())
            {
                analysis.HasHematuria = true;
                analysis.HematuriaType = DetermineHematuriaType(result);
            }
            
            // Proteinuria Analysis
            if (result.HasProteinuria())
            {
                analysis.HasProteinuria = true;
                analysis.ProteinuriaSeverity = DetermineProteinuriaSeverity(result);
            }
            
            // Glycosuria Analysis
            if (result.HasGlycosuria())
            {
                analysis.HasGlycosuria = true;
                analysis.GlycosuriaSeverity = DetermineGlycosuriaSeverity(result);
            }
            
            // pH Analysis
            if (result.pH < 5.0)
                analysis.pHCategory = "Acidic";
            else if (result.pH > 7.5)
                analysis.pHCategory = "Alkaline";
            else
                analysis.pHCategory = "Normal";
            
            // Specific Gravity Analysis
            if (result.SpecificGravity < 1.010)
                analysis.SpecificGravityCategory = "Dilute";
            else if (result.SpecificGravity > 1.025)
                analysis.SpecificGravityCategory = "Concentrated";
            else
                analysis.SpecificGravityCategory = "Normal";
            
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
            if (result.RBC > 50)
                criticalValues.Add($"RBC critically high: {result.RBC}/HPF (Critical: >50)");
            
            if (result.WBC > 100)
                criticalValues.Add($"WBC critically high: {result.WBC}/HPF (Critical: >100)");
            
            // Chemical Critical Values
            if (result.Protein == "4+")
                criticalValues.Add("Protein: 4+ (Critical: Severe proteinuria)");
            
            if (result.Glucose == "4+")
                criticalValues.Add("Glucose: 4+ (Critical: Severe glycosuria)");
            
            if (result.Blood == "4+")
                criticalValues.Add("Blood: 4+ (Critical: Severe hematuria)");
            
            return criticalValues;
        }

        #region Private Methods

        private void SaveUrineResult(UrineTestResult result)
        {
            var sql = @"
                INSERT INTO UrineTestResults (ExamId, Color, pH, SpecificGravity, Appearance, 
                    Protein, Glucose, Ketones, Blood, Leukocytes, Nitrites, Bilirubin, Urobilinogen,
                    RBC, WBC, EpithelialCells, Casts, CastsCount, Crystals, CrystalsCount, 
                    Bacteria, Yeast, Parasites, TestDate, Notes, Status)
                VALUES (@ExamId, @Color, @pH, @SpecificGravity, @Appearance, 
                    @Protein, @Glucose, @Ketones, @Blood, @Leukocytes, @Nitrites, @Bilirubin, @Urobilinogen,
                    @RBC, @WBC, @EpithelialCells, @Casts, @CastsCount, @Crystals, @CrystalsCount, 
                    @Bacteria, @Yeast, @Parasites, @TestDate, @Notes, @Status)";
            
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
            var severity = 0;
            
            if (result.Leukocytes != "Negative") severity++;
            if (result.Nitrites == "Positive") severity++;
            if (result.Bacteria != "None") severity++;
            if (result.WBC > 10) severity++;
            
            if (severity >= 3)
                return "Severe";
            else if (severity >= 2)
                return "Moderate";
            else
                return "Mild";
        }

        private string DetermineHematuriaType(UrineTestResult result)
        {
            if (result.Blood != "Negative" && result.RBC > 10)
                return "Gross Hematuria";
            else if (result.Blood != "Negative" || result.RBC > 3)
                return "Microscopic Hematuria";
            else
                return "None";
        }

        private string DetermineProteinuriaSeverity(UrineTestResult result)
        {
            switch (result.Protein)
            {
                case "Trace":
                    return "Mild";
                case "1+":
                case "2+":
                    return "Moderate";
                case "3+":
                case "4+":
                    return "Severe";
                default:
                    return "None";
            }
        }

        private string DetermineGlycosuriaSeverity(UrineTestResult result)
        {
            switch (result.Glucose)
            {
                case "Trace":
                    return "Mild";
                case "1+":
                case "2+":
                    return "Moderate";
                case "3+":
                case "4+":
                    return "Severe";
                default:
                    return "None";
            }
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
        public int ProteinuriaCount { get; set; }
        public int GlycosuriaCount { get; set; }
        public int HematuriaCount { get; set; }
        public int UTICount { get; set; }
        public double AvgpH { get; set; }
        public double AvgSpecificGravity { get; set; }
        
        public double NormalPercentage => TotalTests > 0 ? (double)NormalTests / TotalTests * 100 : 0;
        public double AbnormalPercentage => TotalTests > 0 ? (double)AbnormalTests / TotalTests * 100 : 0;
        public double CriticalPercentage => TotalTests > 0 ? (double)CriticalTests / TotalTests * 100 : 0;
        public double ProteinuriaPercentage => TotalTests > 0 ? (double)ProteinuriaCount / TotalTests * 100 : 0;
        public double GlycosuriaPercentage => TotalTests > 0 ? (double)GlycosuriaCount / TotalTests * 100 : 0;
        public double HematuriaPercentage => TotalTests > 0 ? (double)HematuriaCount / TotalTests * 100 : 0;
        public double UTIPercentage => TotalTests > 0 ? (double)UTICount / TotalTests * 100 : 0;
    }

    public class UrinePatternAnalysis
    {
        public bool HasUTI { get; set; }
        public string UTISeverity { get; set; }
        public bool HasHematuria { get; set; }
        public string HematuriaType { get; set; }
        public bool HasProteinuria { get; set; }
        public string ProteinuriaSeverity { get; set; }
        public bool HasGlycosuria { get; set; }
        public string GlycosuriaSeverity { get; set; }
        public string pHCategory { get; set; }
        public string SpecificGravityCategory { get; set; }
        
        public List<string> GetPatterns()
        {
            var patterns = new List<string>();
            
            if (HasUTI)
                patterns.Add($"UTI: {UTISeverity}");
            
            if (HasHematuria)
                patterns.Add($"Hematuria: {HematuriaType}");
            
            if (HasProteinuria)
                patterns.Add($"Proteinuria: {ProteinuriaSeverity}");
            
            if (HasGlycosuria)
                patterns.Add($"Glycosuria: {GlycosuriaSeverity}");
            
            patterns.Add($"pH: {pHCategory}");
            patterns.Add($"Specific Gravity: {SpecificGravityCategory}");
            
            return patterns;
        }
    }

    #endregion
}