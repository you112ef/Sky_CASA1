namespace MedicalLabAnalyzer.Models
{
    public class CBC_Result
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public double WBC { get; set; }
        public double RBC { get; set; }
        public double HGB { get; set; }
        public double HCT { get; set; }
        public double MCV { get; set; }
        public double MCH { get; set; }
        public double MCHC { get; set; }
        public double RDW { get; set; }
        public double PLT { get; set; }
        public double MPV { get; set; }
    }
}