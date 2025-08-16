using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.Models
{
    public class LipidProfileTestResult
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ExamId { get; set; }
        
        // Basic Lipid Profile
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 1000, ErrorMessage = "قيمة الكولسترول الكلي يجب أن تكون بين 0 و 1000 mg/dL")]
        public decimal? TotalCholesterol { get; set; } // mg/dL
        
        [StringLength(20, ErrorMessage = "حالة الكولسترول الكلي يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Optimal|Borderline|High|Very High|مثالي|حدي|عالي|عالي جداً)$", ErrorMessage = "حالة الكولسترول الكلي غير صحيحة")]
        public string TotalCholesterolStatus { get; set; } = "Normal"; // Optimal, Borderline, High, Very High
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 200, ErrorMessage = "قيمة الكولسترول الجيد يجب أن تكون بين 0 و 200 mg/dL")]
        public decimal? HDL { get; set; } // mg/dL
        
        [StringLength(20, ErrorMessage = "حالة الكولسترول الجيد يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Low|Normal|High|منخفض|طبيعي|عالي)$", ErrorMessage = "حالة الكولسترول الجيد غير صحيحة")]
        public string HDLStatus { get; set; } = "Normal"; // Low, Normal, High
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 500, ErrorMessage = "قيمة الكولسترول الضار يجب أن تكون بين 0 و 500 mg/dL")]
        public decimal? LDL { get; set; } // mg/dL
        
        [StringLength(20, ErrorMessage = "حالة الكولسترول الضار يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Optimal|Near Optimal|Borderline|High|Very High|مثالي|قريب من المثالي|حدي|عالي|عالي جداً)$", ErrorMessage = "حالة الكولسترول الضار غير صحيحة")]
        public string LDLStatus { get; set; } = "Normal"; // Optimal, Near Optimal, Borderline, High, Very High
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 2000, ErrorMessage = "قيمة الدهون الثلاثية يجب أن تكون بين 0 و 2000 mg/dL")]
        public decimal? Triglycerides { get; set; } // mg/dL
        
        [StringLength(20, ErrorMessage = "حالة الدهون الثلاثية يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Normal|Borderline|High|Very High|طبيعي|حدي|عالي|عالي جداً)$", ErrorMessage = "حالة الدهون الثلاثية غير صحيحة")]
        public string TriglyceridesStatus { get; set; } = "Normal"; // Normal, Borderline, High, Very High
        
        // Calculated Values
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 20, ErrorMessage = "نسبة الكولسترول الكلي/الجيد يجب أن تكون بين 0 و 20")]
        public decimal? TotalCholesterolHDL { get; set; } // Ratio
        
        [StringLength(20, ErrorMessage = "حالة نسبة الكولسترول يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Normal|High|طبيعي|عالي)$", ErrorMessage = "حالة نسبة الكولسترول غير صحيحة")]
        public string TotalCholesterolHDLStatus { get; set; } = "Normal"; // Normal, High
        
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 15, ErrorMessage = "نسبة الكولسترول الضار/الجيد يجب أن تكون بين 0 و 15")]
        public decimal? LDLHDL { get; set; } // Ratio
        
        [StringLength(20, ErrorMessage = "حالة نسبة الكولسترول الضار/الجيد يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Normal|High|طبيعي|عالي)$", ErrorMessage = "حالة نسبة الكولسترول الضار/الجيد غير صحيحة")]
        public string LDLHDLStatus { get; set; } = "Normal"; // Normal, High
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 800, ErrorMessage = "قيمة الكولسترول غير الجيد يجب أن تكون بين 0 و 800 mg/dL")]
        public decimal? NonHDLCholesterol { get; set; } // mg/dL
        
        [StringLength(20, ErrorMessage = "حالة الكولسترول غير الجيد يجب ألا تتجاوز 20 حرف")]
        [RegularExpression("^(Normal|High|طبيعي|عالي)$", ErrorMessage = "حالة الكولسترول غير الجيد غير صحيحة")]
        public string NonHDLCholesterolStatus { get; set; } = "Normal"; // Normal, High
        
        // Additional Tests
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 100, ErrorMessage = "قيمة VLDL يجب أن تكون بين 0 و 100 mg/dL")]
        public decimal? VLDL { get; set; } // mg/dL
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 300, ErrorMessage = "قيمة Apolipoprotein A1 يجب أن تكون بين 0 و 300 mg/dL")]
        public decimal? ApolipoproteinA1 { get; set; } // mg/dL
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 300, ErrorMessage = "قيمة Apolipoprotein B يجب أن تكون بين 0 و 300 mg/dL")]
        public decimal? ApolipoproteinB { get; set; } // mg/dL
        
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 5, ErrorMessage = "نسبة Apolipoprotein B/A1 يجب أن تكون بين 0 و 5")]
        public decimal? ApolipoproteinBA1 { get; set; } // Ratio
        
        [Column(TypeName = "decimal(8,2)")]
        [Range(0, 200, ErrorMessage = "قيمة Lipoprotein A يجب أن تكون بين 0 و 200 mg/dL")]
        public decimal? LipoproteinA { get; set; } // mg/dL
        
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 1000, ErrorMessage = "قيمة Lp-PLA2 يجب أن تكون بين 0 و 1000 ng/mL")]
        public decimal? LpPLA2 { get; set; } // ng/mL
        
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
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; } = null!;
        
        // Methods
        public void AnalyzeResults(IMedicalReferenceService referenceService, Patient patient)
        {
            if (referenceService == null || patient == null) return;

            // Analyze Total Cholesterol
            if (TotalCholesterol.HasValue)
            {
                var tcRef = referenceService.GetTotalCholesterolReference(patient.Age, patient.Gender);
                TotalCholesterolStatus = referenceService.GetLipidStatus(TotalCholesterol.Value, tcRef.optimal, tcRef.borderline, tcRef.high, tcRef.veryHigh);
            }

            // Analyze HDL Cholesterol
            if (HDL.HasValue)
            {
                var hdlRef = referenceService.GetHDLReference(patient.Age, patient.Gender);
                HDLStatus = referenceService.GetLipidStatus(HDL.Value, hdlRef.low, hdlRef.normal, hdlRef.high);
            }

            // Analyze LDL Cholesterol
            if (LDL.HasValue)
            {
                var ldlRef = referenceService.GetLDLReference(patient.Age, patient.Gender);
                LDLStatus = referenceService.GetLipidStatus(LDL.Value, ldlRef.optimal, ldlRef.nearOptimal, ldlRef.borderline, ldlRef.high, ldlRef.veryHigh);
            }

            // Analyze Triglycerides
            if (Triglycerides.HasValue)
            {
                var trigRef = referenceService.GetTriglyceridesReference(patient.Age, patient.Gender);
                TriglyceridesStatus = referenceService.GetLipidStatus(Triglycerides.Value, trigRef.normal, trigRef.borderline, trigRef.high, trigRef.veryHigh);
            }

            // Calculate ratios
            if (TotalCholesterol.HasValue && HDL.HasValue && HDL.Value > 0)
            {
                TotalCholesterolHDL = TotalCholesterol.Value / HDL.Value;
                TotalCholesterolHDLStatus = TotalCholesterolHDL.Value > 5.0m ? "High" : "Normal";
            }

            if (LDL.HasValue && HDL.HasValue && HDL.Value > 0)
            {
                LDLHDL = LDL.Value / HDL.Value;
                LDLHDLStatus = LDLHDL.Value > 3.5m ? "High" : "Normal";
            }

            // Calculate Non-HDL Cholesterol
            if (TotalCholesterol.HasValue && HDL.HasValue)
            {
                NonHDLCholesterol = TotalCholesterol.Value - HDL.Value;
                NonHDLCholesterolStatus = NonHDLCholesterol.Value > 160m ? "High" : "Normal";
            }

            // Calculate VLDL (if not directly measured)
            if (!VLDL.HasValue && Triglycerides.HasValue)
            {
                VLDL = Triglycerides.Value / 5; // Friedewald formula approximation
            }

            // Determine overall interpretation
            var abnormalParameters = new[] { TotalCholesterolStatus, HDLStatus, LDLStatus, TriglyceridesStatus }
                .Count(s => s != "Normal" && s != "Optimal");

            var criticalParameters = new[] { TotalCholesterolStatus, LDLStatus, TriglyceridesStatus }
                .Count(s => s == "Very High") + (HDLStatus == "Low" ? 1 : 0);

            Interpretation = criticalParameters > 0 ? "Critical" : abnormalParameters switch
            {
                0 => "Normal",
                1 => "Borderline",
                _ => "Abnormal"
            };

            // Generate clinical significance and recommendations
            GenerateClinicalSignificanceAndRecommendations();
        }

        private void GenerateClinicalSignificanceAndRecommendations()
        {
            var significance = new List<string>();
            var recommendations = new List<string>();

            // Assess cardiovascular risk based on lipid profile
            if (TotalCholesterolStatus == "High" || TotalCholesterolStatus == "Very High")
            {
                significance.Add("زيادة خطر الإصابة بأمراض القلب والأوعية الدموية");
                recommendations.Add("تعديل النظام الغذائي وزيادة النشاط البدني");
            }

            if (LDLStatus == "High" || LDLStatus == "Very High")
            {
                significance.Add("ارتفاع الكولسترول الضار يزيد من خطر تصلب الشرايين");
                recommendations.Add("النظر في العلاج الدوائي (الستاتينات) بعد استشارة الطبيب");
            }

            if (HDLStatus == "Low")
            {
                significance.Add("انخفاض الكولسترول الجيد يقلل من الحماية الطبيعية للقلب");
                recommendations.Add("ممارسة الرياضة المنتظمة والإقلاع عن التدخين");
            }

            if (TriglyceridesStatus == "High" || TriglyceridesStatus == "Very High")
            {
                significance.Add("ارتفاع الدهون الثلاثية يزيد من خطر التهاب البنكرياس وأمراض القلب");
                recommendations.Add("تقليل السكريات والكربوهيدرات المكررة في النظام الغذائي");
            }

            if (TotalCholesterolHDL.HasValue && TotalCholesterolHDL.Value > 5.0m)
            {
                significance.Add("نسبة الكولسترول الكلي/الجيد مرتفعة وتشير إلى زيادة خطر القلب");
            }

            if (Interpretation == "Normal")
            {
                significance.Add("مستويات الدهون في الدم ضمن المعدل الطبيعي");
                recommendations.Add("الحفاظ على نمط حياة صحي وفحص دوري كل سنتين");
            }

            ClinicalSignificance = significance.Any() ? string.Join(". ", significance) + "." : string.Empty;
            Recommendations = recommendations.Any() ? string.Join(". ", recommendations) + "." : string.Empty;
        }

        public override string ToString()
        {
            return $"Lipid Profile - {Exam?.Patient?.FullName} - {AnalysisDate:dd/MM/yyyy}";
        }
    }
}