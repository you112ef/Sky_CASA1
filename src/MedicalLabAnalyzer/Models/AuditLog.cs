namespace MedicalLabAnalyzer.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}