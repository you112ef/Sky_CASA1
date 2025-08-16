using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Common.Exceptions;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.Common.ErrorHandling
{
    /// <summary>
    /// Centralized error handling service for consistent error management
    /// </summary>
    public interface IErrorHandlerService
    {
        Task<Result<T>> HandleAsync<T>(Func<Task<T>> operation, string operationName, object context = null);
        Task<Result> HandleAsync(Func<Task> operation, string operationName, object context = null);
        Result<T> Handle<T>(Func<T> operation, string operationName, object context = null);
        Result Handle(Action operation, string operationName, object context = null);
        void LogError(Exception exception, string operationName, object context = null);
        void LogWarning(string message, object context = null);
        void LogInfo(string message, object context = null);
        Task NotifyUserAsync(string message, ErrorSeverity severity = ErrorSeverity.Error);
    }

    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    public class ErrorHandlerService : IErrorHandlerService
    {
        private readonly ILogger<ErrorHandlerService> _logger;
        private readonly IAuditLogger _auditLogger;

        public ErrorHandlerService(ILogger<ErrorHandlerService> logger, IAuditLogger auditLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditLogger = auditLogger;
        }

        public async Task<Result<T>> HandleAsync<T>(Func<Task<T>> operation, string operationName, object context = null)
        {
            try
            {
                LogInfo($"Starting operation: {operationName}", context);
                var result = await operation();
                LogInfo($"Successfully completed operation: {operationName}", context);
                return Result.Success(result);
            }
            catch (MedicalLabAnalyzerException ex)
            {
                return await HandleMedicalLabAnalyzerExceptionAsync<T>(ex, operationName, context);
            }
            catch (ArgumentException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"Argument validation failed in {operationName}", ex.Message);
                return Result.Failure<T>($"خطأ في المعطيات: {ex.Message}", "VALIDATION_ERROR", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("SECURITY", $"Unauthorized access in {operationName}", ex.Message);
                return Result.Failure<T>("غير مصرح بالوصول إلى هذه العملية", "UNAUTHORIZED_ACCESS", ex);
            }
            catch (System.Data.Common.DbException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"Database error in {operationName}", ex.Message);
                return Result.Failure<T>("خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى", "DATABASE_ERROR", ex);
            }
            catch (System.IO.IOException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"File I/O error in {operationName}", ex.Message);
                return Result.Failure<T>($"خطأ في الملف: {ex.Message}", "FILE_IO_ERROR", ex);
            }
            catch (OutOfMemoryException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("CRITICAL", $"Out of memory in {operationName}", ex.Message);
                return Result.Failure<T>("الذاكرة غير كافية لإتمام العملية", "OUT_OF_MEMORY", ex);
            }
            catch (Exception ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"Unexpected error in {operationName}", ex.Message);
                return Result.Failure<T>($"خطأ غير متوقع: {ex.Message}", "UNEXPECTED_ERROR", ex);
            }
        }

        public async Task<Result> HandleAsync(Func<Task> operation, string operationName, object context = null)
        {
            try
            {
                LogInfo($"Starting operation: {operationName}", context);
                await operation();
                LogInfo($"Successfully completed operation: {operationName}", context);
                return Result.Success();
            }
            catch (MedicalLabAnalyzerException ex)
            {
                return await HandleMedicalLabAnalyzerExceptionAsync(ex, operationName, context);
            }
            catch (ArgumentException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"Argument validation failed in {operationName}", ex.Message);
                return Result.Failure($"خطأ في المعطيات: {ex.Message}", "VALIDATION_ERROR", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("SECURITY", $"Unauthorized access in {operationName}", ex.Message);
                return Result.Failure("غير مصرح بالوصول إلى هذه العملية", "UNAUTHORIZED_ACCESS", ex);
            }
            catch (System.Data.Common.DbException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"Database error in {operationName}", ex.Message);
                return Result.Failure("خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى", "DATABASE_ERROR", ex);
            }
            catch (System.IO.IOException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"File I/O error in {operationName}", ex.Message);
                return Result.Failure($"خطأ في الملف: {ex.Message}", "FILE_IO_ERROR", ex);
            }
            catch (OutOfMemoryException ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("CRITICAL", $"Out of memory in {operationName}", ex.Message);
                return Result.Failure("الذاكرة غير كافية لإتمام العملية", "OUT_OF_MEMORY", ex);
            }
            catch (Exception ex)
            {
                LogError(ex, operationName, context);
                await _auditLogger?.LogAsync("ERROR", $"Unexpected error in {operationName}", ex.Message);
                return Result.Failure($"خطأ غير متوقع: {ex.Message}", "UNEXPECTED_ERROR", ex);
            }
        }

        public Result<T> Handle<T>(Func<T> operation, string operationName, object context = null)
        {
            try
            {
                LogInfo($"Starting operation: {operationName}", context);
                var result = operation();
                LogInfo($"Successfully completed operation: {operationName}", context);
                return Result.Success(result);
            }
            catch (MedicalLabAnalyzerException ex)
            {
                return HandleMedicalLabAnalyzerException<T>(ex, operationName, context);
            }
            catch (ArgumentException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"Argument validation failed in {operationName}", ex.Message);
                return Result.Failure<T>($"خطأ في المعطيات: {ex.Message}", "VALIDATION_ERROR", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("SECURITY", $"Unauthorized access in {operationName}", ex.Message);
                return Result.Failure<T>("غير مصرح بالوصول إلى هذه العملية", "UNAUTHORIZED_ACCESS", ex);
            }
            catch (System.Data.Common.DbException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"Database error in {operationName}", ex.Message);
                return Result.Failure<T>("خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى", "DATABASE_ERROR", ex);
            }
            catch (System.IO.IOException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"File I/O error in {operationName}", ex.Message);
                return Result.Failure<T>($"خطأ في الملف: {ex.Message}", "FILE_IO_ERROR", ex);
            }
            catch (OutOfMemoryException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("CRITICAL", $"Out of memory in {operationName}", ex.Message);
                return Result.Failure<T>("الذاكرة غير كافية لإتمام العملية", "OUT_OF_MEMORY", ex);
            }
            catch (Exception ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"Unexpected error in {operationName}", ex.Message);
                return Result.Failure<T>($"خطأ غير متوقع: {ex.Message}", "UNEXPECTED_ERROR", ex);
            }
        }

        public Result Handle(Action operation, string operationName, object context = null)
        {
            try
            {
                LogInfo($"Starting operation: {operationName}", context);
                operation();
                LogInfo($"Successfully completed operation: {operationName}", context);
                return Result.Success();
            }
            catch (MedicalLabAnalyzerException ex)
            {
                return HandleMedicalLabAnalyzerException(ex, operationName, context);
            }
            catch (ArgumentException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"Argument validation failed in {operationName}", ex.Message);
                return Result.Failure($"خطأ في المعطيات: {ex.Message}", "VALIDATION_ERROR", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("SECURITY", $"Unauthorized access in {operationName}", ex.Message);
                return Result.Failure("غير مصرح بالوصول إلى هذه العملية", "UNAUTHORIZED_ACCESS", ex);
            }
            catch (System.Data.Common.DbException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"Database error in {operationName}", ex.Message);
                return Result.Failure("خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى", "DATABASE_ERROR", ex);
            }
            catch (System.IO.IOException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"File I/O error in {operationName}", ex.Message);
                return Result.Failure($"خطأ في الملف: {ex.Message}", "FILE_IO_ERROR", ex);
            }
            catch (OutOfMemoryException ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("CRITICAL", $"Out of memory in {operationName}", ex.Message);
                return Result.Failure("الذاكرة غير كافية لإتمام العملية", "OUT_OF_MEMORY", ex);
            }
            catch (Exception ex)
            {
                LogError(ex, operationName, context);
                _auditLogger?.LogAsync("ERROR", $"Unexpected error in {operationName}", ex.Message);
                return Result.Failure($"خطأ غير متوقع: {ex.Message}", "UNEXPECTED_ERROR", ex);
            }
        }

        private async Task<Result<T>> HandleMedicalLabAnalyzerExceptionAsync<T>(MedicalLabAnalyzerException ex, string operationName, object context)
        {
            LogError(ex, operationName, context);
            await _auditLogger?.LogAsync("ERROR", $"Domain error in {operationName}: {ex.ErrorCode}", ex.Message);
            return Result.Failure<T>(GetLocalizedErrorMessage(ex), ex.ErrorCode, ex);
        }

        private async Task<Result> HandleMedicalLabAnalyzerExceptionAsync(MedicalLabAnalyzerException ex, string operationName, object context)
        {
            LogError(ex, operationName, context);
            await _auditLogger?.LogAsync("ERROR", $"Domain error in {operationName}: {ex.ErrorCode}", ex.Message);
            return Result.Failure(GetLocalizedErrorMessage(ex), ex.ErrorCode, ex);
        }

        private Result<T> HandleMedicalLabAnalyzerException<T>(MedicalLabAnalyzerException ex, string operationName, object context)
        {
            LogError(ex, operationName, context);
            _auditLogger?.LogAsync("ERROR", $"Domain error in {operationName}: {ex.ErrorCode}", ex.Message);
            return Result.Failure<T>(GetLocalizedErrorMessage(ex), ex.ErrorCode, ex);
        }

        private Result HandleMedicalLabAnalyzerException(MedicalLabAnalyzerException ex, string operationName, object context)
        {
            LogError(ex, operationName, context);
            _auditLogger?.LogAsync("ERROR", $"Domain error in {operationName}: {ex.ErrorCode}", ex.Message);
            return Result.Failure(GetLocalizedErrorMessage(ex), ex.ErrorCode, ex);
        }

        private string GetLocalizedErrorMessage(MedicalLabAnalyzerException ex)
        {
            return ex switch
            {
                DatabaseException dbEx => $"خطأ في قاعدة البيانات أثناء {dbEx.Operation}: {dbEx.Message}",
                AnalysisException anaEx => $"خطأ في تحليل {anaEx.AnalysisType}: {anaEx.Message}",
                CalibrationException calEx => $"خطأ في المعايرة: {calEx.Message}",
                ValidationException valEx => $"خطأ في التحقق من {valEx.EntityType}: {string.Join(", ", valEx.ValidationErrors)}",
                AuthenticationException authEx => $"خطأ في المصادقة: {authEx.Message}",
                FileOperationException fileEx => $"خطأ في العملية على الملف {fileEx.Operation}: {fileEx.Message}",
                VideoProcessingException videoEx => $"خطأ في معالجة الفيديو: {videoEx.Message}",
                ReportGenerationException reportEx => $"خطأ في إنشاء تقرير {reportEx.ReportType}: {reportEx.Message}",
                _ => $"خطأ في النظام: {ex.Message}"
            };
        }

        public void LogError(Exception exception, string operationName, object context = null)
        {
            _logger?.LogError(exception, "Error in operation {OperationName} with context {@Context}", operationName, context);
        }

        public void LogWarning(string message, object context = null)
        {
            _logger?.LogWarning("Warning: {Message} with context {@Context}", message, context);
        }

        public void LogInfo(string message, object context = null)
        {
            _logger?.LogInformation("Info: {Message} with context {@Context}", message, context);
        }

        public async Task NotifyUserAsync(string message, ErrorSeverity severity = ErrorSeverity.Error)
        {
            // This will be implemented based on UI framework (WPF MessageBox, Toast notifications, etc.)
            await Task.Run(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var icon = severity switch
                    {
                        ErrorSeverity.Info => System.Windows.MessageBoxImage.Information,
                        ErrorSeverity.Warning => System.Windows.MessageBoxImage.Warning,
                        ErrorSeverity.Error => System.Windows.MessageBoxImage.Error,
                        ErrorSeverity.Critical => System.Windows.MessageBoxImage.Error,
                        _ => System.Windows.MessageBoxImage.Information
                    };

                    var title = severity switch
                    {
                        ErrorSeverity.Info => "معلومات",
                        ErrorSeverity.Warning => "تحذير",
                        ErrorSeverity.Error => "خطأ",
                        ErrorSeverity.Critical => "خطأ حرج",
                        _ => "إشعار"
                    };

                    System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, icon);
                });
            });
        }
    }

    /// <summary>
    /// Helper extension methods for error handling
    /// </summary>
    public static class ErrorHandlerExtensions
    {
        public static async Task<Result<T>> HandleWithRetryAsync<T>(this IErrorHandlerService errorHandler,
            Func<Task<T>> operation, string operationName, int maxRetries = 3, 
            TimeSpan? delay = null, object context = null)
        {
            var retryDelay = delay ?? TimeSpan.FromMilliseconds(500);
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var result = await errorHandler.HandleAsync(operation, $"{operationName} (Attempt {attempt})", context);
                
                if (result.IsSuccess || attempt == maxRetries)
                    return result;

                await Task.Delay(retryDelay);
                retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
            }

            return Result.Failure<T>($"فشل في العملية {operationName} بعد {maxRetries} محاولات", "MAX_RETRIES_EXCEEDED");
        }

        public static async Task<Result> HandleWithRetryAsync(this IErrorHandlerService errorHandler,
            Func<Task> operation, string operationName, int maxRetries = 3, 
            TimeSpan? delay = null, object context = null)
        {
            var retryDelay = delay ?? TimeSpan.FromMilliseconds(500);
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var result = await errorHandler.HandleAsync(operation, $"{operationName} (Attempt {attempt})", context);
                
                if (result.IsSuccess || attempt == maxRetries)
                    return result;

                await Task.Delay(retryDelay);
                retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 2); // Exponential backoff
            }

            return Result.Failure($"فشل في العملية {operationName} بعد {maxRetries} محاولات", "MAX_RETRIES_EXCEEDED");
        }
    }
}