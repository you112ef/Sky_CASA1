using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Common.Results;
using MedicalLabAnalyzer.Common.Exceptions;
using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;

namespace MedicalLabAnalyzer.Common.Validation
{
    /// <summary>
    /// Comprehensive medical validation service for all lab test parameters
    /// </summary>
    public interface IMedicalValidationService
    {
        ValidationResult ValidatePatientData(Patient patient);
        ValidationResult ValidateCASAResults(CASA_Result casaResult);
        ValidationResult ValidateCBCResults(CBC_Result cbcResult);
        ValidationResult ValidateUrineResults(Urine_Result urineResult);
        ValidationResult ValidateStoolResults(Stool_Result stoolResult);
        ValidationResult ValidateExamData(Exam exam);
        ValidationResult ValidateNumericRange(double value, double min, double max, string parameterName, bool allowNegative = false);
        ValidationResult ValidatePercentage(double value, string parameterName);
        ValidationResult ValidateAge(int age);
        ValidationResult ValidatePhoneNumber(string phoneNumber);
        ValidationResult ValidateEmail(string email);
        ValidationResult ValidateCrossParameters(object testResult, string testType);
        Task<ValidationResult> ValidateComprehensiveExamAsync(Exam exam);
    }

    public class MedicalValidationService : IMedicalValidationService
    {
        private readonly ILogger<MedicalValidationService> _logger;

        // WHO 2021 Semen Analysis Reference Values (5th percentile)
        private static readonly Dictionary<string, (double Min, double Max)> WHO2021References = new()
        {
            ["Volume"] = (1.4, 8.0), // mL
            ["Concentration"] = (16.0, 300.0), // million/mL  
            ["TotalCount"] = (39.0, 1200.0), // million
            ["TotalMotility"] = (42.0, 100.0), // %
            ["ProgressiveMotility"] = (30.0, 100.0), // %
            ["NonProgressiveMotility"] = (0.0, 50.0), // %
            ["NormalMorphology"] = (4.0, 100.0), // %
            ["Vitality"] = (54.0, 100.0), // %
            ["pH"] = (7.0, 8.5),
            ["Fructose"] = (13.0, 100.0), // μmol/ejaculate
            ["WBCCount"] = (0.0, 1.0) // million/mL
        };

        // Hematology Reference Ranges (Adult)
        private static readonly Dictionary<string, (double Min, double Max)> CBCReferences = new()
        {
            ["WBC"] = (4.0, 11.0), // ×10³/μL
            ["RBC_Male"] = (4.5, 5.9), // ×10⁶/μL
            ["RBC_Female"] = (4.0, 5.2), // ×10⁶/μL  
            ["Hemoglobin_Male"] = (13.5, 17.5), // g/dL
            ["Hemoglobin_Female"] = (12.0, 15.5), // g/dL
            ["Hematocrit_Male"] = (41.0, 53.0), // %
            ["Hematocrit_Female"] = (36.0, 46.0), // %
            ["MCV"] = (80.0, 100.0), // fL
            ["MCH"] = (27.0, 33.0), // pg
            ["MCHC"] = (32.0, 36.0), // g/dL
            ["RDW"] = (11.5, 14.5), // %
            ["Platelets"] = (150.0, 450.0), // ×10³/μL
            ["MPV"] = (7.0, 11.0), // fL
            ["Neutrophils"] = (40.0, 70.0), // %
            ["Lymphocytes"] = (20.0, 45.0), // %
            ["Monocytes"] = (2.0, 10.0), // %
            ["Eosinophils"] = (1.0, 4.0), // %
            ["Basophils"] = (0.0, 2.0) // %
        };

        // Urine Analysis Reference Ranges
        private static readonly Dictionary<string, (double Min, double Max)> UrineReferences = new()
        {
            ["SpecificGravity"] = (1.003, 1.035),
            ["pH"] = (4.5, 8.0),
            ["Protein"] = (0.0, 30.0), // mg/dL
            ["Glucose"] = (0.0, 15.0), // mg/dL
            ["Ketones"] = (0.0, 5.0), // mg/dL
            ["Bilirubin"] = (0.0, 0.2), // mg/dL
            ["Urobilinogen"] = (0.1, 1.0), // EU/dL
            ["RBC_Count"] = (0.0, 2.0), // cells/hpf
            ["WBC_Count"] = (0.0, 5.0), // cells/hpf
            ["Epithelial_Count"] = (0.0, 5.0), // cells/hpf
            ["Bacteria"] = (0.0, 2.0), // 0=None, 1=Few, 2=Moderate, 3=Many
            ["Casts"] = (0.0, 2.0), // /lpf
            ["Crystals"] = (0.0, 2.0) // 0=None, 1=Few, 2=Moderate, 3=Many
        };

