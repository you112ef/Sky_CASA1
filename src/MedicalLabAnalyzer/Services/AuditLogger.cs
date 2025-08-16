using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MedicalLabAnalyzer.Services
{
    public class AuditLogger
    {
        private readonly ILogger<AuditLogger> _logger;

        public AuditLogger(ILogger<AuditLogger> logger = null)
        {
            _logger = logger;
        }

        public async Task<bool> LogUserActionAsync(string userId, string userName, string action, string details)
        {
            try
            {
                _logger?.LogInformation("Logging user action: {UserId} - {Action}", userId, action);
                
                var sql = @"
                    INSERT INTO AuditLogs (UserId, UserName, Action, Details, Timestamp, LogType)
                    VALUES (@UserId, @UserName, @Action, @Details, @Timestamp, 'UserAction')";
                
                var auditLog = new
                {
                    UserId = userId,
                    UserName = userName,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.Now
                };
                
                var rowsAffected = await _db.ExecuteAsync(sql, auditLog);
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("User action logged successfully: {UserId} - {Action}", userId, action);
                }
                else
                {
                    _logger?.LogWarning("Failed to log user action: {UserId} - {Action}", userId, action);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error logging user action: {UserId} - {Action}", userId, action);
                return false;
            }
        }

        public async Task<bool> LogSystemEventAsync(string userId, string userName, string eventType, string details)
        {
            try
            {
                _logger?.LogInformation("Logging system event: {EventType} by {UserId}", eventType, userId);
                
                var sql = @"
                    INSERT INTO AuditLogs (UserId, UserName, Action, Details, Timestamp, LogType)
                    VALUES (@UserId, @UserName, @Action, @Details, @Timestamp, 'SystemEvent')";
                
                var auditLog = new
                {
                    UserId = userId,
                    UserName = userName,
                    Action = eventType,
                    Details = details,
                    Timestamp = DateTime.Now
                };
                
                var rowsAffected = await _db.ExecuteAsync(sql, auditLog);
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("System event logged successfully: {EventType} by {UserId}", eventType, userId);
                }
                else
                {
                    _logger?.LogWarning("Failed to log system event: {EventType} by {UserId}", eventType, userId);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error logging system event: {EventType} by {UserId}", eventType, userId);
                return false;
            }
        }

        public async Task<bool> LogDataAccessAsync(string userId, string userName, string dataType, string operation, string details)
        {
            try
            {
                _logger?.LogInformation("Logging data access: {UserId} - {DataType} - {Operation}", userId, dataType, operation);
                
                var sql = @"
                    INSERT INTO AuditLogs (UserId, UserName, Action, Details, Timestamp, LogType)
                    VALUES (@UserId, @UserName, @Action, @Details, @Timestamp, 'DataAccess')";
                
                var auditLog = new
                {
                    UserId = userId,
                    UserName = userName,
                    Action = $"{dataType}_{operation}",
                    Details = details,
                    Timestamp = DateTime.Now
                };
                
                var rowsAffected = await _db.ExecuteAsync(sql, auditLog);
                var success = rowsAffected > 0;
                
                if (success)
                {
                    _logger?.LogInformation("Data access logged successfully: {UserId} - {DataType} - {Operation}", userId, dataType, operation);
                }
                else
                {
                    _logger?.LogWarning("Failed to log data access: {UserId} - {DataType} - {Operation}", userId, dataType, operation);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error logging data access: {UserId} - {DataType} - {Operation}", userId, dataType, operation);
                return false;
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null, string userId = null, string logType = null)
        {
            try
            {
                _logger?.LogInformation("Retrieving audit logs with filters: StartDate={StartDate}, EndDate={EndDate}, UserId={UserId}, LogType={LogType}", 
                    startDate, endDate, userId, logType);
                
                var sql = "SELECT * FROM AuditLogs WHERE 1=1";
                var parameters = new DynamicParameters();
                
                if (startDate.HasValue)
                {
                    sql += " AND Timestamp >= @StartDate";
                    parameters.Add("@StartDate", startDate.Value);
                }
                
                if (endDate.HasValue)
                {
                    sql += " AND Timestamp <= @EndDate";
                    parameters.Add("@EndDate", endDate.Value);
                }
                
                if (!string.IsNullOrEmpty(userId))
                {
                    sql += " AND UserId = @UserId";
                    parameters.Add("@UserId", userId);
                }
                
                if (!string.IsNullOrEmpty(logType))
                {
                    sql += " AND LogType = @LogType";
                    parameters.Add("@LogType", logType);
                }
                
                sql += " ORDER BY Timestamp DESC";
                
                var auditLogs = await _db.QueryAsync<AuditLog>(sql, parameters);
                
                _logger?.LogInformation("Retrieved {Count} audit logs", auditLogs.Count());
                return auditLogs.ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving audit logs");
                throw;
            }
        }
    }

    public class AuditLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string EventType { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}