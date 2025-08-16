namespace MedicalLabAnalyzer.Models
{
    public class StoolTestResult
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        
        // Physical Properties
        public string Color { get; set; } // بني، أسود، أحمر، أخضر، أصفر، أبيض
        public string Consistency { get; set; } // صلب، طري، سائل، مائي
        public string Shape { get; set; } // طبيعي، رقيق، كرات، قطع صغيرة
        public double Weight { get; set; } // بالجرام
        public string Odor { get; set; } // طبيعي، كريه، حامضي
        
        // Chemical Tests
        public string OccultBlood { get; set; } // Negative, Positive, Weak Positive
        public string pH { get; set; } // 6.0-7.5
        public string ReducingSubstances { get; set; } // Negative, Positive
        public string FatContent { get; set; } // Normal, Increased, Decreased
        
        // Microscopic Examination
        public string Mucus { get; set; } // None, Present, Abundant
        public string UndigestedFood { get; set; } // None, Present, Abundant
        public string MuscleFibers { get; set; } // None, Present, Abundant
        public string Starch { get; set; } // None, Present, Abundant
        public string FatGlobules { get; set; } // None, Present, Abundant
        
        // Parasitology
        public string Parasites { get; set; } // None, Present
        public string ParasiteType { get; set; } // Giardia, Entamoeba, Ascaris, etc.
        public int ParasiteCount { get; set; } // Number of parasites found
        public string Ova { get; set; } // None, Present
        public string OvaType { get; set; } // Type of eggs found
        public int OvaCount { get; set; } // Number of eggs found
        
        // Bacteria and Culture
        public string Bacteria { get; set; } // Normal, Abnormal
        public string BacterialType { get; set; } // Type of bacteria found
        public string Yeast { get; set; } // None, Present
        public string YeastType { get; set; } // Type of yeast found
        
        // Additional Tests
        public string Calprotectin { get; set; } // Normal, Elevated, High
        public double CalprotectinValue { get; set; } // µg/g
        public string Lactoferrin { get; set; } // Negative, Positive
        public string Alpha1Antitrypsin { get; set; } // Normal, Elevated
        
        // Clinical Information
        public DateTime TestDate { get; set; }
        public string CollectionMethod { get; set; } // Spontaneous, Induced
        public string PatientPreparation { get; set; } // Fasting, Normal diet, Special diet
        public string Notes { get; set; }
        public string Status { get; set; } // Normal, Abnormal, Critical
        public string Technician { get; set; }
        public string VerifiedBy { get; set; }

        public StoolTestResult()
        {
            TestDate = DateTime.Now;
            Status = "Normal";
            
            // Default physical properties
            Color = "بني";
            Consistency = "طبيعي";
            Shape = "طبيعي";
            Weight = 100.0; // grams
            Odor = "طبيعي";
            
            // Default chemical tests
            OccultBlood = "Negative";
            pH = "6.5";
            ReducingSubstances = "Negative";
            FatContent = "Normal";
            
            // Default microscopic examination
            Mucus = "None";
            UndigestedFood = "None";
            MuscleFibers = "None";
            Starch = "None";
            FatGlobules = "None";
            
            // Default parasitology
            Parasites = "None";
            ParasiteType = "";
            ParasiteCount = 0;
            Ova = "None";
            OvaType = "";
            OvaCount = 0;
            
            // Default bacteria and culture
            Bacteria = "Normal";
            BacterialType = "";
            Yeast = "None";
            YeastType = "";
            
            // Default additional tests
            Calprotectin = "Normal";
            CalprotectinValue = 0.0;
            Lactoferrin = "Negative";
            Alpha1Antitrypsin = "Normal";
            
            // Default clinical information
            CollectionMethod = "Spontaneous";
            PatientPreparation = "Normal diet";
        }

        /// <summary>
        /// التحقق من القيم المرجعية وتحديد الحالة
        /// </summary>
        public void ValidateResults()
        {
            var abnormalCount = 0;
            
            // Physical properties - check for abnormal colors
            if (Color == "أسود" || Color == "أحمر" || Color == "أبيض")
                abnormalCount++;
            
            // Consistency - check for abnormal consistency
            if (Consistency == "سائل" || Consistency == "مائي")
                abnormalCount++;
            
            // Chemical tests
            if (OccultBlood != "Negative")
                abnormalCount++;
            
            if (ReducingSubstances == "Positive")
                abnormalCount++;
            
            if (FatContent == "Increased")
                abnormalCount++;
            
            // Microscopic examination
            if (Mucus == "Present" || Mucus == "Abundant")
                abnormalCount++;
            
            if (UndigestedFood == "Abundant")
                abnormalCount++;
            
            if (MuscleFibers == "Abundant")
                abnormalCount++;
            
            if (Starch == "Abundant")
                abnormalCount++;
            
            if (FatGlobules == "Present" || FatGlobules == "Abundant")
                abnormalCount++;
            
            // Parasitology
            if (Parasites == "Present")
                abnormalCount++;
            
            if (Ova == "Present")
                abnormalCount++;
            
            // Bacteria and culture
            if (Bacteria == "Abnormal")
                abnormalCount++;
            
            if (Yeast == "Present")
                abnormalCount++;
            
            // Additional tests
            if (Calprotectin == "Elevated" || Calprotectin == "High")
                abnormalCount++;
            
            if (Lactoferrin == "Positive")
                abnormalCount++;
            
            if (Alpha1Antitrypsin == "Elevated")
                abnormalCount++;
            
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
            var summary = $"Color: {Color}, Consistency: {Consistency}, Occult Blood: {OccultBlood}";
            
            var abnormalFindings = new List<string>();
            
            if (OccultBlood != "Negative")
                abnormalFindings.Add($"Occult Blood: {OccultBlood}");
            
            if (Parasites == "Present")
                abnormalFindings.Add($"Parasites: {ParasiteType}");
            
            if (Ova == "Present")
                abnormalFindings.Add($"Ova: {OvaType}");
            
            if (Bacteria == "Abnormal")
                abnormalFindings.Add($"Bacteria: {BacterialType}");
            
            if (Calprotectin == "Elevated" || Calprotectin == "High")
                abnormalFindings.Add($"Calprotectin: {CalprotectinValue:F1} µg/g");
            
            if (abnormalFindings.Count > 0)
                summary += $", Abnormal: {string.Join(", ", abnormalFindings)}";
            
            summary += $", Status: {Status}";
            return summary;
        }

        /// <summary>
        /// الحصول على القيم غير الطبيعية
        /// </summary>
        public List<string> GetAbnormalValues()
        {
            var abnormal = new List<string>();
            
            // Physical properties
            if (Color == "أسود")
                abnormal.Add("Color: أسود (قد يشير إلى نزيف في الجهاز الهضمي العلوي)");
            
            if (Color == "أحمر")
                abnormal.Add("Color: أحمر (قد يشير إلى نزيف في الجهاز الهضمي السفلي)");
            
            if (Color == "أبيض")
                abnormal.Add("Color: أبيض (قد يشير إلى مشاكل في الكبد أو المرارة)");
            
            if (Consistency == "سائل" || Consistency == "مائي")
                abnormal.Add($"Consistency: {Consistency} (قد يشير إلى إسهال)");
            
            // Chemical tests
            if (OccultBlood != "Negative")
                abnormal.Add($"Occult Blood: {OccultBlood} (قد يشير إلى نزيف في الجهاز الهضمي)");
            
            if (ReducingSubstances == "Positive")
                abnormal.Add("Reducing Substances: Positive (قد يشير إلى سوء امتصاص الكربوهيدرات)");
            
            if (FatContent == "Increased")
                abnormal.Add("Fat Content: Increased (قد يشير إلى سوء امتصاص الدهون)");
            
            // Microscopic examination
            if (Mucus == "Present" || Mucus == "Abundant")
                abnormal.Add($"Mucus: {Mucus} (قد يشير إلى التهاب في الأمعاء)");
            
            if (UndigestedFood == "Abundant")
                abnormal.Add("Undigested Food: Abundant (قد يشير إلى مشاكل في الهضم)");
            
            if (FatGlobules == "Present" || FatGlobules == "Abundant")
                abnormal.Add($"Fat Globules: {FatGlobules} (قد يشير إلى سوء امتصاص الدهون)");
            
            // Parasitology
            if (Parasites == "Present")
                abnormal.Add($"Parasites: {ParasiteType} (يتطلب علاج)");
            
            if (Ova == "Present")
                abnormal.Add($"Ova: {OvaType} (يتطلب علاج)");
            
            // Bacteria and culture
            if (Bacteria == "Abnormal")
                abnormal.Add($"Bacteria: {BacterialType} (قد يشير إلى عدوى)");
            
            if (Yeast == "Present")
                abnormal.Add($"Yeast: {YeastType} (قد يشير إلى عدوى فطرية)");
            
            // Additional tests
            if (Calprotectin == "Elevated" || Calprotectin == "High")
                abnormal.Add($"Calprotectin: {CalprotectinValue:F1} µg/g (قد يشير إلى التهاب في الأمعاء)");
            
            if (Lactoferrin == "Positive")
                abnormal.Add("Lactoferrin: Positive (قد يشير إلى التهاب في الأمعاء)");
            
            return abnormal;
        }

        /// <summary>
        /// التحقق من وجود نزيف في الجهاز الهضمي
        /// </summary>
        public bool HasGastrointestinalBleeding()
        {
            return (OccultBlood != "Negative" || Color == "أسود" || Color == "أحمر");
        }

        /// <summary>
        /// التحقق من وجود عدوى طفيلية
        /// </summary>
        public bool HasParasiticInfection()
        {
            return (Parasites == "Present" || Ova == "Present");
        }

        /// <summary>
        /// التحقق من وجود التهاب في الأمعاء
        /// </summary>
        public bool HasInflammatoryBowelDisease()
        {
            return (Calprotectin == "Elevated" || Calprotectin == "High" || 
                    Lactoferrin == "Positive" || Mucus == "Present" || Mucus == "Abundant");
        }

        /// <summary>
        /// التحقق من وجود سوء امتصاص
        /// </summary>
        public bool HasMalabsorption()
        {
            return (FatContent == "Increased" || FatGlobules == "Present" || 
                    FatGlobules == "Abundant" || ReducingSubstances == "Positive");
        }

        /// <summary>
        /// التحقق من وجود إسهال
        /// </summary>
        public bool HasDiarrhea()
        {
            return (Consistency == "سائل" || Consistency == "مائي");
        }
    }
}