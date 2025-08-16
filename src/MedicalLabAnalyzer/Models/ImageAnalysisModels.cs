using System;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    public class ImageAnalysisResult
    {
        public string ImagePath { get; set; }
        public string AnalysisType { get; set; }
        public DateTime AnalysisDate { get; set; }
        public bool IsSuccessful { get; set; }
        public Dictionary<string, object> Results { get; set; } = new Dictionary<string, object>();
        public string ErrorMessage { get; set; }
    }

    public class ImageQualityReport
    {
        public string ImagePath { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string OverallQuality { get; set; }
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
        public string ErrorMessage { get; set; }
    }

    public class VideoAnalysisResult
    {
        public string VideoPath { get; set; }
        public string AnalysisType { get; set; }
        public DateTime AnalysisDate { get; set; }
        public bool IsSuccessful { get; set; }
        public double Duration { get; set; }
        public double FrameRate { get; set; }
        public string Resolution { get; set; }
        public Dictionary<string, object> Results { get; set; } = new Dictionary<string, object>();
        public string ErrorMessage { get; set; }
    }

    public class VideoQualityReport
    {
        public string VideoPath { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string OverallQuality { get; set; }
        public string Resolution { get; set; }
        public double FrameRate { get; set; }
        public double Duration { get; set; }
        public string FileSize { get; set; }
        public string Codec { get; set; }
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
        public string ErrorMessage { get; set; }
    }

    public class CalibrationParameters
    {
        public double MicroscopeMagnification { get; set; }
        public double CameraPixelSize { get; set; }
        public double ReferenceObjectSize { get; set; }
        public string ReferenceObjectType { get; set; }
        public string CalibrationMethod { get; set; }
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
    }

    public class TrackPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Area { get; set; }
        public DateTime Timestamp { get; set; }
        public int FrameNumber { get; set; }
    }

    public class CASAFrameAnalysis
    {
        public int SpermCount { get; set; }
        public double Motility { get; set; }
        public List<TrackPoint> Tracks { get; set; } = new List<TrackPoint>();
    }

    public class CBCFrameAnalysis
    {
        public int CellCount { get; set; }
        public double CellDensity { get; set; }
    }

    public class UrineFrameAnalysis
    {
        public int CrystalCount { get; set; }
        public double AverageCrystalSize { get; set; }
    }

    public class StoolFrameAnalysis
    {
        public int OrganicMatterCount { get; set; }
        public double AverageMatterSize { get; set; }
    }

    public class FrameQualityMetrics
    {
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
    }

    public class VideoQualityMetrics
    {
        public string OverallQuality { get; set; }
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
    }

    public class ImageQualityMetrics
    {
        public string OverallQuality { get; set; }
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
    }
}