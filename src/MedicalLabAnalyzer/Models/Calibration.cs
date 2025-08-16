namespace MedicalLabAnalyzer.Models
{
    public class Calibration
    {
        public int Id { get; set; }
        public double MicronsPerPixel { get; set; }
        public double FPS { get; set; }
        public string CameraName { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}