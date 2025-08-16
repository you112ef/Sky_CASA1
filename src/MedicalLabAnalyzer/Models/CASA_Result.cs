using System;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    /// <summary>
    /// Results of Computer-Aided Sperm Analysis (CASA)
    /// </summary>
    public class CASA_Result
    {
        /// <summary>
        /// Curvilinear Velocity (µm/s) - Total path length / time
        /// </summary>
        public double VCL { get; set; }
        
        /// <summary>
        /// Straight Line Velocity (µm/s) - Straight line distance / time
        /// </summary>
        public double VSL { get; set; }
        
        /// <summary>
        /// Average Path Velocity (µm/s) - Smoothed path length / time
        /// </summary>
        public double VAP { get; set; }
        
        /// <summary>
        /// Amplitude of Lateral Head Displacement (µm)
        /// </summary>
        public double ALH { get; set; }
        
        /// <summary>
        /// Beat Cross Frequency (Hz) - Frequency of crossing the average path
        /// </summary>
        public double BCF { get; set; }
        
        /// <summary>
        /// Linearity (VSL/VCL) - Straightness of movement
        /// </summary>
        public double LIN { get; set; }
        
        /// <summary>
        /// Straightness (VSL/VAP) - How straight the average path is
        /// </summary>
        public double STR { get; set; }
        
        /// <summary>
        /// Wobble (VAP/VCL) - Regularity of the average path
        /// </summary>
        public double WOB { get; set; }
        
        /// <summary>
        /// Number of tracks analyzed
        /// </summary>
        public int TrackCount { get; set; }
        
        /// <summary>
        /// Total analysis duration (seconds)
        /// </summary>
        public double AnalysisDuration { get; set; }
        
        /// <summary>
        /// Analysis timestamp
        /// </summary>
        public DateTime AnalysisTime { get; set; }
        
        /// <summary>
        /// Video file path used for analysis
        /// </summary>
        public string VideoPath { get; set; }
        
        /// <summary>
        /// Calibration used (microns per pixel)
        /// </summary>
        public double MicronsPerPixel { get; set; }
        
        /// <summary>
        /// Video frame rate (FPS)
        /// </summary>
        public double FPS { get; set; }
        
        /// <summary>
        /// Individual track results
        /// </summary>
        public List<TrackResult> TrackResults { get; set; } = new List<TrackResult>();
        
        /// <summary>
        /// Analysis parameters used
        /// </summary>
        public AnalysisParameters Parameters { get; set; }
        
        /// <summary>
        /// Quality metrics
        /// </summary>
        public QualityMetrics Quality { get; set; }

        public CASA_Result()
        {
            AnalysisTime = DateTime.Now;
            Parameters = new AnalysisParameters();
            Quality = new QualityMetrics();
        }

        /// <summary>
        /// Calculate derived parameters
        /// </summary>
        public void CalculateDerivedParameters()
        {
            if (VCL > 0)
            {
                LIN = VSL / VCL;
                WOB = VAP / VCL;
            }
            
            if (VAP > 0)
            {
                STR = VSL / VAP;
            }
        }

        /// <summary>
        /// Get summary statistics
        /// </summary>
        /// <returns>Dictionary with summary statistics</returns>
        public Dictionary<string, object> GetSummary()
        {
            return new Dictionary<string, object>
            {
                ["VCL"] = VCL,
                ["VSL"] = VSL,
                ["VAP"] = VAP,
                ["ALH"] = ALH,
                ["BCF"] = BCF,
                ["LIN"] = LIN,
                ["STR"] = STR,
                ["WOB"] = WOB,
                ["TrackCount"] = TrackCount,
                ["AnalysisDuration"] = AnalysisDuration,
                ["AnalysisTime"] = AnalysisTime,
                ["VideoPath"] = VideoPath,
                ["MicronsPerPixel"] = MicronsPerPixel,
                ["FPS"] = FPS
            };
        }
    }

    /// <summary>
    /// Individual track analysis result
    /// </summary>
    public class TrackResult
    {
        public int TrackId { get; set; }
        public double VCL { get; set; }
        public double VSL { get; set; }
        public double VAP { get; set; }
        public double ALH { get; set; }
        public double BCF { get; set; }
        public double Duration { get; set; }
        public int PointCount { get; set; }
        public double QualityScore { get; set; }
        public List<TrackPoint> Points { get; set; } = new List<TrackPoint>();
    }

    /// <summary>
    /// Analysis parameters used for CASA
    /// </summary>
    public class AnalysisParameters
    {
        public double MinBlobAreaPx { get; set; } = 6.0;
        public double MaxMatchDistancePx { get; set; } = 60.0;
        public int MaxMissedFrames { get; set; } = 8;
        public double MinTrackDuration { get; set; } = 0.5;
        public int MinTrackPoints { get; set; } = 3;
        public double SmoothingWindow { get; set; } = 5.0;
        public double BackgroundSubtractionHistory { get; set; } = 300;
        public double BackgroundSubtractionThreshold { get; set; } = 16;
        public double GaussianBlurSigma { get; set; } = 1.2;
        public int MorphologyKernelSize { get; set; } = 3;
    }

    /// <summary>
    /// Quality metrics for the analysis
    /// </summary>
    public class QualityMetrics
    {
        public double AverageTrackQuality { get; set; }
        public double TrackConsistency { get; set; }
        public double DetectionConfidence { get; set; }
        public int TotalDetections { get; set; }
        public int ValidDetections { get; set; }
        public double DetectionRate { get; set; }
        public double AverageTrackDuration { get; set; }
        public double AverageTrackLength { get; set; }
    }
}