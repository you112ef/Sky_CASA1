using System;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogType { get; set; }
        public string Module { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string SessionId { get; set; }
        public string Result { get; set; }
        public int Duration { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string Severity { get; set; }
        public string Category { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }

    public class AuditLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string EventType { get; set; }
        public string Module { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Result { get; set; }
        public int Duration { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class AuditLogSummary
    {
        public DateTime Date { get; set; }
        public int TotalLogs { get; set; }
        public int UserActions { get; set; }
        public int SystemEvents { get; set; }
        public int SecurityEvents { get; set; }
        public int DataAccess { get; set; }
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public int Info { get; set; }
        public Dictionary<string, int> ActionsByUser { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ActionsByModule { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ActionsByType { get; set; } = new Dictionary<string, int>();
        public List<string> TopActions { get; set; } = new List<string>();
        public List<string> TopUsers { get; set; } = new List<string>();
        public List<string> TopModules { get; set; } = new List<string>();
    }

    public class AuditLogFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string LogType { get; set; }
        public string Module { get; set; }
        public string IpAddress { get; set; }
        public string Result { get; set; }
        public string Severity { get; set; }
        public string Category { get; set; }
        public string Resource { get; set; }
        public string ResourceId { get; set; }
        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }
        public bool IncludeMetadata { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
    }

    public class AuditLogReport
    {
        public string Id { get; set; }
        public DateTime ReportDate { get; set; }
        public string ReportType { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AuditLogFilter Filter { get; set; }
        public List<AuditLog> Logs { get; set; } = new List<AuditLog>();
        public AuditLogSummary Summary { get; set; }
        public List<string> Summary { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();
        public List<string> Anomalies { get; set; } = new List<string>();
        public List<string> SecurityIssues { get; set; } = new List<string>();
        public List<string> ComplianceIssues { get; set; } = new List<string>();
    }

    public class AuditLogAnalytics
    {
        public DateTime Date { get; set; }
        public int TotalLogs { get; set; }
        public double AverageDuration { get; set; }
        public int PeakHour { get; set; }
        public int PeakHourLogs { get; set; }
        public Dictionary<string, int> HourlyDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> DailyDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> WeeklyDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> MonthlyDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> AverageDurationByAction { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> AverageDurationByUser { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> AverageDurationByModule { get; set; } = new Dictionary<string, double>();
        public List<string> TopActions { get; set; } = new List<string>();
        public List<string> TopUsers { get; set; } = new List<string>();
        public List<string> TopModules { get; set; } = new List<string>();
        public List<string> TopIpAddresses { get; set; } = new List<string>();
        public List<string> TopUserAgents { get; set; } = new List<string>();
    }

    public class AuditLogAlert
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public string Severity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public List<string> Triggers { get; set; } = new List<string>();
        public List<string> Actions { get; set; } = new List<string>();
        public List<string> Recipients { get; set; } = new List<string>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public int Threshold { get; set; }
        public string TimeWindow { get; set; }
        public string Status { get; set; }
        public DateTime? LastTriggered { get; set; }
        public int TriggerCount { get; set; }
    }

    public class AuditLogRetention
    {
        public int Id { get; set; }
        public string LogType { get; set; }
        public string Module { get; set; }
        public string Category { get; set; }
        public int RetentionDays { get; set; }
        public string RetentionPolicy { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string Notes { get; set; }
        public List<string> Exceptions { get; set; } = new List<string>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    public class AuditLogBackup
    {
        public int Id { get; set; }
        public string BackupName { get; set; }
        public string BackupType { get; set; }
        public DateTime BackupDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string CompressionType { get; set; }
        public string EncryptionType { get; set; }
        public string Checksum { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RestoredAt { get; set; }
        public string RestoredBy { get; set; }
        public string Notes { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class AuditLogCompliance
    {
        public int Id { get; set; }
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
    }
}