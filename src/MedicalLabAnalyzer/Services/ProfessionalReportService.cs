using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Common.ErrorHandling;
using MedicalLabAnalyzer.Common.Exceptions;
using MedicalLabAnalyzer.Common.Validation;
using Microsoft.Extensions.Logging;
using Dapper;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using OfficeOpenXml;
using System.IO.Compression;

namespace MedicalLabAnalyzer.Services
{
    /// <summary>
    /// خدمة التقارير الطبية المهنية مع التحليل المتقدم والتوصيات السريرية
    /// Professional Medical Reports Service with Advanced Analysis and Clinical Recommendations
    /// </summary>
    public class ProfessionalReportService : IReportService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IErrorHandlerService _errorHandler;
        private readonly IMedicalValidationService _validationService;
        private readonly ILogger<ProfessionalReportService> _logger;
        private readonly AuditLogger _auditLogger;
        private readonly string _reportsDirectory;
        private readonly string _archiveDirectory;
        private readonly string _templatesDirectory;

        public ProfessionalReportService(
            IDatabaseService databaseService,
            IErrorHandlerService errorHandler,
            IMedicalValidationService validationService,
            ILogger<ProfessionalReportService> logger,
            AuditLogger auditLogger)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger;
            _auditLogger = auditLogger;

            // تحديد مسارات التقارير
            _reportsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            _archiveDirectory = Path.Combine(_reportsDirectory, "Archive");
            _templatesDirectory = Path.Combine(_reportsDirectory, "Templates");

            // إنشاء المجلدات المطلوبة
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// إنشاء تقرير CASA كامل مع تحليل WHO 2021
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateCASAReportAsync(int examId, string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                _logger?.LogInformation("Starting CASA report generation for exam {ExamId}", examId);

                // التحقق من صحة البيانات
                var validationResult = await ValidateReportDataAsync("CASA", examId);
                if (!validationResult.IsSuccess)
                    return Result<ReportGenerationResult>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);

                // جلب البيانات المطلوبة
                var examData = await GetExamWithPatientAsync(examId);
                var casaResult = await GetCASAResultAsync(examId);

                if (examData == null)
                    throw new AnalysisException($"لم يتم العثور على الفحص رقم {examId}", "CASA", examId);

                if (casaResult == null)
                    throw new AnalysisException($"لم يتم العثور على نتائج تحليل CASA للفحص رقم {examId}", "CASA", examId);

                // إجراء التحليل المتقدم
                var analysisResults = await PerformAdvancedCASAAnalysis(casaResult, examData.Gender);
                
                // إنشاء التقرير المهني
                var reportData = new ProfessionalCASAReportData
                {
                    Exam = examData,
                    CASAResult = casaResult,
                    AnalysisResults = analysisResults,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName,
                    ValidationResults = analysisResults.ValidationResults,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations,
                    QualityMetrics = analysisResults.QualityMetrics
                };

                // إنشاء ملف PDF مهني
                var reportPath = await GenerateProfessionalPDFReportAsync("CASA", reportData, examId);
                
