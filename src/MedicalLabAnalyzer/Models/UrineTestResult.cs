namespace MedicalLabAnalyzer.Models
{
    public class UrineTestResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        
        // Physical Properties
        public string Color { get; set; } // أصفر، أصفر فاتح، أصفر داكن، بني، أحمر، أخضر
        public double pH { get; set; } // 4.5-8.0
        public double SpecificGravity { get; set; } // 1.005-1.030
        public string Appearance { get; set; } // صافي، عكر، معكر
        
        // Chemical Properties
        public string Protein { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Glucose { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Ketones { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Blood { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Leukocytes { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Nitrites { get; set; } // Negative, Positive
        public string Bilirubin { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        public string Urobilinogen { get; set; } // Negative, Trace, 1+, 2+, 3+, 4+
        
        // Microscopic Examination
        public int RBC { get; set; } // Red Blood Cells per HPF
        public int WBC { get; set; } // White Blood Cells per HPF
        public int EpithelialCells { get; set; } // Epithelial Cells per HPF
        public string Casts { get; set; } // None, Hyaline, Granular, Waxy, Cellular
        public int CastsCount { get; set; } // Number of casts per LPF
        public string Crystals { get; set; } // None, Calcium Oxalate, Uric Acid, Triple Phosphate
        public int CrystalsCount { get; set; } // Number of crystals per HPF
        public string Bacteria { get; set; } // None, Few, Moderate, Many
        public string Yeast { get; set; } // None, Present
        public string Parasites { get; set; } // None, Present
        
        // Additional Information
        public DateTime TestDate { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } // Normal, Abnormal, Critical
        public string Technician { get; set; }
        public string VerifiedBy { get; set; }

        public UrineTestResult()
        {
            TestDate = DateTime.Now;
            Status = "Normal";
            Color = "أصفر";
            Appearance = "صافي";
            pH = 6.0;
            SpecificGravity = 1.020;
            
            // Default negative values
            Protein = "Negative";
            Glucose = "Negative";
            Ketones = "Negative";
            Blood = "Negative";
            Leukocytes = "Negative";
            Nitrites = "Negative";
            Bilirubin = "Negative";
            Urobilinogen = "Negative";
            
            // Default microscopic values
            RBC = 0;
            WBC = 0;
            EpithelialCells = 0;
            Casts = "None";
            CastsCount = 0;
            Crystals = "None";
            CrystalsCount = 0;
            Bacteria = "None";
            Yeast = "None";
            Parasites = "None";
        }

        /// <summary>
        /// التحقق من القيم المرجعية وتحديد الحالة
        /// </summary>
        public void ValidateResults()
        {
            var abnormalCount = 0;
            
            // pH: 4.5-8.0
            if (pH < 4.5 || pH > 8.0) abnormalCount++;
            
            // Specific Gravity: 1.005-1.030
            if (SpecificGravity < 1.005 || SpecificGravity > 1.030) abnormalCount++;
            
            // Chemical tests - check for positive results
            if (Protein != "Negative") abnormalCount++;
            if (Glucose != "Negative") abnormalCount++;
            if (Ketones != "Negative") abnormalCount++;
            if (Blood != "Negative") abnormalCount++;
            if (Leukocytes != "Negative") abnormalCount++;
            if (Nitrites == "Positive") abnormalCount++;
            if (Bilirubin != "Negative") abnormalCount++;
            if (Urobilinogen != "Negative") abnormalCount++;
            
            // Microscopic examination
            if (RBC > 3) abnormalCount++; // Normal: 0-3 RBC/HPF
            if (WBC > 5) abnormalCount++; // Normal: 0-5 WBC/HPF
            if (EpithelialCells > 5) abnormalCount++; // Normal: 0-5 epithelial cells/HPF
            if (Casts != "None") abnormalCount++;
            if (Bacteria != "None") abnormalCount++;
            if (Yeast == "Present") abnormalCount++;
            if (Parasites == "Present") abnormalCount++;
            
            // تحديد الحالة
            if (abnormalCount == 0)
                Status = "Normal";
            else if (abnormalCount <= 3)
                Status = "Abnormal";
            else
                Status = "Critical";
        }

        /// <summary>
        /// الحصول على ملخص النتائج
        /// </summary>
        public string GetSummary()
        {
            var summary = $"pH: {pH:F1}, SG: {SpecificGravity:F3}, Color: {Color}";
            
            var positiveTests = new List<string>();
            if (Protein != "Negative") positiveTests.Add($"Protein: {Protein}");
            if (Glucose != "Negative") positiveTests.Add($"Glucose: {Glucose}");
            if (Ketones != "Negative") positiveTests.Add($"Ketones: {Ketones}");
            if (Blood != "Negative") positiveTests.Add($"Blood: {Blood}");
            if (Leukocytes != "Negative") positiveTests.Add($"Leukocytes: {Leukocytes}");
            if (Nitrites == "Positive") positiveTests.Add("Nitrites: Positive");
            
            if (positiveTests.Count > 0)
                summary += $", Positive: {string.Join(", ", positiveTests)}";
            
            summary += $", Status: {Status}";
            return summary;
        }

        /// <summary>
        /// الحصول على القيم غير الطبيعية
        /// </summary>
        public List<string> GetAbnormalValues()
        {
            var abnormal = new List<string>();
            
            if (pH < 4.5 || pH > 8.0)
                abnormal.Add($"pH: {pH:F1} (Normal: 4.5-8.0)");
            
            if (SpecificGravity < 1.005 || SpecificGravity > 1.030)
                abnormal.Add($"Specific Gravity: {SpecificGravity:F3} (Normal: 1.005-1.030)");
            
            if (Protein != "Negative")
                abnormal.Add($"Protein: {Protein} (Normal: Negative)");
            
            if (Glucose != "Negative")
                abnormal.Add($"Glucose: {Glucose} (Normal: Negative)");
            
            if (Ketones != "Negative")
                abnormal.Add($"Ketones: {Ketones} (Normal: Negative)");
            
            if (Blood != "Negative")
                abnormal.Add($"Blood: {Blood} (Normal: Negative)");
            
            if (Leukocytes != "Negative")
                abnormal.Add($"Leukocytes: {Leukocytes} (Normal: Negative)");
            
            if (Nitrites == "Positive")
                abnormal.Add("Nitrites: Positive (Normal: Negative)");
            
            if (RBC > 3)
                abnormal.Add($"RBC: {RBC}/HPF (Normal: 0-3)");
            
            if (WBC > 5)
                abnormal.Add($"WBC: {WBC}/HPF (Normal: 0-5)");
            
            if (Casts != "None")
                abnormal.Add($"Casts: {Casts} (Normal: None)");
            
            if (Bacteria != "None")
                abnormal.Add($"Bacteria: {Bacteria} (Normal: None)");
            
            return abnormal;
        }

        /// <summary>
        /// التحقق من وجود عدوى في المسالك البولية
        /// </summary>
        public bool HasUTI()
        {
            return (Leukocytes != "Negative" || Nitrites == "Positive" || Bacteria != "None" || WBC > 5);
        }

        /// <summary>
        /// التحقق من وجود نزيف في المسالك البولية
        /// </summary>
        public bool HasHematuria()
        {
            return (Blood != "Negative" || RBC > 3);
        }

        /// <summary>
        /// التحقق من وجود بروتين في البول
        /// </summary>
        public bool HasProteinuria()
        {
            return Protein != "Negative";
        }

        /// <summary>
        /// التحقق من وجود سكر في البول
        /// </summary>
        public bool HasGlycosuria()
        {
            return Glucose != "Negative";
        }
    }
}