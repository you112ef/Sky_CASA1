using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
    /// <summary>
    /// واجهة خدمة التقارير الطبية
    /// Medical Reports Service Interface
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// إنشاء تقرير CASA كامل مع التحليل والتوصيات
        /// Generate comprehensive CASA report with analysis and recommendations
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateCASAReportAsync(int examId, string userId, string userName);

        /// <summary>
        /// إنشاء تقرير CBC كامل مع النطاقات المرجعية
        /// Generate comprehensive CBC report with reference ranges
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateCBCReportAsync(int examId, string userId, string userName);

        /// <summary>
        /// إنشاء تقرير البول مع التحليل الطبي
        /// Generate urine analysis report with medical analysis
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateUrineReportAsync(int examId, string userId, string userName);

        /// <summary>
        /// إنشاء تقرير البراز مع التوصيات السريرية
        /// Generate stool analysis report with clinical recommendations
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateStoolReportAsync(int examId, string userId, string userName);

        /// <summary>
        /// إنشاء تقرير إحصائي شامل للفترة المحددة
        /// Generate comprehensive statistical report for specified period
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateStatisticsReportAsync(DateTime startDate, DateTime endDate, 
            string userId, string userName);

        /// <summary>
        /// إنشاء تقرير مجمع لعدة فحوصات للمريض الواحد
        /// Generate combined report for multiple patient examinations
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateCombinedReportAsync(int patientId, List<int> examIds, 
            string userId, string userName);

        /// <summary>
        /// تصدير التقرير إلى Excel مع التنسيق المهني
        /// Export report to Excel with professional formatting
        /// </summary>
        Task<Result<string>> ExportToExcelAsync(string reportType, int examId, string userId, string userName);

        /// <summary>
        /// معاينة التقرير قبل الإنشاء النهائي
        /// Preview report before final generation
        /// </summary>
        Task<Result<ReportPreviewData>> PreviewReportAsync(string reportType, int examId);

        /// <summary>
        /// الحصول على قائمة التقارير المحفوظة مع المعلومات
        /// Get list of saved reports with information
        /// </summary>
        Result<List<ReportInfo>> GetSavedReports(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// حذف تقرير محفوظ بأمان
        /// Safely delete a saved report
        /// </summary>
        Task<Result<bool>> DeleteReportAsync(string filePath, string userId);

        /// <summary>
        /// ضغط وأرشفة التقارير القديمة
        /// Compress and archive old reports
        /// </summary>
        Task<Result<ArchiveResult>> ArchiveOldReportsAsync(DateTime beforeDate, string userId);

        /// <summary>
        /// التحقق من صحة بيانات التقرير قبل الإنشاء
        /// Validate report data before generation
        /// </summary>
        Task<Result<ValidationResult>> ValidateReportDataAsync(string reportType, int examId);

        /// <summary>
        /// إنشاء تقرير مخصص بناء على القالب
        /// Generate custom report based on template
        /// </summary>
        Task<Result<ReportGenerationResult>> GenerateCustomReportAsync(CustomReportRequest request, 
            string userId, string userName);
    }

    #region Data Transfer Objects

    /// <summary>
    /// نتيجة إنشاء التقرير
    /// Report Generation Result
    /// </summary>
    public class ReportGenerationResult
    {
        public string ReportPath { get; set; }
        public string FileName { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string ReportType { get; set; }
        public long FileSizeBytes { get; set; }
        public int ExamId { get; set; }
        public string PatientName { get; set; }
        public ReportQualityMetrics QualityMetrics { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// مقاييس جودة التقرير
    /// Report Quality Metrics
    /// </summary>
    public class ReportQualityMetrics
    {
        public int TotalParameters { get; set; }
        public int ValidatedParameters { get; set; }
        public int AbnormalParameters { get; set; }
        public int CriticalParameters { get; set; }
        public double CompletenessScore { get; set; }
        public string QualityGrade { get; set; }
        public List<string> MissingData { get; set; } = new List<string>();
    }

    /// <summary>
    /// بيانات معاينة التقرير
    /// Report Preview Data
    /// </summary>
    public class ReportPreviewData
    {
        public string ReportType { get; set; }
        public ExamWithPatient Exam { get; set; }
        public object TestResults { get; set; }
        public List<ParameterSummary> ParametersSummary { get; set; } = new List<ParameterSummary>();
        public List<string> AbnormalFindings { get; set; } = new List<string>();
        public List<string> CriticalFindings { get; set; } = new List<string>();
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
    }

    /// <summary>
    /// ملخص المعامل الطبي
    /// Medical Parameter Summary
    /// </summary>
    public class ParameterSummary
    {
        public string ParameterName { get; set; }
        public string ParameterNameArabic { get; set; }
        public object Value { get; set; }
        public string Unit { get; set; }
        public string ReferenceRange { get; set; }
        public string Status { get; set; } // Normal, Abnormal, Critical
        public string ClinicalSignificance { get; set; }
        public bool IsCalculated { get; set; }
    }

    /// <summary>
    /// نتيجة الأرشفة
    /// Archive Result
    /// </summary>
    public class ArchiveResult
    {
        public int ArchivedFilesCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public string ArchiveFilePath { get; set; }
        public DateTime ArchiveDate { get; set; }
        public List<string> ArchivedFiles { get; set; } = new List<string>();
        public List<string> ArchiveErrors { get; set; } = new List<string>();
    }

    /// <summary>
    /// طلب تقرير مخصص
    /// Custom Report Request
    /// </summary>
    public class CustomReportRequest
    {
        public string TemplateName { get; set; }
        public List<int> ExamIds { get; set; } = new List<int>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string OutputFormat { get; set; } = "PDF"; // PDF, Excel, Word
        public bool IncludeClinicalRecommendations { get; set; } = true;
        public bool IncludeReferenceRanges { get; set; } = true;
        public string Language { get; set; } = "Arabic"; // Arabic, English, Bilingual
        public string Orientation { get; set; } = "Portrait"; // Portrait, Landscape
    }

    /// <summary>
    /// معلومات التقرير المحفوظ
    /// Saved Report Information  
    /// </summary>
    public class ReportInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string ReportType { get; set; }
        public DateTime CreatedDate { get; set; }
        public long FileSize { get; set; }
        public string CreatedBy { get; set; }
        public int ExamId { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; } // Active, Archived, Deleted
        public string FileHash { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    #endregion
}