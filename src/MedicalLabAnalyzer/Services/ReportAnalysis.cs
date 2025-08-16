using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Common.Validation;

namespace MedicalLabAnalyzer.Services
{
    /// <summary>
    /// فئات البيانات للتقارير المهنية
    /// Professional Report Data Classes
    /// </summary>
    
    public class ProfessionalCASAReportData
    {
        public ExamWithPatient Exam { get; set; }
        public CASA_Result CASAResult { get; set; }
        public CASAAnalysisResults AnalysisResults { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public ValidationResult ValidationResults { get; set; }
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
    }

    public class ProfessionalCBCReportData
    {
        public ExamWithPatient Exam { get; set; }
        public CBCTestResult CBCResult { get; set; }
        public CBCAnalysisResults AnalysisResults { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public ValidationResult ValidationResults { get; set; }
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
    }

    public class ProfessionalUrineReportData
    {
        public ExamWithPatient Exam { get; set; }
        public UrineTestResult UrineResult { get; set; }
        public UrineAnalysisResults AnalysisResults { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public ValidationResult ValidationResults { get; set; }
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
    }

    public class ProfessionalStoolReportData
    {
        public ExamWithPatient Exam { get; set; }
        public StoolTestResult StoolResult { get; set; }
        public StoolAnalysisResults AnalysisResults { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public ValidationResult ValidationResults { get; set; }
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
    }

    public class ProfessionalStatisticsReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ComprehensiveStatisticsData Statistics { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class CombinedPatientReportData
    {
        public Patient Patient { get; set; }
        public List<ExamWithResults> Examinations { get; set; } = new List<ExamWithResults>();
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    /// <summary>
    /// نتائج التحليل المتقدم لـ CASA
    /// Advanced CASA Analysis Results
    /// </summary>
    public class CASAAnalysisResults
    {
        public ValidationResult ValidationResults { get; set; } = new ValidationResult();
        public List<ParameterSummary> ParametersSummary { get; set; } = new List<ParameterSummary>();
        public List<string> AbnormalFindings { get; set; } = new List<string>();
        public List<string> CriticalFindings { get; set; } = new List<string>();
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
        public FertilityAssessment FertilityAssessment { get; set; }
        public WHO2021Analysis WHO2021Analysis { get; set; }
    }

    /// <summary>
    /// نتائج التحليل المتقدم لـ CBC
    /// Advanced CBC Analysis Results
    /// </summary>
    public class CBCAnalysisResults
    {
        public ValidationResult ValidationResults { get; set; } = new ValidationResult();
        public List<ParameterSummary> ParametersSummary { get; set; } = new List<ParameterSummary>();
        public List<string> AbnormalFindings { get; set; } = new List<string>();
        public List<string> CriticalFindings { get; set; } = new List<string>();
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
        public HematologyAssessment HematologyAssessment { get; set; }
        public AnemiaAnalysis AnemiaAnalysis { get; set; }
        public InfectionAssessment InfectionAssessment { get; set; }
    }

    /// <summary>
    /// نتائج التحليل المتقدم للبول
    /// Advanced Urine Analysis Results
    /// </summary>
    public class UrineAnalysisResults
    {
        public ValidationResult ValidationResults { get; set; } = new ValidationResult();
        public List<ParameterSummary> ParametersSummary { get; set; } = new List<ParameterSummary>();
        public List<string> AbnormalFindings { get; set; } = new List<string>();
        public List<string> CriticalFindings { get; set; } = new List<string>();
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
        public UTIAssessment UTIAssessment { get; set; }
        public KidneyFunctionAssessment KidneyFunctionAssessment { get; set; }
        public DiabetesScreening DiabetesScreening { get; set; }
    }

    /// <summary>
    /// نتائج التحليل المتقدم للبراز
    /// Advanced Stool Analysis Results
    /// </summary>
    public class StoolAnalysisResults
    {
        public ValidationResult ValidationResults { get; set; } = new ValidationResult();
        public List<ParameterSummary> ParametersSummary { get; set; } = new List<ParameterSummary>();
        public List<string> AbnormalFindings { get; set; } = new List<string>();
        public List<string> CriticalFindings { get; set; } = new List<string>();
        public List<string> ClinicalRecommendations { get; set; } = new List<string>();
        public ReportQualityMetrics QualityMetrics { get; set; }
        public GIBleedingAssessment GIBleedingAssessment { get; set; }
        public ParasiticInfectionAssessment ParasiticInfectionAssessment { get; set; }
        public IBDAssessment IBDAssessment { get; set; }
        public MalabsorptionAssessment MalabsorptionAssessment { get; set; }
    }

