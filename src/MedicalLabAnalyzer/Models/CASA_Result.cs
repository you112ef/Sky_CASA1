using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedicalLabAnalyzer.Common.Validation;

namespace MedicalLabAnalyzer.Models
{
    /// <summary>
    /// Results of Computer-Aided Sperm Analysis (CASA) following WHO 2021 guidelines
    /// </summary>
    public class CASA_Result
    {
        // WHO 2021 Basic Semen Parameters
        
        /// <summary>
        /// Sample volume in mL (WHO 2021: ≥1.4 mL)
        /// </summary>
        [WHO2021Parameter("الحجم - Volume", 1.4, 8.0)]
        [Display(Name = "الحجم (مل) - Volume (mL)")]
        public double Volume { get; set; }
        
        /// <summary>
        /// Total sperm concentration (million/mL) (WHO 2021: ≥16 million/mL)
        /// </summary>
        [WHO2021Parameter("التركيز - Concentration", 16.0, 300.0)]
        [Display(Name = "التركيز (مليون/مل) - Concentration (million/mL)")]
        public double Concentration { get; set; }
        
        /// <summary>
        /// Total sperm count in ejaculate (million) (WHO 2021: ≥39 million total)
        /// </summary>
        [WHO2021Parameter("العدد الكلي - Total Count", 39.0, 1200.0)]
        [Display(Name = "العدد الكلي (مليون) - Total Count (million)")]
        public double TotalCount { get; set; }
        
        /// <summary>
        /// Total motility percentage (WHO 2021: ≥42%)
        /// </summary>
        [WHO2021Parameter("الحركة الكلية - Total Motility", 42.0, 100.0)]
        [Display(Name = "الحركة الكلية (%) - Total Motility (%)")]
        public double TotalMotility { get; set; }
        
        /// <summary>
        /// Progressive motility percentage (WHO 2021: ≥30%)
        /// </summary>
        [WHO2021Parameter("الحركة التقدمية - Progressive Motility", 30.0, 100.0)]
        [Display(Name = "الحركة التقدمية (%) - Progressive Motility (%)")]
        public double ProgressiveMotility { get; set; }
        
        /// <summary>
        /// Non-progressive motility percentage (WHO 2021: typical 30%)
        /// </summary>
        [Percentage]
        [Display(Name = "الحركة غير التقدمية (%) - Non-Progressive Motility (%)")]
        public double NonProgressiveMotility { get; set; }
        
        /// <summary>
        /// Immotile sperm percentage
        /// </summary>
        [Percentage]
        [Display(Name = "النسبة غير الحركية (%) - Immotile Percentage (%)")]
        public double ImmotilePercentage { get; set; }
        
        /// <summary>
        /// Normal morphology percentage using Tyberberg method (WHO 2021: ≥4%)
        /// </summary>
        [WHO2021Parameter("الشكل الطبيعي - Normal Morphology", 4.0, 100.0)]
        [Display(Name = "الشكل الطبيعي (%) - Normal Morphology (%)")]
        public double NormalMorphology { get; set; }
        
        /// <summary>
        /// Sperm vitality percentage (WHO 2021: ≥54%)
        /// </summary>
        [WHO2021Parameter("الحيوية - Vitality", 54.0, 100.0)]
        [Display(Name = "الحيوية (%) - Vitality (%)")]
        public double Vitality { get; set; }
        
        /// <summary>
        /// Sample pH (WHO 2021: typically 7.2)
        /// </summary>
        [BiologicalPH(7.0, 8.5)]
        [Display(Name = "الأس الهيدروجيني - pH")]
        public double pH { get; set; }
        
        /// <summary>
        /// Seminal fructose concentration (WHO 2021: ≥13 μmol/ejaculate)
        /// </summary>
        [WHO2021Parameter("الفركتوز - Fructose", 13.0, 100.0)]
        [Display(Name = "الفركتوز (μmol/ejaculate) - Fructose (μmol/ejaculate)")]
        public double Fructose { get; set; }
        
        /// <summary>
        /// Sample appearance and color
        /// </summary>
        [StringLength(100)]
        [Display(Name = "المظهر - Appearance")]
        public string Appearance { get; set; }
        
