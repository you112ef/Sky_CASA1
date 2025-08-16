using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace MedicalLabAnalyzer.Models
{
    public class StoolTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Physical Properties
        [StringLength(50, ErrorMessage = "اللون يجب أن يكون أقل من 50 حرف")]
        public string Color { get; set; } = "Brown"; // Brown, Black, Green, Yellow, etc.
        
        [StringLength(50, ErrorMessage = "القوام يجب أن يكون أقل من 50 حرف")]
        public string Consistency { get; set; } = "Formed"; // Formed, Soft, Liquid, Hard
        
        [StringLength(50, ErrorMessage = "الشكل يجب أن يكون أقل من 50 حرف")]
        public string Shape { get; set; } = "Normal"; // Normal, Abnormal
        
        [Range(0, 1000, ErrorMessage = "الوزن يجب أن يكون بين 0 و 1000 جرام")]
        public double Weight { get; set; } = 100.0; // grams
        
        [StringLength(50, ErrorMessage = "الرائحة يجب أن تكون أقل من 50 حرف")]
        public string Odor { get; set; } = "Normal"; // Normal, Foul, etc.
        
        [Range(0, 1000, ErrorMessage = "الكمية يجب أن تكون بين 0 و 1000 جرام")]
        public double? Quantity { get; set; } // grams
        
        [StringLength(50, ErrorMessage = "الدم يجب أن يكون أقل من 50 حرف")]
        public string Blood { get; set; } = "None"; // None, Visible, Occult
        
        [StringLength(50, ErrorMessage = "المخاط يجب أن يكون أقل من 50 حرف")]
        public string Mucus { get; set; } = "None"; // None, Present
        
        [StringLength(50, ErrorMessage = "الصديد يجب أن يكون أقل من 50 حرف")]
        public string Pus { get; set; } = "None"; // None, Present
        
        // Chemical Tests
        [StringLength(20, ErrorMessage = "الدم الخفي يجب أن يكون أقل من 20 حرف")]
        [RegularExpression("^(Negative|Positive|سالب|موجب)$", ErrorMessage = "الدم الخفي يجب أن يكون: Negative أو Positive")]
        public string OccultBlood { get; set; } = "Negative"; // Negative, Positive
        
        [StringLength(20, ErrorMessage = "المواد المختزلة يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(Negative|Positive|سالب|موجب)$", ErrorMessage = "المواد المختزلة يجب أن تكون: Negative أو Positive")]
        public string ReducingSubstances { get; set; } = "Negative"; // Negative, Positive
        
        [StringLength(20, ErrorMessage = "الحموضة يجب أن تكون أقل من 20 حرف")]
        public string pH { get; set; } = "7.0"; // Acidic, Neutral, Alkaline
        
        [Range(1.0, 14.0, ErrorMessage = "قيمة الحموضة يجب أن تكون بين 1.0 و 14.0")]
        public double? pHValue { get; set; } = 7.0;
        
        [StringLength(20, ErrorMessage = "الدهون يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(Negative|Positive|Normal|Increased|سالب|موجب|طبيعي|مرتفع)$", ErrorMessage = "الدهون يجب أن تكون: Negative أو Positive أو Normal أو Increased")]
        public string Fat { get; set; } = "Negative"; // Negative, Positive
        
        [StringLength(50, ErrorMessage = "محتوى الدهون يجب أن يكون أقل من 50 حرف")]
        public string FatContent { get; set; } = "Normal"; // Normal, Increased
        
        [StringLength(20, ErrorMessage = "البروتين يجب أن يكون أقل من 20 حرف")]
        [RegularExpression("^(Negative|Positive|سالب|موجب)$", ErrorMessage = "البروتين يجب أن يكون: Negative أو Positive")]
        public string Protein { get; set; } = "Negative"; // Negative, Positive
        
        // Microscopic Examination
        [Range(0, 1000, ErrorMessage = "عدد كريات الدم الحمراء يجب أن يكون بين 0 و 1000")]
        public int? RBC { get; set; } = 0; // per HPF
        
        [Range(0, 1000, ErrorMessage = "عدد كريات الدم البيضاء يجب أن يكون بين 0 و 1000")]
        public int? WBC { get; set; } = 0; // per HPF
        
        [Range(0, 1000, ErrorMessage = "عدد الخلايا الظهارية يجب أن يكون بين 0 و 1000")]
        public int? EpithelialCells { get; set; } = 0; // per HPF
        
        [Range(0, 1000, ErrorMessage = "عدد الخلايا البلعمية يجب أن يكون بين 0 و 1000")]
        public int? Macrophages { get; set; } = 0; // per HPF
        
        [Range(0, 1000, ErrorMessage = "عدد الخلايا الحمضية يجب أن يكون بين 0 و 1000")]
        public int? Eosinophils { get; set; } = 0; // per HPF
        
        // Parasites
        [StringLength(20, ErrorMessage = "الطفيليات يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|لا يوجد|موجود)$", ErrorMessage = "الطفيليات يجب أن تكون: None أو Present")]
        public string Parasites { get; set; } = "None"; // None, Present
        
        [StringLength(100, ErrorMessage = "نوع الطفيلي يجب أن يكون أقل من 100 حرف")]
        public string ParasiteType { get; set; } = ""; // Giardia, Entamoeba, etc.
        
        [Range(0, 1000, ErrorMessage = "عدد الطفيليات يجب أن يكون بين 0 و 1000")]
        public int ParasiteCount { get; set; } = 0; // per HPF
        
        [StringLength(50, ErrorMessage = "مرحلة الطفيلي يجب أن تكون أقل من 50 حرف")]
        public string ParasiteStage { get; set; } = ""; // Cyst, Trophozoite, Egg, etc.
        
        // Ova and Parasites
        [StringLength(20, ErrorMessage = "البيوض يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|لا يوجد|موجود)$", ErrorMessage = "البيوض يجب أن تكون: None أو Present")]
        public string Ova { get; set; } = "None"; // None, Present
        
        [StringLength(100, ErrorMessage = "نوع البيضة يجب أن يكون أقل من 100 حرف")]
        public string OvaType { get; set; } = ""; // Ascaris, Hookworm, etc.
        
        [Range(0, 1000, ErrorMessage = "عدد البيوض يجب أن يكون بين 0 و 1000")]
        public int OvaCount { get; set; } = 0; // per HPF
        
        // Bacteria
        [StringLength(50, ErrorMessage = "البكتيريا يجب أن تكون أقل من 50 حرف")]
        [RegularExpression("^(Normal|Abnormal|Normal Flora|طبيعي|غير طبيعي)$", ErrorMessage = "البكتيريا يجب أن تكون: Normal أو Abnormal أو Normal Flora")]
        public string Bacteria { get; set; } = "Normal"; // Normal Flora, Abnormal
        
        [StringLength(100, ErrorMessage = "نوع البكتيريا يجب أن يكون أقل من 100 حرف")]
        public string BacterialType { get; set; } = ""; // E. coli, Salmonella, etc.
        
        [Range(0, 1000000, ErrorMessage = "عدد البكتيريا يجب أن يكون بين 0 و 1000000")]
        public int? BacterialCount { get; set; } = 0; // per HPF
        
        // Yeast and Fungi
        [StringLength(20, ErrorMessage = "الخمائر يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|لا يوجد|موجود)$", ErrorMessage = "الخمائر يجب أن تكون: None أو Present")]
        public string Yeast { get; set; } = "None"; // None, Present
        
        [StringLength(100, ErrorMessage = "نوع الخميرة يجب أن يكون أقل من 100 حرف")]
        public string YeastType { get; set; } = ""; // Candida, etc.
        
        [Range(0, 1000, ErrorMessage = "عدد الخمائر يجب أن يكون بين 0 و 1000")]
        public int? YeastCount { get; set; } = 0; // per HPF
        
        // Undigested Food
        [StringLength(20, ErrorMessage = "الطعام غير المهضوم يجب أن يكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|لا يوجد|موجود)$", ErrorMessage = "الطعام غير المهضوم يجب أن يكون: None أو Present")]
        public string UndigestedFood { get; set; } = "None"; // None, Present
        
        [StringLength(100, ErrorMessage = "نوع الطعام يجب أن يكون أقل من 100 حرف")]
        public string FoodType { get; set; } = ""; // Meat, Vegetable, etc.
        
        // Fat Globules
        [StringLength(20, ErrorMessage = "كريات الدهون يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|Abundant|لا يوجد|موجود|كثير)$", ErrorMessage = "كريات الدهون يجب أن تكون: None أو Present أو Abundant")]
        public string FatGlobules { get; set; } = "None"; // None, Present
        
        [Range(0, 1000, ErrorMessage = "عدد كريات الدهون يجب أن يكون بين 0 و 1000")]
        public int? FatGlobuleCount { get; set; } = 0; // per HPF
        
        // Muscle Fibers
        [StringLength(20, ErrorMessage = "الألياف العضلية يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|لا يوجد|موجود)$", ErrorMessage = "الألياف العضلية يجب أن تكون: None أو Present")]
        public string MuscleFibers { get; set; } = "None"; // None, Present
        
        [Range(0, 1000, ErrorMessage = "عدد الألياف العضلية يجب أن يكون بين 0 و 1000")]
        public int? MuscleFiberCount { get; set; } = 0; // per HPF
        
        // Starch Granules
        [StringLength(20, ErrorMessage = "حبيبات النشا يجب أن تكون أقل من 20 حرف")]
        [RegularExpression("^(None|Present|لا يوجد|موجود)$", ErrorMessage = "حبيبات النشا يجب أن تكون: None أو Present")]
        public string StarchGranules { get; set; } = "None"; // None, Present
        
        [StringLength(20, ErrorMessage = "النشا يجب أن يكون أقل من 20 حرف")]
        public string Starch { get; set; } = "None"; // Alternative property name
        
        [Range(0, 1000, ErrorMessage = "عدد حبيبات النشا يجب أن يكون بين 0 و 1000")]
        public int? StarchGranuleCount { get; set; } = 0; // per HPF
        
        // Additional Tests
        [StringLength(20, ErrorMessage = "الكالبروتيكتين يجب أن يكون أقل من 20 حرف")]
        [RegularExpression("^(Normal|Elevated|High|طبيعي|مرتفع|عالي)$", ErrorMessage = "الكالبروتيكتين يجب أن يكون: Normal أو Elevated أو High")]
        public string Calprotectin { get; set; } = "Normal"; // Normal, Elevated
        
        [Range(0, 2000, ErrorMessage = "قيمة الكالبروتيكتين يجب أن تكون بين 0 و 2000 μg/g")]
        public double CalprotectinValue { get; set; } = 50.0; // μg/g
        
        [StringLength(20, ErrorMessage = "اللاكتوفيرين يجب أن يكون أقل من 20 حرف")]
        [RegularExpression("^(Normal|Elevated|Negative|Positive|طبيعي|مرتفع|سالب|موجب)$", ErrorMessage = "اللاكتوفيرين يجب أن يكون: Normal أو Elevated أو Negative أو Positive")]
        public string Lactoferrin { get; set; } = "Negative"; // Normal, Elevated
        
        [Range(0, 500, ErrorMessage = "قيمة اللاكتوفيرين يجب أن تكون بين 0 و 500 μg/g")]
        public double? LactoferrinValue { get; set; } = 0.0; // μg/g
        
        [StringLength(50, ErrorMessage = "ألفا-1 أنتي تريبسين يجب أن يكون أقل من 50 حرف")]
        public string Alpha1Antitrypsin { get; set; } = "Normal";
        
        [StringLength(50, ErrorMessage = "طريقة الجمع يجب أن تكون أقل من 50 حرف")]
        public string CollectionMethod { get; set; } = "Spontaneous";
        
        [StringLength(200, ErrorMessage = "تحضير المريض يجب أن يكون أقل من 200 حرف")]
        public string PatientPreparation { get; set; } = "Normal diet";
        
        [Required(ErrorMessage = "تاريخ الفحص مطلوب")]
        public DateTime TestDate { get; set; } = DateTime.Now;
        
        // Culture and Sensitivity
        [StringLength(100, ErrorMessage = "نتيجة الزراعة يجب أن تكون أقل من 100 حرف")]
        public string CultureResult { get; set; } = "No Growth"; // No Growth, Mixed Flora, etc.
        
        [StringLength(100, ErrorMessage = "الكائن الممرض يجب أن يكون أقل من 100 حرف")]
        public string PathogenicOrganism { get; set; } = "";
        
        [StringLength(200, ErrorMessage = "الحساسية يجب أن تكون أقل من 200 حرف")]
        public string Sensitivity { get; set; } = ""; // Sensitive, Resistant, Intermediate
        
        // Quality Control
        public bool IsQualityControlPassed { get; set; } = false;
        
        [StringLength(500, ErrorMessage = "ملاحظات مراقبة الجودة يجب أن تكون أقل من 500 حرف")]
        public string QualityControlNotes { get; set; } = "";
        
        // Results Interpretation
        [StringLength(50, ErrorMessage = "التفسير يجب أن يكون أقل من 50 حرف")]
        [RegularExpression("^(Normal|Abnormal|Critical|Borderline|طبيعي|غير طبيعي|حرج|حدي)$", ErrorMessage = "التفسير يجب أن يكون: Normal أو Abnormal أو Critical أو Borderline")]
        public string Interpretation { get; set; } = "Normal"; // Normal, Abnormal, Borderline
        
        [StringLength(50, ErrorMessage = "الحالة يجب أن تكون أقل من 50 حرف")]
        public string Status { get; set; } = "Normal"; // For compatibility with analyzer
        
        [StringLength(1000, ErrorMessage = "الأهمية السريرية يجب أن تكون أقل من 1000 حرف")]
        public string ClinicalSignificance { get; set; } = "";
        
        [StringLength(1000, ErrorMessage = "التوصيات يجب أن تكون أقل من 1000 حرف")]
        public string Recommendations { get; set; } = "";
        
        // Analysis Details
        [Required(ErrorMessage = "تاريخ التحليل مطلوب")]
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        
        [StringLength(100, ErrorMessage = "اسم المحلل يجب أن يكون أقل من 100 حرف")]
        public string AnalyzedBy { get; set; } = "";
        
        [StringLength(100, ErrorMessage = "اسم المراجع يجب أن يكون أقل من 100 حرف")]
        public string VerifiedBy { get; set; } = "";
        
        public DateTime? VerificationDate { get; set; }
        
        [StringLength(1000, ErrorMessage = "التعليقات يجب أن تكون أقل من 1000 حرف")]
        public string Comments { get; set; } = "";
        
        [StringLength(1000, ErrorMessage = "الملاحظات يجب أن تكون أقل من 1000 حرف")]
        public string Notes { get; set; } = ""; // For compatibility with analyzer
        
        [Required(ErrorMessage = "تاريخ الإنشاء مطلوب")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? ModifiedDate { get; set; }
        
        // Navigation Properties
        public virtual Exam Exam { get; set; }
        
        /// <summary>
        /// التحقق من صحة النتائج وتحديد الحالة العامة
        /// </summary>
        public void ValidateResults()
        {
            var criticalConditions = new List<string>();
            var abnormalConditions = new List<string>();
            
            // فحص القيم الحرجة
            if (Color == "أسود" || Color == "Black")
                criticalConditions.Add("لون أسود قد يشير إلى نزيف في الجهاز الهضمي العلوي");
                
            if (Color == "أحمر" || Color == "Red")
                criticalConditions.Add("لون أحمر قد يشير إلى نزيف في الجهاز الهضمي السفلي");
                
            if (Color == "أبيض" || Color == "White")
                criticalConditions.Add("لون أبيض قد يشير إلى مشاكل في الكبد أو المرارة");
                
            if (OccultBlood == "Positive")
                criticalConditions.Add("دم خفي إيجابي");
                
            if (CalprotectinValue > 200)
                criticalConditions.Add($"كالبروتيكتين مرتفع: {CalprotectinValue:F1} μg/g");
                
            if (Parasites == "Present")
                criticalConditions.Add($"طفيليات موجودة: {ParasiteType}");
                
            if (Ova == "Present")
                criticalConditions.Add($"بيوض موجودة: {OvaType}");
            
            // فحص الحالات غير الطبيعية
            if (Consistency == "مائي" || Consistency == "Watery")
                abnormalConditions.Add("قوام مائي");
                
            if (FatGlobules == "Present" || FatGlobules == "Abundant")
                abnormalConditions.Add("كريات دهون موجودة");
                
            if (Bacteria == "Abnormal")
                abnormalConditions.Add("بكتيريا غير طبيعية");
                
            if (Yeast == "Present")
                abnormalConditions.Add("خمائر موجودة");
                
            if (WBC > 5)
                abnormalConditions.Add($"كريات دم بيضاء مرتفعة: {WBC}");
                
            if (RBC > 3)
                abnormalConditions.Add($"كريات دم حمراء مرتفعة: {RBC}");
                
            if (CalprotectinValue > 50 && CalprotectinValue <= 200)
                abnormalConditions.Add($"كالبروتيكتين مرتفع قليلاً: {CalprotectinValue:F1} μg/g");
            
            // تحديد الحالة العامة
            if (criticalConditions.Any())
            {
                Status = "Critical";
                Interpretation = "Critical";
                Notes = $"حالة حرجة: {string.Join("; ", criticalConditions)}";
            }
            else if (abnormalConditions.Any())
            {
                Status = "Abnormal";
                Interpretation = "Abnormal";
                Notes = $"حالة غير طبيعية: {string.Join("; ", abnormalConditions)}";
            }
            else
            {
                Status = "Normal";
                Interpretation = "Normal";
                Notes = "نتائج طبيعية";
            }
        }
        
        /// <summary>
        /// فحص وجود عدوى طفيلية
        /// </summary>
        public bool HasParasiticInfection()
        {
            return Parasites == "Present" || Ova == "Present" || 
                   ParasiteCount > 0 || OvaCount > 0;
        }
        
        /// <summary>
        /// فحص وجود نزيف في الجهاز الهضمي
        /// </summary>
        public bool HasGastrointestinalBleeding()
        {
            return OccultBlood == "Positive" || 
                   Blood == "Visible" || Blood == "Present" ||
                   Color == "أسود" || Color == "Black" ||
                   Color == "أحمر" || Color == "Red";
        }
        
        /// <summary>
        /// فحص وجود مرض التهاب الأمعاء
        /// </summary>
        public bool HasInflammatoryBowelDisease()
        {
            return CalprotectinValue > 100 || 
                   Lactoferrin == "Positive" || Lactoferrin == "Elevated" ||
                   (WBC != null && WBC > 10) ||
                   Mucus == "Present" || Mucus == "Abundant";
        }
        
        /// <summary>
        /// فحص وجود سوء امتصاص
        /// </summary>
        public bool HasMalabsorption()
        {
            return FatContent == "Increased" || 
                   Fat == "Positive" ||
                   FatGlobules == "Present" || FatGlobules == "Abundant" ||
                   ReducingSubstances == "Positive" ||
                   UndigestedFood == "Present";
        }
        
        /// <summary>
        /// فحص وجود إسهال
        /// </summary>
        public bool HasDiarrhea()
        {
            return Consistency == "مائي" || Consistency == "Watery" ||
                   Consistency == "سائل" || Consistency == "Liquid" ||
                   (Quantity != null && Quantity > 200) ||
                   (Weight > 200);
        }
    }
}