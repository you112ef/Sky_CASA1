using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalLabAnalyzer.Models
{
    public class StoolTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Physical Properties
        public string Color { get; set; } // Brown, Black, Green, Yellow, etc.
        public string Consistency { get; set; } // Formed, Soft, Liquid, Hard
        public string Odor { get; set; } // Normal, Foul, etc.
        public double? Quantity { get; set; } // grams
        public string Blood { get; set; } // None, Visible, Occult
        public string Mucus { get; set; } // None, Present
        public string Pus { get; set; } // None, Present
        
        // Chemical Tests
        public string OccultBlood { get; set; } // Negative, Positive
        public string ReducingSubstances { get; set; } // Negative, Positive
        public string pH { get; set; } // Acidic, Neutral, Alkaline
        public double? pHValue { get; set; }
        public string Fat { get; set; } // Negative, Positive
        public string Protein { get; set; } // Negative, Positive
        
        // Microscopic Examination
        public int? RBC { get; set; } // per HPF
        public int? WBC { get; set; } // per HPF
        public int? EpithelialCells { get; set; } // per HPF
        public int? Macrophages { get; set; } // per HPF
        public int? Eosinophils { get; set; } // per HPF
        
        // Parasites
        public string Parasites { get; set; } // None, Present
        public string ParasiteType { get; set; } // Giardia, Entamoeba, etc.
        public int? ParasiteCount { get; set; } // per HPF
        public string ParasiteStage { get; set; } // Cyst, Trophozoite, Egg, etc.
        
        // Ova and Parasites
        public string Ova { get; set; } // None, Present
        public string OvaType { get; set; } // Ascaris, Hookworm, etc.
        public int? OvaCount { get; set; } // per HPF
        
        // Bacteria
        public string Bacteria { get; set; } // Normal Flora, Abnormal
        public string BacterialType { get; set; } // E. coli, Salmonella, etc.
        public int? BacterialCount { get; set; } // per HPF
        
        // Yeast and Fungi
        public string Yeast { get; set; } // None, Present
        public string YeastType { get; set; } // Candida, etc.
        public int? YeastCount { get; set; } // per HPF
        
        // Undigested Food
        public string UndigestedFood { get; set; } // None, Present
        public string FoodType { get; set; } // Meat, Vegetable, etc.
        
        // Fat Globules
        public string FatGlobules { get; set; } // None, Present
        public int? FatGlobuleCount { get; set; } // per HPF
        
        // Muscle Fibers
        public string MuscleFibers { get; set; } // None, Present
        public int? MuscleFiberCount { get; set; } // per HPF
        
        // Starch Granules
        public string StarchGranules { get; set; } // None, Present
        public int? StarchGranuleCount { get; set; } // per HPF
        
        // Additional Tests
        public string Calprotectin { get; set; } // Normal, Elevated
        public double? CalprotectinValue { get; set; } // μg/g
        public string Lactoferrin { get; set; } // Normal, Elevated
        public double? LactoferrinValue { get; set; } // μg/g
        
        // Culture and Sensitivity
        public string CultureResult { get; set; } // No Growth, Mixed Flora, etc.
        public string PathogenicOrganism { get; set; }
        public string Sensitivity { get; set; } // Sensitive, Resistant, Intermediate
        
        // Quality Control
        public bool IsQualityControlPassed { get; set; } = false;
        public string QualityControlNotes { get; set; }
        
        // Results Interpretation
        public string Interpretation { get; set; } // Normal, Abnormal, Borderline
        public string ClinicalSignificance { get; set; }
        public string Recommendations { get; set; }
        
        // Analysis Details
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        public string AnalyzedBy { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        
        public string Comments { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Properties
        public virtual Exam Exam { get; set; }
    }
}