        /// <summary>
        /// Sample viscosity assessment
        /// </summary>
        [StringLength(50)]
        [Display(Name = "اللزوجة - Viscosity")]
        public string Viscosity { get; set; }
        
        /// <summary>
        /// White blood cell count (WHO 2021: <1,000,000/mL)
        /// </summary>
        [WHO2021Parameter("كريات الدم البيضاء - WBC Count", 0.0, 1.0)]
        [Display(Name = "كريات الدم البيضاء (مليون/مل) - WBC Count (million/mL)")]
        public double WBCCount { get; set; }
        
        /// <summary>
        /// Presence of agglutination
        /// </summary>
        [Display(Name = "وجود تلازن - Has Agglutination")]
        public bool HasAgglutination { get; set; }
        
        /// <summary>
        /// WHO 2021 classification result
        /// </summary>
        [Display(Name = "التصنيف - Classification")]
        public WHOClassification Classification { get; set; }
        
        // CASA Kinematic Parameters
        /// <summary>
        /// Curvilinear Velocity (µm/s) - Total path length / time
        /// </summary>
        public double VCL { get; set; }
        
        /// <summary>
        /// Straight Line Velocity (µm/s) - Straight line distance / time
        /// </summary>
        public double VSL { get; set; }
        
        /// <summary>
        /// Average Path Velocity (µm/s) - Smoothed path length / time
        /// </summary>
        public double VAP { get; set; }
        
        /// <summary>
        /// Amplitude of Lateral Head Displacement (µm)
        /// </summary>
        public double ALH { get; set; }
        
        /// <summary>
        /// Beat Cross Frequency (Hz) - Frequency of crossing the average path
        /// </summary>
        public double BCF { get; set; }
        
        /// <summary>
        /// Linearity (VSL/VCL) - Straightness of movement
        /// </summary>
        public double LIN { get; set; }
        
        /// <summary>
        /// Straightness (VSL/VAP) - How straight the average path is
        /// </summary>
        public double STR { get; set; }
        
        /// <summary>
        /// Wobble (VAP/VCL) - Regularity of the average path
        /// </summary>
        public double WOB { get; set; }
        
        /// <summary>
        /// Number of tracks analyzed
        /// </summary>
        public int TrackCount { get; set; }
        
        /// <summary>
        /// Total analysis duration (seconds)
        /// </summary>
        public double AnalysisDuration { get; set; }
        
        /// <summary>
        /// Analysis timestamp
        /// </summary>
        [Required]
        [Display(Name = "وقت التحليل - Analysis Time")]
        public DateTime AnalysisTime { get; set; }
        
        /// <summary>
        /// Video file path used for analysis
        /// </summary>
        [StringLength(500)]
        [Display(Name = "مسار الفيديو - Video Path")]
        public string VideoPath { get; set; }
        
        /// <summary>
        /// Calibration used (microns per pixel)
        /// </summary>
        public double MicronsPerPixel { get; set; }
        
        /// <summary>
        /// Video frame rate (FPS)
        /// </summary>
        public double FPS { get; set; }
        
        /// <summary>
        /// Individual track results
        /// </summary>
        public List<TrackResult> TrackResults { get; set; } = new List<TrackResult>();
        
        /// <summary>
        /// Analysis parameters used
        /// </summary>
        public AnalysisParameters Parameters { get; set; }
        
        /// <summary>
        /// Quality metrics
        /// </summary>
        public QualityMetrics Quality { get; set; }

        public CASA_Result()
        {
            AnalysisTime = DateTime.Now;
            Parameters = new AnalysisParameters();
            Quality = new QualityMetrics();
        }

        /// <summary>
        /// Calculate derived parameters
        /// </summary>
        public void CalculateDerivedParameters()
        {
            if (VCL > 0)
            {
                LIN = VSL / VCL;
                WOB = VAP / VCL;
            }
            
            if (VAP > 0)
            {
                STR = VSL / VAP;
            }
        }

