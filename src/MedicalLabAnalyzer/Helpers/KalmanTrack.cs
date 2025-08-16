using System;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using MedicalLabAnalyzer.Models;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Helpers
{
    /// <summary>
    /// Kalman filter-based track for robust sperm tracking
    /// State: [x, y, vx, vy] ; Measurement: [x, y]
    /// </summary>
    public class KalmanTrack
    {
        public int Id { get; set; }
        public KalmanFilter KF { get; private set; }
        public double LastUpdateTime { get; set; }
        public int MissedFrames { get; set; } = 0;
        public List<TrackPoint> Points { get; } = new List<TrackPoint>();
        
        /// <summary>
        /// Track quality score (higher = better)
        /// </summary>
        public double QualityScore { get; set; } = 1.0;
        
        /// <summary>
        /// Whether this track is considered valid
        /// </summary>
        public bool IsValid => Points.Count >= 3 && MissedFrames <= 10;

        public KalmanTrack(int id, double x, double y, double timeSec)
        {
            Id = id;
            InitializeKalmanFilter();
            
            // Set initial state
            KF.StatePost = new Matrix<float>(new float[,] {
                {(float)x, (float)y, 0f, 0f}
            });
            
            LastUpdateTime = timeSec;
            Points.Add(new TrackPoint(x, y, timeSec));
        }

        private void InitializeKalmanFilter()
        {
            // State: [x, y, vx, vy] - 4 dimensions
            // Measurement: [x, y] - 2 dimensions
            // Control: none - 0 dimensions
            KF = new KalmanFilter(4, 2, 0);
            
            // Transition matrix: constant velocity model
            // x(t+1) = x(t) + vx(t)*dt
            // y(t+1) = y(t) + vy(t)*dt
            // vx(t+1) = vx(t)
            // vy(t+1) = vy(t)
            KF.TransitionMatrix = new Matrix<float>(new float[,] {
                {1, 0, 1, 0},  // x(t+1) = x(t) + vx(t)
                {0, 1, 0, 1},  // y(t+1) = y(t) + vy(t)
                {0, 0, 1, 0},  // vx(t+1) = vx(t)
                {0, 0, 0, 1}   // vy(t+1) = vy(t)
            });
            
            // Measurement matrix: we only observe position, not velocity
            KF.MeasurementMatrix = new Matrix<float>(new float[,] {
                {1, 0, 0, 0},  // measure x
                {0, 1, 0, 0}   // measure y
            });
            
            // Process noise covariance - how much we trust our model
            // Lower values = more trust in model predictions
            KF.ProcessNoiseCovariance = new Matrix<float>(new float[,] {
                {1e-2f, 0,    0,    0},    // position noise
                {0,    1e-2f, 0,    0},    // position noise
                {0,    0,    1e-1f, 0},    // velocity noise
                {0,    0,    0,    1e-1f}  // velocity noise
            });
            
            // Measurement noise covariance - how much we trust our measurements
            // Higher values = less trust in measurements
            KF.MeasurementNoiseCovariance = new Matrix<float>(new float[,] {
                {1e-1f, 0},    // measurement noise for x
                {0,    1e-1f}  // measurement noise for y
            });
            
            // Initial error covariance - initial uncertainty
            KF.ErrorCovPost = new Matrix<float>(new float[,] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            });
        }

        /// <summary>
        /// Predict next position using Kalman filter
        /// </summary>
        /// <param name="dt">Time step (not used in current implementation)</param>
        /// <returns>Predicted position (x, y)</returns>
        public (double x, double y) Predict(double dt = 1.0)
        {
            // Note: In this implementation, dt is fixed at 1.0
            // For variable dt, you would need to update the transition matrix
            var pred = KF.Predict();
            double px = pred[0, 0];
            double py = pred[1, 0];
            return (px, py);
        }

        /// <summary>
        /// Update track with new measurement
        /// </summary>
        /// <param name="x">Measured x position</param>
        /// <param name="y">Measured y position</param>
        /// <param name="timeSec">Current time</param>
        public void Correct(double x, double y, double timeSec)
        {
            var measurement = new Matrix<float>(new float[,] { { (float)x }, { (float)y } });
            KF.Correct(measurement);
            
            LastUpdateTime = timeSec;
            MissedFrames = 0;
            
            // Add point to track
            var point = new TrackPoint(x, y, timeSec);
            
            // Calculate velocity if we have previous points
            if (Points.Count > 0)
            {
                var lastPoint = Points[^1];
                double dt = timeSec - lastPoint.T;
                if (dt > 0)
                {
                    point.VX = (x - lastPoint.X) / dt;
                    point.VY = (y - lastPoint.Y) / dt;
                }
            }
            
            Points.Add(point);
            
            // Update quality score based on track consistency
            UpdateQualityScore();
        }

        /// <summary>
        /// Update track quality score based on consistency
        /// </summary>
        private void UpdateQualityScore()
        {
            if (Points.Count < 3) return;
            
            // Calculate average velocity consistency
            double totalVelocity = 0;
            int velocityCount = 0;
            
            for (int i = 1; i < Points.Count; i++)
            {
                var prev = Points[i - 1];
                var curr = Points[i];
                
                if (prev.VX.HasValue && prev.VY.HasValue)
                {
                    double vel = Math.Sqrt(prev.VX.Value * prev.VX.Value + prev.VY.Value * prev.VY.Value);
                    totalVelocity += vel;
                    velocityCount++;
                }
            }
            
            if (velocityCount > 0)
            {
                double avgVelocity = totalVelocity / velocityCount;
                // Higher average velocity = better quality (sperm should move)
                QualityScore = Math.Min(avgVelocity / 50.0, 1.0); // Normalize to 0-1
            }
        }

        /// <summary>
        /// Get current velocity from Kalman state
        /// </summary>
        /// <returns>Current velocity (vx, vy)</returns>
        public (double vx, double vy) GetCurrentVelocity()
        {
            var state = KF.StatePost;
            return (state[2, 0], state[3, 0]);
        }

        /// <summary>
        /// Get track duration in seconds
        /// </summary>
        /// <returns>Duration in seconds</returns>
        public double GetDuration()
        {
            if (Points.Count < 2) return 0;
            return Points[^1].T - Points[0].T;
        }

        /// <summary>
        /// Get total path length in micrometers
        /// </summary>
        /// <returns>Total path length</returns>
        public double GetPathLength()
        {
            if (Points.Count < 2) return 0;
            
            double totalLength = 0;
            for (int i = 1; i < Points.Count; i++)
            {
                var prev = Points[i - 1];
                var curr = Points[i];
                double dx = curr.X - prev.X;
                double dy = curr.Y - prev.Y;
                totalLength += Math.Sqrt(dx * dx + dy * dy);
            }
            
            return totalLength;
        }
    }
}