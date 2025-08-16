using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
    public class UrineAnalyzer
    {
        private readonly ILogger<UrineAnalyzer> _logger;

        public UrineAnalyzer(ILogger<UrineAnalyzer> logger = null)
        {
            _logger = logger;
        }

        public async Task<UrineTestResult> AnalyzeUrineAsync(UrineTestResult testData)
        {
            try
            {
                _logger?.LogInformation("Starting urine analysis for patient: {PatientId}", testData.PatientId);
                
                // تحليل النتائج
                AnalyzeResults(testData);
                
                // توليد التفسير
                testData.Interpretation = GenerateInterpretation(testData);
                
                testData.AnalysisDate = DateTime.Now;
                testData.Status = "Completed";
                
                _logger?.LogInformation("Urine analysis completed for patient: {PatientId}", testData.PatientId);
                return testData;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Urine analysis failed for patient: {PatientId}", testData.PatientId);
                testData.Status = "Failed";
                testData.Interpretation = $"Analysis failed: {ex.Message}";
                return testData;
            }
        }

        private void AnalyzeResults(UrineTestResult testData)
        {
            // Analyze pH
            if (testData.pH < 4.5 || testData.pH > 8.0)
            {
                testData.pHStatus = "Abnormal";
            }
            else
            {
                testData.pHStatus = "Normal";
            }

            // Analyze specific gravity
            if (testData.SpecificGravity < 1.005 || testData.SpecificGravity > 1.030)
            {
                testData.SpecificGravityStatus = "Abnormal";
            }
            else
            {
                testData.SpecificGravityStatus = "Normal";
            }

            // Analyze protein
            if (testData.Protein > 20)
            {
                testData.ProteinStatus = "High";
            }
            else if (testData.Protein > 0)
            {
                testData.ProteinStatus = "Trace";
            }
            else
            {
                testData.ProteinStatus = "Normal";
            }

            // Analyze glucose
            if (testData.Glucose > 0)
            {
                testData.GlucoseStatus = "Present";
            }
            else
            {
                testData.GlucoseStatus = "Normal";
            }

            // Analyze ketones
            if (testData.Ketones > 0)
            {
                testData.KetonesStatus = "Present";
            }
            else
            {
                testData.KetonesStatus = "Normal";
            }

            // Analyze blood
            if (testData.Blood > 0)
            {
                testData.BloodStatus = "Present";
            }
            else
            {
                testData.BloodStatus = "Normal";
            }

            // Analyze leukocytes
            if (testData.Leukocytes > 0)
            {
                testData.LeukocytesStatus = "Present";
            }
            else
            {
                testData.LeukocytesStatus = "Normal";
            }

            // Analyze nitrites
            if (testData.Nitrites)
            {
                testData.NitritesStatus = "Positive";
            }
            else
            {
                testData.NitritesStatus = "Negative";
            }

            // Analyze bilirubin
            if (testData.Bilirubin > 0)
            {
                testData.BilirubinStatus = "Present";
            }
            else
            {
                testData.BilirubinStatus = "Normal";
            }

            // Analyze urobilinogen
            if (testData.Urobilinogen > 1.0)
            {
                testData.UrobilinogenStatus = "High";
            }
            else
            {
                testData.UrobilinogenStatus = "Normal";
            }
        }

        private string GenerateInterpretation(UrineTestResult testData)
        {
            var interpretation = new List<string>();

            // pH interpretation
            if (testData.pHStatus == "Abnormal")
            {
                if (testData.pH < 4.5)
                    interpretation.Add("Acidic urine - may indicate metabolic acidosis or high protein diet");
                else if (testData.pH > 8.0)
                    interpretation.Add("Alkaline urine - may indicate urinary tract infection or vegetarian diet");
            }

            // Specific gravity interpretation
            if (testData.SpecificGravityStatus == "Abnormal")
            {
                if (testData.SpecificGravity < 1.005)
                    interpretation.Add("Low specific gravity - may indicate diabetes insipidus or excessive fluid intake");
                else if (testData.SpecificGravity > 1.030)
                    interpretation.Add("High specific gravity - may indicate dehydration or diabetes mellitus");
            }

            // Protein interpretation
            if (testData.ProteinStatus == "High")
                interpretation.Add("Proteinuria detected - may indicate kidney disease, urinary tract infection, or hypertension");

            // Glucose interpretation
            if (testData.GlucoseStatus == "Present")
                interpretation.Add("Glucosuria detected - may indicate diabetes mellitus or renal glycosuria");

            // Ketones interpretation
            if (testData.KetonesStatus == "Present")
                interpretation.Add("Ketonuria detected - may indicate diabetic ketoacidosis, starvation, or low-carbohydrate diet");

            // Blood interpretation
            if (testData.BloodStatus == "Present")
                interpretation.Add("Hematuria detected - may indicate urinary tract infection, kidney stones, or kidney disease");

            // Leukocytes interpretation
            if (testData.LeukocytesStatus == "Present")
                interpretation.Add("Leukocyturia detected - may indicate urinary tract infection or inflammation");

            // Nitrites interpretation
            if (testData.NitritesStatus == "Positive")
                interpretation.Add("Nitrites positive - may indicate bacterial urinary tract infection");

            // Bilirubin interpretation
            if (testData.BilirubinStatus == "Present")
                interpretation.Add("Bilirubinuria detected - may indicate liver disease or bile duct obstruction");

            // Urobilinogen interpretation
            if (testData.UrobilinogenStatus == "High")
                interpretation.Add("High urobilinogen - may indicate liver disease or hemolytic anemia");

            // Overall assessment
            if (interpretation.Count == 0)
            {
                interpretation.Add("Normal urinalysis results");
            }
            else
            {
                interpretation.Insert(0, "Abnormal findings detected:");
            }

            return string.Join("\n", interpretation);
        }

        public async Task<bool> ValidateUrineResultsAsync(UrineTestResult testData)
        {
            try
            {
                _logger?.LogInformation("Validating urine test results for patient: {PatientId}", testData.PatientId);

                var errors = new List<string>();

                // Validate pH
                if (testData.pH < 0 || testData.pH > 14)
                    errors.Add("pH value is out of valid range (0-14)");

                // Validate specific gravity
                if (testData.SpecificGravity < 1.000 || testData.SpecificGravity > 1.050)
                    errors.Add("Specific gravity is out of valid range (1.000-1.050)");

                // Validate protein
                if (testData.Protein < 0)
                    errors.Add("Protein value cannot be negative");

                // Validate glucose
                if (testData.Glucose < 0)
                    errors.Add("Glucose value cannot be negative");

                // Validate ketones
                if (testData.Ketones < 0)
                    errors.Add("Ketones value cannot be negative");

                // Validate blood
                if (testData.Blood < 0)
                    errors.Add("Blood value cannot be negative");

                // Validate leukocytes
                if (testData.Leukocytes < 0)
                    errors.Add("Leukocytes value cannot be negative");

                // Validate urobilinogen
                if (testData.Urobilinogen < 0)
                    errors.Add("Urobilinogen value cannot be negative");

                if (errors.Count > 0)
                {
                    _logger?.LogWarning("Urine test validation failed: {Errors}", string.Join(", ", errors));
                    return false;
                }

                _logger?.LogInformation("Urine test validation passed for patient: {PatientId}", testData.PatientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Urine test validation failed for patient: {PatientId}", testData.PatientId);
                return false;
            }
        }
    }
}