        /// <summary>
        /// Validate against WHO 2021 reference values
        /// </summary>
        /// <returns>Validation result with details</returns>
        public WHO2021ValidationResult ValidateAgainstWHO2021()
        {
            var result = new WHO2021ValidationResult();
            
            // WHO 2021 5th percentile reference values
            result.IsVolumeNormal = Volume >= 1.4;
            result.IsConcentrationNormal = Concentration >= 16.0; // million/mL
            result.IsTotalCountNormal = TotalCount >= 39.0; // million total
            result.IsTotalMotilityNormal = TotalMotility >= 42.0;
            result.IsProgressiveMotilityNormal = ProgressiveMotility >= 30.0;
            result.IsMorphologyNormal = NormalMorphology >= 4.0;
            result.IsVitalityNormal = Vitality >= 54.0;
            result.IspHNormal = pH >= 7.0 && pH <= 8.0; // Normal physiological range
            result.IsFructoseNormal = Fructose >= 13.0; // μmol/ejaculate
            result.IsWBCNormal = WBCCount < 1000000; // <1 million/mL
            
            // Overall assessment
            result.IsNormalOverall = result.IsVolumeNormal && 
                                   result.IsConcentrationNormal && 
                                   result.IsTotalCountNormal && 
                                   result.IsTotalMotilityNormal && 
                                   result.IsProgressiveMotilityNormal && 
                                   result.IsMorphologyNormal && 
                                   result.IsVitalityNormal;
            
            // Classification based on WHO 2021 criteria
            if (result.IsNormalOverall)
            {
                Classification = WHOClassification.Normal;
            }
            else if (!result.IsConcentrationNormal || !result.IsTotalCountNormal)
            {
                Classification = WHOClassification.Oligozoospermia;
            }
            else if (!result.IsTotalMotilityNormal || !result.IsProgressiveMotilityNormal)
            {
                Classification = WHOClassification.Asthenozoospermia;
            }
            else if (!result.IsMorphologyNormal)
            {
                Classification = WHOClassification.Teratozoospermia;
            }
            else
            {
                Classification = WHOClassification.OtherAbnormality;
            }
            
            return result;
        }
        
        /// <summary>
        /// Get comprehensive analysis report following WHO 2021 standards
        /// </summary>
        /// <returns>Formatted analysis report</returns>
        public string GetWHO2021Report()
        {
            var validation = ValidateAgainstWHO2021();
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== تقرير تحليل الحيوانات المنوية (WHO 2021) ===");
            report.AppendLine($"تاريخ التحليل: {AnalysisTime:dd/MM/yyyy HH:mm}");
            report.AppendLine();
            
            report.AppendLine("المعايير الأساسية:");
            report.AppendLine($"الحجم: {Volume:F1} مل {(validation.IsVolumeNormal ? "✓" : "✗")} (المرجع: ≥1.4 مل)");
            report.AppendLine($"التركيز: {Concentration:F1} مليون/مل {(validation.IsConcentrationNormal ? "✓" : "✗")} (المرجع: ≥16 مليون/مل)");
            report.AppendLine($"العدد الكلي: {TotalCount:F1} مليون {(validation.IsTotalCountNormal ? "✓" : "✗")} (المرجع: ≥39 مليون)");
            report.AppendLine($"الحركة الكلية: {TotalMotility:F1}% {(validation.IsTotalMotilityNormal ? "✓" : "✗")} (المرجع: ≥42%)");
            report.AppendLine($"الحركة التقدمية: {ProgressiveMotility:F1}% {(validation.IsProgressiveMotilityNormal ? "✓" : "✗")} (المرجع: ≥30%)");
            report.AppendLine($"الشكل الطبيعي: {NormalMorphology:F1}% {(validation.IsMorphologyNormal ? "✓" : "✗")} (المرجع: ≥4%)");
            report.AppendLine($"الحيوية: {Vitality:F1}% {(validation.IsVitalityNormal ? "✓" : "✗")} (المرجع: ≥54%)");
            report.AppendLine();
            
            if (TrackCount > 0)
            {
                report.AppendLine("المعايير الحركية (CASA):");
                report.AppendLine($"السرعة المنحنية (VCL): {VCL:F1} μm/s");
                report.AppendLine($"السرعة المستقيمة (VSL): {VSL:F1} μm/s");
                report.AppendLine($"السرعة المتوسطة (VAP): {VAP:F1} μm/s");
                report.AppendLine($"الخطية (LIN): {(LIN * 100):F1}%");
                report.AppendLine($"الاستقامة (STR): {(STR * 100):F1}%");
                report.AppendLine($"التذبذب (WOB): {(WOB * 100):F1}%");
                report.AppendLine($"عدد المسارات المحللة: {TrackCount}");
                report.AppendLine();
            }
            
            report.AppendLine($"التصنيف النهائي: {GetClassificationArabic(Classification)}");
            report.AppendLine($"التقييم العام: {(validation.IsNormalOverall ? "طبيعي" : "يحتاج لتقييم طبي")}");
            
            return report.ToString();
        }
        
