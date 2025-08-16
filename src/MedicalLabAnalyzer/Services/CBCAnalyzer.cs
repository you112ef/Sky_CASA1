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
    public class CBCAnalyzer
    {
        private readonly IDbConnection _db;
        private readonly ILogger<CBCAnalyzer> _logger;
        private readonly AuditLogger _auditLogger;

        public CBCAnalyzer(IDbConnection db, ILogger<CBCAnalyzer> logger = null, AuditLogger auditLogger = null)
        {
            _db = db;
            _logger = logger;
            _auditLogger = auditLogger;
        }

        /// <summary>
        /// تحليل نتائج CBC وتحديد الحالة
        /// </summary>
        public CBCTestResult AnalyzeCBCResults(Dictionary<string, double> values, int examId, string userId, string userName)
        {
            try
            {
                var result = new CBCTestResult
                {
                    ExamId = examId,
                    WBC = values.GetValueOrDefault("WBC", 0),
                    RBC = values.GetValueOrDefault("RBC", 0),
                    Hemoglobin = values.GetValueOrDefault("Hemoglobin", 0),
                    Hematocrit = values.GetValueOrDefault("Hematocrit", 0),
                    Platelets = values.GetValueOrDefault("Platelets", 0),
                    MCV = values.GetValueOrDefault("MCV", 0),
                    MCH = values.GetValueOrDefault("MCH", 0),
                    MCHC = values.GetValueOrDefault("MCHC", 0),
                    RDW = values.GetValueOrDefault("RDW", 0),
                    Neutrophils = values.GetValueOrDefault("Neutrophils", 0),
                    Lymphocytes = values.GetValueOrDefault("Lymphocytes", 0),
                    Monocytes = values.GetValueOrDefault("Monocytes", 0),
                    Eosinophils = values.GetValueOrDefault("Eosinophils", 0),
                    Basophils = values.GetValueOrDefault("Basophils", 0),
                    TestDate = DateTime.Now
                };

                // التحقق من القيم المرجعية
                result.ValidateResults();

                // حفظ النتائج في قاعدة البيانات
                SaveCBCResult(result);

                // تسجيل في AuditLog
                _auditLogger?.LogSystemEvent(userId, userName, "CBC_Analysis_Completed", "CBC", 
                    new { ExamId = examId, Status = result.Status, AbnormalCount = result.GetAbnormalValues().Count });

                _logger?.LogInformation("CBC analysis completed for exam {ExamId} with status {Status}", examId, result.Status);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing CBC results for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// تحليل CBC من ملف Excel أو CSV
        /// </summary>
        public async Task<CBCTestResult> AnalyzeCBCFromFileAsync(string filePath, int examId, string userId, string userName)
        {
            try
            {
                var values = await ParseCBCFileAsync(filePath);
                return AnalyzeCBCResults(values, examId, userId, userName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing CBC from file {FilePath} for exam {ExamId}", filePath, examId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على نتائج CBC من قاعدة البيانات
        /// </summary>
        public async Task<CBCTestResult> GetCBCResultAsync(int examId)
        {
            try
            {
                var sql = "SELECT * FROM CBC_Results WHERE ExamId = @ExamId";
                return await _db.QueryFirstOrDefaultAsync<CBCTestResult>(sql, new { ExamId = examId });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving CBC result for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على جميع نتائج CBC لمريض معين
        /// </summary>
        public async Task<List<CBCTestResult>> GetPatientCBCResultsAsync(int patientId)
        {
            try
            {
                var sql = @"
                    SELECT c.* FROM CBC_Results c
                    JOIN Exams e ON c.ExamId = e.ExamId
                    WHERE e.PatientId = @PatientId
                    ORDER BY c.CreatedAt DESC";
                
                var results = await _db.QueryAsync<CBCTestResult>(sql, new { PatientId = patientId });
                return results.AsList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving CBC results for patient {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// تحديث نتائج CBC
        /// </summary>
        public async Task<bool> UpdateCBCResultAsync(CBCTestResult result, string userId, string userName)
        {
            try
            {
                result.ValidateResults();
                
                var sql = @"
                    UPDATE CBC_Results SET
                        WBC = @WBC, RBC = @RBC, HGB = @Hemoglobin, HCT = @Hematocrit,
                        PLT = @Platelets, MCV = @MCV, MCH = @MCH, MCHC = @MCHC, RDW = @RDW,
                        CreatedAt = @TestDate
                    WHERE ExamId = @ExamId";
                
                var rowsAffected = await _db.ExecuteAsync(sql, new
                {
                    result.ExamId,
                    result.WBC,
                    result.RBC,
                    result.Hemoglobin,
                    result.Hematocrit,
                    result.Platelets,
                    result.MCV,
                    result.MCH,
                    result.MCHC,
                    result.RDW,
                    result.TestDate
                });
                
                if (rowsAffected > 0)
                {
                    _auditLogger?.LogSystemEvent(userId, userName, "CBC_Result_Updated", "CBC", 
                        new { ExamId = result.ExamId, Status = result.Status });
                    
                    _logger?.LogInformation("CBC result updated for exam {ExamId}", result.ExamId);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating CBC result for exam {ExamId}", result.ExamId);
                throw;
            }
        }

        /// <summary>
        /// الحصول على إحصائيات CBC
        /// </summary>
        public async Task<CBCStatistics> GetCBCStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
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
                        AVG(WBC) as AvgWBC,
                        AVG(RBC) as AvgRBC,
                        AVG(HGB) as AvgHemoglobin,
                        AVG(HCT) as AvgHematocrit,
                        AVG(PLT) as AvgPlatelets
                    FROM CBC_Results {whereClause}";

                var stats = await _db.QueryFirstAsync<CBCStatistics>(sql, parameters);
                return stats;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving CBC statistics");
                throw;
            }
        }

        /// <summary>
        /// التحقق من القيم الحرجة
        /// </summary>
        public List<string> CheckCriticalValues(CBCTestResult result)
        {
            var criticalValues = new List<string>();
            
            // WBC Critical Values
            if (result.WBC < 1.0)
                criticalValues.Add($"WBC critically low: {result.WBC:F2} K/µL (Critical: <1.0)");
            else if (result.WBC > 50.0)
                criticalValues.Add($"WBC critically high: {result.WBC:F2} K/µL (Critical: >50.0)");
            
            // Hemoglobin Critical Values
            if (result.Hemoglobin < 7.0)
                criticalValues.Add($"Hemoglobin critically low: {result.Hemoglobin:F2} g/dL (Critical: <7.0)");
            else if (result.Hemoglobin > 20.0)
                criticalValues.Add($"Hemoglobin critically high: {result.Hemoglobin:F2} g/dL (Critical: >20.0)");
            
            // Platelets Critical Values
            if (result.Platelets < 20)
                criticalValues.Add($"Platelets critically low: {result.Platelets:F0} K/µL (Critical: <20)");
            else if (result.Platelets > 1000)
                criticalValues.Add($"Platelets critically high: {result.Platelets:F0} K/µL (Critical: >1000)");
            
            // Hematocrit Critical Values
            if (result.Hematocrit < 20.0)
                criticalValues.Add($"Hematocrit critically low: {result.Hematocrit:F2}% (Critical: <20.0)");
            else if (result.Hematocrit > 60.0)
                criticalValues.Add($"Hematocrit critically high: {result.Hematocrit:F2}% (Critical: >60.0)");
            
            return criticalValues;
        }

        /// <summary>
        /// تحليل الأنماط المرضية
        /// </summary>
        public CBCPatternAnalysis AnalyzePatterns(CBCTestResult result)
        {
            var analysis = new CBCPatternAnalysis();
            
            // Anemia Analysis
            if (result.Hemoglobin < 13.5 && result.RBC < 4.5)
            {
                analysis.HasAnemia = true;
                analysis.AnemiaType = DetermineAnemiaType(result);
            }
            
            // Leukocytosis/Leukopenia Analysis
            if (result.WBC > 11.0)
            {
                analysis.HasLeukocytosis = true;
                analysis.LeukocytosisType = DetermineLeukocytosisType(result);
            }
            else if (result.WBC < 4.0)
            {
                analysis.HasLeukopenia = true;
            }
            
            // Thrombocytosis/Thrombocytopenia Analysis
            if (result.Platelets > 450)
            {
                analysis.HasThrombocytosis = true;
            }
            else if (result.Platelets < 150)
            {
                analysis.HasThrombocytopenia = true;
            }
            
            // MCV Analysis for Anemia Classification
            if (result.MCV < 80)
                analysis.MCVCategory = "Microcytic";
            else if (result.MCV > 100)
                analysis.MCVCategory = "Macrocytic";
            else
                analysis.MCVCategory = "Normocytic";
            
            return analysis;
        }

        #region Private Methods

        private void SaveCBCResult(CBCTestResult result)
        {
            var sql = @"
                INSERT INTO CBC_Results (ExamId, WBC, RBC, HGB, HCT, PLT, 
                    MCV, MCH, MCHC, RDW, CreatedAt)
                VALUES (@ExamId, @WBC, @RBC, @Hemoglobin, @Hematocrit, @Platelets, 
                    @MCV, @MCH, @MCHC, @RDW, @TestDate)";
            
            _db.Execute(sql, new
            {
                result.ExamId,
                result.WBC,
                result.RBC,
                result.Hemoglobin,
                result.Hematocrit,
                result.Platelets,
                result.MCV,
                result.MCH,
                result.MCHC,
                result.RDW,
                result.TestDate
            });
        }

        private async Task<Dictionary<string, double>> ParseCBCFileAsync(string filePath)
        {
            // هذا مثال مبسط - في التطبيق الحقيقي ستحتاج لاستخدام مكتبة لقراءة Excel/CSV
            var values = new Dictionary<string, double>();
            
            // قراءة الملف واستخراج القيم
            // يمكن استخدام EPPlus لقراءة Excel أو CsvHelper لقراءة CSV
            
            return values;
        }

        private string DetermineAnemiaType(CBCTestResult result)
        {
            if (result.MCV < 80)
                return "Microcytic Anemia";
            else if (result.MCV > 100)
                return "Macrocytic Anemia";
            else
                return "Normocytic Anemia";
        }

        private string DetermineLeukocytosisType(CBCTestResult result)
        {
            if (result.Neutrophils > 70)
                return "Neutrophilic Leukocytosis";
            else if (result.Lymphocytes > 40)
                return "Lymphocytic Leukocytosis";
            else if (result.Eosinophils > 5)
                return "Eosinophilic Leukocytosis";
            else
                return "Mixed Leukocytosis";
        }

        #endregion
    }

    #region Data Classes

    public class CBCStatistics
    {
        public int TotalTests { get; set; }
        public int NormalTests { get; set; }
        public int AbnormalTests { get; set; }
        public int CriticalTests { get; set; }
        public double AvgWBC { get; set; }
        public double AvgRBC { get; set; }
        public double AvgHemoglobin { get; set; }
        public double AvgHematocrit { get; set; }
        public double AvgPlatelets { get; set; }
        
        public double NormalPercentage => TotalTests > 0 ? (double)NormalTests / TotalTests * 100 : 0;
        public double AbnormalPercentage => TotalTests > 0 ? (double)AbnormalTests / TotalTests * 100 : 0;
        public double CriticalPercentage => TotalTests > 0 ? (double)CriticalTests / TotalTests * 100 : 0;
    }

    public class CBCPatternAnalysis
    {
        public bool HasAnemia { get; set; }
        public string AnemiaType { get; set; }
        public bool HasLeukocytosis { get; set; }
        public string LeukocytosisType { get; set; }
        public bool HasLeukopenia { get; set; }
        public bool HasThrombocytosis { get; set; }
        public bool HasThrombocytopenia { get; set; }
        public string MCVCategory { get; set; }
        
        public List<string> GetPatterns()
        {
            var patterns = new List<string>();
            
            if (HasAnemia)
                patterns.Add($"Anemia: {AnemiaType}");
            
            if (HasLeukocytosis)
                patterns.Add($"Leukocytosis: {LeukocytosisType}");
            
            if (HasLeukopenia)
                patterns.Add("Leukopenia");
            
            if (HasThrombocytosis)
                patterns.Add("Thrombocytosis");
            
            if (HasThrombocytopenia)
                patterns.Add("Thrombocytopenia");
            
            patterns.Add($"MCV Category: {MCVCategory}");
            
            return patterns;
        }
    }

    #endregion
}