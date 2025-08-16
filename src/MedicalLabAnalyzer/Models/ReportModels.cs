using System;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    public class ReportRequest
    {
        public string Id { get; set; }
        public string ReportType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string CompletedBy { get; set; }
        public ReportParameters Parameters { get; set; }
        public string OutputFormat { get; set; }
        public string OutputPath { get; set; }
        public long FileSize { get; set; }
        public string Notes { get; set; }
        public int Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ReportParameters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> PatientIds { get; set; } = new List<string>();
        public List<string> TestTypes { get; set; } = new List<string>();
        public List<string> Physicians { get; set; } = new List<string>();
        public List<string> Departments { get; set; } = new List<string>();
        public List<string> Statuses { get; set; } = new List<string>();
        public string GroupBy { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public bool IncludeCharts { get; set; }
        public bool IncludeTables { get; set; }
        public bool IncludeSummary { get; set; }
        public List<string> CustomFields { get; set; } = new List<string>();
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    }

    public class ReportTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReportType { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public ReportParameters DefaultParameters { get; set; }
        public string Layout { get; set; }
        public List<string> RequiredFields { get; set; } = new List<string>();
        public List<string> OptionalFields { get; set; } = new List<string>();
        public Dictionary<string, object> Styling { get; set; } = new Dictionary<string, object>();
        public List<string> SupportedFormats { get; set; } = new List<string>();
        public int EstimatedDuration { get; set; }
        public string Difficulty { get; set; }
        public List<string> Prerequisites { get; set; } = new List<string>();
    }

    public class ReportSchedule
    {
        public string Id { get; set; }
        public string ReportType { get; set; }
        public string ReportTemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ScheduleType { get; set; }
        public string CronExpression { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ReportParameters Parameters { get; set; }
        public string OutputFormat { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public List<string> NotificationMethods { get; set; } = new List<string>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    public class ReportExecution
    {
        public string Id { get; set; }
        public string ReportRequestId { get; set; }
        public string ReportType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public string ExecutedBy { get; set; }
        public int Progress { get; set; }
        public string CurrentStep { get; set; }
        public List<string> CompletedSteps { get; set; } = new List<string>();
        public List<string> PendingSteps { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        public string OutputPath { get; set; }
        public long FileSize { get; set; }
        public string Checksum { get; set; }
        public Dictionary<string, object> ExecutionContext { get; set; } = new Dictionary<string, object>();
    }

    public class ReportDistribution
    {
        public string Id { get; set; }
        public string ReportExecutionId { get; set; }
        public string ReportType { get; set; }
        public DateTime DistributionDate { get; set; }
        public string DistributedBy { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public List<string> SuccessfulDeliveries { get; set; } = new List<string>();
        public List<string> FailedDeliveries { get; set; } = new List<string>();
        public string DeliveryNotes { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool RequireConfirmation { get; set; }
        public List<string> ConfirmedDeliveries { get; set; } = new List<string>();
        public List<string> PendingConfirmations { get; set; } = new List<string>();
        public Dictionary<string, object> DeliveryMetadata { get; set; } = new Dictionary<string, object>();
    }

    public class ReportArchive
    {
        public string Id { get; set; }
        public string ReportExecutionId { get; set; }
        public string ReportType { get; set; }
        public string Title { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public string ArchivePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileFormat { get; set; }
        public string Checksum { get; set; }
        public string CompressionType { get; set; }
        public string EncryptionType { get; set; }
        public DateTime ArchivedDate { get; set; }
        public string ArchivedBy { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ReportAnalytics
    {
        public DateTime Date { get; set; }
        public int TotalReports { get; set; }
        public int SuccessfulReports { get; set; }
        public int FailedReports { get; set; }
        public int PendingReports { get; set; }
        public double AverageExecutionTime { get; set; }
        public double AverageFileSize { get; set; }
        public Dictionary<string, int> ReportsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ReportsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ReportsByFormat { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> AverageExecutionTimeByType { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> AverageFileSizeByType { get; set; } = new Dictionary<string, double>();
        public List<string> TopReportTypes { get; set; } = new List<string>();
        public List<string> TopUsers { get; set; } = new List<string>();
        public List<string> TopFormats { get; set; } = new List<string>();
        public List<string> PeakHours { get; set; } = new List<string>();
        public List<string> PeakDays { get; set; } = new List<string>();
    }

    public class ReportSecurity
    {
        public string Id { get; set; }
        public string ReportType { get; set; }
        public string SecurityLevel { get; set; }
        public List<string> RequiredPermissions { get; set; } = new List<string>();
        public List<string> RequiredRoles { get; set; } = new List<string>();
        public List<string> RestrictedUsers { get; set; } = new List<string>();
        public List<string> RestrictedGroups { get; set; } = new List<string>();
        public bool RequireAuthentication { get; set; }
        public bool RequireAuthorization { get; set; }
        public bool RequireAudit { get; set; }
        public bool RequireEncryption { get; set; }
        public string EncryptionAlgorithm { get; set; }
        public string EncryptionKey { get; set; }
        public bool RequireWatermark { get; set; }
        public string WatermarkText { get; set; }
        public bool RequireDigitalSignature { get; set; }
        public string DigitalSignatureCertificate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public Dictionary<string, object> SecuritySettings { get; set; } = new Dictionary<string, object>();
    }

    public class ReportCompliance
    {
        public string Id { get; set; }
        public string ReportType { get; set; }
        public string ComplianceStandard { get; set; }
        public string Requirement { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public bool IsCompliant { get; set; }
        public DateTime LastAssessment { get; set; }
        public DateTime NextAssessment { get; set; }
        public string AssessedBy { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public List<string> Evidence { get; set; } = new List<string>();
        public List<string> Violations { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
        public double ComplianceScore { get; set; }
        public string RiskLevel { get; set; }
        public List<string> RequiredActions { get; set; } = new List<string>();
        public DateTime? DueDate { get; set; }
        public string ResponsiblePerson { get; set; }
    }
}