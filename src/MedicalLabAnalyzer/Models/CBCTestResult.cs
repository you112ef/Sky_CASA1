namespace MedicalLabAnalyzer.Models
{
    public class CBCTestResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public double WBC { get; set; } // White Blood Cells (K/µL)
        public double RBC { get; set; } // Red Blood Cells (M/µL)
        public double Hemoglobin { get; set; } // g/dL
        public double Hematocrit { get; set; } // %
        public double Platelets { get; set; } // K/µL
        public double MCV { get; set; } // Mean Corpuscular Volume (fL)
        public double MCH { get; set; } // Mean Corpuscular Hemoglobin (pg)
        public double MCHC { get; set; } // Mean Corpuscular Hemoglobin Concentration (g/dL)
        public double RDW { get; set; } // Red Cell Distribution Width (%)
        public double Neutrophils { get; set; } // %
        public double Lymphocytes { get; set; } // %
        public double Monocytes { get; set; } // %
        public double Eosinophils { get; set; } // %
        public double Basophils { get; set; } // %
        public DateTime TestDate { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } // Normal, Abnormal, Critical

        public CBCTestResult()
        {
            TestDate = DateTime.Now;
            Status = "Normal";
        }

        /// <summary>
        /// التحقق من القيم المرجعية وتحديد الحالة
        /// </summary>
        public void ValidateResults()
        {
            var abnormalCount = 0;
            
            // WBC: 4.0-11.0 K/µL
            if (WBC < 4.0 || WBC > 11.0) abnormalCount++;
            
            // RBC: 4.5-5.5 M/µL (للذكور)
            if (RBC < 4.5 || RBC > 5.5) abnormalCount++;
            
            // Hemoglobin: 13.5-17.5 g/dL (للذكور)
            if (Hemoglobin < 13.5 || Hemoglobin > 17.5) abnormalCount++;
            
            // Hematocrit: 41-50% (للذكور)
            if (Hematocrit < 41.0 || Hematocrit > 50.0) abnormalCount++;
            
            // Platelets: 150-450 K/µL
            if (Platelets < 150 || Platelets > 450) abnormalCount++;
            
            // MCV: 80-100 fL
            if (MCV < 80 || MCV > 100) abnormalCount++;
            
            // MCH: 27-33 pg
            if (MCH < 27 || MCH > 33) abnormalCount++;
            
            // MCHC: 32-36 g/dL
            if (MCHC < 32 || MCHC > 36) abnormalCount++;
            
            // RDW: 11.5-14.5%
            if (RDW < 11.5 || RDW > 14.5) abnormalCount++;
            
            // تحديد الحالة
            if (abnormalCount == 0)
                Status = "Normal";
            else if (abnormalCount <= 2)
                Status = "Abnormal";
            else
                Status = "Critical";
        }

        /// <summary>
        /// الحصول على ملخص النتائج
        /// </summary>
        public string GetSummary()
        {
            return $"WBC: {WBC:F2} K/µL, RBC: {RBC:F2} M/µL, HGB: {Hemoglobin:F2} g/dL, HCT: {Hematocrit:F2}%, PLT: {Platelets:F0} K/µL - Status: {Status}";
        }

        /// <summary>
        /// الحصول على القيم غير الطبيعية
        /// </summary>
        public List<string> GetAbnormalValues()
        {
            var abnormal = new List<string>();
            
            if (WBC < 4.0 || WBC > 11.0)
                abnormal.Add($"WBC: {WBC:F2} K/µL (Normal: 4.0-11.0)");
            
            if (RBC < 4.5 || RBC > 5.5)
                abnormal.Add($"RBC: {RBC:F2} M/µL (Normal: 4.5-5.5)");
            
            if (Hemoglobin < 13.5 || Hemoglobin > 17.5)
                abnormal.Add($"Hemoglobin: {Hemoglobin:F2} g/dL (Normal: 13.5-17.5)");
            
            if (Hematocrit < 41.0 || Hematocrit > 50.0)
                abnormal.Add($"Hematocrit: {Hematocrit:F2}% (Normal: 41.0-50.0)");
            
            if (Platelets < 150 || Platelets > 450)
                abnormal.Add($"Platelets: {Platelets:F0} K/µL (Normal: 150-450)");
            
            if (MCV < 80 || MCV > 100)
                abnormal.Add($"MCV: {MCV:F1} fL (Normal: 80-100)");
            
            if (MCH < 27 || MCH > 33)
                abnormal.Add($"MCH: {MCH:F1} pg (Normal: 27-33)");
            
            if (MCHC < 32 || MCHC > 36)
                abnormal.Add($"MCHC: {MCHC:F1} g/dL (Normal: 32-36)");
            
            if (RDW < 11.5 || RDW > 14.5)
                abnormal.Add($"RDW: {RDW:F1}% (Normal: 11.5-14.5)");
            
            return abnormal;
        }
    }
}