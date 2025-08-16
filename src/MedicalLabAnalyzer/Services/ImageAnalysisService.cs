using System;
using System.Collections.Generic;
using System.IO;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Linq;
using MedicalLabAnalyzer.Helpers;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
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
    }
}