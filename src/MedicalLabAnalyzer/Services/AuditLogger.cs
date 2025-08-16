using System;
using System.Data.SQLite;
using System.IO;

namespace MedicalLabAnalyzer.Services
{
    public static class AuditLogger
    {
        private static string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
        private static string _connStr => $"Data Source={_dbPath};Version=3;";

        /// <summary>
        /// تسجيل إجراء في قاعدة البيانات
        /// </summary>
        /// <param name="action">نوع الإجراء</param>
        /// <param name="description">وصف الإجراء</param>
        public static void Log(string action, string description)
        {
            try
            {
                using var conn = new SQLiteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO AuditLogs (Action, Description, CreatedAt) VALUES (@Action,@Description,@CreatedAt)";
                cmd.Parameters.AddWithValue("@Action", action);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Log to file if database fails
                LogToFile(action, description, ex);
            }
        }

        /// <summary>
        /// تسجيل إجراء مع تفاصيل إضافية
        /// </summary>
        /// <param name="action">نوع الإجراء</param>
        /// <param name="description">وصف الإجراء</param>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="additionalData">بيانات إضافية</param>
        public static void Log(string action, string description, int? userId = null, string additionalData = null)
        {
            var fullDescription = description;
            if (userId.HasValue)
                fullDescription += $" | UserId: {userId}";
            if (!string.IsNullOrEmpty(additionalData))
                fullDescription += $" | Data: {additionalData}";

            Log(action, fullDescription);
        }

        /// <summary>
        /// تسجيل تحليل CASA
        /// </summary>
        /// <param name="examId">معرف الفحص</param>
        /// <param name="videoPath">مسار الفيديو</param>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="calibrationId">معرف المعايرة المستخدمة</param>
        public static void LogCasaAnalysis(int examId, string videoPath, int userId, int? calibrationId = null)
        {
            var description = $"CASA Analysis | ExamId: {examId} | Video: {Path.GetFileName(videoPath)}";
            if (calibrationId.HasValue)
                description += $" | CalibrationId: {calibrationId}";

            Log("CASA_Analysis", description, userId);
        }

        /// <summary>
        /// تسجيل تسجيل دخول المستخدم
        /// </summary>
        /// <param name="username">اسم المستخدم</param>
        /// <param name="success">نجح تسجيل الدخول</param>
        /// <param name="ipAddress">عنوان IP (اختياري)</param>
        public static void LogLogin(string username, bool success, string ipAddress = null)
        {
            var description = $"Login {(success ? "Success" : "Failed")} | Username: {username}";
            if (!string.IsNullOrEmpty(ipAddress))
                description += $" | IP: {ipAddress}";

            Log("User_Login", description);
        }

        /// <summary>
        /// تسجيل إنشاء تقرير
        /// </summary>
        /// <param name="reportType">نوع التقرير</param>
        /// <param name="examId">معرف الفحص</param>
        /// <param name="userId">معرف المستخدم</param>
        /// <param name="reportPath">مسار التقرير</param>
        public static void LogReportGeneration(string reportType, int examId, int userId, string reportPath)
        {
            var description = $"{reportType} Report | ExamId: {examId} | Path: {Path.GetFileName(reportPath)}";
            Log("Report_Generation", description, userId);
        }

        /// <summary>
        /// تسجيل في ملف محلي إذا فشلت قاعدة البيانات
        /// </summary>
        private static void LogToFile(string action, string description, Exception ex = null)
        {
            try
            {
                var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                var logFile = Path.Combine(logDir, $"audit_{DateTime.Now:yyyyMMdd}.log");
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {action} | {description}";
                if (ex != null)
                    logEntry += $" | Error: {ex.Message}";

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // If even file logging fails, we can't do much more
            }
        }

        /// <summary>
        /// الحصول على سجلات التدقيق لفترة زمنية محددة
        /// </summary>
        /// <param name="startDate">تاريخ البداية</param>
        /// <param name="endDate">تاريخ النهاية</param>
        /// <param name="action">نوع الإجراء (اختياري)</param>
        /// <returns>عدد السجلات</returns>
        public static int GetAuditLogCount(DateTime startDate, DateTime endDate, string action = null)
        {
            try
            {
                using var conn = new SQLiteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                
                var sql = "SELECT COUNT(*) FROM AuditLogs WHERE CreatedAt BETWEEN @StartDate AND @EndDate";
                if (!string.IsNullOrEmpty(action))
                {
                    sql += " AND Action = @Action";
                    cmd.Parameters.AddWithValue("@Action", action);
                }
                
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch
            {
                return 0;
            }
        }
    }
}