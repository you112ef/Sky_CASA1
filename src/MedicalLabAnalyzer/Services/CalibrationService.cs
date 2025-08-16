using System;
using Dapper;

namespace MedicalLabAnalyzer.Services
{
    public class CalibrationService
    {
        private readonly DatabaseService _db;
        public CalibrationService(DatabaseService db) { _db = db; }

        public void SaveCalibration(double micronsPerPixel, double fps, string cameraName, string userName, string notes = null)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            conn.Execute("INSERT INTO Calibration (MicronsPerPixel, FPS, CameraName, UserName, Notes, CreatedAt) VALUES (@m,@f,@c,@u,@n,@t)",
                new { m = micronsPerPixel, f = fps, c = cameraName, u = userName, n = notes, t = DateTime.UtcNow });
            AuditLogger.Log("Calibration.Save", $"MicronsPerPixel={micronsPerPixel}, FPS={fps}, camera={cameraName}, user={userName}");
        }

        public Calibration GetLatestCalibration(string cameraName = null)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            if (string.IsNullOrEmpty(cameraName))
                return conn.QuerySingleOrDefault<Calibration>("SELECT * FROM Calibration ORDER BY CreatedAt DESC LIMIT 1");
            return conn.QuerySingleOrDefault<Calibration>("SELECT * FROM Calibration WHERE CameraName=@c ORDER BY CreatedAt DESC LIMIT 1", new { c = cameraName });
        }
    }
}