        public MedicalValidationService(ILogger<MedicalValidationService> logger = null)
        {
            _logger = logger;
        }

        public ValidationResult ValidatePatientData(Patient patient)
        {
            var errors = new List<string>();

            if (patient == null)
            {
                errors.Add("بيانات المريض مطلوبة - Patient data is required");
                return ValidationResult.Failure(errors);
            }

            // Name validation
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                errors.Add("الاسم الأول مطلوب - First name is required");
            else if (patient.FirstName.Length < 2)
                errors.Add("الاسم الأول يجب أن يكون حرفين على الأقل - First name must be at least 2 characters");
            else if (patient.FirstName.Length > 50)
                errors.Add("الاسم الأول طويل جداً - First name is too long");

            if (string.IsNullOrWhiteSpace(patient.LastName))
                errors.Add("الاسم الأخير مطلوب - Last name is required");
            else if (patient.LastName.Length < 2)
                errors.Add("الاسم الأخير يجب أن يكون حرفين على الأقل - Last name must be at least 2 characters");
            else if (patient.LastName.Length > 50)
                errors.Add("الاسم الأخير طويل جداً - Last name is too long");

            // Date of birth validation
            if (patient.DateOfBirth == default)
                errors.Add("تاريخ الميلاد مطلوب - Date of birth is required");
            else if (patient.DateOfBirth > DateTime.Today)
                errors.Add("تاريخ الميلاد لا يمكن أن يكون في المستقبل - Date of birth cannot be in the future");
            else if (patient.DateOfBirth < DateTime.Today.AddYears(-150))
                errors.Add("تاريخ الميلاد غير معقول - Date of birth is unrealistic");

            // Age validation
            var ageValidation = ValidateAge(patient.Age);
            if (!ageValidation.IsSuccess)
                errors.AddRange(ageValidation.Errors);

            // Gender validation
            if (string.IsNullOrWhiteSpace(patient.Gender))
                errors.Add("الجنس مطلوب - Gender is required");
            else if (!new[] { "M", "F", "Male", "Female", "ذكر", "أنثى" }.Contains(patient.Gender, StringComparer.OrdinalIgnoreCase))
                errors.Add("الجنس غير صالح - Invalid gender value");

            // Phone validation
            if (!string.IsNullOrWhiteSpace(patient.Phone))
            {
                var phoneValidation = ValidatePhoneNumber(patient.Phone);
                if (!phoneValidation.IsSuccess)
                    errors.AddRange(phoneValidation.Errors);
            }

            // Email validation
            if (!string.IsNullOrWhiteSpace(patient.Email))
            {
                var emailValidation = ValidateEmail(patient.Email);
                if (!emailValidation.IsSuccess)
                    errors.AddRange(emailValidation.Errors);
            }

            // National ID validation (Saudi format as example)
            if (!string.IsNullOrWhiteSpace(patient.NationalId))
            {
                if (!Regex.IsMatch(patient.NationalId, @"^[12]\d{9}$"))
                    errors.Add("رقم الهوية الوطنية غير صالح - Invalid National ID format");
            }

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateCASAResults(CASA_Result casaResult)
        {
            var errors = new List<string>();

            if (casaResult == null)
            {
                errors.Add("نتائج تحليل CASA مطلوبة - CASA results are required");
                return ValidationResult.Failure(errors);
            }

            // WHO 2021 Basic Parameters Validation
            var volumeValidation = ValidateWHO2021Parameter(casaResult.Volume, "Volume", "الحجم");
            if (!volumeValidation.IsSuccess) errors.AddRange(volumeValidation.Errors);

            var concentrationValidation = ValidateWHO2021Parameter(casaResult.Concentration, "Concentration", "التركيز");
            if (!concentrationValidation.IsSuccess) errors.AddRange(concentrationValidation.Errors);

            var totalCountValidation = ValidateWHO2021Parameter(casaResult.TotalCount, "TotalCount", "العدد الكلي");
            if (!totalCountValidation.IsSuccess) errors.AddRange(totalCountValidation.Errors);

            // Motility Parameters
            var totalMotilityValidation = ValidatePercentage(casaResult.TotalMotility, "الحركة الكلية");
            if (!totalMotilityValidation.IsSuccess) errors.AddRange(totalMotilityValidation.Errors);

            var progressiveMotilityValidation = ValidatePercentage(casaResult.ProgressiveMotility, "الحركة التقدمية");
            if (!progressiveMotilityValidation.IsSuccess) errors.AddRange(progressiveMotilityValidation.Errors);

            var nonProgressiveMotilityValidation = ValidatePercentage(casaResult.NonProgressiveMotility, "الحركة غير التقدمية");
            if (!nonProgressiveMotilityValidation.IsSuccess) errors.AddRange(nonProgressiveMotilityValidation.Errors);

            // Cross-validation: motility percentages should add up correctly
            var totalMotilitySum = casaResult.ProgressiveMotility + casaResult.NonProgressiveMotility;
            var immotile = 100 - casaResult.TotalMotility;
            if (Math.Abs(totalMotilitySum + immotile - 100) > 5) // Allow 5% tolerance
                errors.Add("مجموع نسب الحركة غير صحيح - Motility percentages sum is incorrect");

            // Morphology and Vitality
            var morphologyValidation = ValidateWHO2021Parameter(casaResult.NormalMorphology, "NormalMorphology", "الشكل الطبيعي");
            if (!morphologyValidation.IsSuccess) errors.AddRange(morphologyValidation.Errors);

            var vitalityValidation = ValidateWHO2021Parameter(casaResult.Vitality, "Vitality", "الحيوية");
            if (!vitalityValidation.IsSuccess) errors.AddRange(vitalityValidation.Errors);

            // pH and Fructose
            var pHValidation = ValidateWHO2021Parameter(casaResult.pH, "pH", "الأس الهيدروجيني");
            if (!pHValidation.IsSuccess) errors.AddRange(pHValidation.Errors);

            var fructoseValidation = ValidateWHO2021Parameter(casaResult.Fructose, "Fructose", "الفركتوز");
            if (!fructoseValidation.IsSuccess) errors.AddRange(fructoseValidation.Errors);

            var wbcValidation = ValidateWHO2021Parameter(casaResult.WBCCount, "WBCCount", "كريات الدم البيضاء");
            if (!wbcValidation.IsSuccess) errors.AddRange(wbcValidation.Errors);

            // Kinematic Parameters (CASA-specific)
            if (casaResult.VCL > 0)
            {
                var vclValidation = ValidateNumericRange(casaResult.VCL, 0, 300, "سرعة المسار المنحني");
                if (!vclValidation.IsSuccess) errors.AddRange(vclValidation.Errors);
            }

            if (casaResult.VSL > 0)
            {
                var vslValidation = ValidateNumericRange(casaResult.VSL, 0, 200, "السرعة الخطية المستقيمة");
                if (!vslValidation.IsSuccess) errors.AddRange(vslValidation.Errors);
            }

            if (casaResult.VAP > 0)
            {
                var vapValidation = ValidateNumericRange(casaResult.VAP, 0, 250, "السرعة المتوسطة للمسار");
                if (!vapValidation.IsSuccess) errors.AddRange(vapValidation.Errors);
            }

            // Cross-validation for kinematic parameters: VSL <= VAP <= VCL
            if (casaResult.VCL > 0 && casaResult.VAP > 0 && casaResult.VSL > 0)
            {
                if (casaResult.VSL > casaResult.VAP)
                    errors.Add("السرعة الخطية المستقيمة لا يمكن أن تكون أكبر من السرعة المتوسطة - VSL cannot be greater than VAP");
                
                if (casaResult.VAP > casaResult.VCL)
                    errors.Add("السرعة المتوسطة لا يمكن أن تكون أكبر من سرعة المسار المنحني - VAP cannot be greater than VCL");
            }

            if (casaResult.ALH > 0)
            {
                var alhValidation = ValidateNumericRange(casaResult.ALH, 0, 20, "سعة الانحراف الجانبي");
                if (!alhValidation.IsSuccess) errors.AddRange(alhValidation.Errors);
            }

            if (casaResult.BCF > 0)
            {
                var bcfValidation = ValidateNumericRange(casaResult.BCF, 0, 50, "تكرار ضربات العبور");
                if (!bcfValidation.IsSuccess) errors.AddRange(bcfValidation.Errors);
            }

            // Appearance and Viscosity validation
            if (!string.IsNullOrWhiteSpace(casaResult.Appearance))
            {
                var validAppearances = new[] { "Normal", "Abnormal", "طبيعي", "غير طبيعي", "Greyish-white", "Yellow", "Red-brown" };
                if (!validAppearances.Contains(casaResult.Appearance, StringComparer.OrdinalIgnoreCase))
                    errors.Add("شكل العينة غير صالح - Invalid appearance value");
            }

            if (!string.IsNullOrWhiteSpace(casaResult.Viscosity))
            {
                var validViscosities = new[] { "Normal", "High", "Low", "طبيعي", "عالي", "منخفض" };
                if (!validViscosities.Contains(casaResult.Viscosity, StringComparer.OrdinalIgnoreCase))
                    errors.Add("لزوجة العينة غير صالحة - Invalid viscosity value");
            }

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateCBCResults(CBC_Result cbcResult)
        {
            var errors = new List<string>();

            if (cbcResult == null)
            {
                errors.Add("نتائج تحليل الدم الشامل مطلوبة - CBC results are required");
                return ValidationResult.Failure(errors);
            }

            // Determine gender for gender-specific ranges
            var isFemaleLikely = cbcResult.Hemoglobin < 13.0; // Rough estimation

            // White Blood Cell Count
            var wbcValidation = ValidateCBCParameter(cbcResult.WBC, "WBC", "كريات الدم البيضاء");
            if (!wbcValidation.IsSuccess) errors.AddRange(wbcValidation.Errors);

            // Red Blood Cell Count (gender-specific)
            var rbcKey = isFemaleLikely ? "RBC_Female" : "RBC_Male";
            var rbcValidation = ValidateCBCParameter(cbcResult.RBC, rbcKey, "كريات الدم الحمراء");
            if (!rbcValidation.IsSuccess) errors.AddRange(rbcValidation.Errors);

            // Hemoglobin (gender-specific)
            var hgbKey = isFemaleLikely ? "Hemoglobin_Female" : "Hemoglobin_Male";
            var hgbValidation = ValidateCBCParameter(cbcResult.Hemoglobin, hgbKey, "الهيموجلوبين");
            if (!hgbValidation.IsSuccess) errors.AddRange(hgbValidation.Errors);

            // Hematocrit (gender-specific)
            var hctKey = isFemaleLikely ? "Hematocrit_Female" : "Hematocrit_Male";
            var hctValidation = ValidateCBCParameter(cbcResult.Hematocrit, hctKey, "الهيماتوكريت");
            if (!hctValidation.IsSuccess) errors.AddRange(hctValidation.Errors);

            // Red Blood Cell Indices
            var mcvValidation = ValidateCBCParameter(cbcResult.MCV, "MCV", "متوسط حجم الكرية");
            if (!mcvValidation.IsSuccess) errors.AddRange(mcvValidation.Errors);

            var mchValidation = ValidateCBCParameter(cbcResult.MCH, "MCH", "متوسط الهيموجلوبين");
            if (!mchValidation.IsSuccess) errors.AddRange(mchValidation.Errors);

            var mchcValidation = ValidateCBCParameter(cbcResult.MCHC, "MCHC", "تركيز متوسط الهيموجلوبين");
            if (!mchcValidation.IsSuccess) errors.AddRange(mchcValidation.Errors);

            var rdwValidation = ValidateCBCParameter(cbcResult.RDW, "RDW", "عرض توزيع الكريات الحمراء");
            if (!rdwValidation.IsSuccess) errors.AddRange(rdwValidation.Errors);

            // Platelets
            var pltValidation = ValidateCBCParameter(cbcResult.Platelets, "Platelets", "الصفائح الدموية");
            if (!pltValidation.IsSuccess) errors.AddRange(pltValidation.Errors);

            var mpvValidation = ValidateCBCParameter(cbcResult.MPV, "MPV", "متوسط حجم الصفيحة");
            if (!mpvValidation.IsSuccess) errors.AddRange(mpvValidation.Errors);

            // Differential Count
            var neutValidation = ValidatePercentage(cbcResult.Neutrophils, "العدلات");
            if (!neutValidation.IsSuccess) errors.AddRange(neutValidation.Errors);

            var lymphValidation = ValidatePercentage(cbcResult.Lymphocytes, "اللمفاويات");
            if (!lymphValidation.IsSuccess) errors.AddRange(lymphValidation.Errors);

            var monoValidation = ValidatePercentage(cbcResult.Monocytes, "وحيدات النوى");
            if (!monoValidation.IsSuccess) errors.AddRange(monoValidation.Errors);

            var eosinValidation = ValidatePercentage(cbcResult.Eosinophils, "الحمضات");
            if (!eosinValidation.IsSuccess) errors.AddRange(eosinValidation.Errors);

            var basoValidation = ValidatePercentage(cbcResult.Basophils, "القعدات");
            if (!basoValidation.IsSuccess) errors.AddRange(basoValidation.Errors);

            // Cross-validation: differential percentages should add up to ~100%
            var differentialSum = cbcResult.Neutrophils + cbcResult.Lymphocytes + cbcResult.Monocytes + 
                                cbcResult.Eosinophils + cbcResult.Basophils;
            if (Math.Abs(differentialSum - 100) > 5) // Allow 5% tolerance
                errors.Add("مجموع نسب كريات الدم البيضاء غير صحيح - Differential count percentages sum is incorrect");

            // Cross-validation: Calculate indices consistency
            if (cbcResult.RBC > 0 && cbcResult.Hematocrit > 0)
            {
                var calculatedMCV = (cbcResult.Hematocrit / cbcResult.RBC) * 10;
                if (Math.Abs(calculatedMCV - cbcResult.MCV) > 5)
                    errors.Add("قيم MCV محسوبة وقياس مباشر غير متطابقة - Calculated and measured MCV values are inconsistent");
            }

            if (cbcResult.RBC > 0 && cbcResult.Hemoglobin > 0)
            {
                var calculatedMCH = (cbcResult.Hemoglobin / cbcResult.RBC) * 10;
                if (Math.Abs(calculatedMCH - cbcResult.MCH) > 3)
                    errors.Add("قيم MCH محسوبة وقياس مباشر غير متطابقة - Calculated and measured MCH values are inconsistent");
            }

            if (cbcResult.MCH > 0 && cbcResult.MCV > 0)
            {
                var calculatedMCHC = (cbcResult.MCH / cbcResult.MCV) * 100;
                if (Math.Abs(calculatedMCHC - cbcResult.MCHC) > 2)
                    errors.Add("قيم MCHC محسوبة وقياس مباشر غير متطابقة - Calculated and measured MCHC values are inconsistent");
            }

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateUrineResults(Urine_Result urineResult)
        {
            var errors = new List<string>();

            if (urineResult == null)
            {
                errors.Add("نتائج تحليل البول مطلوبة - Urine analysis results are required");
                return ValidationResult.Failure(errors);
            }

            // Physical Properties
            if (!string.IsNullOrWhiteSpace(urineResult.Color))
            {
                var validColors = new[] { "Yellow", "Pale Yellow", "Dark Yellow", "Amber", "Red", "Brown", 
                                        "أصفر", "أصفر فاتح", "أصفر داكن", "كهرماني", "أحمر", "بني" };
                if (!validColors.Contains(urineResult.Color, StringComparer.OrdinalIgnoreCase))
                    errors.Add("لون البول غير صالح - Invalid urine color");
            }

            if (!string.IsNullOrWhiteSpace(urineResult.Clarity))
            {
                var validClarities = new[] { "Clear", "Slightly Cloudy", "Cloudy", "Very Cloudy", 
                                           "صافي", "عكر قليلاً", "عكر", "عكر جداً" };
                if (!validClarities.Contains(urineResult.Clarity, StringComparer.OrdinalIgnoreCase))
                    errors.Add("صفاء البول غير صالح - Invalid urine clarity");
            }

            // Chemical Properties
            var sgValidation = ValidateUrineParameter(urineResult.SpecificGravity, "SpecificGravity", "الوزن النوعي");
            if (!sgValidation.IsSuccess) errors.AddRange(sgValidation.Errors);

            var phValidation = ValidateUrineParameter(urineResult.pH, "pH", "الأس الهيدروجيني");
            if (!phValidation.IsSuccess) errors.AddRange(phValidation.Errors);

            var proteinValidation = ValidateUrineParameter(urineResult.Protein, "Protein", "البروتين");
            if (!proteinValidation.IsSuccess) errors.AddRange(proteinValidation.Errors);

            var glucoseValidation = ValidateUrineParameter(urineResult.Glucose, "Glucose", "الجلوكوز");
            if (!glucoseValidation.IsSuccess) errors.AddRange(glucoseValidation.Errors);

            var ketonesValidation = ValidateUrineParameter(urineResult.Ketones, "Ketones", "الكيتونات");
            if (!ketonesValidation.IsSuccess) errors.AddRange(ketonesValidation.Errors);

            var bilirubinValidation = ValidateUrineParameter(urineResult.Bilirubin, "Bilirubin", "البيليروبين");
            if (!bilirubinValidation.IsSuccess) errors.AddRange(bilirubinValidation.Errors);

            var urobilinogenValidation = ValidateUrineParameter(urineResult.Urobilinogen, "Urobilinogen", "اليوروبيلينوجين");
            if (!urobilinogenValidation.IsSuccess) errors.AddRange(urobilinogenValidation.Errors);

            // Microscopic Examination
            var rbcValidation = ValidateUrineParameter(urineResult.RBC_Count, "RBC_Count", "كريات الدم الحمراء");
            if (!rbcValidation.IsSuccess) errors.AddRange(rbcValidation.Errors);

            var wbcValidation = ValidateUrineParameter(urineResult.WBC_Count, "WBC_Count", "كريات الدم البيضاء");
            if (!wbcValidation.IsSuccess) errors.AddRange(wbcValidation.Errors);

            var epithelialValidation = ValidateUrineParameter(urineResult.Epithelial_Count, "Epithelial_Count", "الخلايا الظهارية");
            if (!epithelialValidation.IsSuccess) errors.AddRange(epithelialValidation.Errors);

            var bacteriaValidation = ValidateUrineParameter(urineResult.Bacteria, "Bacteria", "البكتيريا");
            if (!bacteriaValidation.IsSuccess) errors.AddRange(bacteriaValidation.Errors);

            var castsValidation = ValidateUrineParameter(urineResult.Casts, "Casts", "الأسطوانات");
            if (!castsValidation.IsSuccess) errors.AddRange(castsValidation.Errors);

            var crystalsValidation = ValidateUrineParameter(urineResult.Crystals, "Crystals", "البلورات");
            if (!crystalsValidation.IsSuccess) errors.AddRange(crystalsValidation.Errors);

            // Nitrites and Leukocyte Esterase validation
            if (!string.IsNullOrWhiteSpace(urineResult.Nitrites))
            {
                var validNitrites = new[] { "Positive", "Negative", "إيجابي", "سلبي" };
                if (!validNitrites.Contains(urineResult.Nitrites, StringComparer.OrdinalIgnoreCase))
                    errors.Add("نتيجة النيتريت غير صالحة - Invalid nitrites result");
            }

            if (!string.IsNullOrWhiteSpace(urineResult.LeukocyteEsterase))
            {
                var validLE = new[] { "Positive", "Negative", "Trace", "إيجابي", "سلبي", "أثر" };
                if (!validLE.Contains(urineResult.LeukocyteEsterase, StringComparer.OrdinalIgnoreCase))
                    errors.Add("نتيجة استيراز الكريات البيضاء غير صالحة - Invalid leukocyte esterase result");
            }

            // Cross-validation: High WBC count should correlate with positive leukocyte esterase
            if (urineResult.WBC_Count > 5 && urineResult.LeukocyteEsterase?.ToLower().Contains("negative") == true)
                errors.Add("تضارب بين عدد الكريات البيضاء واستيراز الكريات البيضاء - Inconsistency between WBC count and leukocyte esterase");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateStoolResults(Stool_Result stoolResult)
        {
            var errors = new List<string>();

            if (stoolResult == null)
            {
                errors.Add("نتائج تحليل البراز مطلوبة - Stool analysis results are required");
                return ValidationResult.Failure(errors);
            }

            // Physical Properties
            if (!string.IsNullOrWhiteSpace(stoolResult.Color))
            {
                var validColors = new[] { "Brown", "Dark Brown", "Light Brown", "Yellow", "Green", 
                                        "Black", "Red", "Pale", "بني", "بني داكن", "بني فاتح", 
                                        "أصفر", "أخضر", "أسود", "أحمر", "شاحب" };
                if (!validColors.Contains(stoolResult.Color, StringComparer.OrdinalIgnoreCase))
                    errors.Add("لون البراز غير صالح - Invalid stool color");
            }

            if (!string.IsNullOrWhiteSpace(stoolResult.Consistency))
            {
                var validConsistencies = new[] { "Formed", "Semi-formed", "Loose", "Watery", "Hard", 
                                               "متماسك", "شبه متماسك", "رخو", "مائي", "صلب" };
                if (!validConsistencies.Contains(stoolResult.Consistency, StringComparer.OrdinalIgnoreCase))
                    errors.Add("قوام البراز غير صالح - Invalid stool consistency");
            }

            // Microscopic Examination
            if (!string.IsNullOrWhiteSpace(stoolResult.BloodCells))
            {
                var validBloodCells = new[] { "None", "Few", "Moderate", "Many", "لا يوجد", "قليل", "متوسط", "كثير" };
                if (!validBloodCells.Contains(stoolResult.BloodCells, StringComparer.OrdinalIgnoreCase))
                    errors.Add("خلايا الدم غير صالحة - Invalid blood cells result");
            }

            if (!string.IsNullOrWhiteSpace(stoolResult.WBC))
            {
                var validWBC = new[] { "None", "Few", "Moderate", "Many", "لا يوجد", "قليل", "متوسط", "كثير" };
                if (!validWBC.Contains(stoolResult.WBC, StringComparer.OrdinalIgnoreCase))
                    errors.Add("كريات الدم البيضاء غير صالحة - Invalid WBC result");
            }

            if (!string.IsNullOrWhiteSpace(stoolResult.Parasites))
            {
                var validParasites = new[] { "None", "Giardia", "Entamoeba", "Ascaris", "Hookworm", 
                                           "Trichuris", "لا يوجد", "جيارديا", "الأميبا", "الأسكاريس" };
                if (!validParasites.Contains(stoolResult.Parasites, StringComparer.OrdinalIgnoreCase))
                    errors.Add("الطفيليات غير صالحة - Invalid parasites result");
            }

            if (!string.IsNullOrWhiteSpace(stoolResult.Ova))
            {
                var validOva = new[] { "None", "Present", "لا يوجد", "موجود" };
                if (!validOva.Contains(stoolResult.Ova, StringComparer.OrdinalIgnoreCase))
                    errors.Add("البيوض غير صالحة - Invalid ova result");
            }

            // Chemical Tests
            if (!string.IsNullOrWhiteSpace(stoolResult.OccultBlood))
            {
                var validOccultBlood = new[] { "Positive", "Negative", "إيجابي", "سلبي" };
                if (!validOccultBlood.Contains(stoolResult.OccultBlood, StringComparer.OrdinalIgnoreCase))
                    errors.Add("الدم الخفي غير صالح - Invalid occult blood result");
            }

            if (!string.IsNullOrWhiteSpace(stoolResult.FatGlobules))
            {
                var validFat = new[] { "None", "Few", "Moderate", "Many", "لا يوجد", "قليل", "متوسط", "كثير" };
                if (!validFat.Contains(stoolResult.FatGlobules, StringComparer.OrdinalIgnoreCase))
                    errors.Add("كريات الدهن غير صالحة - Invalid fat globules result");
            }

            if (!string.IsNullOrWhiteSpace(stoolResult.MuscleFibers))
            {
                var validMuscle = new[] { "None", "Few", "Moderate", "Many", "لا يوجد", "قليل", "متوسط", "كثير" };
                if (!validMuscle.Contains(stoolResult.MuscleFibers, StringComparer.OrdinalIgnoreCase))
                    errors.Add("الألياف العضلية غير صالحة - Invalid muscle fibers result");
            }

            // Cross-validation: Blood cells and occult blood should be consistent
            if (stoolResult.BloodCells?.ToLower().Contains("many") == true && 
                stoolResult.OccultBlood?.ToLower().Contains("negative") == true)
                errors.Add("تضارب بين خلايا الدم والدم الخفي - Inconsistency between blood cells and occult blood");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateExamData(Exam exam)
        {
            var errors = new List<string>();

            if (exam == null)
            {
                errors.Add("بيانات الفحص مطلوبة - Exam data is required");
                return ValidationResult.Failure(errors);
            }

            // Exam ID validation
            if (string.IsNullOrWhiteSpace(exam.ExamId))
                errors.Add("رقم الفحص مطلوب - Exam ID is required");

            // Patient ID validation
            if (exam.PatientId <= 0)
                errors.Add("رقم المريض غير صالح - Invalid patient ID");

            // Exam type validation
            if (string.IsNullOrWhiteSpace(exam.ExamType))
                errors.Add("نوع الفحص مطلوب - Exam type is required");
            else
            {
                var validExamTypes = new[] { "CASA", "CBC", "Urine", "Stool", "تحليل السائل المنوي", 
                                           "فحص الدم الشامل", "تحليل البول", "تحليل البراز" };
                if (!validExamTypes.Contains(exam.ExamType, StringComparer.OrdinalIgnoreCase))
                    errors.Add("نوع الفحص غير صالح - Invalid exam type");
            }

            // Date validation
            if (exam.ExamDate == default)
                errors.Add("تاريخ الفحص مطلوب - Exam date is required");
            else if (exam.ExamDate > DateTime.Now)
                errors.Add("تاريخ الفحص لا يمكن أن يكون في المستقبل - Exam date cannot be in the future");
            else if (exam.ExamDate < DateTime.Now.AddYears(-10))
                errors.Add("تاريخ الفحص قديم جداً - Exam date is too old");

            // Status validation
            if (string.IsNullOrWhiteSpace(exam.Status))
                errors.Add("حالة الفحص مطلوبة - Exam status is required");
            else
            {
                var validStatuses = new[] { "Pending", "InProgress", "Completed", "Failed", 
                                          "قيد الانتظار", "جاري", "مكتمل", "فاشل" };
                if (!validStatuses.Contains(exam.Status, StringComparer.OrdinalIgnoreCase))
                    errors.Add("حالة الفحص غير صالحة - Invalid exam status");
            }

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateNumericRange(double value, double min, double max, string parameterName, bool allowNegative = false)
        {
            var errors = new List<string>();

            if (double.IsNaN(value) || double.IsInfinity(value))
                errors.Add($"{parameterName} قيمة غير صالحة - {parameterName} is not a valid number");
            else if (!allowNegative && value < 0)
                errors.Add($"{parameterName} لا يمكن أن يكون سالباً - {parameterName} cannot be negative");
            else if (value < min || value > max)
                errors.Add($"{parameterName} خارج النطاق المقبول ({min:F1} - {max:F1}) - {parameterName} is out of acceptable range ({min:F1} - {max:F1})");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidatePercentage(double value, string parameterName)
        {
            return ValidateNumericRange(value, 0, 100, parameterName);
        }

        public ValidationResult ValidateAge(int age)
        {
            var errors = new List<string>();

            if (age < 0)
                errors.Add("العمر لا يمكن أن يكون سالباً - Age cannot be negative");
            else if (age > 150)
                errors.Add("العمر غير معقول - Age is unrealistic");
            else if (age == 0)
                errors.Add("العمر مطلوب - Age is required");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidatePhoneNumber(string phoneNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return ValidationResult.Success(); // Phone is optional

            // Remove common formatting
            var cleanPhone = Regex.Replace(phoneNumber, @"[\s\-\(\)]", "");

            // Saudi phone number patterns
            if (!Regex.IsMatch(cleanPhone, @"^(\+966|966|0)?[5][0-9]{8}$"))
                errors.Add("رقم الهاتف غير صالح - Invalid phone number format");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateEmail(string email)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
                return ValidationResult.Success(); // Email is optional

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors.Add("البريد الإلكتروني غير صالح - Invalid email format");

            return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
        }

        public ValidationResult ValidateCrossParameters(object testResult, string testType)
        {
            // This method performs cross-parameter validation specific to test types
            return testType?.ToLower() switch
            {
                "casa" when testResult is CASA_Result casaResult => ValidateCASAResults(casaResult),
                "cbc" when testResult is CBC_Result cbcResult => ValidateCBCResults(cbcResult),
                "urine" when testResult is Urine_Result urineResult => ValidateUrineResults(urineResult),
                "stool" when testResult is Stool_Result stoolResult => ValidateStoolResults(stoolResult),
                _ => ValidationResult.Success()
            };
        }

        public async Task<ValidationResult> ValidateComprehensiveExamAsync(Exam exam)
        {
            var results = new List<ValidationResult>();

            // Basic exam validation
            results.Add(ValidateExamData(exam));

            // Patient validation if available
            if (exam.Patient != null)
                results.Add(ValidatePatientData(exam.Patient));

            // Test-specific validation based on exam type
            await Task.Run(() =>
            {
                switch (exam.ExamType?.ToLower())
                {
                    case "casa":
                        // Validate CASA-specific requirements
                        if (string.IsNullOrWhiteSpace(exam.VideoPath))
                            results.Add(ValidationResult.Failure("مسار الفيديو مطلوب لتحليل CASA - Video path required for CASA analysis"));
                        break;
                    case "cbc":
                        // Validate CBC-specific requirements
                        break;
                    case "urine":
                        // Validate urine-specific requirements
                        break;
                    case "stool":
                        // Validate stool-specific requirements
                        break;
                }
            });

            // Combine all validation results
            return results.Where(r => !r.IsSuccess)
                          .SelectMany(r => r.Errors)
                          .Any() 
                   ? ValidationResult.Failure(results.Where(r => !r.IsSuccess).SelectMany(r => r.Errors))
                   : ValidationResult.Success();
        }

        private ValidationResult ValidateWHO2021Parameter(double value, string parameterKey, string arabicName)
        {
            if (!WHO2021References.TryGetValue(parameterKey, out var range))
                return ValidationResult.Success();

            return ValidateNumericRange(value, range.Min, range.Max, arabicName);
        }

        private ValidationResult ValidateCBCParameter(double value, string parameterKey, string arabicName)
        {
            if (!CBCReferences.TryGetValue(parameterKey, out var range))
                return ValidationResult.Success();

            return ValidateNumericRange(value, range.Min, range.Max, arabicName);
        }

        private ValidationResult ValidateUrineParameter(double value, string parameterKey, string arabicName)
        {
            if (!UrineReferences.TryGetValue(parameterKey, out var range))
                return ValidationResult.Success();

            return ValidateNumericRange(value, range.Min, range.Max, arabicName);
        }
    }
}