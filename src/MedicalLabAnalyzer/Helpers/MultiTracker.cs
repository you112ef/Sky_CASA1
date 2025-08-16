using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalLabAnalyzer.Helpers
{
    /// <summary>
    /// Multi-object tracker using Kalman filters and Hungarian algorithm for data association
    /// </summary>
    public class MultiTracker
    {
        private int _nextId = 1;
        public List<KalmanTrack> Tracks { get; } = new List<KalmanTrack>();
        
        /// <summary>
        /// Maximum number of frames a track can be missed before deletion
        /// </summary>
        public int MaxMissedFrames { get; set; } = 8;
        
        /// <summary>
        /// Maximum distance for track-detection association (pixels)
        /// </summary>
        public double MaxMatchDistance { get; set; } = 60.0;
        
        /// <summary>
        /// Minimum track duration to be considered valid (seconds)
        /// </summary>
        public double MinTrackDuration { get; set; } = 0.5;
        
        /// <summary>
        /// Minimum number of points for a valid track
        /// </summary>
        public int MinTrackPoints { get; set; } = 3;

        /// <summary>
        /// Predict positions for all tracks using their Kalman filters
        /// </summary>
        public void PredictAll()
        {
            foreach (var track in Tracks)
            {
                track.Predict(1.0); // dt = 1 frame
            }
        }

        /// <summary>
        /// Update tracks with new detections using Hungarian algorithm for optimal assignment
        /// </summary>
        /// <param name="detections">List of detected positions (x, y)</param>
        /// <param name="timeSec">Current time in seconds</param>
        public void Update(List<(double x, double y)> detections, double timeSec)
        {
            // Handle empty detections case
            if (detections.Count == 0)
            {
                // Increment missed frames for all tracks
                foreach (var track in Tracks)
                {
                    track.MissedFrames++;
                }
                return;
            }

            // Handle empty tracks case
            if (Tracks.Count == 0)
            {
                // Create new tracks for all detections
                foreach (var detection in detections)
                {
                    var track = new KalmanTrack(_nextId++, detection.x, detection.y, timeSec);
                    Tracks.Add(track);
                }
                return;
            }

            // Build cost matrix for Hungarian algorithm
            // Rows = tracks, Columns = detections
            int n = Tracks.Count;
            int m = detections.Count;
            double[,] costMatrix = new double[n, m];

            for (int i = 0; i < n; i++)
            {
                var track = Tracks[i];
                var predictedPos = track.Predict();
                
                for (int j = 0; j < m; j++)
                {
                    var detection = detections[j];
                    double dx = predictedPos.x - detection.x;
                    double dy = predictedPos.y - detection.y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    
                    // Cost is the distance between predicted and detected positions
                    costMatrix[i, j] = distance;
                }
            }

            // Solve assignment problem using Hungarian algorithm
            int[] assignment = HungarianAlgorithm.Solve(costMatrix);

            // Process assignments
            var assignedDetections = new HashSet<int>();
            
            for (int i = 0; i < Math.Min(assignment.Length, n); i++)
            {
                int assignedDetectionIndex = assignment[i];
                
                // Check if assignment is valid and within distance threshold
                if (assignedDetectionIndex >= 0 && 
                    assignedDetectionIndex < m && 
                    costMatrix[i, assignedDetectionIndex] <= MaxMatchDistance)
                {
                    // Valid assignment - update track
                    var detection = detections[assignedDetectionIndex];
                    Tracks[i].Correct(detection.x, detection.y, timeSec);
                    assignedDetections.Add(assignedDetectionIndex);
                }
                else
                {
                    // No valid assignment - increment missed frames
                    Tracks[i].MissedFrames++;
                }
            }

            // Create new tracks for unmatched detections
            for (int j = 0; j < m; j++)
            {
                if (!assignedDetections.Contains(j))
                {
                    var detection = detections[j];
                    var newTrack = new KalmanTrack(_nextId++, detection.x, detection.y, timeSec);
                    Tracks.Add(newTrack);
                }
            }

            // Remove stale tracks
            RemoveStaleTracks();
        }

        /// <summary>
        /// Remove tracks that have been missed for too many frames or are too short
        /// </summary>
        private void RemoveStaleTracks()
        {
            Tracks.RemoveAll(track => 
                track.MissedFrames > MaxMissedFrames || 
                track.GetDuration() < MinTrackDuration ||
                track.Points.Count < MinTrackPoints);
        }

        /// <summary>
        /// Get all valid tracks (tracks that meet quality criteria)
        /// </summary>
        /// <returns>List of valid tracks</returns>
        public List<KalmanTrack> GetValidTracks()
        {
            return Tracks.Where(track => 
                track.IsValid && 
                track.GetDuration() >= MinTrackDuration &&
                track.Points.Count >= MinTrackPoints).ToList();
        }

        /// <summary>
        /// Get track statistics
        /// </summary>
        /// <returns>Dictionary with track statistics</returns>
        public Dictionary<string, object> GetStatistics()
        {
            var validTracks = GetValidTracks();
            
            return new Dictionary<string, object>
            {
                ["TotalTracks"] = Tracks.Count,
                ["ValidTracks"] = validTracks.Count,
                ["AverageTrackDuration"] = validTracks.Any() ? validTracks.Average(t => t.GetDuration()) : 0.0,
                ["AverageTrackLength"] = validTracks.Any() ? validTracks.Average(t => t.GetPathLength()) : 0.0,
                ["AverageQualityScore"] = validTracks.Any() ? validTracks.Average(t => t.QualityScore) : 0.0
            };
        }

        /// <summary>
        /// Clear all tracks
        /// </summary>
        public void Clear()
        {
            Tracks.Clear();
            _nextId = 1;
        }

        /// <summary>
        /// Get track by ID
        /// </summary>
        /// <param name="id">Track ID</param>
        /// <returns>Track with specified ID or null if not found</returns>
        public KalmanTrack GetTrackById(int id)
        {
            return Tracks.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Export tracks to a format suitable for CASA analysis
        /// </summary>
        /// <returns>List of track points for each valid track</returns>
        public List<List<Models.TrackPoint>> ExportTracks()
        {
            var validTracks = GetValidTracks();
            return validTracks.Select(track => track.Points.ToList()).ToList();
        }
    }
}