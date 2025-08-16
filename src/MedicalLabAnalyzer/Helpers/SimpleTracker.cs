using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalLabAnalyzer.Helpers
{
    public class TrackPoint
    {
        public double X { get; set; }  // pixel or µm depending on usage
        public double Y { get; set; }
        public double T { get; set; }  // seconds
    }

    public class Track
    {
        public int Id { get; set; }
        public List<TrackPoint> Points { get; } = new List<TrackPoint>();
        public int MissedFrames { get; set; } = 0;
    }

    public class SimpleTracker
    {
        private int _nextId = 1;
        public List<Track> Tracks { get; } = new List<Track>();
        public int MaxMissedFrames { get; set; } = 6;
        public double MaxMatchDistancePx { get; set; } = 40.0;

        public void Update(List<(double x, double y)> detections, double timeSeconds)
        {
            var assigned = new HashSet<int>();
            var detList = detections.Select((d, idx) => new { d.x, d.y, idx }).ToList();

            var trackLast = Tracks.Select(t => new { t, last = t.Points.LastOrDefault() }).Where(x => x.last != null).ToList();

            foreach (var tinfo in trackLast)
            {
                double bestDist = double.MaxValue;
                int bestIdx = -1;
                for (int i = 0; i < detList.Count; i++)
                {
                    if (assigned.Contains(detList[i].idx)) continue;
                    var dx = detList[i].x - tinfo.last.X;
                    var dy = detList[i].y - tinfo.last.Y;
                    var d = Math.Sqrt(dx * dx + dy * dy);
                    if (d < bestDist) { bestDist = d; bestIdx = detList[i].idx; }
                }
                if (bestIdx != -1 && bestDist <= MaxMatchDistancePx)
                {
                    var det = detList.First(z => z.idx == bestIdx);
                    tinfo.t.Points.Add(new TrackPoint { X = det.x, Y = det.y, T = timeSeconds });
                    tinfo.t.MissedFrames = 0;
                    assigned.Add(bestIdx);
                }
                else
                {
                    tinfo.t.MissedFrames++;
                }
            }

            // create new tracks for unassigned detections
            for (int i = 0; i < detList.Count; i++)
            {
                if (assigned.Contains(detList[i].idx)) continue;
                var nt = new Track { Id = _nextId++ };
                nt.Points.Add(new TrackPoint { X = detList[i].x, Y = detList[i].y, T = timeSeconds });
                Tracks.Add(nt);
            }

            Tracks.RemoveAll(t => t.MissedFrames > MaxMissedFrames);
        }
    }
}