        private string GetClassificationArabic(WHOClassification classification)
        {
            return classification switch
            {
                WHOClassification.Normal => "طبيعي",
                WHOClassification.Oligozoospermia => "قلة الحيوانات المنوية",
                WHOClassification.Asthenozoospermia => "ضعف حركة الحيوانات المنوية",
                WHOClassification.Teratozoospermia => "تشوه الحيوانات المنوية",
                WHOClassification.OtherAbnormality => "خلل آخر",
                _ => "غير محدد"
            };
        }
        
        /// <summary>
        /// Get summary statistics including WHO 2021 parameters
        /// </summary>
        /// <returns>Dictionary with summary statistics</returns>
        public Dictionary<string, object> GetSummary()
        {
            return new Dictionary<string, object>
            {
                // WHO 2021 Basic Parameters
                ["Volume"] = Volume,
                ["Concentration"] = Concentration,
                ["TotalCount"] = TotalCount,
                ["TotalMotility"] = TotalMotility,
                ["ProgressiveMotility"] = ProgressiveMotility,
                ["NormalMorphology"] = NormalMorphology,
                ["Vitality"] = Vitality,
                ["pH"] = pH,
                ["Classification"] = Classification.ToString(),
                
                // CASA Kinematic Parameters
                ["VCL"] = VCL,
                ["VSL"] = VSL,
                ["VAP"] = VAP,
                ["ALH"] = ALH,
                ["BCF"] = BCF,
                ["LIN"] = LIN,
                ["STR"] = STR,
                ["WOB"] = WOB,
                
                // Analysis metadata
                ["TrackCount"] = TrackCount,
                ["AnalysisDuration"] = AnalysisDuration,
                ["AnalysisTime"] = AnalysisTime,
                ["VideoPath"] = VideoPath,
                ["MicronsPerPixel"] = MicronsPerPixel,
                ["FPS"] = FPS
            };
        }
    }

    /// <summary>
    /// Individual track analysis result
    /// </summary>
    public class TrackResult
    {
        public int TrackId { get; set; }
        public double VCL { get; set; }
        public double VSL { get; set; }
        public double VAP { get; set; }
        public double ALH { get; set; }
        public double BCF { get; set; }
        public double Duration { get; set; }
        public int PointCount { get; set; }
        public double QualityScore { get; set; }
        public List<TrackPoint> Points { get; set; } = new List<TrackPoint>();
    }

    /// <summary>
    /// Analysis parameters used for CASA
    /// </summary>
    public class AnalysisParameters
    {
        public double MinBlobAreaPx { get; set; } = 6.0;
        public double MaxMatchDistancePx { get; set; } = 60.0;
        public int MaxMissedFrames { get; set; } = 8;
        public double MinTrackDuration { get; set; } = 0.5;
        public int MinTrackPoints { get; set; } = 3;
        public double SmoothingWindow { get; set; } = 5.0;
        public double BackgroundSubtractionHistory { get; set; } = 300;
        public double BackgroundSubtractionThreshold { get; set; } = 16;
        public double GaussianBlurSigma { get; set; } = 1.2;
        public int MorphologyKernelSize { get; set; } = 3;
    }

