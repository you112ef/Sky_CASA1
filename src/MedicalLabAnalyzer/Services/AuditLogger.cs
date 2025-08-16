using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;

namespace MedicalLabAnalyzer.Services
{
    /// <summary>
    /// Comprehensive audit logging service for medical laboratory compliance
    /// </summary>
    public class AuditLogger
    {
        private readonly IDbConnection _db;
        private readonly ILogger<AuditLogger> _logger;
        private static readonly object _lock = new object();

        public AuditLogger(IDbConnection db, ILogger<AuditLogger> logger = null)
        {
            _db = db;
            _logger = logger;
            InitializeDatabase();
        }

        /// <summary>
        /// Initialize audit log table
        /// </summary>
        private void InitializeDatabase()
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS AuditLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UserId TEXT,
                    UserName TEXT,
                    Action TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Details TEXT,
                    VideoPath TEXT,
                    CalibrationId INTEGER,
                    AnalysisId TEXT,
                    SoftwareVersion TEXT,
                    ClientIP TEXT,
                    SessionId TEXT,
                    Severity TEXT DEFAULT 'INFO',
                    Metadata TEXT
                )";

            _db.Execute(sql);
            _logger?.LogInformation("Audit database initialized");
        }

        /// <summary>
        /// Log a CASA analysis event
        /// </summary>
        /// <param name="userId">User performing analysis</param>
        /// <param name="userName">User display name</param>
        /// <param name="videoPath">Video file path</param>
        /// <param name="calibrationId">Calibration used</param>
        /// <param name="analysisId">Unique analysis identifier</param>
        /// <param name="parameters">Analysis parameters</param>
        /// <param name="result">Analysis results</param>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="clientIP">Client IP address</param>
        public void LogCASAnalysis(
            string userId, 
            string userName, 
            string videoPath, 
            int calibrationId, 
            string analysisId,
            object parameters,
            object result,
            string sessionId = null,
            string clientIP = null)
        {
            var details = new
            {
                VideoPath = videoPath,
                CalibrationId = calibrationId,
                AnalysisId = analysisId,
                Parameters = parameters,
                Result = result,
                Timestamp = DateTime.UtcNow
            };

            LogEvent(
                userId: userId,
                userName: userName,
                action: "CASA_ANALYSIS",
                category: "ANALYSIS",
                details: JsonSerializer.Serialize(details),
                videoPath: videoPath,
                calibrationId: calibrationId,
                analysisId: analysisId,
                sessionId: sessionId,
                clientIP: clientIP,
                severity: "INFO"
            );
        }

        /// <summary>
        /// Log calibration event
        /// </summary>
        /// <param name="userId">User performing calibration</param>
        /// <param name="userName">User display name</param>
        /// <param name="action">Calibration action (CREATE, UPDATE, DELETE, ACTIVATE)</param>
        /// <param name="calibrationData">Calibration data</param>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="clientIP">Client IP address</param>
        public void LogCalibration(
            string userId,
            string userName,
            string action,
            object calibrationData,
            string sessionId = null,
            string clientIP = null)
        {
            var details = new
            {
                Action = action,
                CalibrationData = calibrationData,
                Timestamp = DateTime.UtcNow
            };

            LogEvent(
                userId: userId,
                userName: userName,
                action: $"CALIBRATION_{action}",
                category: "CALIBRATION",
                details: JsonSerializer.Serialize(details),
                sessionId: sessionId,
                clientIP: clientIP,
                severity: "INFO"
            );
        }

        /// <summary>
        /// Log system event
        /// </summary>
        /// <param name="userId">User performing action</param>
        /// <param name="userName">User display name</param>
        /// <param name="action">Action performed</param>
        /// <param name="category">Event category</param>
        /// <param name="details">Event details</param>
        /// <param name="severity">Event severity</param>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="clientIP">Client IP address</param>
        public void LogSystemEvent(
            string userId,
            string userName,
            string action,
            string category,
            object details,
            string severity = "INFO",
            string sessionId = null,
            string clientIP = null)
        {
            LogEvent(
                userId: userId,
                userName: userName,
                action: action,
                category: category,
                details: JsonSerializer.Serialize(details),
                sessionId: sessionId,
                clientIP: clientIP,
                severity: severity
            );
        }

        /// <summary>
        /// Log security event
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userName">User name</param>
        /// <param name="action">Security action</param>
        /// <param name="details">Event details</param>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="clientIP">Client IP address</param>
        public void LogSecurityEvent(
            string userId,
            string userName,
            string action,
            object details,
            string sessionId = null,
            string clientIP = null)
        {
            LogEvent(
                userId: userId,
                userName: userName,
                action: action,
                category: "SECURITY",
                details: JsonSerializer.Serialize(details),
                sessionId: sessionId,
                clientIP: clientIP,
                severity: "WARNING"
            );
        }

        /// <summary>
        /// Log error event
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="userName">User name</param>
        /// <param name="action">Action that caused error</param>
        /// <param name="error">Error details</param>
        /// <param name="sessionId">Session identifier</param>
        /// <param name="clientIP">Client IP address</param>
        public void LogError(
            string userId,
            string userName,
            string action,
            Exception error,
            string sessionId = null,
            string clientIP = null)
        {
            var details = new
            {
                ErrorMessage = error.Message,
                ErrorType = error.GetType().Name,
                StackTrace = error.StackTrace,
                InnerException = error.InnerException?.Message,
                Timestamp = DateTime.UtcNow
            };

            LogEvent(
                userId: userId,
                userName: userName,
                action: action,
                category: "ERROR",
                details: JsonSerializer.Serialize(details),
                sessionId: sessionId,
                clientIP: clientIP,
                severity: "ERROR"
            );
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private void LogEvent(
            string userId,
            string userName,
            string action,
            string category,
            string details,
            string videoPath = null,
            int? calibrationId = null,
            string analysisId = null,
            string sessionId = null,
            string clientIP = null,
            string severity = "INFO")
        {
            lock (_lock)
            {
                try
                {
                    var sql = @"
                        INSERT INTO AuditLogs (
                            UserId, UserName, Action, Category, Details, VideoPath, 
                            CalibrationId, AnalysisId, SoftwareVersion, ClientIP, 
                            SessionId, Severity, Metadata
                        ) VALUES (
                            @UserId, @UserName, @Action, @Category, @Details, @VideoPath,
                            @CalibrationId, @AnalysisId, @SoftwareVersion, @ClientIP,
                            @SessionId, @Severity, @Metadata
                        )";

                    var metadata = new
                    {
                        SystemInfo = GetSystemInfo(),
                        Timestamp = DateTime.UtcNow
                    };

                    var parameters = new
                    {
                        UserId = userId,
                        UserName = userName,
                        Action = action,
                        Category = category,
                        Details = details,
                        VideoPath = videoPath,
                        CalibrationId = calibrationId,
                        AnalysisId = analysisId,
                        SoftwareVersion = GetSoftwareVersion(),
                        ClientIP = clientIP,
                        SessionId = sessionId,
                        Severity = severity,
                        Metadata = JsonSerializer.Serialize(metadata)
                    };

                    _db.Execute(sql, parameters);

                    _logger?.LogInformation($"Audit log: {action} by {userName} ({userId})");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to write audit log");
                    // Don't throw - audit logging should not break main functionality
                }
            }
        }

        /// <summary>
        /// Get audit logs with filtering
        /// </summary>
        /// <param name="filters">Filter criteria</param>
        /// <param name="limit">Maximum number of records</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of audit log entries</returns>
        public List<AuditLogEntry> GetAuditLogs(AuditLogFilters filters = null, int limit = 1000, int offset = 0)
        {
            filters ??= new AuditLogFilters();

            var sql = @"
                SELECT * FROM AuditLogs 
                WHERE (@UserId IS NULL OR UserId = @UserId)
                  AND (@Category IS NULL OR Category = @Category)
                  AND (@Action IS NULL OR Action = @Action)
                  AND (@Severity IS NULL OR Severity = @Severity)
                  AND (@StartDate IS NULL OR Timestamp >= @StartDate)
                  AND (@EndDate IS NULL OR Timestamp <= @EndDate)
                ORDER BY Timestamp DESC
                LIMIT @Limit OFFSET @Offset";

            var parameters = new
            {
                filters.UserId,
                filters.Category,
                filters.Action,
                filters.Severity,
                filters.StartDate,
                filters.EndDate,
                Limit = limit,
                Offset = offset
            };

            return _db.Query<AuditLogEntry>(sql, parameters).ToList();
        }

        /// <summary>
        /// Get audit statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Audit statistics</returns>
        public AuditStatistics GetStatistics(DateTime? startDate = null, DateTime? endDate = null)
        {
            var sql = @"
                SELECT 
                    COUNT(*) as TotalEvents,
                    COUNT(CASE WHEN Category = 'ANALYSIS' THEN 1 END) as AnalysisEvents,
                    COUNT(CASE WHEN Category = 'CALIBRATION' THEN 1 END) as CalibrationEvents,
                    COUNT(CASE WHEN Category = 'SECURITY' THEN 1 END) as SecurityEvents,
                    COUNT(CASE WHEN Category = 'ERROR' THEN 1 END) as ErrorEvents,
                    COUNT(DISTINCT UserId) as UniqueUsers,
                    MIN(Timestamp) as FirstEvent,
                    MAX(Timestamp) as LastEvent
                FROM AuditLogs
                WHERE (@StartDate IS NULL OR Timestamp >= @StartDate)
                  AND (@EndDate IS NULL OR Timestamp <= @EndDate)";

            var parameters = new { StartDate = startDate, EndDate = endDate };
            return _db.QueryFirstOrDefault<AuditStatistics>(sql, parameters) ?? new AuditStatistics();
        }

        /// <summary>
        /// Export audit logs to CSV
        /// </summary>
        /// <param name="filters">Filter criteria</param>
        /// <param name="filePath">Output file path</param>
        public void ExportToCSV(AuditLogFilters filters, string filePath)
        {
            var logs = GetAuditLogs(filters, int.MaxValue, 0);
            
            using var writer = new System.IO.StreamWriter(filePath);
            using var csv = new CsvHelper.CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
            
            csv.WriteRecords(logs);
        }

        /// <summary>
        /// Get system information
        /// </summary>
        private object GetSystemInfo()
        {
            return new
            {
                OS = Environment.OSVersion.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                Is64BitProcess = Environment.Is64BitProcess,
                UserDomainName = Environment.UserDomainName,
                UserName = Environment.UserName
            };
        }

        /// <summary>
        /// Get software version
        /// </summary>
        private string GetSoftwareVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
        }
    }

    /// <summary>
    /// Audit log entry
    /// </summary>
    public class AuditLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";
        public string Category { get; set; } = "";
        public string Details { get; set; } = "";
        public string VideoPath { get; set; } = "";
        public int? CalibrationId { get; set; }
        public string AnalysisId { get; set; } = "";
        public string SoftwareVersion { get; set; } = "";
        public string ClientIP { get; set; } = "";
        public string SessionId { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Metadata { get; set; } = "";
    }

    /// <summary>
    /// Audit log filters
    /// </summary>
    public class AuditLogFilters
    {
        public string UserId { get; set; }
        public string Category { get; set; }
        public string Action { get; set; }
        public string Severity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Audit statistics
    /// </summary>
    public class AuditStatistics
    {
        public int TotalEvents { get; set; }
        public int AnalysisEvents { get; set; }
        public int CalibrationEvents { get; set; }
        public int SecurityEvents { get; set; }
        public int ErrorEvents { get; set; }
        public int UniqueUsers { get; set; }
        public DateTime? FirstEvent { get; set; }
        public DateTime? LastEvent { get; set; }
    }
}