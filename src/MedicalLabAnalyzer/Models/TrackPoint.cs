namespace MedicalLabAnalyzer.Models
{
    /// <summary>
    /// Represents a single point in a sperm track with position and time
    /// </summary>
    public class TrackPoint
    {
        /// <summary>
        /// X coordinate in micrometers
        /// </summary>
        public double X { get; set; }
        
        /// <summary>
        /// Y coordinate in micrometers
        /// </summary>
        public double Y { get; set; }
        
        /// <summary>
        /// Time in seconds from video start
        /// </summary>
        public double T { get; set; }
        
        /// <summary>
        /// Optional: velocity components at this point
        /// </summary>
        public double? VX { get; set; }
        
        /// <summary>
        /// Optional: velocity components at this point
        /// </summary>
        public double? VY { get; set; }
        
        public TrackPoint()
        {
        }
        
        public TrackPoint(double x, double y, double t)
        {
            X = x;
            Y = y;
            T = t;
        }
        
        public TrackPoint(double x, double y, double t, double? vx, double? vy)
        {
            X = x;
            Y = y;
            T = t;
            VX = vx;
            VY = vy;
        }
    }
}