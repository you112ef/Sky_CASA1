using System;
using System.IO;
using MedicalLabAnalyzer.Services;
using System.Collections.Generic;
using MedicalLabAnalyzer.Helpers;

namespace MedicalLabAnalyzer.Tests
{
    public static class CasaAnalysisTest
    {
        /// <summary>
        /// Usage:
        /// - Place sample video at Samples/sperm_sample.mp4
        /// - Make sure Database/medical_lab.db exists and contains a calibration row or pass pixelsPerMicron param.
        /// </summary>
        public static void Run(string videoPath = null, double? pixelsPerMicronOverride = null)
        {
            Console.WriteLine("=== CASA Analysis Test ===");
            videoPath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples", "sperm_sample.mp4");
            if (!File.Exists(videoPath))
            {
                Console.WriteLine("Error: sample video not found: " + videoPath);
                return;
            }

            var db = new DatabaseService();
            var cal = new CalibrationService(db);
            var latest = cal.GetLatestCalibration();
            double pixelsPerMicron = pixelsPerMicronOverride ?? (latest?.MicronsPerPixel ?? 0.5);
            Console.WriteLine($"Using pixelsPerMicron = {pixelsPerMicron} µm/px");

            var imgService = new ImageAnalysisService();
            double fps;
            var tracks = imgService.ExtractTracksFromVideo(videoPath, pixelsPerMicron, out fps);
            Console.WriteLine($"Extracted {tracks.Count} tracks, video FPS={fps}");

            var result = imgService.AnalyzeCASAFromTracks(tracks);

            Console.WriteLine($"Aggregated CASA:");
            Console.WriteLine($"Tracks: {result.TrackCount}");
            Console.WriteLine($"VCL: {result.VCL:F2} µm/s");
            Console.WriteLine($"VSL: {result.VSL:F2} µm/s");
            Console.WriteLine($"VAP: {result.VAP:F2} µm/s");
            Console.WriteLine($"ALH: {result.ALH:F2} µm");
            Console.WriteLine($"BCF: {result.BCF:F2} Hz");
            Console.WriteLine($"Motility%: {result.MotilityPercent:F1} %");
            Console.WriteLine($"Progressive%: {result.ProgressivePercent:F1} %");

            // print per-track details
            int i = 1;
            foreach (var t in tracks)
            {
                if (t.Count < 2) continue;
                double curv = 0; for (int k = 1; k < t.Count; k++) curv += Dist(t[k - 1], t[k]);
                double dur = t.Last().T - t.First().T;
                if (dur <= 0) continue;
                double vcl = curv / dur;
                double vsl = Dist(t.First(), t.Last()) / dur;
                var sm = SmoothPath(t, 3);
                double vapLen = 0; for (int k = 1; k < sm.Count; k++) vapLen += Dist(sm[k - 1], sm[k]);
                double vap = vapLen / dur;
                Console.WriteLine($"Track {i++}: points={t.Count}, VCL={vcl:F2}, VSL={vsl:F2}, VAP={vap:F2}");
            }

            Console.WriteLine("=== Test completed ===");
        }

        // duplicate helpers for convenience
        private static double Dist(TrackPoint a, TrackPoint b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        private static List<TrackPoint> SmoothPath(List<TrackPoint> path, int window)
        {
            var outp = new List<TrackPoint>();
            for (int i = 0; i < path.Count; i++)
            {
                int s = Math.Max(0, i - window);
                int e = Math.Min(path.Count - 1, i + window);
                double sx = 0, sy = 0, st = 0; int c = 0;
                for (int j = s; j <= e; j++) { sx += path[j].X; sy += path[j].Y; st += path[j].T; c++; }
                outp.Add(new TrackPoint { X = sx / c, Y = sy / c, T = st / c });
            }
            return outp;
        }
    }
}