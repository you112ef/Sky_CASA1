using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Helpers;

namespace MedicalLabAnalyzer.Tests
{
    public static class CasaAnalysisTest
    {
        /// <summary>
        /// اختبار تحليل CASA من ملف فيديو
        /// Usage:
        /// - Place sample video at Samples/sperm_sample.mp4
        /// - Make sure Database/medical_lab.db exists and contains a calibration row or pass pixelsPerMicron param.
        /// </summary>
        public static void Run(string videoPath = null, double? pixelsPerMicronOverride = null)
        {
            Console.WriteLine("=== CASA Analysis Test ===");
            Console.WriteLine("اختبار تحليل CASA");
            Console.WriteLine();
            
            videoPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "sperm_sample.mp4");
            if (!File.Exists(videoPath))
            {
                Console.WriteLine("Error: sample video not found: " + videoPath);
                Console.WriteLine("خطأ: ملف الفيديو العينة غير موجود: " + videoPath);
                Console.WriteLine();
                Console.WriteLine("Please place a sample video file at: " + videoPath);
                Console.WriteLine("الرجاء وضع ملف فيديو عينة في: " + videoPath);
                return;
            }

            try
            {
                // Get calibration data
                var db = new DatabaseService();
                var cal = new CalibrationService(db);
                var latest = cal.GetLatestCalibration();
                double pixelsPerMicron = pixelsPerMicronOverride ?? (latest?.MicronsPerPixel ?? 0.5);
                
                Console.WriteLine($"Using pixelsPerMicron = {pixelsPerMicron} µm/px");
                Console.WriteLine($"يتم استخدام معامل المعايرة = {pixelsPerMicron} ميكرومتر/بكسل");
                Console.WriteLine();

                // Perform video analysis
                var imgService = new ImageAnalysisService();
                double fps;
                var tracks = imgService.ExtractTracksFromVideo(videoPath, pixelsPerMicron, out fps);
                
                Console.WriteLine($"Extracted {tracks.Count} tracks, video FPS={fps}");
                Console.WriteLine($"تم استخراج {tracks.Count} مسار، معدل الإطار = {fps}");
                Console.WriteLine();

                if (tracks.Count == 0)
                {
                    Console.WriteLine("No tracks found. This could be due to:");
                    Console.WriteLine("لم يتم العثور على مسارات. قد يكون السبب:");
                    Console.WriteLine("- Video quality or lighting issues");
                    Console.WriteLine("- مشاكل في جودة الفيديو أو الإضاءة");
                    Console.WriteLine("- Incorrect calibration values");
                    Console.WriteLine("- قيم معايرة غير صحيحة");
                    Console.WriteLine("- No moving objects detected");
                    Console.WriteLine("- لم يتم اكتشاف أجسام متحركة");
                    return;
                }

                // Analyze CASA metrics
                var result = imgService.AnalyzeCASAFromTracks(tracks);

                Console.WriteLine($"Aggregated CASA Results:");
                Console.WriteLine($"نتائج CASA المجمعة:");
                Console.WriteLine($"Tracks: {result.TrackCount}");
                Console.WriteLine($"VCL: {result.VCL:F2} µm/s");
                Console.WriteLine($"VSL: {result.VSL:F2} µm/s");
                Console.WriteLine($"VAP: {result.VAP:F2} µm/s");
                Console.WriteLine($"ALH: {result.ALH:F2} µm");
                Console.WriteLine($"BCF: {result.BCF:F2} Hz");
                Console.WriteLine($"Motility%: {result.MotilityPercent:F1} %");
                Console.WriteLine($"Progressive%: {result.ProgressivePercent:F1} %");
                Console.WriteLine();

                // Print per-track details
                Console.WriteLine("Per-Track Details:");
                Console.WriteLine("تفاصيل كل مسار:");
                int i = 1;
                foreach (var t in tracks)
                {
                    if (t.Count < 2) continue;
                    
                    double curv = 0; 
                    for (int k = 1; k < t.Count; k++) 
                        curv += Dist(t[k - 1], t[k]);
                    
                    double dur = t.Last().T - t.First().T;
                    if (dur <= 0) continue;
                    
                    double vcl = curv / dur;
                    double vsl = Dist(t.First(), t.Last()) / dur;
                    var sm = SmoothPath(t, 3);
                    double vapLen = 0; 
                    for (int k = 1; k < sm.Count; k++) 
                        vapLen += Dist(sm[k - 1], sm[k]);
                    double vap = vapLen / dur;
                    
                    Console.WriteLine($"Track {i++}: points={t.Count}, VCL={vcl:F2}, VSL={vsl:F2}, VAP={vap:F2}");
                }

                // Log the test results
                AuditLogger.Log("Test", $"CASA Analysis Test completed - {tracks.Count} tracks, VCL={result.VCL:F2}, VSL={result.VSL:F2}");

                Console.WriteLine();
                Console.WriteLine("=== Test completed successfully ===");
                Console.WriteLine("=== تم إكمال الاختبار بنجاح ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during CASA analysis: {ex.Message}");
                Console.WriteLine($"خطأ أثناء تحليل CASA: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                AuditLogger.Log("Test", $"CASA Analysis Test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// اختبار بسيط بدون فيديو (للتطوير)
        /// </summary>
        public static void RunSimpleTest()
        {
            Console.WriteLine("=== Simple CASA Test (No Video) ===");
            Console.WriteLine("اختبار CASA بسيط (بدون فيديو)");
            Console.WriteLine();

            try
            {
                // Create sample tracks
                var tracks = CreateSampleTracks();
                Console.WriteLine($"Created {tracks.Count} sample tracks");
                Console.WriteLine($"تم إنشاء {tracks.Count} مسار عينة");
                Console.WriteLine();

                // Analyze
                var imgService = new ImageAnalysisService();
                var result = imgService.AnalyzeCASAFromTracks(tracks);

                Console.WriteLine("Sample CASA Results:");
                Console.WriteLine("نتائج CASA العينة:");
                Console.WriteLine($"VCL: {result.VCL:F2} µm/s");
                Console.WriteLine($"VSL: {result.VSL:F2} µm/s");
                Console.WriteLine($"VAP: {result.VAP:F2} µm/s");
                Console.WriteLine($"ALH: {result.ALH:F2} µm");
                Console.WriteLine($"BCF: {result.BCF:F2} Hz");
                Console.WriteLine($"Motility%: {result.MotilityPercent:F1} %");
                Console.WriteLine($"Progressive%: {result.ProgressivePercent:F1} %");

                Console.WriteLine();
                Console.WriteLine("Simple test completed successfully!");
                Console.WriteLine("تم إكمال الاختبار البسيط بنجاح!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in simple test: {ex.Message}");
                Console.WriteLine($"خطأ في الاختبار البسيط: {ex.Message}");
            }
        }

        /// <summary>
        /// إنشاء مسارات عينة للاختبار
        /// </summary>
        private static List<List<TrackPoint>> CreateSampleTracks()
        {
            var tracks = new List<List<TrackPoint>>();
            var random = new Random(42); // Fixed seed for reproducible results

            for (int t = 0; t < 3; t++)
            {
                var track = new List<TrackPoint>();
                double x = random.Next(100, 200);
                double y = random.Next(100, 200);
                
                for (int i = 0; i < 30; i++)
                {
                    x += random.Next(-5, 6);
                    y += random.Next(-5, 6);
                    track.Add(new TrackPoint
                    {
                        X = x,
                        Y = y,
                        T = i * 0.04 // 25 FPS
                    });
                }
                tracks.Add(track);
            }

            return tracks;
        }

        // Helper methods (duplicate from ImageAnalysisService for convenience)
        private static double Dist(TrackPoint a, TrackPoint b) => 
            Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        
        private static List<TrackPoint> SmoothPath(List<TrackPoint> path, int window)
        {
            var outp = new List<TrackPoint>();
            for (int i = 0; i < path.Count; i++)
            {
                int s = Math.Max(0, i - window);
                int e = Math.Min(path.Count - 1, i + window);
                double sx = 0, sy = 0, st = 0; int c = 0;
                for (int j = s; j <= e; j++) 
                { 
                    sx += path[j].X; 
                    sy += path[j].Y; 
                    st += path[j].T; 
                    c++; 
                }
                outp.Add(new TrackPoint { X = sx / c, Y = sy / c, T = st / c });
            }
            return outp;
        }
    }
}