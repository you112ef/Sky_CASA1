using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalLabAnalyzer.Models
{
    public class Exam
    {
        public int ExamId { get; set; }
        public int PatientId { get; set; }
        public string ExamType { get; set; }
        public string OrderedBy { get; set; }
        public string CollectedAt { get; set; }
        public int PerformedBy { get; set; }
        public string PerformedAt { get; set; }
        public string ResultJson { get; set; }
        public string ReportPath { get; set; }
    }
}