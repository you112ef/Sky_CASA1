namespace MedicalLabAnalyzer.Models
{
    public class CASA_Result
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public double VCL { get; set; }
        public double VSL { get; set; }
        public double VAP { get; set; }
        public double ALH { get; set; }
        public double BCF { get; set; }
        public double MotilityPercent { get; set; }
        public double ProgressivePercent { get; set; }
        public int TrackCount { get; set; }
        public string MetaJson { get; set; }
    }
}