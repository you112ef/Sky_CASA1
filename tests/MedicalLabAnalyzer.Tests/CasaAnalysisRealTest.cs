using System;
using System.IO;
using System.Linq;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Models;
using System.Data.SQLite;

namespace MedicalLabAnalyzer.Tests
{
    /// <summary>
    /// Real-world test for CASA analysis system
    /// </summary>
    public static class CasaAnalysisRealTest
    {
        /// <summary>
        /// Run a complete CASA analysis test with real video
        /// </summary>
        /// <param name="videoPath">Path to video file (optional)</param>
        /// <param name="outputPath">Path for output results (optional)</param>
        public static void RunReal(string videoPath = null, string outputPath = null)
        {
            Console.WriteLine("=== CASA Analysis Real Test ===");
            Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();

            // Setup video path
            videoPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "sperm_sample.mp4");
            if (!File.Exists(videoPath))
            {
                Console.WriteLine($"❌ Video file not found: {videoPath}");
                Console.WriteLine("Please place a sample video file in the Samples folder.");
                return;
            }

            Console.WriteLine($"📹 Video file: {Path.GetFileName(videoPath)}");
            Console.WriteLine($"📁 Full path: {videoPath}");

            try
            {
                // Initialize database connection
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
                var connectionString = $"Data Source={dbPath};Version=3;";
                using var db = new SQLiteConnection(connectionString);
                db.Open();

                // Initialize services
                var calibrationService = new CalibrationService(db);
                var auditLogger = new AuditLogger(db);

                // Get latest calibration
                var latestCalibration = calibrationService.GetLatestCalibration();
                if (latestCalibration == null)
                {
                    Console.WriteLine("⚠️  No calibration found. Creating default calibration...");
                    
                    // Create a default calibration
                    var defaultCalibration = new CalibrationService.CalibrationData
                    {
                        Name = "Default Calibration",
                        MicronsPerPixel = 0.5, // Default value - should be calibrated properly
                        FPS = 25.0,
                        Objective = "40x",
                        Magnification = 40,
                        CameraModel = "Default Camera",
                        CreatedBy = "System",
                        IsActive = true,
                        Notes = "Default calibration created automatically. Please calibrate properly."
                    };

                    var validation = calibrationService.ValidateCalibration(defaultCalibration);
                    if (validation.IsValid)
                    {
                        var calId = calibrationService.SaveCalibration(defaultCalibration);
                        latestCalibration = calibrationService.GetCalibrationById(calId);
                        Console.WriteLine($"✅ Default calibration created (ID: {calId})");
                    }
                    else
                    {
                        Console.WriteLine("❌ Failed to create default calibration:");
                        foreach (var error in validation.Errors)
                        {
                            Console.WriteLine($"   - {error}");
                        }
                        return;
                    }
                }

                Console.WriteLine($"🔧 Using calibration: {latestCalibration.Name}");
                Console.WriteLine($"   Microns per pixel: {latestCalibration.MicronsPerPixel:F3} µm/px");
                Console.WriteLine($"   Frame rate: {latestCalibration.FPS:F1} FPS");
                Console.WriteLine($"   Objective: {latestCalibration.Objective}");
                Console.WriteLine();

                // Initialize analysis service
                var analysisParameters = new AnalysisParameters
                {
                    MinBlobAreaPx = 6.0,
                    MaxMatchDistancePx = 60.0,
                    MaxMissedFrames = 8,
                    MinTrackDuration = 0.5,
                    MinTrackPoints = 3,
                    SmoothingWindow = 5.0,
                    BackgroundSubtractionHistory = 300,
                    BackgroundSubtractionThreshold = 16,
                    GaussianBlurSigma = 1.2,
                    MorphologyKernelSize = 3
                };

                var imageAnalysisService = new ImageAnalysisService(analysisParameters);

                Console.WriteLine("🚀 Starting video analysis...");
                Console.WriteLine("   This may take several minutes depending on video length.");

                // Extract tracks from video
                double fps;
                var tracks = imageAnalysisService.ExtractTracksFromVideo(
                    videoPath, 
                    latestCalibration.MicronsPerPixel, 
                    out fps, 
                    (time, frame, total) =>
                    {
                        if (frame % 100 == 0 || frame == total)
                        {
                            var progress = (double)frame / total * 100;
                            Console.WriteLine($"   📊 Progress: {progress:F1}% ({frame}/{total} frames, t={time:F2}s)");
                        }
                    });

                Console.WriteLine($"✅ Video analysis complete!");
                Console.WriteLine($"   Extracted tracks: {tracks.Count}");
                Console.WriteLine($"   Video FPS: {fps:F1}");
                Console.WriteLine();

                if (tracks.Count == 0)
                {
                    Console.WriteLine("⚠️  No tracks found. This could indicate:");
                    Console.WriteLine("   - Video quality issues");
                    Console.WriteLine("   - Incorrect calibration");
                    Console.WriteLine("   - Analysis parameters need adjustment");
                    Console.WriteLine("   - No sperm detected in video");
                    return;
                }

                // Analyze CASA parameters
                Console.WriteLine("🔬 Analyzing CASA parameters...");
                var analysisId = Guid.NewGuid().ToString();
                var casaResult = imageAnalysisService.AnalyzeCASAFromTracks(
                    tracks, 
                    fps, 
                    videoPath, 
                    latestCalibration.MicronsPerPixel);

                // Display results
                DisplayResults(casaResult, tracks);

                // Log analysis
                auditLogger.LogCASAnalysis(
                    userId: "test_user",
                    userName: "Test User",
                    videoPath: videoPath,
                    calibrationId: latestCalibration.Id,
                    analysisId: analysisId,
                    parameters: analysisParameters,
                    result: casaResult.GetSummary()
                );

                // Save results to file if output path specified
                if (!string.IsNullOrEmpty(outputPath))
                {
                    SaveResultsToFile(casaResult, tracks, outputPath);
                }

                Console.WriteLine();
                Console.WriteLine("✅ CASA analysis test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during analysis: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Display CASA analysis results
        /// </summary>
        private static void DisplayResults(CASA_Result result, System.Collections.Generic.List<System.Collections.Generic.List<TrackPoint>> tracks)
        {
            Console.WriteLine("📊 CASA Analysis Results:");
            Console.WriteLine("=========================");
            Console.WriteLine($"   Total tracks analyzed: {result.TrackCount}");
            Console.WriteLine($"   Analysis duration: {result.AnalysisDuration:F2} seconds");
            Console.WriteLine();

            Console.WriteLine("📈 Velocity Parameters:");
            Console.WriteLine($"   VCL (Curvilinear Velocity): {result.VCL:F2} µm/s");
            Console.WriteLine($"   VSL (Straight Line Velocity): {result.VSL:F2} µm/s");
            Console.WriteLine($"   VAP (Average Path Velocity): {result.VAP:F2} µm/s");
            Console.WriteLine();

            Console.WriteLine("📐 Movement Parameters:");
            Console.WriteLine($"   ALH (Amplitude of Lateral Head): {result.ALH:F2} µm");
            Console.WriteLine($"   BCF (Beat Cross Frequency): {result.BCF:F2} Hz");
            Console.WriteLine($"   LIN (Linearity): {result.LIN:F3}");
            Console.WriteLine($"   STR (Straightness): {result.STR:F3}");
            Console.WriteLine($"   WOB (Wobble): {result.WOB:F3}");
            Console.WriteLine();

            Console.WriteLine("🎯 Quality Metrics:");
            Console.WriteLine($"   Average track quality: {result.Quality.AverageTrackQuality:F3}");
            Console.WriteLine($"   Average track duration: {result.Quality.AverageTrackDuration:F2} seconds");
            Console.WriteLine($"   Average track length: {result.Quality.AverageTrackLength:F1} points");
            Console.WriteLine();

            // Display individual track details
            Console.WriteLine("🔍 Individual Track Details:");
            Console.WriteLine("Track ID | Points | Duration | VCL     | VSL     | Quality");
            Console.WriteLine("---------|--------|----------|---------|---------|--------");
            
            for (int i = 0; i < Math.Min(result.TrackResults.Count, 10); i++)
            {
                var track = result.TrackResults[i];
                Console.WriteLine($"   {track.TrackId,-7} | {track.PointCount,-6} | {track.Duration,-8:F2} | {track.VCL,-7:F2} | {track.VSL,-7:F2} | {track.QualityScore,-7:F3}");
            }

            if (result.TrackResults.Count > 10)
            {
                Console.WriteLine($"   ... and {result.TrackResults.Count - 10} more tracks");
            }
            Console.WriteLine();

            // Display parameter ranges
            if (result.TrackResults.Count > 0)
            {
                var vclValues = result.TrackResults.Select(t => t.VCL).ToList();
                var vslValues = result.TrackResults.Select(t => t.VSL).ToList();
                var vapValues = result.TrackResults.Select(t => t.VAP).ToList();

                Console.WriteLine("📊 Parameter Ranges:");
                Console.WriteLine($"   VCL range: {vclValues.Min():F2} - {vclValues.Max():F2} µm/s");
                Console.WriteLine($"   VSL range: {vslValues.Min():F2} - {vslValues.Max():F2} µm/s");
                Console.WriteLine($"   VAP range: {vapValues.Min():F2} - {vapValues.Max():F2} µm/s");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Save results to file
        /// </summary>
        private static void SaveResultsToFile(CASA_Result result, System.Collections.Generic.List<System.Collections.Generic.List<TrackPoint>> tracks, string outputPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var writer = new StreamWriter(outputPath);
                writer.WriteLine("CASA Analysis Results");
                writer.WriteLine("====================");
                writer.WriteLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"Video Path: {result.VideoPath}");
                writer.WriteLine($"Microns per Pixel: {result.MicronsPerPixel:F3}");
                writer.WriteLine($"Frame Rate: {result.FPS:F1} FPS");
                writer.WriteLine();

                writer.WriteLine("Summary Results:");
                writer.WriteLine($"VCL: {result.VCL:F2} µm/s");
                writer.WriteLine($"VSL: {result.VSL:F2} µm/s");
                writer.WriteLine($"VAP: {result.VAP:F2} µm/s");
                writer.WriteLine($"ALH: {result.ALH:F2} µm");
                writer.WriteLine($"BCF: {result.BCF:F2} Hz");
                writer.WriteLine($"LIN: {result.LIN:F3}");
                writer.WriteLine($"STR: {result.STR:F3}");
                writer.WriteLine($"WOB: {result.WOB:F3}");
                writer.WriteLine($"Track Count: {result.TrackCount}");
                writer.WriteLine();

                writer.WriteLine("Individual Track Results:");
                writer.WriteLine("TrackID,Points,Duration,VCL,VSL,VAP,ALH,BCF,Quality");
                
                foreach (var track in result.TrackResults)
                {
                    writer.WriteLine($"{track.TrackId},{track.PointCount},{track.Duration:F2},{track.VCL:F2},{track.VSL:F2},{track.VAP:F2},{track.ALH:F2},{track.BCF:F2},{track.QualityScore:F3}");
                }

                Console.WriteLine($"💾 Results saved to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to save results: {ex.Message}");
            }
        }

        /// <summary>
        /// Run a quick calibration test
        /// </summary>
        public static void RunCalibrationTest()
        {
            Console.WriteLine("=== Calibration Test ===");
            
            try
            {
                var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "medical_lab.db");
                var connectionString = $"Data Source={dbPath};Version=3;";
                using var db = new SQLiteConnection(connectionString);
                db.Open();

                var calibrationService = new CalibrationService(db);

                // Test calibration calculation
                double knownDistance = 100.0; // 100 µm
                double measuredPixels = 200.0; // 200 pixels
                double micronsPerPixel = calibrationService.CalculateMicronsPerPixel(knownDistance, measuredPixels);

                Console.WriteLine($"📏 Calibration Test:");
                Console.WriteLine($"   Known distance: {knownDistance} µm");
                Console.WriteLine($"   Measured pixels: {measuredPixels} px");
                Console.WriteLine($"   Calculated microns per pixel: {micronsPerPixel:F3} µm/px");
                Console.WriteLine();

                // Test validation
                var testCalibration = new CalibrationService.CalibrationData
                {
                    Name = "Test Calibration",
                    MicronsPerPixel = micronsPerPixel,
                    FPS = 25.0,
                    Objective = "40x",
                    Magnification = 40,
                    CameraModel = "Test Camera",
                    CreatedBy = "Test User",
                    IsActive = true
                };

                var validation = calibrationService.ValidateCalibration(testCalibration);
                Console.WriteLine($"✅ Calibration validation: {(validation.IsValid ? "PASSED" : "FAILED")}");
                
                if (!validation.IsValid)
                {
                    foreach (var error in validation.Errors)
                    {
                        Console.WriteLine($"   - {error}");
                    }
                }

                // Get statistics
                var stats = calibrationService.GetStatistics();
                Console.WriteLine($"📊 Calibration Statistics:");
                Console.WriteLine($"   Total calibrations: {stats.TotalCalibrations}");
                Console.WriteLine($"   Active calibrations: {stats.ActiveCalibrations}");
                Console.WriteLine($"   Average microns per pixel: {stats.AverageMicronsPerPixel:F3}");
                Console.WriteLine($"   Average FPS: {stats.AverageFPS:F1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Calibration test failed: {ex.Message}");
            }
        }
    }
}