                // إنشاء نتيجة التقرير
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = "CASA",
                    FileSizeBytes = new FileInfo(reportPath).Length,
                    ExamId = examId,
                    PatientName = examData.PatientName,
                    QualityMetrics = analysisResults.QualityMetrics,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations
                };

                // تسجيل في سجل المراجعة
                _auditLogger?.LogCASAnalysis(userId, userName, "", 0, $"CASA_Professional_Report_{examId}", 
                    new { ExamId = examId, ReportPath = reportPath, Quality = analysisResults.QualityMetrics.QualityGrade }, 
                    new { ReportGenerated = true, ReportPath = reportPath, QualityScore = analysisResults.QualityMetrics.CompletenessScore });

                _logger?.LogInformation("CASA report generated successfully for exam {ExamId} at {ReportPath}", examId, reportPath);

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        /// <summary>
        /// إنشاء تقرير CBC كامل مع النطاقات المرجعية
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateCBCReportAsync(int examId, string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                _logger?.LogInformation("Starting CBC report generation for exam {ExamId}", examId);

                var validationResult = await ValidateReportDataAsync("CBC", examId);
                if (!validationResult.IsSuccess)
                    return Result<ReportGenerationResult>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);

                var examData = await GetExamWithPatientAsync(examId);
                var cbcResult = await GetCBCResultAsync(examId);

                if (examData == null || cbcResult == null)
                    throw new AnalysisException($"لم يتم العثور على بيانات CBC للفحص رقم {examId}", "CBC", examId);

                var analysisResults = await PerformAdvancedCBCAnalysis(cbcResult, examData.Gender, examData.Age);

                var reportData = new ProfessionalCBCReportData
                {
                    Exam = examData,
                    CBCResult = cbcResult,
                    AnalysisResults = analysisResults,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName,
                    ValidationResults = analysisResults.ValidationResults,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations,
                    QualityMetrics = analysisResults.QualityMetrics
                };

                var reportPath = await GenerateProfessionalPDFReportAsync("CBC", reportData, examId);
                
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = "CBC",
                    FileSizeBytes = new FileInfo(reportPath).Length,
                    ExamId = examId,
                    PatientName = examData.PatientName,
                    QualityMetrics = analysisResults.QualityMetrics,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations
                };

                _auditLogger?.LogSystemEvent(userId, userName, "CBC_Professional_Report_Generated", "Reports", 
                    new { ExamId = examId, ReportPath = reportPath, Quality = analysisResults.QualityMetrics.QualityGrade });

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        /// <summary>
        /// إنشاء تقرير البول مع التحليل الطبي المتقدم
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateUrineReportAsync(int examId, string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                _logger?.LogInformation("Starting Urine report generation for exam {ExamId}", examId);

                var validationResult = await ValidateReportDataAsync("Urine", examId);
                if (!validationResult.IsSuccess)
                    return Result<ReportGenerationResult>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);

                var examData = await GetExamWithPatientAsync(examId);
                var urineResult = await GetUrineResultAsync(examId);

                if (examData == null || urineResult == null)
                    throw new AnalysisException($"لم يتم العثور على بيانات تحليل البول للفحص رقم {examId}", "Urine", examId);

                var analysisResults = await PerformAdvancedUrineAnalysis(urineResult, examData.Gender, examData.Age);

                var reportData = new ProfessionalUrineReportData
                {
                    Exam = examData,
                    UrineResult = urineResult,
                    AnalysisResults = analysisResults,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName,
                    ValidationResults = analysisResults.ValidationResults,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations,
                    QualityMetrics = analysisResults.QualityMetrics
                };

                var reportPath = await GenerateProfessionalPDFReportAsync("Urine", reportData, examId);
                
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = "Urine",
                    FileSizeBytes = new FileInfo(reportPath).Length,
                    ExamId = examId,
                    PatientName = examData.PatientName,
                    QualityMetrics = analysisResults.QualityMetrics,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations
                };

                _auditLogger?.LogSystemEvent(userId, userName, "Urine_Professional_Report_Generated", "Reports", 
                    new { ExamId = examId, ReportPath = reportPath, Quality = analysisResults.QualityMetrics.QualityGrade });

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        /// <summary>
        /// إنشاء تقرير البراز مع التوصيات السريرية
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateStoolReportAsync(int examId, string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                _logger?.LogInformation("Starting Stool report generation for exam {ExamId}", examId);

                var validationResult = await ValidateReportDataAsync("Stool", examId);
                if (!validationResult.IsSuccess)
                    return Result<ReportGenerationResult>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);

                var examData = await GetExamWithPatientAsync(examId);
                var stoolResult = await GetStoolResultAsync(examId);

                if (examData == null || stoolResult == null)
                    throw new AnalysisException($"لم يتم العثور على بيانات تحليل البراز للفحص رقم {examId}", "Stool", examId);

                var analysisResults = await PerformAdvancedStoolAnalysis(stoolResult, examData.Gender, examData.Age);

                var reportData = new ProfessionalStoolReportData
                {
                    Exam = examData,
                    StoolResult = stoolResult,
                    AnalysisResults = analysisResults,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName,
                    ValidationResults = analysisResults.ValidationResults,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations,
                    QualityMetrics = analysisResults.QualityMetrics
                };

                var reportPath = await GenerateProfessionalPDFReportAsync("Stool", reportData, examId);
                
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = "Stool",
                    FileSizeBytes = new FileInfo(reportPath).Length,
                    ExamId = examId,
                    PatientName = examData.PatientName,
                    QualityMetrics = analysisResults.QualityMetrics,
                    ClinicalRecommendations = analysisResults.ClinicalRecommendations
                };

                _auditLogger?.LogSystemEvent(userId, userName, "Stool_Professional_Report_Generated", "Reports", 
                    new { ExamId = examId, ReportPath = reportPath, Quality = analysisResults.QualityMetrics.QualityGrade });

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        /// <summary>
        /// إنشاء تقرير إحصائي شامل مع الرسوم البيانية
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateStatisticsReportAsync(DateTime startDate, DateTime endDate, 
            string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                _logger?.LogInformation("Starting Statistics report generation from {StartDate} to {EndDate}", startDate, endDate);

                if (startDate >= endDate)
                    throw new ValidationException("تاريخ البداية يجب أن يكون أقل من تاريخ النهاية", "ReportService");

                var statisticsData = await GetComprehensiveStatisticsAsync(startDate, endDate);
                var reportData = new ProfessionalStatisticsReportData
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Statistics = statisticsData,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };

                var reportPath = await GenerateProfessionalPDFReportAsync("Statistics", reportData, 0);
                
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = "Statistics",
                    FileSizeBytes = new FileInfo(reportPath).Length,
                    ExamId = 0,
                    PatientName = "تقرير إحصائي"
                };

                _auditLogger?.LogSystemEvent(userId, userName, "Statistics_Professional_Report_Generated", "Reports", 
                    new { StartDate = startDate, EndDate = endDate, ReportPath = reportPath, TotalExams = statisticsData.TotalExams });

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        /// <summary>
        /// إنشاء تقرير مجمع لعدة فحوصات للمريض الواحد
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateCombinedReportAsync(int patientId, List<int> examIds, 
            string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                _logger?.LogInformation("Starting Combined report generation for patient {PatientId}", patientId);

                if (examIds == null || examIds.Count == 0)
                    throw new ValidationException("يجب تحديد فحص واحد على الأقل", "ReportService");

                var patientData = await GetPatientAsync(patientId);
                if (patientData == null)
                    throw new ValidationException($"لم يتم العثور على المريض رقم {patientId}", "ReportService");

                var examData = new List<ExamWithResults>();
                foreach (var examId in examIds)
                {
                    var exam = await GetExamWithPatientAsync(examId);
                    if (exam != null && exam.PatientId == patientId)
                    {
                        var examResults = await GetAllTestResultsForExam(examId);
                        examData.Add(new ExamWithResults { Exam = exam, Results = examResults });
                    }
                }

                if (examData.Count == 0)
                    throw new ValidationException("لم يتم العثور على فحوصات صالحة للمريض", "ReportService");

                var reportData = new CombinedPatientReportData
                {
                    Patient = patientData,
                    Examinations = examData,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };

                var reportPath = await GenerateProfessionalPDFReportAsync("Combined", reportData, patientId);
                
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = "Combined",
                    FileSizeBytes = new FileInfo(reportPath).Length,
                    ExamId = patientId,
                    PatientName = patientData.FullName
                };

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        /// <summary>
        /// تصدير التقرير إلى Excel مع التنسيق المهني
        /// </summary>
        public async Task<Result<string>> ExportToExcelAsync(string reportType, int examId, string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                var fileName = $"{reportType}_Excel_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(_reportsDirectory, fileName);

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add($"{reportType} Report");
                    
                    // تطبيق التنسيق المهني
                    ApplyProfessionalExcelFormatting(worksheet);
                    
                    switch (reportType.ToUpper())
                    {
                        case "CASA":
                            await ExportCASAToExcelAsync(worksheet, examId);
                            break;
                        case "CBC":
                            await ExportCBCToExcelAsync(worksheet, examId);
                            break;
                        case "URINE":
                            await ExportUrineToExcelAsync(worksheet, examId);
                            break;
                        case "STOOL":
                            await ExportStoolToExcelAsync(worksheet, examId);
                            break;
                        default:
                            throw new ArgumentException($"نوع التقرير غير مدعوم: {reportType}");
                    }

                    package.SaveAs(new FileInfo(filePath));
                }

                _auditLogger?.LogSystemEvent(userId, userName, $"{reportType}_Excel_Export", "Reports", 
                    new { ExamId = examId, FilePath = filePath });

                return Result<string>.Success(filePath);
            });
        }

        /// <summary>
        /// معاينة التقرير قبل الإنشاء النهائي
        /// </summary>
        public async Task<Result<ReportPreviewData>> PreviewReportAsync(string reportType, int examId)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                var examData = await GetExamWithPatientAsync(examId);
                if (examData == null)
                    throw new ValidationException($"لم يتم العثور على الفحص رقم {examId}", "ReportService");

                object testResults = null;
                var parametersSummary = new List<ParameterSummary>();
                var abnormalFindings = new List<string>();
                var criticalFindings = new List<string>();
                var recommendations = new List<string>();

                switch (reportType.ToUpper())
                {
                    case "CASA":
                        var casaResult = await GetCASAResultAsync(examId);
                        if (casaResult != null)
                        {
                            testResults = casaResult;
                            var analysis = await PerformAdvancedCASAAnalysis(casaResult, examData.Gender);
                            parametersSummary = analysis.ParametersSummary;
                            abnormalFindings = analysis.AbnormalFindings;
                            criticalFindings = analysis.CriticalFindings;
                            recommendations = analysis.ClinicalRecommendations;
                        }
                        break;
                    case "CBC":
                        var cbcResult = await GetCBCResultAsync(examId);
                        if (cbcResult != null)
                        {
                            testResults = cbcResult;
                            var analysis = await PerformAdvancedCBCAnalysis(cbcResult, examData.Gender, examData.Age);
                            parametersSummary = analysis.ParametersSummary;
                            abnormalFindings = analysis.AbnormalFindings;
                            criticalFindings = analysis.CriticalFindings;
                            recommendations = analysis.ClinicalRecommendations;
                        }
                        break;
                    case "URINE":
                        var urineResult = await GetUrineResultAsync(examId);
                        if (urineResult != null)
                        {
                            testResults = urineResult;
                            var analysis = await PerformAdvancedUrineAnalysis(urineResult, examData.Gender, examData.Age);
                            parametersSummary = analysis.ParametersSummary;
                            abnormalFindings = analysis.AbnormalFindings;
                            criticalFindings = analysis.CriticalFindings;
                            recommendations = analysis.ClinicalRecommendations;
                        }
                        break;
                    case "STOOL":
                        var stoolResult = await GetStoolResultAsync(examId);
                        if (stoolResult != null)
                        {
                            testResults = stoolResult;
                            var analysis = await PerformAdvancedStoolAnalysis(stoolResult, examData.Gender, examData.Age);
                            parametersSummary = analysis.ParametersSummary;
                            abnormalFindings = analysis.AbnormalFindings;
                            criticalFindings = analysis.CriticalFindings;
                            recommendations = analysis.ClinicalRecommendations;
                        }
                        break;
                }

                var preview = new ReportPreviewData
                {
                    ReportType = reportType,
                    Exam = examData,
                    TestResults = testResults,
                    ParametersSummary = parametersSummary,
                    AbnormalFindings = abnormalFindings,
                    CriticalFindings = criticalFindings,
                    ClinicalRecommendations = recommendations,
                    QualityMetrics = CalculateQualityMetrics(parametersSummary)
                };

                return Result<ReportPreviewData>.Success(preview);
            });
        }

        /// <summary>
        /// الحصول على قائمة التقارير المحفوظة
        /// </summary>
        public Result<List<ReportInfo>> GetSavedReports(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return _errorHandler.Handle(() =>
            {
                var reports = new List<ReportInfo>();
                
                // البحث في المجلد الرئيسي والأرشيف
                var searchDirectories = new[] { _reportsDirectory, _archiveDirectory };
                
                foreach (var directory in searchDirectories)
                {
                    if (!Directory.Exists(directory)) continue;
                    
                    var files = Directory.GetFiles(directory, "*.pdf")
                                      .Concat(Directory.GetFiles(directory, "*.xlsx"));
                    
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        
                        // تطبيق فلتر التاريخ
                        if (fromDate.HasValue && fileInfo.CreationTime < fromDate.Value) continue;
                        if (toDate.HasValue && fileInfo.CreationTime > toDate.Value) continue;
                        
                        var reportInfo = new ReportInfo
                        {
                            FileName = fileInfo.Name,
                            FilePath = file,
                            CreatedDate = fileInfo.CreationTime,
                            FileSize = fileInfo.Length,
                            Status = directory.EndsWith("Archive") ? "Archived" : "Active",
                            FileHash = CalculateFileHash(file)
                        };
                        
                        // استخراج معلومات إضافية من اسم الملف
                        ParseFileNameInfo(reportInfo);
                        reports.Add(reportInfo);
                    }
                }

                return Result<List<ReportInfo>>.Success(reports.OrderByDescending(r => r.CreatedDate).ToList());
            });
        }

        /// <summary>
        /// حذف تقرير محفوظ بأمان
        /// </summary>
        public async Task<Result<bool>> DeleteReportAsync(string filePath, string userId)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                if (!File.Exists(filePath))
                    throw new FileOperationException($"الملف غير موجود: {filePath}", filePath, "Delete");

                // إنشاء نسخة احتياطية قبل الحذف
                var backupPath = Path.Combine(_archiveDirectory, "Deleted", Path.GetFileName(filePath));
                Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                File.Copy(filePath, backupPath, true);

                File.Delete(filePath);
                
                _auditLogger?.LogSystemEvent(userId, "", "Report_Deleted", "Reports", 
                    new { FilePath = filePath, BackupPath = backupPath });

                return Result<bool>.Success(true);
            });
        }

        /// <summary>
        /// ضغط وأرشفة التقارير القديمة
        /// </summary>
        public async Task<Result<ArchiveResult>> ArchiveOldReportsAsync(DateTime beforeDate, string userId)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                var archiveFileName = $"Reports_Archive_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                var archiveFilePath = Path.Combine(_archiveDirectory, archiveFileName);
                
                var filesToArchive = Directory.GetFiles(_reportsDirectory, "*.*", SearchOption.TopDirectoryOnly)
                                              .Where(f => new FileInfo(f).CreationTime < beforeDate)
                                              .ToList();

                if (filesToArchive.Count == 0)
                {
                    return Result<ArchiveResult>.Success(new ArchiveResult
                    {
                        ArchivedFilesCount = 0,
                        ArchiveDate = DateTime.Now
                    });
                }

                using (var archive = ZipFile.Open(archiveFilePath, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToArchive)
                    {
                        archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }

                // حذف الملفات الأصلية بعد الأرشفة
                var deletedFiles = new List<string>();
                foreach (var file in filesToArchive)
                {
                    try
                    {
                        File.Delete(file);
                        deletedFiles.Add(file);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "فشل في حذف الملف: {FilePath}", file);
                    }
                }

                var result = new ArchiveResult
                {
                    ArchivedFilesCount = deletedFiles.Count,
                    TotalSizeBytes = new FileInfo(archiveFilePath).Length,
                    ArchiveFilePath = archiveFilePath,
                    ArchiveDate = DateTime.Now,
                    ArchivedFiles = deletedFiles
                };

                _auditLogger?.LogSystemEvent(userId, "", "Reports_Archived", "Reports", 
                    new { ArchivedCount = result.ArchivedFilesCount, ArchiveFilePath = archiveFilePath });

                return Result<ArchiveResult>.Success(result);
            });
        }

        /// <summary>
        /// التحقق من صحة بيانات التقرير قبل الإنشاء
        /// </summary>
        public async Task<Result<ValidationResult>> ValidateReportDataAsync(string reportType, int examId)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                var validationResult = new ValidationResult();

                // التحقق من وجود الفحص
                var exam = await GetExamWithPatientAsync(examId);
                if (exam == null)
                {
                    validationResult.AddError($"لم يتم العثور على الفحص رقم {examId}");
                    return Result<ValidationResult>.Success(validationResult);
                }

                // التحقق من نوع الفحص
                if (!exam.ExamType.Equals(reportType, StringComparison.OrdinalIgnoreCase))
                {
                    validationResult.AddError($"نوع الفحص ({exam.ExamType}) لا يتطابق مع نوع التقرير المطلوب ({reportType})");
                }

                // التحقق من وجود النتائج حسب نوع التقرير
                switch (reportType.ToUpper())
                {
                    case "CASA":
                        var casaResult = await GetCASAResultAsync(examId);
                        if (casaResult == null)
                            validationResult.AddError("لم يتم العثور على نتائج تحليل CASA");
                        else
                        {
                            var casaValidation = await _validationService.ValidateCASAResultAsync(casaResult);
                            if (!casaValidation.IsValid)
                                validationResult.Combine(casaValidation);
                        }
                        break;
                        
                    case "CBC":
                        var cbcResult = await GetCBCResultAsync(examId);
                        if (cbcResult == null)
                            validationResult.AddError("لم يتم العثور على نتائج تحليل CBC");
                        else
                        {
                            var cbcValidation = await _validationService.ValidateCBCResultAsync(cbcResult, exam.Gender, exam.Age);
                            if (!cbcValidation.IsValid)
                                validationResult.Combine(cbcValidation);
                        }
                        break;
                        
                    case "URINE":
                        var urineResult = await GetUrineResultAsync(examId);
                        if (urineResult == null)
                            validationResult.AddError("لم يتم العثور على نتائج تحليل البول");
                        else
                        {
                            var urineValidation = await _validationService.ValidateUrineResultAsync(urineResult);
                            if (!urineValidation.IsValid)
                                validationResult.Combine(urineValidation);
                        }
                        break;
                        
                    case "STOOL":
                        var stoolResult = await GetStoolResultAsync(examId);
                        if (stoolResult == null)
                            validationResult.AddError("لم يتم العثور على نتائج تحليل البراز");
                        else
                        {
                            var stoolValidation = await _validationService.ValidateStoolResultAsync(stoolResult);
                            if (!stoolValidation.IsValid)
                                validationResult.Combine(stoolValidation);
                        }
                        break;
                        
                    default:
                        validationResult.AddError($"نوع التقرير غير مدعوم: {reportType}");
                        break;
                }

                return Result<ValidationResult>.Success(validationResult);
            });
        }

        /// <summary>
        /// إنشاء تقرير مخصص بناء على القالب
        /// </summary>
        public async Task<Result<ReportGenerationResult>> GenerateCustomReportAsync(CustomReportRequest request, 
            string userId, string userName)
        {
            return await _errorHandler.HandleAsync(async () =>
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.ExamIds == null || request.ExamIds.Count == 0)
                    throw new ValidationException("يجب تحديد فحص واحد على الأقل", "ReportService");

                // تحميل القالب المخصص
                var templatePath = Path.Combine(_templatesDirectory, $"{request.TemplateName}.json");
                if (!File.Exists(templatePath))
                    throw new FileOperationException($"قالب التقرير غير موجود: {request.TemplateName}", templatePath, "Read");

                // إنشاء التقرير المخصص
                var reportPath = await GenerateCustomReportFromTemplate(request, templatePath, userName);
                
                var result = new ReportGenerationResult
                {
                    ReportPath = reportPath,
                    FileName = Path.GetFileName(reportPath),
                    GeneratedDate = DateTime.Now,
                    ReportType = $"Custom_{request.TemplateName}",
                    FileSizeBytes = new FileInfo(reportPath).Length
                };

                _auditLogger?.LogSystemEvent(userId, userName, "Custom_Report_Generated", "Reports", 
                    new { TemplateName = request.TemplateName, ExamIds = request.ExamIds, ReportPath = reportPath });

                return Result<ReportGenerationResult>.Success(result);
            });
        }

        #region Private Helper Methods

        private void EnsureDirectoriesExist()
        {
            var directories = new[] { _reportsDirectory, _archiveDirectory, _templatesDirectory, 
                                      Path.Combine(_archiveDirectory, "Deleted") };
            
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
        }

        private async Task<ExamWithPatient> GetExamWithPatientAsync(int examId)
        {
            var connection = await _databaseService.GetConnectionAsync();
            var sql = @"
                SELECT e.Id, e.PatientId, e.ExamType, e.ExamDate, e.Notes, 
                       p.FirstName + ' ' + p.LastName as PatientName, p.Age, p.Gender 
                FROM Exams e 
                JOIN Patients p ON e.PatientId = p.Id 
                WHERE e.Id = @ExamId";
            
            return await connection.QueryFirstOrDefaultAsync<ExamWithPatient>(sql, new { ExamId = examId });
        }

        private async Task<Patient> GetPatientAsync(int patientId)
        {
            var connection = await _databaseService.GetConnectionAsync();
            var sql = "SELECT * FROM Patients WHERE Id = @PatientId";
            return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { PatientId = patientId });
        }

        private async Task<CASA_Result> GetCASAResultAsync(int examId)
        {
            var connection = await _databaseService.GetConnectionAsync();
            var sql = "SELECT * FROM CASAResults WHERE ExamId = @ExamId";
            return await connection.QueryFirstOrDefaultAsync<CASA_Result>(sql, new { ExamId = examId });
        }

        private async Task<CBCTestResult> GetCBCResultAsync(int examId)
        {
            var connection = await _databaseService.GetConnectionAsync();
            var sql = "SELECT * FROM CBCTestResults WHERE ExamId = @ExamId";
            return await connection.QueryFirstOrDefaultAsync<CBCTestResult>(sql, new { ExamId = examId });
        }

        private async Task<UrineTestResult> GetUrineResultAsync(int examId)
        {
            var connection = await _databaseService.GetConnectionAsync();
            var sql = "SELECT * FROM UrineTestResults WHERE ExamId = @ExamId";
            return await connection.QueryFirstOrDefaultAsync<UrineTestResult>(sql, new { ExamId = examId });
        }

        private async Task<StoolTestResult> GetStoolResultAsync(int examId)
        {
            var connection = await _databaseService.GetConnectionAsync();
            var sql = "SELECT * FROM StoolTestResults WHERE ExamId = @ExamId";
            return await connection.QueryFirstOrDefaultAsync<StoolTestResult>(sql, new { ExamId = examId });
        }

        private string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha256.ComputeHash(stream);
                return Convert.ToBase64String(hash);
            }
        }

        private void ParseFileNameInfo(ReportInfo reportInfo)
        {
            // استخراج معلومات من اسم الملف مثل نوع التقرير ورقم الفحص
            var fileName = Path.GetFileNameWithoutExtension(reportInfo.FileName);
            var parts = fileName.Split('_');
            
            if (parts.Length >= 3)
            {
                reportInfo.ReportType = parts[0];
                if (int.TryParse(parts[parts.Length - 1], out int examId))
                {
                    reportInfo.ExamId = examId;
                }
            }
        }

        private ReportQualityMetrics CalculateQualityMetrics(List<ParameterSummary> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return new ReportQualityMetrics
                {
                    QualityGrade = "Insufficient Data",
                    CompletenessScore = 0
                };
            }

            var totalParams = parameters.Count;
            var validatedParams = parameters.Count(p => !string.IsNullOrEmpty(p.Status));
            var abnormalParams = parameters.Count(p => p.Status == "Abnormal");
            var criticalParams = parameters.Count(p => p.Status == "Critical");

            var completeness = (double)validatedParams / totalParams * 100;
            string grade;

            if (completeness >= 95) grade = "Excellent";
            else if (completeness >= 85) grade = "Good";
            else if (completeness >= 70) grade = "Fair";
            else grade = "Poor";

            return new ReportQualityMetrics
            {
                TotalParameters = totalParams,
                ValidatedParameters = validatedParams,
                AbnormalParameters = abnormalParams,
                CriticalParameters = criticalParams,
                CompletenessScore = completeness,
                QualityGrade = grade,
                MissingData = parameters.Where(p => string.IsNullOrEmpty(p.Status))
                                       .Select(p => p.ParameterNameArabic)
                                       .ToList()
            };
        }

        // تتم إضافة باقي الدوال المساعدة في الجزء التالي...

        #endregion

        // سيتم إضافة باقي التطبيق في ملف منفصل للحفاظ على الحجم المناسب
    }
}