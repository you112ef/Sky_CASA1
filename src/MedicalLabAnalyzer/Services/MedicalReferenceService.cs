using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
    public class MedicalReferenceService : IMedicalReferenceService
    {
        #region CBC Reference Values

        public (decimal min, decimal max) GetRBCReference(int ageYears, string gender)
        {
            // Age and gender specific RBC reference ranges (10^12/L)
            return ageYears switch
            {
                < 1 => (3.0m, 5.3m),  // Infants
                >= 1 and < 6 => (4.0m, 5.0m),  // Children 1-5 years
                >= 6 and < 12 => (4.0m, 5.2m),  // Children 6-11 years
                >= 12 and < 18 => gender.ToLower() switch
                {
                    "ذكر" or "male" or "m" => (4.5m, 5.3m),  // Adolescent males
                    _ => (4.1m, 5.1m)   // Adolescent females
                },
                _ => gender.ToLower() switch  // Adults
                {
                    "ذكر" or "male" or "m" => (4.5m, 5.9m),  // Adult males
                    _ => (4.0m, 5.2m)   // Adult females
                }
            };
        }

        public (decimal min, decimal max) GetHemoglobinReference(int ageYears, string gender)
        {
            // Age and gender specific Hemoglobin reference ranges (g/dL)
            return ageYears switch
            {
                < 1 => (10.0m, 15.0m),  // Infants
                >= 1 and < 6 => (11.5m, 13.5m),  // Children 1-5 years
                >= 6 and < 12 => (11.5m, 15.5m),  // Children 6-11 years
                >= 12 and < 18 => gender.ToLower() switch
                {
                    "ذكر" or "male" or "m" => (13.0m, 16.0m),  // Adolescent males
                    _ => (12.0m, 15.2m)   // Adolescent females
                },
                _ => gender.ToLower() switch  // Adults
                {
                    "ذكر" or "male" or "m" => (13.5m, 17.5m),  // Adult males
                    _ => (12.0m, 15.5m)   // Adult females
                }
            };
        }

        public (decimal min, decimal max) GetHematocritReference(int ageYears, string gender)
        {
            // Age and gender specific Hematocrit reference ranges (%)
            return ageYears switch
            {
                < 1 => (31.0m, 45.0m),  // Infants
                >= 1 and < 6 => (35.0m, 40.0m),  // Children 1-5 years
                >= 6 and < 12 => (35.0m, 45.0m),  // Children 6-11 years
                >= 12 and < 18 => gender.ToLower() switch
                {
                    "ذكر" or "male" or "m" => (37.0m, 49.0m),  // Adolescent males
                    _ => (36.0m, 46.0m)   // Adolescent females
                },
                _ => gender.ToLower() switch  // Adults
                {
                    "ذكر" or "male" or "m" => (41.0m, 50.0m),  // Adult males
                    _ => (36.0m, 46.0m)   // Adult females
                }
            };
        }

        public (decimal min, decimal max) GetMCVReference(int ageYears, string gender)
        {
            // MCV reference ranges by age (fL) - less gender specific
            return ageYears switch
            {
                < 1 => (70.0m, 86.0m),   // Infants
                >= 1 and < 6 => (75.0m, 87.0m),   // Children 1-5 years
                >= 6 and < 12 => (77.0m, 95.0m),   // Children 6-11 years
                >= 12 and < 18 => (78.0m, 98.0m),   // Adolescents
                _ => (80.0m, 100.0m)  // Adults
            };
        }

        public (decimal min, decimal max) GetMCHReference(int ageYears, string gender)
        {
            // MCH reference ranges by age (pg)
            return ageYears switch
            {
                < 1 => (24.0m, 30.0m),   // Infants
                >= 1 and < 6 => (25.0m, 31.0m),   // Children 1-5 years
                >= 6 and < 12 => (25.0m, 33.0m),   // Children 6-11 years
                >= 12 and < 18 => (25.0m, 35.0m),   // Adolescents
                _ => (27.0m, 32.0m)  // Adults
            };
        }

        public (decimal min, decimal max) GetMCHCReference(int ageYears, string gender)
        {
            // MCHC reference ranges (g/dL) - relatively stable across ages
            return (32.0m, 36.0m);
        }

        public (decimal min, decimal max) GetRDWReference(int ageYears, string gender)
        {
            // RDW reference ranges (%) - stable across ages
            return (11.5m, 14.5m);
        }

        public (decimal min, decimal max) GetWBCReference(int ageYears, string gender)
        {
            // Age-specific WBC reference ranges (10^9/L)
            return ageYears switch
            {
                < 1 => (6.0m, 17.5m),   // Infants
                >= 1 and < 6 => (5.5m, 15.5m),   // Children 1-5 years
                >= 6 and < 12 => (4.5m, 13.5m),   // Children 6-11 years
                >= 12 and < 18 => (4.5m, 13.0m),   // Adolescents
                _ => (4.0m, 11.0m)  // Adults
            };
        }

        public (decimal min, decimal max) GetNeutrophilsReference(int ageYears, string gender)
        {
            // Age-specific Neutrophils reference ranges (%)
            return ageYears switch
            {
                < 1 => (30.0m, 60.0m),   // Infants
                >= 1 and < 6 => (35.0m, 65.0m),   // Children 1-5 years
                >= 6 and < 12 => (40.0m, 60.0m),   // Children 6-11 years
                _ => (40.0m, 70.0m)  // Adolescents and Adults
            };
        }

        public (decimal min, decimal max) GetLymphocytesReference(int ageYears, string gender)
        {
            // Age-specific Lymphocytes reference ranges (%)
            return ageYears switch
            {
                < 1 => (40.0m, 70.0m),   // Infants - higher in children
                >= 1 and < 6 => (35.0m, 65.0m),   // Children 1-5 years
                >= 6 and < 12 => (30.0m, 50.0m),   // Children 6-11 years
                _ => (20.0m, 40.0m)  // Adolescents and Adults
            };
        }

        public (decimal min, decimal max) GetMonocytesReference(int ageYears, string gender)
        {
            // Monocytes reference ranges (%) - relatively stable
            return (2.0m, 8.0m);
        }

        public (decimal min, decimal max) GetEosinophilsReference(int ageYears, string gender)
        {
            // Eosinophils reference ranges (%)
            return (1.0m, 4.0m);
        }

        public (decimal min, decimal max) GetBasophilsReference(int ageYears, string gender)
        {
            // Basophils reference ranges (%)
            return (0.5m, 1.0m);
        }

        public (decimal min, decimal max) GetPlateletsReference(int ageYears, string gender)
        {
            // Age-specific Platelet reference ranges (10^9/L)
            return ageYears switch
            {
                < 1 => (150.0m, 450.0m),   // Infants
                >= 1 and < 6 => (150.0m, 450.0m),   // Children
                _ => (150.0m, 450.0m)  // Adolescents and Adults
            };
        }

        public (decimal min, decimal max) GetMPVReference(int ageYears, string gender)
        {
            // MPV reference ranges (fL)
            return (7.5m, 11.5m);
        }

        public (decimal min, decimal max) GetPDWReference(int ageYears, string gender)
        {
            // PDW reference ranges (%)
            return (10.0m, 17.0m);
        }

        public (decimal min, decimal max) GetPCTReference(int ageYears, string gender)
        {
            // PCT reference ranges (%)
            return (0.1m, 0.3m);
        }

        public (decimal min, decimal max) GetReticulocytesReference(int ageYears, string gender)
        {
            // Reticulocytes reference ranges (%)
            return ageYears switch
            {
                < 1 => (0.5m, 6.0m),   // Higher in infants
                _ => (0.5m, 2.5m)  // Children and Adults
            };
        }

        public (decimal min, decimal max) GetESRReference(int ageYears, string gender)
        {
            // Age and gender specific ESR reference ranges (mm/hr)
            if (ageYears < 18)
            {
                return (0.0m, 10.0m);  // Children
            }

            return gender.ToLower() switch
            {
                "ذكر" or "male" or "m" => ageYears switch
                {
                    < 50 => (0.0m, 15.0m),   // Adult males < 50
                    _ => (0.0m, 20.0m)       // Adult males >= 50
                },
                _ => ageYears switch
                {
                    < 50 => (0.0m, 20.0m),   // Adult females < 50
                    _ => (0.0m, 30.0m)       // Adult females >= 50
                }
            };
        }

        public (decimal min, decimal max) GetCRPReference(int ageYears, string gender)
        {
            // CRP reference ranges (mg/L) - relatively stable across ages
            return (0.0m, 3.0m);
        }

        #endregion

        #region Lipid Profile Reference Values

        public (decimal optimal, decimal borderline, decimal high, decimal veryHigh) GetTotalCholesterolReference(int ageYears, string gender)
        {
            // Total Cholesterol thresholds (mg/dL)
            // Optimal: <200, Borderline: 200-239, High: ≥240
            return (200.0m, 239.0m, 240.0m, 300.0m);
        }

        public (decimal low, decimal normal, decimal high) GetHDLReference(int ageYears, string gender)
        {
            // HDL Cholesterol thresholds (mg/dL)
            return gender.ToLower() switch
            {
                "ذكر" or "male" or "m" => (40.0m, 60.0m, 100.0m),  // Male: Low <40, Normal 40-60, High >60
                _ => (50.0m, 70.0m, 100.0m)   // Female: Low <50, Normal 50-70, High >70
            };
        }

        public (decimal optimal, decimal nearOptimal, decimal borderline, decimal high, decimal veryHigh) GetLDLReference(int ageYears, string gender)
        {
            // LDL Cholesterol thresholds (mg/dL)
            // Optimal: <100, Near Optimal: 100-129, Borderline: 130-159, High: 160-189, Very High: ≥190
            return (100.0m, 129.0m, 159.0m, 189.0m, 300.0m);
        }

        public (decimal normal, decimal borderline, decimal high, decimal veryHigh) GetTriglyceridesReference(int ageYears, string gender)
        {
            // Triglycerides thresholds (mg/dL)
            // Normal: <150, Borderline: 150-199, High: 200-499, Very High: ≥500
            return (150.0m, 199.0m, 499.0m, 1000.0m);
        }

        #endregion

        #region Glucose Reference Values

        public (decimal normal, decimal prediabetes, decimal diabetes) GetFastingGlucoseReference(int ageYears, string gender)
        {
            // Fasting Glucose thresholds (mg/dL)
            // Normal: <100, Prediabetes: 100-125, Diabetes: ≥126
            return (100.0m, 125.0m, 400.0m);
        }

        public (decimal normal, decimal prediabetes, decimal diabetes) GetRandomGlucoseReference(int ageYears, string gender)
        {
            // Random Glucose thresholds (mg/dL)
            // Normal: <140, Elevated: 140-199, Diabetes: ≥200
            return (140.0m, 199.0m, 600.0m);
        }

        public (decimal normal, decimal prediabetes, decimal diabetes) GetHbA1cReference(int ageYears, string gender)
        {
            // HbA1c thresholds (%)
            // Normal: <5.7, Prediabetes: 5.7-6.4, Diabetes: ≥6.5
            return (5.7m, 6.4m, 15.0m);
        }

        #endregion

        #region Kidney Function Reference Values

        public (decimal min, decimal max) GetCreatinineReference(int ageYears, string gender)
        {
            // Age and gender specific Creatinine reference ranges (mg/dL)
            if (ageYears < 18)
            {
                return ageYears switch
                {
                    < 1 => (0.2m, 0.4m),     // Infants
                    >= 1 and < 10 => (0.3m, 0.7m),   // Children 1-9
                    _ => (0.5m, 1.0m)        // Children 10-17
                };
            }

            return gender.ToLower() switch
            {
                "ذكر" or "male" or "m" => (0.7m, 1.3m),  // Adult males
                _ => (0.6m, 1.1m)   // Adult females
            };
        }

        public (decimal min, decimal max) GetBUNReference(int ageYears, string gender)
        {
            // BUN reference ranges (mg/dL)
            return ageYears switch
            {
                < 1 => (5.0m, 18.0m),     // Infants
                >= 1 and < 18 => (7.0m, 20.0m),   // Children
                _ => (6.0m, 24.0m)        // Adults
            };
        }

        public (decimal min, decimal max) GetUricAcidReference(int ageYears, string gender)
        {
            // Age and gender specific Uric Acid reference ranges (mg/dL)
            if (ageYears < 18)
            {
                return (2.5m, 5.5m);  // Children
            }

            return gender.ToLower() switch
            {
                "ذكر" or "male" or "m" => (3.4m, 7.0m),  // Adult males
                _ => (2.4m, 6.0m)   // Adult females
            };
        }

        #endregion

        #region Liver Function Reference Values

        public (decimal min, decimal max) GetALTReference(int ageYears, string gender)
        {
            // ALT reference ranges (U/L)
            return gender.ToLower() switch
            {
                "ذكر" or "male" or "m" => (10.0m, 40.0m),  // Males
                _ => (7.0m, 35.0m)   // Females
            };
        }

        public (decimal min, decimal max) GetASTReference(int ageYears, string gender)
        {
            // AST reference ranges (U/L)
            return gender.ToLower() switch
            {
                "ذكر" or "male" or "m" => (10.0m, 40.0m),  // Males
                _ => (9.0m, 32.0m)   // Females
            };
        }

        public (decimal min, decimal max) GetAlkalinePhosphataseReference(int ageYears, string gender)
        {
            // Age-specific Alkaline Phosphatase reference ranges (U/L)
            return ageYears switch
            {
                < 1 => (80.0m, 306.0m),     // Infants
                >= 1 and < 10 => (80.0m, 306.0m),   // Children 1-9
                >= 10 and < 18 => (50.0m, 130.0m),  // Adolescents
                _ => (44.0m, 147.0m)        // Adults
            };
        }

        public (decimal min, decimal max) GetBilirubinTotalReference(int ageYears, string gender)
        {
            // Total Bilirubin reference ranges (mg/dL)
            return ageYears switch
            {
                < 1 => (1.0m, 12.0m),     // Neonates (higher due to physiological jaundice)
                _ => (0.2m, 1.2m)         // Children and Adults
            };
        }

        public (decimal min, decimal max) GetBilirubinDirectReference(int ageYears, string gender)
        {
            // Direct Bilirubin reference ranges (mg/dL)
            return (0.0m, 0.3m);
        }

        #endregion

        #region Thyroid Function Reference Values

        public (decimal min, decimal max) GetTSHReference(int ageYears, string gender)
        {
            // Age-specific TSH reference ranges (mIU/L)
            return ageYears switch
            {
                < 1 => (0.7m, 15.2m),     // Infants
                >= 1 and < 18 => (0.7m, 5.7m),   // Children
                _ => (0.27m, 4.2m)        // Adults
            };
        }

        public (decimal min, decimal max) GetT3Reference(int ageYears, string gender)
        {
            // T3 reference ranges (ng/dL)
            return ageYears switch
            {
                < 1 => (100.0m, 292.0m),     // Infants
                >= 1 and < 18 => (94.0m, 269.0m),   // Children
                _ => (80.0m, 200.0m)          // Adults
            };
        }

        public (decimal min, decimal max) GetT4Reference(int ageYears, string gender)
        {
            // T4 reference ranges (μg/dL)
            return ageYears switch
            {
                < 1 => (6.0m, 13.9m),     // Infants
                >= 1 and < 18 => (5.4m, 12.5m),   // Children
                _ => (4.5m, 12.0m)        // Adults
            };
        }

        #endregion

        #region Electrolytes Reference Values

        public (decimal min, decimal max) GetSodiumReference(int ageYears, string gender)
        {
            // Sodium reference ranges (mEq/L) - stable across ages
            return (136.0m, 145.0m);
        }

        public (decimal min, decimal max) GetPotassiumReference(int ageYears, string gender)
        {
            // Potassium reference ranges (mEq/L)
            return ageYears switch
            {
                < 1 => (4.1m, 5.3m),     // Infants
                _ => (3.5m, 5.2m)        // Children and Adults
            };
        }

        public (decimal min, decimal max) GetChlorideReference(int ageYears, string gender)
        {
            // Chloride reference ranges (mEq/L)
            return (98.0m, 107.0m);
        }

        public (decimal min, decimal max) GetCalciumReference(int ageYears, string gender)
        {
            // Age-specific Calcium reference ranges (mg/dL)
            return ageYears switch
            {
                < 1 => (7.6m, 10.4m),     // Infants
                >= 1 and < 18 => (8.8m, 10.8m),   // Children
                _ => (8.4m, 10.2m)        // Adults
            };
        }

        public (decimal min, decimal max) GetPhosphorusReference(int ageYears, string gender)
        {
            // Age-specific Phosphorus reference ranges (mg/dL)
            return ageYears switch
            {
                < 1 => (4.3m, 9.3m),     // Infants
                >= 1 and < 10 => (3.8m, 6.5m),   // Children 1-9
                >= 10 and < 18 => (2.9m, 5.4m),  // Adolescents
                _ => (2.7m, 4.5m)         // Adults
            };
        }

        public (decimal min, decimal max) GetMagnesiumReference(int ageYears, string gender)
        {
            // Magnesium reference ranges (mg/dL)
            return (1.7m, 2.2m);
        }

        #endregion

        #region Helper Methods

        public string GetTestStatus(decimal value, decimal min, decimal max)
        {
            if (value < min) return "Low";
            if (value > max) return "High";
            return "Normal";
        }

        public string GetLipidStatus(decimal value, params decimal[] thresholds)
        {
            return thresholds.Length switch
            {
                3 => value switch  // HDL pattern
                {
                    var v when v < thresholds[0] => "Low",
                    var v when v <= thresholds[1] => "Normal",
                    _ => "High"
                },
                4 => value switch  // Total Cholesterol or Triglycerides pattern
                {
                    var v when v < thresholds[0] => "Optimal",
                    var v when v < thresholds[1] => "Borderline",
                    var v when v < thresholds[2] => "High",
                    _ => "Very High"
                },
                5 => value switch  // LDL pattern
                {
                    var v when v < thresholds[0] => "Optimal",
                    var v when v < thresholds[1] => "Near Optimal",
                    var v when v < thresholds[2] => "Borderline",
                    var v when v < thresholds[3] => "High",
                    _ => "Very High"
                },
                _ => "Unknown"
            };
        }

        public string GetGlucoseStatus(decimal value, decimal normal, decimal prediabetes, decimal diabetes)
        {
            return value switch
            {
                var v when v < normal => "Normal",
                var v when v < prediabetes => "Prediabetes",
                _ => "Diabetes"
            };
        }

        public void UpdateCBCReferenceRanges(CBCTestResult cbcResult, Patient patient)
        {
            var age = patient.Age;
            var gender = patient.Gender;

            // Update RBC reference ranges
            var rbcRef = GetRBCReference(age, gender);
            cbcResult.RBCMin = rbcRef.min;
            cbcResult.RBCMax = rbcRef.max;

            // Update Hemoglobin reference ranges
            var hbRef = GetHemoglobinReference(age, gender);
            cbcResult.HemoglobinMin = hbRef.min;
            cbcResult.HemoglobinMax = hbRef.max;

            // Update Hematocrit reference ranges
            var hctRef = GetHematocritReference(age, gender);
            cbcResult.HematocritMin = hctRef.min;
            cbcResult.HematocritMax = hctRef.max;

            // Update MCV reference ranges
            var mcvRef = GetMCVReference(age, gender);
            cbcResult.MCVMin = mcvRef.min;
            cbcResult.MCVMax = mcvRef.max;

            // Update MCH reference ranges
            var mchRef = GetMCHReference(age, gender);
            cbcResult.MCHMin = mchRef.min;
            cbcResult.MCHMax = mchRef.max;

            // Update MCHC reference ranges
            var mchcRef = GetMCHCReference(age, gender);
            cbcResult.MCHCMin = mchcRef.min;
            cbcResult.MCHCMax = mchcRef.max;

            // Update RDW reference ranges
            var rdwRef = GetRDWReference(age, gender);
            cbcResult.RDWMin = rdwRef.min;
            cbcResult.RDWMax = rdwRef.max;

            // Update WBC reference ranges
            var wbcRef = GetWBCReference(age, gender);
            cbcResult.WBCMin = wbcRef.min;
            cbcResult.WBCMax = wbcRef.max;

            // Update Neutrophils reference ranges
            var neutRef = GetNeutrophilsReference(age, gender);
            cbcResult.NeutrophilsMin = neutRef.min;
            cbcResult.NeutrophilsMax = neutRef.max;

            // Update Lymphocytes reference ranges
            var lymphRef = GetLymphocytesReference(age, gender);
            cbcResult.LymphocytesMin = lymphRef.min;
            cbcResult.LymphocytesMax = lymphRef.max;

            // Update Monocytes reference ranges
            var monoRef = GetMonocytesReference(age, gender);
            cbcResult.MonocytesMin = monoRef.min;
            cbcResult.MonocytesMax = monoRef.max;

            // Update Eosinophils reference ranges
            var eosinoRef = GetEosinophilsReference(age, gender);
            cbcResult.EosinophilsMin = eosinoRef.min;
            cbcResult.EosinophilsMax = eosinoRef.max;

            // Update Basophils reference ranges
            var basoRef = GetBasophilsReference(age, gender);
            cbcResult.BasophilsMin = basoRef.min;
            cbcResult.BasophilsMax = basoRef.max;

            // Update Platelets reference ranges
            var platRef = GetPlateletsReference(age, gender);
            cbcResult.PlateletsMin = platRef.min;
            cbcResult.PlateletsMax = platRef.max;

            // Update MPV reference ranges
            var mpvRef = GetMPVReference(age, gender);
            cbcResult.MPVMin = mpvRef.min;
            cbcResult.MPVMax = mpvRef.max;

            // Update PDW reference ranges
            var pdwRef = GetPDWReference(age, gender);
            cbcResult.PDWMin = pdwRef.min;
            cbcResult.PDWMax = pdwRef.max;

            // Update PCT reference ranges
            var pctRef = GetPCTReference(age, gender);
            cbcResult.PCTMin = pctRef.min;
            cbcResult.PCTMax = pctRef.max;

            // Update Reticulocytes reference ranges
            var reticRef = GetReticulocytesReference(age, gender);
            cbcResult.ReticulocytesMin = reticRef.min;
            cbcResult.ReticulocytesMax = reticRef.max;

            // Update ESR reference ranges
            var esrRef = GetESRReference(age, gender);
            cbcResult.ESRMin = esrRef.min;
            cbcResult.ESRMax = esrRef.max;

            // Update CRP reference ranges
            var crpRef = GetCRPReference(age, gender);
            cbcResult.CRPMin = crpRef.min;
            cbcResult.CRPMax = crpRef.max;
        }

        #endregion
    }
}