    /// <summary>
    /// Quality metrics for the analysis
    /// </summary>
    public class QualityMetrics
    {
        public double AverageTrackQuality { get; set; }
        public double TrackConsistency { get; set; }
        public double DetectionConfidence { get; set; }
        public int TotalDetections { get; set; }
        public int ValidDetections { get; set; }
        public double DetectionRate { get; set; }
        public double AverageTrackDuration { get; set; }
        public double AverageTrackLength { get; set; }
    }
    
    /// <summary>
    /// WHO 2021 classification categories for semen analysis
    /// </summary>
    public enum WHOClassification
    {
        Normal,
        Oligozoospermia,     // Low sperm concentration
        Asthenozoospermia,   // Poor sperm motility
        Teratozoospermia,    // Abnormal sperm morphology
        OtherAbnormality,    // Other parameters abnormal
        Unknown
    }
    
    /// <summary>
    /// Validation result against WHO 2021 reference values
    /// </summary>
    public class WHO2021ValidationResult
    {
        public bool IsVolumeNormal { get; set; }
        public bool IsConcentrationNormal { get; set; }
        public bool IsTotalCountNormal { get; set; }
        public bool IsTotalMotilityNormal { get; set; }
        public bool IsProgressiveMotilityNormal { get; set; }
        public bool IsMorphologyNormal { get; set; }
        public bool IsVitalityNormal { get; set; }
        public bool IspHNormal { get; set; }
        public bool IsFructoseNormal { get; set; }
        public bool IsWBCNormal { get; set; }
        public bool IsNormalOverall { get; set; }
        
        /// <summary>
        /// Get list of abnormal parameters
        /// </summary>
        /// <returns>List of parameter names that are below reference values</returns>
        public List<string> GetAbnormalParameters()
        {
            var abnormal = new List<string>();
            
            if (!IsVolumeNormal) abnormal.Add("الحجم");
            if (!IsConcentrationNormal) abnormal.Add("التركيز");
            if (!IsTotalCountNormal) abnormal.Add("العدد الكلي");
            if (!IsTotalMotilityNormal) abnormal.Add("الحركة الكلية");
            if (!IsProgressiveMotilityNormal) abnormal.Add("الحركة التقدمية");
            if (!IsMorphologyNormal) abnormal.Add("الشكل الطبيعي");
            if (!IsVitalityNormal) abnormal.Add("الحيوية");
            if (!IspHNormal) abnormal.Add("درجة الحموضة");
            if (!IsFructoseNormal) abnormal.Add("الفركتوز");
            if (!IsWBCNormal) abnormal.Add("كريات الدم البيضاء");
            
            return abnormal;
        }
        
        /// <summary>
        /// Get recommendations based on abnormal parameters
        /// </summary>
        /// <returns>Clinical recommendations in Arabic</returns>
        public string GetClinicalRecommendations()
        {
            if (IsNormalOverall)
            {
                return "جميع المعايير ضمن الحدود الطبيعية حسب معايير WHO 2021";
            }
            
            var recommendations = new System.Text.StringBuilder();
            recommendations.AppendLine("توصيات سريرية:");
            
            if (!IsConcentrationNormal || !IsTotalCountNormal)
            {
                recommendations.AppendLine("• يُنصح بإجراء تحليل إضافي بعد 2-3 أشهر للتأكد من النتائج");
                recommendations.AppendLine("• قد يكون هناك حاجة لتقييم هرموني شامل");
            }
            
            if (!IsTotalMotilityNormal || !IsProgressiveMotilityNormal)
            {
                recommendations.AppendLine("• فحص وجود التهابات في الجهاز التناسلي");
                recommendations.AppendLine("• تقييم العوامل البيئية والغذائية");
            }
            
            if (!IsMorphologyNormal)
            {
                recommendations.AppendLine("• تقييم العوامل الوراثية والبيئية");
                recommendations.AppendLine("• فحص وجود دوالي الخصية");
            }
            
            if (!IsVitalityNormal)
            {
                recommendations.AppendLine("• فحص شامل للجهاز التناسلي الذكري");
                recommendations.AppendLine("• تقييم وجود انسدادات في الجهاز التناسلي");
            }
            
            recommendations.AppendLine("• يُنصح بمراجعة أخصائي الخصوبة للتقييم الشامل والعلاج");
            
            return recommendations.ToString();
        }
    }
}