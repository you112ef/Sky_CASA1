using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MedicalLabAnalyzer.Helpers;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
<<<<<<< HEAD
    public class ImageAnalysisService
    {
        private readonly double _defaultPixelsPerMicron;
        private readonly double _minBlobAreaPx;

        public ImageAnalysisService(double pixelsPerMicron = 0.5, double minBlobAreaPx = 8)
        {
            _defaultPixelsPerMicron = pixelsPerMicron;
            _minBlobAreaPx = minBlobAreaPx;
        }

        /// <summary>
        /// Extract tracks from local video file, return tracks in micrometers & seconds.
        /// </summary>
        public List<List<TrackPoint>> ExtractTracksFromVideo(string videoPath, double pixelsPerMicron, out double fps)
        {
            if (!File.Exists(videoPath)) throw new FileNotFoundException(videoPath);
            var cap = new VideoCapture(videoPath);
            fps = cap.Get(CapProp.Fps);
            if (fps <= 0) fps = 25; // fallback
            var tracker = new SimpleTracker { MaxMissedFrames = 6, MaxMatchDistancePx = 40 };

            var bg = new BackgroundSubtractorMOG2(200, 25, false);
            Mat frame = new Mat();
            int idx = 0;

            while (cap.Read(frame) && !frame.IsEmpty)
            {
                idx++;
                var tSec = idx / fps;

                using var gray = new Mat();
                CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
                CvInvoke.GaussianBlur(gray, gray, new System.Drawing.Size(5, 5), 1.5);

                var fg = new Mat();
                bg.Apply(gray, fg);

                CvInvoke.Threshold(fg, fg, 127, 255, ThresholdType.Binary);
                var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));
                CvInvoke.MorphologyEx(fg, fg, MorphOp.Close, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

                using var contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(fg, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                var detections = new List<(double x, double y)>();
                for (int i = 0; i < contours.Size; i++)
                {
                    var cnt = contours[i];
                    double area = CvInvoke.ContourArea(cnt);
                    if (area < _minBlobAreaPx) continue;
                    var rect = CvInvoke.BoundingRectangle(cnt);
                    var cx = rect.X + rect.Width / 2.0;
                    var cy = rect.Y + rect.Height / 2.0;
                    detections.Add((cx, cy));
                }

                tracker.Update(detections, tSec);
            }

            cap.Dispose();

            var outTracks = new List<List<TrackPoint>>();
            foreach (var t in tracker.Tracks)
            {
                if (t.Points.Count < 2) continue;
                var tr = new List<TrackPoint>();
                foreach (var p in t.Points)
                {
                    tr.Add(new TrackPoint
                    {
                        X = p.X / pixelsPerMicron, // convert px -> µm
                        Y = p.Y / pixelsPerMicron,
                        T = p.T
                    });
                }
                outTracks.Add(tr);
            }
            return outTracks;
        }

        /// <summary>
        /// Given tracks (µm, seconds), compute aggregated CASA metrics.
        /// </summary>
        public CASA_Result AnalyzeCASAFromTracks(List<List<TrackPoint>> tracks)
        {
            var vclList = new List<double>();
            var vslList = new List<double>();
            var vapList = new List<double>();
            var alhList = new List<double>();
            var bcfList = new List<double>();
            int motile = 0, progressive = 0;
            foreach (var t in tracks)
            {
                if (t.Count < 2) continue;
                double curvLen = 0;
                for (int i = 1; i < t.Count; i++) curvLen += Dist(t[i - 1], t[i]);
                double dur = t.Last().T - t.First().T;
                if (dur <= 0) continue;
                double vcl = curvLen / dur;
                vclList.Add(vcl);

                double straight = Dist(t.First(), t.Last());
                double vsl = straight / dur;
                vslList.Add(vsl);

                var smoothed = SmoothPath(t, 3);
                double vapLen = 0;
                for (int i = 1; i < smoothed.Count; i++) vapLen += Dist(smoothed[i - 1], smoothed[i]);
                double vap = vapLen / dur;
                vapList.Add(vap);

                double alh = CalcALH(t, smoothed);
                alhList.Add(alh);

                double bcf = CalcBCF(t, smoothed, dur);
                bcfList.Add(bcf);

                // motility criteria (lab-tunable thresholds)
                if (vcl > 20) motile++;
                if (vsl > 25 && (vsl / vcl) > 0.5) progressive++;
            }

            return new CASA_Result
            {
                VCL = Mean(vclList),
                VSL = Mean(vslList),
                VAP = Mean(vapList),
                ALH = Mean(alhList),
                BCF = Mean(bcfList),
                MotilityPercent = tracks.Count == 0 ? 0 : 100.0 * motile / tracks.Count,
                ProgressivePercent = tracks.Count == 0 ? 0 : 100.0 * progressive / tracks.Count,
                TrackCount = tracks.Count,
                MetaJson = $"{{\"ComputedAt\":\"{DateTime.UtcNow:O}\"}}"
            };
        }

        // helpers
        private double Dist(TrackPoint a, TrackPoint b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private List<TrackPoint> SmoothPath(List<TrackPoint> path, int window)
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

        private double CalcALH(List<TrackPoint> path, List<TrackPoint> smooth)
        {
            double sum = 0;
            for (int i = 0; i < path.Count; i++)
            {
                var s = FindClosest(smooth, path[i]);
                sum += Dist(s, path[i]);
            }
            return sum / path.Count;
        }

        private TrackPoint FindClosest(List<TrackPoint> poly, TrackPoint p)
        {
            TrackPoint best = null; double bd = double.MaxValue;
            foreach (var q in poly)
            {
                var d = Dist(q, p);
                if (d < bd) { bd = d; best = q; }
            }
            return best;
        }

        private double CalcBCF(List<TrackPoint> path, List<TrackPoint> smooth, double duration)
        {
            var lateral = new List<double>();
            for (int i = 0; i < path.Count; i++)
            {
                var s = FindClosest(smooth, path[i]);
                var dx = path[i].X - s.X;
                var dy = path[i].Y - s.Y;
                lateral.Add(Math.Sqrt(dx * dx + dy * dy));
            }
            if (lateral.Count < 3 || duration <= 0) return 0;
            double mean = lateral.Average();
            int crossings = 0;
            for (int i = 1; i < lateral.Count; i++)
            {
                if ((lateral[i - 1] - mean) * (lateral[i] - mean) < 0) crossings++;
            }
            return crossings / duration;
        }

        private double Mean(List<double> list) { if (list == null || list.Count == 0) return 0; return list.Average(); }
=======
    /// <summary>
    /// Advanced image analysis service for CASA using Kalman tracking and Hungarian algorithm
    /// </summary>
    public class ImageAnalysisService
    {
        private readonly AnalysisParameters _parameters;
        private readonly ILogger<ImageAnalysisService> _logger;

        public ImageAnalysisService(AnalysisParameters parameters = null, ILogger<ImageAnalysisService> logger = null)
        {
            _parameters = parameters ?? new AnalysisParameters();
            _logger = logger;
        }

        /// <summary>
        /// Extract tracks from video using robust tracker (Kalman + Hungarian)
        /// </summary>
        /// <param name="videoPath">Path to video file</param>
        /// <param name="pixelsPerMicron">Conversion factor (pixels per µm)</param>
        /// <param name="fps">Output parameter: video frame rate</param>
        /// <param name="progress">Progress callback</param>
        /// <returns>List of track points for each valid track</returns>
        public List<List<TrackPoint>> ExtractTracksFromVideo(
            string videoPath, 
            double pixelsPerMicron, 
            out double fps, 
            Action<double, int, int> progress = null)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException($"Video file not found: {videoPath}");

            _logger?.LogInformation($"Starting video analysis: {videoPath}");

            var cap = new VideoCapture(videoPath);
            fps = cap.Get(CapProp.Fps);
            if (fps <= 0) fps = 25.0;

            _logger?.LogInformation($"Video FPS: {fps}");

            // Initialize background subtractor
            var bg = new BackgroundSubtractorMOG2(
                (int)_parameters.BackgroundSubtractionHistory, 
                _parameters.BackgroundSubtractionThreshold, 
                false);

            // Initialize multi-tracker
            var tracker = new MultiTracker
            {
                MaxMissedFrames = _parameters.MaxMissedFrames,
                MaxMatchDistance = _parameters.MaxMatchDistancePx,
                MinTrackDuration = _parameters.MinTrackDuration,
                MinTrackPoints = _parameters.MinTrackPoints
            };

            Mat frame = new Mat();
            int frameIndex = 0;
            int totalFrames = (int)cap.Get(CapProp.FrameCount);

            try
            {
                while (cap.Read(frame) && !frame.IsEmpty)
                {
                    frameIndex++;
                    double timeSec = frameIndex / fps;

                    // Process frame
                    var detections = ProcessFrame(frame, bg);
                    
                    // Update tracker
                    tracker.PredictAll();
                    tracker.Update(detections, timeSec);

                    // Report progress
                    if (frameIndex % 50 == 0 || frameIndex == totalFrames)
                    {
                        progress?.Invoke(timeSec, frameIndex, totalFrames);
                        _logger?.LogDebug($"Processed frame {frameIndex}/{totalFrames} (t={timeSec:F2}s), tracks: {tracker.Tracks.Count}");
                    }
                }
            }
            finally
            {
                cap.Dispose();
                frame.Dispose();
            }

            // Convert tracks to µm units and filter valid tracks
            var result = new List<List<TrackPoint>>();
            var validTracks = tracker.GetValidTracks();

            foreach (var track in validTracks)
            {
                var trackPoints = new List<TrackPoint>();
                foreach (var point in track.Points)
                {
                    trackPoints.Add(new TrackPoint(
                        point.X / pixelsPerMicron,
                        point.Y / pixelsPerMicron,
                        point.T,
                        point.VX / pixelsPerMicron,
                        point.VY / pixelsPerMicron
                    ));
                }
                result.Add(trackPoints);
            }

            _logger?.LogInformation($"Analysis complete. Valid tracks: {result.Count}");
            return result;
        }

        /// <summary>
        /// Process a single frame to detect sperm heads
        /// </summary>
        /// <param name="frame">Input frame</param>
        /// <param name="bg">Background subtractor</param>
        /// <returns>List of detected positions (x, y)</returns>
        private List<(double x, double y)> ProcessFrame(Mat frame, BackgroundSubtractorMOG2 bg)
        {
            var detections = new List<(double x, double y)>();

            using (var gray = new Mat())
            using (var blurred = new Mat())
            using (var fg = new Mat())
            using (var processed = new Mat())
            {
                // Convert to grayscale
                CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);

                // Apply Gaussian blur to reduce noise
                CvInvoke.GaussianBlur(gray, blurred, 
                    new System.Drawing.Size(5, 5), 
                    _parameters.GaussianBlurSigma);

                // Background subtraction
                bg.Apply(blurred, fg);

                // Threshold to create binary image
                CvInvoke.Threshold(fg, processed, 127, 255, ThresholdType.Binary);

                // Morphological operations to clean up
                var kernel = CvInvoke.GetStructuringElement(
                    ElementShape.Ellipse, 
                    new System.Drawing.Size(_parameters.MorphologyKernelSize, _parameters.MorphologyKernelSize), 
                    new System.Drawing.Point(-1, -1));
                
                CvInvoke.MorphologyEx(processed, processed, MorphOp.Close, kernel, 
                    new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

                // Find contours
                using var contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(processed, contours, null, RetrType.External, 
                    ChainApproxMethod.ChainApproxSimple);

                // Filter contours by area and extract centroids
                for (int i = 0; i < contours.Size; i++)
                {
                    var contour = contours[i];
                    double area = CvInvoke.ContourArea(contour);
                    
                    if (area >= _parameters.MinBlobAreaPx)
                    {
                        var rect = CvInvoke.BoundingRectangle(contour);
                        double cx = rect.X + rect.Width / 2.0;
                        double cy = rect.Y + rect.Height / 2.0;
                        detections.Add((cx, cy));
                    }
                }
            }

            return detections;
        }

        /// <summary>
        /// Analyze CASA parameters from extracted tracks
        /// </summary>
        /// <param name="tracks">List of track points for each track</param>
        /// <param name="fps">Video frame rate</param>
        /// <param name="videoPath">Video file path</param>
        /// <param name="micronsPerPixel">Calibration factor</param>
        /// <returns>CASA analysis results</returns>
        public CASA_Result AnalyzeCASAFromTracks(
            List<List<TrackPoint>> tracks, 
            double fps, 
            string videoPath = "", 
            double micronsPerPixel = 1.0)
        {
            var result = new CASA_Result
            {
                VideoPath = videoPath,
                FPS = fps,
                MicronsPerPixel = micronsPerPixel,
                TrackCount = tracks.Count,
                Parameters = _parameters
            };

            if (tracks.Count == 0)
            {
                _logger?.LogWarning("No tracks found for CASA analysis");
                return result;
            }

            var trackResults = new List<TrackResult>();
            var allVCL = new List<double>();
            var allVSL = new List<double>();
            var allVAP = new List<double>();
            var allALH = new List<double>();
            var allBCF = new List<double>();

            foreach (var track in tracks)
            {
                if (track.Count < 2) continue;

                var trackResult = AnalyzeSingleTrack(track, fps);
                if (trackResult != null)
                {
                    trackResults.Add(trackResult);
                    allVCL.Add(trackResult.VCL);
                    allVSL.Add(trackResult.VSL);
                    allVAP.Add(trackResult.VAP);
                    allALH.Add(trackResult.ALH);
                    allBCF.Add(trackResult.BCF);
                }
            }

            result.TrackResults = trackResults;

            // Calculate aggregate statistics
            if (allVCL.Count > 0)
            {
                result.VCL = allVCL.Average();
                result.VSL = allVSL.Average();
                result.VAP = allVAP.Average();
                result.ALH = allALH.Average();
                result.BCF = allBCF.Average();
                result.AnalysisDuration = trackResults.Max(t => t.Duration);
                result.CalculateDerivedParameters();

                // Calculate quality metrics
                result.Quality.AverageTrackQuality = trackResults.Average(t => t.QualityScore);
                result.Quality.AverageTrackDuration = trackResults.Average(t => t.Duration);
                result.Quality.AverageTrackLength = trackResults.Average(t => t.Points.Count);
            }

            _logger?.LogInformation($"CASA analysis complete. VCL: {result.VCL:F2}, VSL: {result.VSL:F2}, VAP: {result.VAP:F2}");
            return result;
        }

        /// <summary>
        /// Analyze a single track to calculate CASA parameters
        /// </summary>
        /// <param name="track">Track points</param>
        /// <param name="fps">Frame rate</param>
        /// <returns>Track analysis result</returns>
        private TrackResult AnalyzeSingleTrack(List<TrackPoint> track, double fps)
        {
            if (track.Count < 2) return null;

            var result = new TrackResult
            {
                TrackId = track.GetHashCode(), // Simple ID generation
                PointCount = track.Count,
                Duration = track[^1].T - track[0].T,
                Points = track.ToList()
            };

            if (result.Duration <= 0) return null;

            // Calculate VCL (Curvilinear Velocity)
            double totalPathLength = 0;
            for (int i = 1; i < track.Count; i++)
            {
                var prev = track[i - 1];
                var curr = track[i];
                double dx = curr.X - prev.X;
                double dy = curr.Y - prev.Y;
                totalPathLength += Math.Sqrt(dx * dx + dy * dy);
            }
            result.VCL = totalPathLength / result.Duration;

            // Calculate VSL (Straight Line Velocity)
            var first = track[0];
            var last = track[^1];
            double straightLineDistance = Math.Sqrt(
                Math.Pow(last.X - first.X, 2) + Math.Pow(last.Y - first.Y, 2));
            result.VSL = straightLineDistance / result.Duration;

            // Calculate VAP (Average Path Velocity) using smoothing
            result.VAP = CalculateVAP(track, result.Duration);

            // Calculate ALH (Amplitude of Lateral Head Displacement)
            result.ALH = CalculateALH(track);

            // Calculate BCF (Beat Cross Frequency)
            result.BCF = CalculateBCF(track, fps);

            // Calculate quality score
            result.QualityScore = CalculateTrackQuality(track, result);

            return result;
        }

        /// <summary>
        /// Calculate VAP using path smoothing
        /// </summary>
        private double CalculateVAP(List<TrackPoint> track, double duration)
        {
            if (track.Count < 3) return 0;

            // Simple smoothing: average position over window
            var smoothedPoints = new List<TrackPoint>();
            int windowSize = Math.Min(5, track.Count / 2);

            for (int i = 0; i < track.Count; i++)
            {
                int start = Math.Max(0, i - windowSize);
                int end = Math.Min(track.Count - 1, i + windowSize);
                int count = end - start + 1;

                double avgX = track.Skip(start).Take(count).Average(p => p.X);
                double avgY = track.Skip(start).Take(count).Average(p => p.Y);
                double avgT = track.Skip(start).Take(count).Average(p => p.T);

                smoothedPoints.Add(new TrackPoint(avgX, avgY, avgT));
            }

            // Calculate path length of smoothed trajectory
            double smoothedPathLength = 0;
            for (int i = 1; i < smoothedPoints.Count; i++)
            {
                var prev = smoothedPoints[i - 1];
                var curr = smoothedPoints[i];
                double dx = curr.X - prev.X;
                double dy = curr.Y - prev.Y;
                smoothedPathLength += Math.Sqrt(dx * dx + dy * dy);
            }

            return smoothedPathLength / duration;
        }

        /// <summary>
        /// Calculate ALH (Amplitude of Lateral Head Displacement)
        /// </summary>
        private double CalculateALH(List<TrackPoint> track)
        {
            if (track.Count < 3) return 0;

            var deviations = new List<double>();
            
            // Calculate average path (simple moving average)
            for (int i = 1; i < track.Count - 1; i++)
            {
                var prev = track[i - 1];
                var curr = track[i];
                var next = track[i + 1];

                // Average position
                double avgX = (prev.X + curr.X + next.X) / 3.0;
                double avgY = (prev.Y + curr.Y + next.Y) / 3.0;

                // Distance from average path
                double deviation = Math.Sqrt(
                    Math.Pow(curr.X - avgX, 2) + Math.Pow(curr.Y - avgY, 2));
                deviations.Add(deviation);
            }

            return deviations.Count > 0 ? deviations.Average() : 0;
        }

        /// <summary>
        /// Calculate BCF (Beat Cross Frequency)
        /// </summary>
        private double CalculateBCF(List<TrackPoint> track, double fps)
        {
            if (track.Count < 3) return 0;

            int crossings = 0;
            var smoothedPoints = new List<TrackPoint>();
            
            // Simple smoothing for average path
            int windowSize = Math.Min(3, track.Count / 2);
            for (int i = 0; i < track.Count; i++)
            {
                int start = Math.Max(0, i - windowSize);
                int end = Math.Min(track.Count - 1, i + windowSize);
                int count = end - start + 1;

                double avgX = track.Skip(start).Take(count).Average(p => p.X);
                double avgY = track.Skip(start).Take(count).Average(p => p.Y);
                double avgT = track.Skip(start).Take(count).Average(p => p.T);

                smoothedPoints.Add(new TrackPoint(avgX, avgY, avgT));
            }

            // Count crossings of the average path
            for (int i = 1; i < track.Count; i++)
            {
                var prev = track[i - 1];
                var curr = track[i];
                var prevAvg = smoothedPoints[i - 1];
                var currAvg = smoothedPoints[i];

                // Check if track crossed the average path
                bool prevAbove = prev.Y > prevAvg.Y;
                bool currAbove = curr.Y > currAvg.Y;
                
                if (prevAbove != currAbove)
                {
                    crossings++;
                }
            }

            double duration = track[^1].T - track[0].T;
            return duration > 0 ? crossings / duration : 0;
        }

        /// <summary>
        /// Calculate track quality score
        /// </summary>
        private double CalculateTrackQuality(List<TrackPoint> track, TrackResult result)
        {
            if (track.Count < 2) return 0;

            double quality = 1.0;

            // Penalize very short tracks
            if (result.Duration < 1.0)
                quality *= 0.5;

            // Penalize very slow tracks (likely noise)
            if (result.VCL < 10.0)
                quality *= 0.3;

            // Reward consistent movement
            double velocityVariance = CalculateVelocityVariance(track);
            if (velocityVariance > 0)
            {
                double consistency = Math.Max(0, 1.0 - velocityVariance / 100.0);
                quality *= consistency;
            }

            return Math.Max(0, Math.Min(1, quality));
        }

        /// <summary>
        /// Calculate velocity variance to assess track consistency
        /// </summary>
        private double CalculateVelocityVariance(List<TrackPoint> track)
        {
            if (track.Count < 2) return 0;

            var velocities = new List<double>();
            for (int i = 1; i < track.Count; i++)
            {
                var prev = track[i - 1];
                var curr = track[i];
                double dt = curr.T - prev.T;
                
                if (dt > 0)
                {
                    double vx = (curr.X - prev.X) / dt;
                    double vy = (curr.Y - prev.Y) / dt;
                    double vel = Math.Sqrt(vx * vx + vy * vy);
                    velocities.Add(vel);
                }
            }

            if (velocities.Count == 0) return 0;

            double mean = velocities.Average();
            double variance = velocities.Select(v => Math.Pow(v - mean, 2)).Average();
            return variance;
        }
>>>>>>> release/v1.0.0
    }
}