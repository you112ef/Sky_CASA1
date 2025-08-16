using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System.Data;
using Dapper;
using OfficeOpenXml;
using System.IO;
using System.Globalization;
using System.Text;

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
                    Weight = Convert.ToDouble(values.GetValueOrDefault("Weight", 50.0)),
                    Odor = values.GetValueOrDefault("Odor", "Normal").ToString(),
                    OccultBlood = values.GetValueOrDefault("OccultBlood", "Negative").ToString(),
                    pH = values.GetValueOrDefault("pH", "7.0").ToString(),
                    ReducingSubstances = values.GetValueOrDefault("ReducingSubstances", "Negative").ToString(),
                    FatContent = values.GetValueOrDefault("FatContent", "Normal").ToString(),
                    Mucus = values.GetValueOrDefault("Mucus", "None").ToString(),
                    UndigestedFood = values.GetValueOrDefault("UndigestedFood", "None").ToString(),
                    MuscleFibers = values.GetValueOrDefault("MuscleFibers", "None").ToString(),
                    Starch = values.GetValueOrDefault("Starch", "None").ToString(),
                    FatGlobules = values.GetValueOrDefault("FatGlobules", "None").ToString(),
                    Parasites = values.GetValueOrDefault("Parasites", "None").ToString(),
                    ParasiteType = values.GetValueOrDefault("ParasiteType", "").ToString(),
                    ParasiteCount = Convert.ToInt32(values.GetValueOrDefault("ParasiteCount", 0)),
                    Ova = values.GetValueOrDefault("Ova", "None").ToString(),
                    OvaType = values.GetValueOrDefault("OvaType", "").ToString(),
                    OvaCount = Convert.ToInt32(values.GetValueOrDefault("OvaCount", 0)),
                    Bacteria = values.GetValueOrDefault("Bacteria", "Normal").ToString(),
                    BacterialType = values.GetValueOrDefault("BacterialType", "").ToString(),
                    Yeast = values.GetValueOrDefault("Yeast", "None").ToString(),
                    YeastType = values.GetValueOrDefault("YeastType", "").ToString(),
                    Calprotectin = values.GetValueOrDefault("Calprotectin", "Normal").ToString(),
                    CalprotectinValue = Convert.ToDouble(values.GetValueOrDefault("CalprotectinValue", 50.0)),
                    Lactoferrin = values.GetValueOrDefault("Lactoferrin", "Negative").ToString(),
                    Alpha1Antitrypsin = values.GetValueOrDefault("Alpha1Antitrypsin", "Normal").ToString(),
                    CollectionMethod = values.GetValueOrDefault("CollectionMethod", "Spontaneous").ToString(),
                    PatientPreparation = values.GetValueOrDefault("PatientPreparation", "Normal diet").ToString(),
                    TestDate = DateTime.Now
                };

                // التحقق من القيم المرجعية
                result.ValidateResults();

                // حفظ النتائج في قاعدة البيانات
                SaveStoolResult(result);

                // تسجيل في AuditLog
                _auditLogger?.LogSystemEvent(userId, userName, "Stool_Analysis_Completed", "Stool", 
                    new { ExamId = examId, Status = result.Status, HasParasites = result.HasParasiticInfection() });

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
                var sql = "SELECT * FROM StoolTestResults WHERE ExamId = @ExamId";
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
                    SELECT s.* FROM StoolTestResults s
                    JOIN Exams e ON s.ExamId = e.Id
                    WHERE e.PatientId = @PatientId
                    ORDER BY s.TestDate DESC";
                
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
                    UPDATE StoolTestResults SET
                        Color = @Color, Consistency = @Consistency, Shape = @Shape, Weight = @Weight, Odor = @Odor,
                        OccultBlood = @OccultBlood, pH = @pH, ReducingSubstances = @ReducingSubstances, FatContent = @FatContent,
                        Mucus = @Mucus, UndigestedFood = @UndigestedFood, MuscleFibers = @MuscleFibers, Starch = @Starch,
                        FatGlobules = @FatGlobules, Parasites = @Parasites, ParasiteType = @ParasiteType, ParasiteCount = @ParasiteCount,
                        Ova = @Ova, OvaType = @OvaType, OvaCount = @OvaCount, Bacteria = @Bacteria, BacterialType = @BacterialType,
                        Yeast = @Yeast, YeastType = @YeastType, Calprotectin = @Calprotectin, CalprotectinValue = @CalprotectinValue,
                        Lactoferrin = @Lactoferrin, Alpha1Antitrypsin = @Alpha1Antitrypsin, Status = @Status, Notes = @Notes, TestDate = @TestDate
                    WHERE Id = @Id";
                
                var rowsAffected = await _db.ExecuteAsync(sql, result);
                
                if (rowsAffected > 0)
                {
                    _auditLogger?.LogSystemEvent(userId, userName, "Stool_Result_Updated", "Stool", 
                        new { ResultId = result.Id, ExamId = result.ExamId, Status = result.Status });
                    
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
                        COUNT(CASE WHEN OccultBlood != 'Negative' THEN 1 END) as OccultBloodCount,
                        COUNT(CASE WHEN Parasites = 'Present' THEN 1 END) as ParasiteCount,
                        COUNT(CASE WHEN Ova = 'Present' THEN 1 END) as OvaCount,
                        COUNT(CASE WHEN Bacteria = 'Abnormal' THEN 1 END) as AbnormalBacteriaCount,
                        COUNT(CASE WHEN Calprotectin = 'Elevated' OR Calprotectin = 'High' THEN 1 END) as ElevatedCalprotectinCount,
                        AVG(Weight) as AvgWeight,
                        AVG(CAST(REPLACE(pH, ',', '.') AS FLOAT)) as AvgpH
                    FROM StoolTestResults {whereClause}";

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
        /// تحليل الأنماط المرضية في البراز
        /// </summary>
        public StoolPatternAnalysis AnalyzePatterns(StoolTestResult result)
        {
            var analysis = new StoolPatternAnalysis();
            
            // Gastrointestinal Bleeding Analysis
            if (result.HasGastrointestinalBleeding())
            {
                analysis.HasGastrointestinalBleeding = true;
                analysis.BleedingType = DetermineBleedingType(result);
            }
            
            // Parasitic Infection Analysis
            if (result.HasParasiticInfection())
            {
                analysis.HasParasiticInfection = true;
                analysis.ParasiteType = result.ParasiteType;
                analysis.OvaType = result.OvaType;
            }
            
            // Inflammatory Bowel Disease Analysis
            if (result.HasInflammatoryBowelDisease())
            {
                analysis.HasInflammatoryBowelDisease = true;
                analysis.IBDSeverity = DetermineIBDSeverity(result);
            }
            
            // Malabsorption Analysis
            if (result.HasMalabsorption())
            {
                analysis.HasMalabsorption = true;
                analysis.MalabsorptionType = DetermineMalabsorptionType(result);
            }
            
            // Diarrhea Analysis
            if (result.HasDiarrhea())
            {
                analysis.HasDiarrhea = true;
                analysis.DiarrheaType = DetermineDiarrheaType(result);
            }
            
            // Color Analysis
            analysis.ColorCategory = DetermineColorCategory(result.Color);
            
            // Consistency Analysis
            analysis.ConsistencyCategory = DetermineConsistencyCategory(result.Consistency);
            
            return analysis;
        }

        /// <summary>
        /// التحقق من القيم الحرجة
        /// </summary>
        public List<string> CheckCriticalValues(StoolTestResult result)
        {
            var criticalValues = new List<string>();
            
            // Color Critical Values
            if (result.Color == "أسود")
                criticalValues.Add("Color: أسود (قد يشير إلى نزيف في الجهاز الهضمي العلوي)");
            
            if (result.Color == "أحمر")
                criticalValues.Add("Color: أحمر (قد يشير إلى نزيف في الجهاز الهضمي السفلي)");
            
            if (result.Color == "أبيض")
                criticalValues.Add("Color: أبيض (قد يشير إلى مشاكل في الكبد أو المرارة)");
            
            // Occult Blood Critical Values
            if (result.OccultBlood == "Positive")
                criticalValues.Add("Occult Blood: Positive (يتطلب فحص إضافي)");
            
            // Calprotectin Critical Values
            if (result.CalprotectinValue > 200)
                criticalValues.Add($"Calprotectin: {result.CalprotectinValue:F1} µg/g (Critical: >200)");
            
            // Parasites Critical Values
            if (result.Parasites == "Present")
                criticalValues.Add($"Parasites: {result.ParasiteType} (يتطلب علاج فوري)");
            
            if (result.Ova == "Present")
                criticalValues.Add($"Ova: {result.OvaType} (يتطلب علاج فوري)");
            
            // Weight Critical Values
            if (result.Weight < 50)
                criticalValues.Add($"Weight: {result.Weight:F1}g (Critical: <50g)");
            
            return criticalValues;
        }

        #region Private Methods

        private void SaveStoolResult(StoolTestResult result)
        {
            var sql = @"
                INSERT INTO StoolTestResults (ExamId, Color, Consistency, Shape, Weight, Odor,
                    OccultBlood, pH, ReducingSubstances, FatContent, Mucus, UndigestedFood,
                    MuscleFibers, Starch, FatGlobules, Parasites, ParasiteType, ParasiteCount,
                    Ova, OvaType, OvaCount, Bacteria, BacterialType, Yeast, YeastType,
                    Calprotectin, CalprotectinValue, Lactoferrin, Alpha1Antitrypsin,
                    CollectionMethod, PatientPreparation, TestDate, Notes, Status)
                VALUES (@ExamId, @Color, @Consistency, @Shape, @Weight, @Odor,
                    @OccultBlood, @pH, @ReducingSubstances, @FatContent, @Mucus, @UndigestedFood,
                    @MuscleFibers, @Starch, @FatGlobules, @Parasites, @ParasiteType, @ParasiteCount,
                    @Ova, @OvaType, @OvaCount, @Bacteria, @BacterialType, @Yeast, @YeastType,
                    @Calprotectin, @CalprotectinValue, @Lactoferrin, @Alpha1Antitrypsin,
                    @CollectionMethod, @PatientPreparation, @TestDate, @Notes, @Status)";
            
            _db.Execute(sql, result);
        }

        /// <summary>
        /// قراءة وتحليل ملفات Excel/CSV لاستخراج نتائج البراز
        /// </summary>
        private async Task<Dictionary<string, object>> ParseStoolFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"ملف البيانات غير موجود: {filePath}");

            var extension = Path.GetExtension(filePath).ToLower();
            
            try
            {
                return extension switch
                {
                    ".xlsx" or ".xls" => await ParseExcelFileAsync(filePath),
                    ".csv" => await ParseCsvFileAsync(filePath),
                    _ => throw new NotSupportedException($"نوع الملف غير مدعوم: {extension}. الأنواع المدعومة: .xlsx, .xls, .csv")
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "خطأ في قراءة الملف: {FilePath}", filePath);
                throw new InvalidOperationException($"فشل في قراءة الملف: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// قراءة ملف Excel لاستخراج نتائج البراز
        /// </summary>
        private async Task<Dictionary<string, object>> ParseExcelFileAsync(string filePath)
        {
            var values = new Dictionary<string, object>();
            
            await Task.Run(() =>
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                
                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0]; // أول ورقة عمل
                
                if (worksheet == null)
                    throw new InvalidOperationException("لا توجد أوراق عمل في الملف");
                
                // قراءة البيانات من العمودين A (المعامل) و B (القيمة)
                // أو من صف واحد إذا كانت البيانات مرتبة أفقياً
                var rows = worksheet.Dimension?.Rows ?? 0;
                var cols = worksheet.Dimension?.Columns ?? 0;
                
                if (rows == 0 || cols == 0)
                    throw new InvalidOperationException("الملف فارغ أو لا يحتوي على بيانات صالحة");
                
                // محاولة قراءة البيانات كعمودين (معامل - قيمة)
                if (cols >= 2)
                {
                    for (int row = 1; row <= rows; row++)
                    {
                        var parameter = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var value = worksheet.Cells[row, 2].Value;
                        
                        if (!string.IsNullOrEmpty(parameter) && value != null)
                        {
                            values[parameter] = ParseValue(value.ToString());
                        }
                    }
                }
                // أو قراءة البيانات كصف واحد مع عناوين
                else if (rows >= 2)
                {
                    for (int col = 1; col <= cols; col++)
                    {
                        var parameter = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                        var value = worksheet.Cells[2, col].Value;
                        
                        if (!string.IsNullOrEmpty(parameter) && value != null)
                        {
                            values[parameter] = ParseValue(value.ToString());
                        }
                    }
                }
                
                // إضافة قيم افتراضية للحقول المطلوبة إذا لم تكن موجودة
                AddDefaultValuesIfMissing(values);
            });
            
            _logger?.LogInformation("تم قراءة {Count} معامل من ملف Excel: {FilePath}", values.Count, filePath);
            return values;
        }
        
        /// <summary>
        /// قراءة ملف CSV لاستخراج نتائج البراز
        /// </summary>
        private async Task<Dictionary<string, object>> ParseCsvFileAsync(string filePath)
        {
            var values = new Dictionary<string, object>();
            
            var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);
            
            if (lines.Length == 0)
                throw new InvalidOperationException("ملف CSV فارغ");
            
            // محاولة تحديد الفاصل المستخدم
            var separator = DetectCsvSeparator(lines[0]);
            
            // قراءة البيانات كعمودين: معامل، قيمة
            if (lines.Length >= 2)
            {
                var headers = lines[0].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                
                // إذا كان هناك عمودين فقط (معامل - قيمة)
                if (headers.Length == 2)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var parts = lines[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[0]))
                        {
                            values[parts[0].Trim().Trim('"')] = ParseValue(parts[1].Trim().Trim('"'));
                        }
                    }
                }
                // أو قراءة البيانات كصف واحد مع عناوين متعددة
                else if (lines.Length >= 2)
                {
                    var dataRow = lines[1].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    
                    for (int i = 0; i < Math.Min(headers.Length, dataRow.Length); i++)
                    {
                        if (!string.IsNullOrEmpty(headers[i]) && !string.IsNullOrEmpty(dataRow[i]))
                        {
                            values[headers[i].Trim().Trim('"')] = ParseValue(dataRow[i].Trim().Trim('"'));
                        }
                    }
                }
            }
            
            // إضافة قيم افتراضية للحقول المطلوبة إذا لم تكن موجودة
            AddDefaultValuesIfMissing(values);
            
            _logger?.LogInformation("تم قراءة {Count} معامل من ملف CSV: {FilePath}", values.Count, filePath);
            return values;
        }
        
        /// <summary>
        /// تحديد نوع الفاصل المستخدم في ملف CSV
        /// </summary>
        private char DetectCsvSeparator(string firstLine)
        {
            var separators = new[] { ',', ';', '\t', '|' };
            
            return separators
                .OrderByDescending(sep => firstLine.Count(c => c == sep))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// تحويل النص إلى القيمة المناسبة (رقم، منطق، أو نص)
        /// </summary>
        private object ParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            
            value = value.Trim();
            
            // محاولة تحويل إلى رقم
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;
            
            // محاولة تحويل إلى عدد صحيح
            if (int.TryParse(value, out int intValue))
                return intValue;
            
            // محاولة تحويل إلى قيمة منطقية
            if (bool.TryParse(value, out bool boolValue))
                return boolValue;
            
            // التحقق من القيم المنطقية بالعربية أو الإنجليزية
            switch (value.ToLower())
            {
                case "true" or "yes" or "نعم" or "موجب" or "إيجابي":
                    return "Positive";
                case "false" or "no" or "لا" or "سالب" or "سلبي":
                    return "Negative";
                case "present" or "موجود":
                    return "Present";
                case "absent" or "none" or "غير موجود" or "لا يوجد":
                    return "None";
                case "normal" or "طبيعي":
                    return "Normal";
                case "abnormal" or "غير طبيعي":
                    return "Abnormal";
                default:
                    return value;
            }
        }
        
        /// <summary>
        /// إضافة قيم افتراضية للحقول المطلوبة إذا لم تكن موجودة في الملف
        /// </summary>
        private void AddDefaultValuesIfMissing(Dictionary<string, object> values)
        {
            var defaults = new Dictionary<string, object>
            {
                { "Color", "Brown" },
                { "Consistency", "Formed" },
                { "Shape", "Normal" },
                { "Weight", 100.0 },
                { "Quantity", 100.0 },
                { "Odor", "Normal" },
                { "OccultBlood", "Negative" },
                { "pH", "7.0" },
                { "pHValue", 7.0 },
                { "ReducingSubstances", "Negative" },
                { "FatContent", "Normal" },
                { "Fat", "Negative" },
                { "Protein", "Negative" },
                { "Mucus", "None" },
                { "Pus", "None" },
                { "Blood", "None" },
                { "UndigestedFood", "None" },
                { "FoodType", "" },
                { "MuscleFibers", "None" },
                { "Starch", "None" },
                { "StarchGranules", "None" },
                { "FatGlobules", "None" },
                { "Parasites", "None" },
                { "ParasiteType", "" },
                { "ParasiteCount", 0 },
                { "ParasiteStage", "" },
                { "Ova", "None" },
                { "OvaType", "" },
                { "OvaCount", 0 },
                { "Bacteria", "Normal" },
                { "BacterialType", "" },
                { "BacterialCount", 0 },
                { "Yeast", "None" },
                { "YeastType", "" },
                { "YeastCount", 0 },
                { "Calprotectin", "Normal" },
                { "CalprotectinValue", 50.0 },
                { "Lactoferrin", "Negative" },
                { "LactoferrinValue", 0.0 },
                { "Alpha1Antitrypsin", "Normal" },
                { "CollectionMethod", "Spontaneous" },
                { "PatientPreparation", "Normal diet" },
                { "CultureResult", "No Growth" },
                { "PathogenicOrganism", "" },
                { "Sensitivity", "" },
                { "RBC", 0 },
                { "WBC", 0 },
                { "EpithelialCells", 0 },
                { "Macrophages", 0 },
                { "Eosinophils", 0 }
            };
            
            foreach (var defaultValue in defaults)
            {
                if (!values.ContainsKey(defaultValue.Key))
                {
                    values[defaultValue.Key] = defaultValue.Value;
                }
            }
        }

        private string DetermineBleedingType(StoolTestResult result)
        {
            if (result.Color == "أسود")
                return "Upper Gastrointestinal Bleeding";
            else if (result.Color == "أحمر")
                return "Lower Gastrointestinal Bleeding";
            else if (result.OccultBlood != "Negative")
                return "Occult Bleeding";
            else
                return "None";
        }

        private string DetermineIBDSeverity(StoolTestResult result)
        {
            var severity = 0;
            
            if (result.Calprotectin == "High") severity += 3;
            else if (result.Calprotectin == "Elevated") severity += 2;
            
            if (result.Lactoferrin == "Positive") severity += 2;
            
            if (result.Mucus == "Present" || result.Mucus == "Abundant") severity += 1;
            
            if (severity >= 4)
                return "Severe";
            else if (severity >= 2)
                return "Moderate";
            else
                return "Mild";
        }

        private string DetermineMalabsorptionType(StoolTestResult result)
        {
            if (result.FatContent == "Increased" || result.FatGlobules == "Present" || result.FatGlobules == "Abundant")
                return "Fat Malabsorption";
            else if (result.ReducingSubstances == "Positive")
                return "Carbohydrate Malabsorption";
            else
                return "Mixed Malabsorption";
        }

        private string DetermineDiarrheaType(StoolTestResult result)
        {
            if (result.Consistency == "مائي")
                return "Watery Diarrhea";
            else if (result.Consistency == "سائل")
                return "Liquid Diarrhea";
            else
                return "Soft Stool";
        }

        private string DetermineColorCategory(string color)
        {
            switch (color)
            {
                case "بني":
                    return "Normal";
                case "أصفر":
                case "أخضر":
                    return "Abnormal";
                case "أسود":
                case "أحمر":
                case "أبيض":
                    return "Critical";
                default:
                    return "Unknown";
            }
        }

        private string DetermineConsistencyCategory(string consistency)
        {
            switch (consistency)
            {
                case "طبيعي":
                    return "Normal";
                case "طري":
                    return "Soft";
                case "سائل":
                case "مائي":
                    return "Liquid";
                default:
                    return "Unknown";
            }
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
        public int OccultBloodCount { get; set; }
        public int ParasiteCount { get; set; }
        public int OvaCount { get; set; }
        public int AbnormalBacteriaCount { get; set; }
        public int ElevatedCalprotectinCount { get; set; }
        public double AvgWeight { get; set; }
        public double AvgpH { get; set; }
        
        public double NormalPercentage => TotalTests > 0 ? (double)NormalTests / TotalTests * 100 : 0;
        public double AbnormalPercentage => TotalTests > 0 ? (double)AbnormalTests / TotalTests * 100 : 0;
        public double CriticalPercentage => TotalTests > 0 ? (double)CriticalTests / TotalTests * 100 : 0;
        public double OccultBloodPercentage => TotalTests > 0 ? (double)OccultBloodCount / TotalTests * 100 : 0;
        public double ParasitePercentage => TotalTests > 0 ? (double)ParasiteCount / TotalTests * 100 : 0;
        public double ElevatedCalprotectinPercentage => TotalTests > 0 ? (double)ElevatedCalprotectinCount / TotalTests * 100 : 0;
    }

    public class StoolPatternAnalysis
    {
        public bool HasGastrointestinalBleeding { get; set; }
        public string BleedingType { get; set; }
        public bool HasParasiticInfection { get; set; }
        public string ParasiteType { get; set; }
        public string OvaType { get; set; }
        public bool HasInflammatoryBowelDisease { get; set; }
        public string IBDSeverity { get; set; }
        public bool HasMalabsorption { get; set; }
        public string MalabsorptionType { get; set; }
        public bool HasDiarrhea { get; set; }
        public string DiarrheaType { get; set; }
        public string ColorCategory { get; set; }
        public string ConsistencyCategory { get; set; }
        
        public List<string> GetPatterns()
        {
            var patterns = new List<string>();
            
            if (HasGastrointestinalBleeding)
                patterns.Add($"Gastrointestinal Bleeding: {BleedingType}");
            
            if (HasParasiticInfection)
            {
                patterns.Add($"Parasitic Infection: {ParasiteType}");
                if (!string.IsNullOrEmpty(OvaType))
                    patterns.Add($"Ova: {OvaType}");
            }
            
            if (HasInflammatoryBowelDisease)
                patterns.Add($"Inflammatory Bowel Disease: {IBDSeverity}");
            
            if (HasMalabsorption)
                patterns.Add($"Malabsorption: {MalabsorptionType}");
            
            if (HasDiarrhea)
                patterns.Add($"Diarrhea: {DiarrheaType}");
            
            patterns.Add($"Color: {ColorCategory}");
            patterns.Add($"Consistency: {ConsistencyCategory}");
            
            return patterns;
        }
    }

    #endregion
}