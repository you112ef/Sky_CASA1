using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace MedicalLabAnalyzer.Services
{
    public class CBCAnalyzer
    {
        private readonly ILogger<CBCAnalyzer> _logger;

        public CBCAnalyzer(ILogger<CBCAnalyzer> logger)
        {
            _logger = logger;
        }

        public async Task<CBCTestResult> AnalyzeCBCAsync(CBCTestResult result, string gender, int age)
        {
            try
            {
                _logger.LogInformation("Starting CBC analysis for exam {ExamId}", result.ExamId);

                // Set reference ranges based on gender and age
                SetReferenceRanges(result, gender, age);

                // Perform analysis
                result.AnalyzeResults();

                // Generate interpretation
                result.Interpretation = GenerateInterpretation(result);

                // Set analysis metadata
                result.AnalysisDate = DateTime.Now;
                result.AnalyzedBy = "CBCAnalyzer";
                result.VerifiedDate = DateTime.Now;
                result.VerifiedBy = "System";

                _logger.LogInformation("CBC analysis completed successfully for exam {ExamId}", result.ExamId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing CBC for exam {ExamId}", result.ExamId);
                throw;
            }
        }

        private void SetReferenceRanges(CBCTestResult result, string gender, int age)
        {
            // RBC Reference Ranges
            if (gender == "ذكر")
            {
                result.RBCMin = 4.5m;
                result.RBCMax = 5.5m;
            }
            else
            {
                result.RBCMin = 4.0m;
                result.RBCMax = 5.0m;
            }

            // Hemoglobin Reference Ranges
            if (gender == "ذكر")
            {
                result.HemoglobinMin = 13.5m;
                result.HemoglobinMax = 17.5m;
            }
            else
            {
                result.HemoglobinMin = 12.0m;
                result.HemoglobinMax = 15.5m;
            }

            // Hematocrit Reference Ranges
            if (gender == "ذكر")
            {
                result.HematocritMin = 41.0m;
                result.HematocritMax = 50.0m;
            }
            else
            {
                result.HematocritMin = 36.0m;
                result.HematocritMax = 46.0m;
            }

            // WBC Reference Ranges (age-dependent)
            if (age < 1)
            {
                result.WBCMin = 6.0m;
                result.WBCMax = 17.5m;
            }
            else if (age < 2)
            {
                result.WBCMin = 6.0m;
                result.WBCMax = 17.0m;
            }
            else if (age < 4)
            {
                result.WBCMin = 5.5m;
                result.WBCMax = 15.5m;
            }
            else if (age < 6)
            {
                result.WBCMin = 5.0m;
                result.WBCMax = 14.5m;
            }
            else if (age < 8)
            {
                result.WBCMin = 4.5m;
                result.WBCMax = 13.5m;
            }
            else if (age < 12)
            {
                result.WBCMin = 4.5m;
                result.WBCMax = 13.0m;
            }
            else
            {
                result.WBCMin = 4.0m;
                result.WBCMax = 11.0m;
            }

            // ESR Reference Ranges
            if (gender == "ذكر")
            {
                result.ESRMin = 0.0m;
                result.ESRMax = 15.0m;
            }
            else
            {
                result.ESRMin = 0.0m;
                result.ESRMax = 20.0m;
            }

            // Set other reference ranges (standard for adults)
            result.MCVMin = 80.0m;
            result.MCVMax = 100.0m;

            result.MCHMin = 27.0m;
            result.MCHMax = 32.0m;

            result.MCHCMin = 32.0m;
            result.MCHCMax = 36.0m;

            result.RDWMin = 11.5m;
            result.RDWMax = 14.5m;

            result.NeutrophilsMin = 40.0m;
            result.NeutrophilsMax = 70.0m;

            result.LymphocytesMin = 20.0m;
            result.LymphocytesMax = 40.0m;

            result.MonocytesMin = 2.0m;
            result.MonocytesMax = 8.0m;

            result.EosinophilsMin = 1.0m;
            result.EosinophilsMax = 4.0m;

            result.BasophilsMin = 0.5m;
            result.BasophilsMax = 1.0m;

            result.PlateletsMin = 150.0m;
            result.PlateletsMax = 450.0m;

            result.MPVMin = 7.5m;
            result.MPVMax = 11.5m;

            result.PDWMin = 10.0m;
            result.PDWMax = 17.0m;

            result.PCTMin = 0.1m;
            result.PCTMax = 0.3m;

            result.ReticulocytesMin = 0.5m;
            result.ReticulocytesMax = 2.5m;

            result.CRPMin = 0.0m;
            result.CRPMax = 3.0m;
        }

        private string GenerateInterpretation(CBCTestResult result)
        {
            var interpretation = new System.Text.StringBuilder();

            // Overall assessment
            interpretation.AppendLine($"التقييم العام: {GetStatusInArabic(result.OverallStatus)}");
            interpretation.AppendLine();

            // RBC assessment
            if (!string.IsNullOrEmpty(result.RBCStatus) && result.RBCStatus != "Normal")
            {
                interpretation.AppendLine($"خلايا الدم الحمراء: {GetStatusInArabic(result.RBCStatus)}");
                if (result.RBCStatus == "Low")
                    interpretation.AppendLine("  - قد يشير إلى فقر دم");
                else if (result.RBCStatus == "High")
                    interpretation.AppendLine("  - قد يشير إلى كثرة الحمر");
            }

            // Hemoglobin assessment
            if (!string.IsNullOrEmpty(result.HemoglobinStatus) && result.HemoglobinStatus != "Normal")
            {
                interpretation.AppendLine($"الهيموغلوبين: {GetStatusInArabic(result.HemoglobinStatus)}");
                if (result.HemoglobinStatus == "Low")
                    interpretation.AppendLine("  - قد يشير إلى فقر دم");
                else if (result.HemoglobinStatus == "High")
                    interpretation.AppendLine("  - قد يشير إلى كثرة الحمر");
            }

            // WBC assessment
            if (!string.IsNullOrEmpty(result.WBCStatus) && result.WBCStatus != "Normal")
            {
                interpretation.AppendLine($"خلايا الدم البيضاء: {GetStatusInArabic(result.WBCStatus)}");
                if (result.WBCStatus == "Low")
                    interpretation.AppendLine("  - قد يشير إلى نقص المناعة");
                else if (result.WBCStatus == "High")
                    interpretation.AppendLine("  - قد يشير إلى التهاب أو عدوى");
            }

            // Platelets assessment
            if (!string.IsNullOrEmpty(result.PlateletsStatus) && result.PlateletsStatus != "Normal")
            {
                interpretation.AppendLine($"الصفائح الدموية: {GetStatusInArabic(result.PlateletsStatus)}");
                if (result.PlateletsStatus == "Low")
                    interpretation.AppendLine("  - قد يشير إلى نقص الصفائح");
                else if (result.PlateletsStatus == "High")
                    interpretation.AppendLine("  - قد يشير إلى كثرة الصفائح");
            }

            // Differential count assessment
            if (!string.IsNullOrEmpty(result.NeutrophilsStatus) && result.NeutrophilsStatus != "Normal")
            {
                interpretation.AppendLine($"العدلات: {GetStatusInArabic(result.NeutrophilsStatus)}");
            }

            if (!string.IsNullOrEmpty(result.LymphocytesStatus) && result.LymphocytesStatus != "Normal")
            {
                interpretation.AppendLine($"الخلايا الليمفاوية: {GetStatusInArabic(result.LymphocytesStatus)}");
            }

            // ESR assessment
            if (!string.IsNullOrEmpty(result.ESRStatus) && result.ESRStatus != "Normal")
            {
                interpretation.AppendLine($"معدل ترسيب كريات الدم الحمراء: {GetStatusInArabic(result.ESRStatus)}");
                if (result.ESRStatus == "High")
                    interpretation.AppendLine("  - قد يشير إلى التهاب أو عدوى");
            }

            // CRP assessment
            if (!string.IsNullOrEmpty(result.CRPStatus) && result.CRPStatus != "Normal")
            {
                interpretation.AppendLine($"بروتين سي التفاعلي: {GetStatusInArabic(result.CRPStatus)}");
                if (result.CRPStatus == "High")
                    interpretation.AppendLine("  - قد يشير إلى التهاب حاد");
            }

            // Recommendations
            interpretation.AppendLine();
            interpretation.AppendLine("التوصيات:");
            if (result.OverallStatus == "Normal")
            {
                interpretation.AppendLine("- النتائج ضمن النطاق الطبيعي");
                interpretation.AppendLine("- لا حاجة لإجراءات إضافية");
            }
            else if (result.OverallStatus == "Abnormal")
            {
                interpretation.AppendLine("- مراجعة الطبيب المعالج");
                interpretation.AppendLine("- إجراء فحوصات إضافية إذا لزم الأمر");
            }
            else if (result.OverallStatus == "Critical")
            {
                interpretation.AppendLine("- مراجعة طبية عاجلة");
                interpretation.AppendLine("- إجراء فحوصات إضافية فورية");
            }

            return interpretation.ToString();
        }

        private string GetStatusInArabic(string status)
        {
            return status switch
            {
                "Normal" => "طبيعي",
                "Low" => "منخفض",
                "High" => "مرتفع",
                "Abnormal" => "غير طبيعي",
                "Critical" => "حرج",
                _ => status
            };
        }

        public async Task<bool> ValidateCBCResultsAsync(CBCTestResult result)
        {
            try
            {
                _logger.LogInformation("Validating CBC results for exam {ExamId}", result.ExamId);

                var isValid = true;
                var validationErrors = new System.Text.StringBuilder();

                // Check for required values
                if (!result.RBC.HasValue)
                {
                    validationErrors.AppendLine("- قيمة RBC مطلوبة");
                    isValid = false;
                }

                if (!result.Hemoglobin.HasValue)
                {
                    validationErrors.AppendLine("- قيمة الهيموغلوبين مطلوبة");
                    isValid = false;
                }

                if (!result.WBC.HasValue)
                {
                    validationErrors.AppendLine("- قيمة WBC مطلوبة");
                    isValid = false;
                }

                if (!result.Platelets.HasValue)
                {
                    validationErrors.AppendLine("- قيمة الصفائح الدموية مطلوبة");
                    isValid = false;
                }

                // Check for logical consistency
                if (result.RBC.HasValue && result.Hemoglobin.HasValue)
                {
                    var expectedHgb = result.RBC.Value * 3.5m; // Rough estimate
                    if (Math.Abs((decimal)(result.Hemoglobin.Value - expectedHgb)) > 5)
                    {
                        validationErrors.AppendLine("- عدم تطابق بين RBC والهيموغلوبين");
                        isValid = false;
                    }
                }

                // Check WBC differential adds up to 100%
                var differentialSum = (result.Neutrophils ?? 0) + (result.Lymphocytes ?? 0) + 
                                    (result.Monocytes ?? 0) + (result.Eosinophils ?? 0) + 
                                    (result.Basophils ?? 0);

                if (differentialSum > 0 && Math.Abs(differentialSum - 100) > 5)
                {
                    validationErrors.AppendLine("- مجموع التفاضل الخلوي لا يساوي 100%");
                    isValid = false;
                }

                if (!isValid)
                {
                    _logger.LogWarning("CBC validation failed for exam {ExamId}: {Errors}", 
                        result.ExamId, validationErrors.ToString());
                }
                else
                {
                    _logger.LogInformation("CBC validation passed for exam {ExamId}", result.ExamId);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CBC results for exam {ExamId}", result.ExamId);
                return false;
            }
        }
    }
}