    /// <summary>
    /// الإحصائيات الشاملة
    /// Comprehensive Statistics Data
    /// </summary>
    public class ComprehensiveStatisticsData
    {
        public int TotalExams { get; set; }
        public int CASAExams { get; set; }
        public int CBCExams { get; set; }
        public int UrineExams { get; set; }
        public int StoolExams { get; set; }
        public int TotalPatients { get; set; }
        public int MalePatients { get; set; }
        public int FemalePatients { get; set; }
        public Dictionary<string, int> AgeGroups { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AbnormalResults { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> CriticalResults { get; set; } = new Dictionary<string, int>();
        public List<TrendData> Trends { get; set; } = new List<TrendData>();
        public QualityMetrics LabQualityMetrics { get; set; }
    }

    public class ExamWithResults
    {
        public ExamWithPatient Exam { get; set; }
        public Dictionary<string, object> Results { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// تقييم الخصوبة
    /// Fertility Assessment
    /// </summary>
    public class FertilityAssessment
    {
        public string FertilityStatus { get; set; } // Excellent, Good, Fair, Poor, Infertile
        public double FertilityScore { get; set; } // 0-100
        public List<string> PositiveFactors { get; set; } = new List<string>();
        public List<string> NegativeFactors { get; set; } = new List<string>();
        public List<string> RecommendedActions { get; set; } = new List<string>();
        public bool RequiresSpecialistReferral { get; set; }
    }

    /// <summary>
    /// تحليل WHO 2021
    /// WHO 2021 Analysis
    /// </summary>
    public class WHO2021Analysis
    {
        public Dictionary<string, WHO2021ParameterResult> ParameterResults { get; set; } = new Dictionary<string, WHO2021ParameterResult>();
        public string OverallAssessment { get; set; }
        public List<string> WHO2021Recommendations { get; set; } = new List<string>();
        public bool MeetsWHO2021Standards { get; set; }
    }

    public class WHO2021ParameterResult
    {
        public string ParameterName { get; set; }
        public object Value { get; set; }
        public double ReferenceValue { get; set; }
        public string Status { get; set; } // Above, Below, Normal
        public double PercentileRank { get; set; }
    }

    /// <summary>
    /// تقييم أمراض الدم
    /// Hematology Assessment
    /// </summary>
    public class HematologyAssessment
    {
        public string BloodStatus { get; set; } // Normal, Mild Abnormality, Moderate Abnormality, Severe Abnormality
        public List<string> PossibleConditions { get; set; } = new List<string>();
        public List<string> RecommendedFollowUp { get; set; } = new List<string>();
        public bool RequiresUrgentAttention { get; set; }
    }

    /// <summary>
    /// تحليل فقر الدم
    /// Anemia Analysis
    /// </summary>
    public class AnemiaAnalysis
    {
        public bool HasAnemia { get; set; }
        public string AnemiaType { get; set; } // Microcytic, Normocytic, Macrocytic
        public string AnemiaSeverity { get; set; } // Mild, Moderate, Severe
        public List<string> PossibleCauses { get; set; } = new List<string>();
        public List<string> RecommendedTests { get; set; } = new List<string>();
    }

    /// <summary>
    /// تقييم العدوى
    /// Infection Assessment
    /// </summary>
    public class InfectionAssessment
    {
        public bool SuggestsInfection { get; set; }
        public string InfectionType { get; set; } // Bacterial, Viral, Fungal, Parasitic
        public string InfectionSeverity { get; set; } // Mild, Moderate, Severe
        public List<string> InfectionMarkers { get; set; } = new List<string>();
        public bool RequiresAntibiotics { get; set; }
    }

    /// <summary>
    /// تقييم التهاب المسالك البولية
    /// UTI Assessment
    /// </summary>
    public class UTIAssessment
    {
        public bool HasUTI { get; set; }
        public string UTIType { get; set; } // Lower UTI, Upper UTI, Complicated UTI
        public string UTISeverity { get; set; } // Mild, Moderate, Severe
        public List<string> UTIMarkers { get; set; } = new List<string>();
        public List<string> RecommendedTreatment { get; set; } = new List<string>();
        public bool RequiresCulture { get; set; }
    }

    /// <summary>
    /// تقييم وظائف الكلى
    /// Kidney Function Assessment
    /// </summary>
    public class KidneyFunctionAssessment
    {
        public string KidneyStatus { get; set; } // Normal, Mild Impairment, Moderate Impairment, Severe Impairment
        public List<string> KidneyMarkers { get; set; } = new List<string>();
        public List<string> RecommendedTests { get; set; } = new List<string>();
        public bool RequiresNephrologistReferral { get; set; }
    }

    /// <summary>
    /// فحص السكري
    /// Diabetes Screening
    /// </summary>
    public class DiabetesScreening
    {
        public bool SuggestsDiabetes { get; set; }
        public string DiabetesRisk { get; set; } // Low, Moderate, High
        public List<string> DiabetesMarkers { get; set; } = new List<string>();
        public List<string> RecommendedTests { get; set; } = new List<string>();
        public bool RequiresEndocrinologistReferral { get; set; }
    }

    /// <summary>
    /// تقييم النزيف الهضمي
    /// GI Bleeding Assessment
    /// </summary>
    public class GIBleedingAssessment
    {
        public bool HasGIBleeding { get; set; }
        public string BleedingLocation { get; set; } // Upper GI, Lower GI, Unknown
        public string BleedingSeverity { get; set; } // Mild, Moderate, Severe
        public List<string> BleedingMarkers { get; set; } = new List<string>();
        public bool RequiresUrgentEndoscopy { get; set; }
        public List<string> RecommendedActions { get; set; } = new List<string>();
    }

    /// <summary>
    /// تقييم العدوى الطفيلية
    /// Parasitic Infection Assessment
    /// </summary>
    public class ParasiticInfectionAssessment
    {
        public bool HasParasiticInfection { get; set; }
        public List<string> IdentifiedParasites { get; set; } = new List<string>();
        public string InfectionSeverity { get; set; } // Mild, Moderate, Severe
        public List<string> RecommendedTreatments { get; set; } = new List<string>();
        public bool RequiresContactTracing { get; set; }
        public List<string> PreventionMeasures { get; set; } = new List<string>();
    }

    /// <summary>
    /// تقييم أمراض الأمعاء الالتهابية
    /// IBD Assessment
    /// </summary>
    public class IBDAssessment
    {
        public bool SuggestsIBD { get; set; }
        public string IBDType { get; set; } // Crohn's Disease, Ulcerative Colitis, Indeterminate
        public string ActivityLevel { get; set; } // Remission, Mild, Moderate, Severe
        public List<string> IBDMarkers { get; set; } = new List<string>();
        public List<string> RecommendedTests { get; set; } = new List<string>();
        public bool RequiresGastroenterologistReferral { get; set; }
    }

    /// <summary>
    /// تقييم سوء الامتصاص
    /// Malabsorption Assessment
    /// </summary>
    public class MalabsorptionAssessment
    {
        public bool HasMalabsorption { get; set; }
        public string MalabsorptionType { get; set; } // Fat, Carbohydrate, Protein, Combined
        public List<string> MalabsorptionMarkers { get; set; } = new List<string>();
        public List<string> PossibleCauses { get; set; } = new List<string>();
        public List<string> RecommendedTests { get; set; } = new List<string>();
        public List<string> DietaryRecommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// بيانات الاتجاه
    /// Trend Data
    /// </summary>
    public class TrendData
    {
        public string Parameter { get; set; }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public string TrendDirection { get; set; } // Increasing, Decreasing, Stable
    }

    /// <summary>
    /// مقاييس جودة المختبر
    /// Lab Quality Metrics
    /// </summary>
    public class QualityMetrics
    {
        public double AccuracyScore { get; set; }
        public double PrecisionScore { get; set; }
        public double CompletenessScore { get; set; }
        public double TimelinessScore { get; set; }
        public double OverallQualityScore { get; set; }
        public List<string> QualityIssues { get; set; } = new List<string>();
    }
}