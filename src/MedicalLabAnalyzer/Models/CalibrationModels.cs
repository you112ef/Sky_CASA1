using System;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    public class CalibrationParameters
    {
        public double MicroscopeMagnification { get; set; }
        public double CameraPixelSize { get; set; }
        public double ReferenceObjectSize { get; set; }
        public string ReferenceObjectType { get; set; }
        public string CalibrationMethod { get; set; }
        public DateTime CalibrationDate { get; set; }
        public string PerformedBy { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public Dictionary<string, object> AdditionalParameters { get; set; } = new Dictionary<string, object>();
    }

    public class CalibrationResult
    {
        public string Id { get; set; }
        public DateTime CalibrationDate { get; set; }
        public CalibrationParameters Parameters { get; set; }
        public double CalibrationFactor { get; set; }
        public double PixelSize { get; set; }
        public double Accuracy { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public string PerformedBy { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsValid { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class CalibrationHistory
    {
        public string Id { get; set; }
        public string DeviceType { get; set; }
        public string DeviceId { get; set; }
        public DateTime CalibrationDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string PerformedBy { get; set; }
        public string Status { get; set; }
        public double Accuracy { get; set; }
        public string Notes { get; set; }
        public CalibrationParameters Parameters { get; set; }
        public CalibrationResult Result { get; set; }
    }

    public class CalibrationValidation
    {
        public string CalibrationId { get; set; }
        public DateTime ValidationDate { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMethod { get; set; }
        public double ValidationAccuracy { get; set; }
        public string ValidatedBy { get; set; }
        public string Notes { get; set; }
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class CalibrationDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public DateTime LastCalibration { get; set; }
        public DateTime NextCalibrationDue { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string ResponsiblePerson { get; set; }
        public List<CalibrationHistory> CalibrationHistory { get; set; } = new List<CalibrationHistory>();
    }

    public class CalibrationSchedule
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string AssignedTo { get; set; }
        public string Priority { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurrenceInterval { get; set; }
        public string RecurrenceUnit { get; set; }
    }

    public class CalibrationReport
    {
        public string Id { get; set; }
        public DateTime ReportDate { get; set; }
        public string ReportType { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CalibrationResult> Calibrations { get; set; } = new List<CalibrationResult>();
        public List<CalibrationValidation> Validations { get; set; } = new List<CalibrationValidation>();
        public List<CalibrationDevice> Devices { get; set; } = new List<CalibrationDevice>();
        public CalibrationStatistics Statistics { get; set; }
        public List<string> Summary { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    public class CalibrationStatistics
    {
        public int TotalCalibrations { get; set; }
        public int SuccessfulCalibrations { get; set; }
        public int FailedCalibrations { get; set; }
        public int ExpiredCalibrations { get; set; }
        public int DueCalibrations { get; set; }
        public double AverageAccuracy { get; set; }
        public double ComplianceRate { get; set; }
        public Dictionary<string, int> CalibrationsByDeviceType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CalibrationsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> AverageAccuracyByDeviceType { get; set; } = new Dictionary<string, double>();
    }

    public class CalibrationTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DeviceType { get; set; }
        public CalibrationParameters DefaultParameters { get; set; }
        public List<string> RequiredSteps { get; set; } = new List<string>();
        public List<string> ValidationSteps { get; set; } = new List<string>();
        public Dictionary<string, object> DefaultSettings { get; set; } = new Dictionary<string, object>();
        public int EstimatedDuration { get; set; }
        public string Difficulty { get; set; }
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> SafetyNotes { get; set; } = new List<string>();
    }

    public class CalibrationWorkflow
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DeviceType { get; set; }
        public List<CalibrationStep> Steps { get; set; } = new List<CalibrationStep>();
        public List<CalibrationValidation> Validations { get; set; } = new List<CalibrationValidation>();
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int Version { get; set; }
        public bool IsActive { get; set; }
    }

    public class CalibrationStep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public List<string> RequiredTools { get; set; } = new List<string>();
        public List<string> RequiredMaterials { get; set; } = new List<string>();
        public int EstimatedDuration { get; set; }
        public string Difficulty { get; set; }
        public List<string> Instructions { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public bool IsRequired { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string CompletedBy { get; set; }
        public string Notes { get; set; }
    }
}