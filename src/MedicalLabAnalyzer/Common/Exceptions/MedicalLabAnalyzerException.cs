using System;

namespace MedicalLabAnalyzer.Common.Exceptions
{
    /// <summary>
    /// Base exception class for all Medical Lab Analyzer domain-specific exceptions
    /// </summary>
    public class MedicalLabAnalyzerException : Exception
    {
        public string ErrorCode { get; }
        public DateTime Timestamp { get; }

        public MedicalLabAnalyzerException(string message) : base(message)
        {
            Timestamp = DateTime.UtcNow;
            ErrorCode = GenerateErrorCode();
        }

        public MedicalLabAnalyzerException(string message, Exception innerException) : base(message, innerException)
        {
            Timestamp = DateTime.UtcNow;
            ErrorCode = GenerateErrorCode();
        }

        public MedicalLabAnalyzerException(string message, string errorCode) : base(message)
        {
            Timestamp = DateTime.UtcNow;
            ErrorCode = errorCode;
        }

        public MedicalLabAnalyzerException(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            Timestamp = DateTime.UtcNow;
            ErrorCode = errorCode;
        }

        private string GenerateErrorCode()
        {
            return $"MLA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }

    /// <summary>
    /// Exception thrown for database-related operations
    /// </summary>
    public class DatabaseException : MedicalLabAnalyzerException
    {
        public string Operation { get; }

        public DatabaseException(string message, string operation) : base(message, $"DB-{operation?.ToUpper()}")
        {
            Operation = operation;
        }

        public DatabaseException(string message, string operation, Exception innerException) 
            : base(message, $"DB-{operation?.ToUpper()}", innerException)
        {
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown during medical analysis operations
    /// </summary>
    public class AnalysisException : MedicalLabAnalyzerException
    {
        public string AnalysisType { get; }
        public string ExamId { get; }

        public AnalysisException(string message, string analysisType, string examId = null) 
            : base(message, $"ANALYSIS-{analysisType?.ToUpper()}")
        {
            AnalysisType = analysisType;
            ExamId = examId;
        }

        public AnalysisException(string message, string analysisType, string examId, Exception innerException) 
            : base(message, $"ANALYSIS-{analysisType?.ToUpper()}", innerException)
        {
            AnalysisType = analysisType;
            ExamId = examId;
        }
    }

    /// <summary>
    /// Exception thrown during calibration operations
    /// </summary>
    public class CalibrationException : MedicalLabAnalyzerException
    {
        public double? MicronsPerPixel { get; }
        public double? FPS { get; }

        public CalibrationException(string message) : base(message, "CALIBRATION")
        {
        }

        public CalibrationException(string message, double micronsPerPixel, double fps) 
            : base(message, "CALIBRATION")
        {
            MicronsPerPixel = micronsPerPixel;
            FPS = fps;
        }

        public CalibrationException(string message, Exception innerException) 
            : base(message, "CALIBRATION", innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown for validation failures
    /// </summary>
    public class ValidationException : MedicalLabAnalyzerException
    {
        public string[] ValidationErrors { get; }
        public string EntityType { get; }

        public ValidationException(string message, string[] validationErrors, string entityType = null) 
            : base(message, $"VALIDATION-{entityType?.ToUpper()}")
        {
            ValidationErrors = validationErrors;
            EntityType = entityType;
        }

        public ValidationException(string message, string[] validationErrors, string entityType, Exception innerException) 
            : base(message, $"VALIDATION-{entityType?.ToUpper()}", innerException)
        {
            ValidationErrors = validationErrors;
            EntityType = entityType;
        }
    }

    /// <summary>
    /// Exception thrown during authentication operations
    /// </summary>
    public class AuthenticationException : MedicalLabAnalyzerException
    {
        public string Username { get; }
        public string Operation { get; }

        public AuthenticationException(string message, string username = null, string operation = null) 
            : base(message, $"AUTH-{operation?.ToUpper()}")
        {
            Username = username;
            Operation = operation;
        }

        public AuthenticationException(string message, string username, string operation, Exception innerException) 
            : base(message, $"AUTH-{operation?.ToUpper()}", innerException)
        {
            Username = username;
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown for file operations (video, images, reports)
    /// </summary>
    public class FileOperationException : MedicalLabAnalyzerException
    {
        public string FilePath { get; }
        public string Operation { get; }

        public FileOperationException(string message, string filePath, string operation) 
            : base(message, $"FILE-{operation?.ToUpper()}")
        {
            FilePath = filePath;
            Operation = operation;
        }

        public FileOperationException(string message, string filePath, string operation, Exception innerException) 
            : base(message, $"FILE-{operation?.ToUpper()}", innerException)
        {
            FilePath = filePath;
            Operation = operation;
        }
    }

    /// <summary>
    /// Exception thrown during video processing operations
    /// </summary>
    public class VideoProcessingException : MedicalLabAnalyzerException
    {
        public string VideoPath { get; }
        public int? FrameNumber { get; }

        public VideoProcessingException(string message, string videoPath, int? frameNumber = null) 
            : base(message, "VIDEO-PROCESSING")
        {
            VideoPath = videoPath;
            FrameNumber = frameNumber;
        }

        public VideoProcessingException(string message, string videoPath, int? frameNumber, Exception innerException) 
            : base(message, "VIDEO-PROCESSING", innerException)
        {
            VideoPath = videoPath;
            FrameNumber = frameNumber;
        }
    }

    /// <summary>
    /// Exception thrown during report generation
    /// </summary>
    public class ReportGenerationException : MedicalLabAnalyzerException
    {
        public string ReportType { get; }
        public string ExamId { get; }

        public ReportGenerationException(string message, string reportType, string examId) 
            : base(message, $"REPORT-{reportType?.ToUpper()}")
        {
            ReportType = reportType;
            ExamId = examId;
        }

        public ReportGenerationException(string message, string reportType, string examId, Exception innerException) 
            : base(message, $"REPORT-{reportType?.ToUpper()}", innerException)
        {
            ReportType = reportType;
            ExamId = examId;
        }
    }
}