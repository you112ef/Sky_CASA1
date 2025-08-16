using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
    public interface IMedicalReferenceService
    {
        // CBC Reference Values
        (decimal min, decimal max) GetRBCReference(int ageYears, string gender);
        (decimal min, decimal max) GetHemoglobinReference(int ageYears, string gender);
        (decimal min, decimal max) GetHematocritReference(int ageYears, string gender);
        (decimal min, decimal max) GetMCVReference(int ageYears, string gender);
        (decimal min, decimal max) GetMCHReference(int ageYears, string gender);
        (decimal min, decimal max) GetMCHCReference(int ageYears, string gender);
        (decimal min, decimal max) GetRDWReference(int ageYears, string gender);
        (decimal min, decimal max) GetWBCReference(int ageYears, string gender);
        (decimal min, decimal max) GetNeutrophilsReference(int ageYears, string gender);
        (decimal min, decimal max) GetLymphocytesReference(int ageYears, string gender);
        (decimal min, decimal max) GetMonocytesReference(int ageYears, string gender);
        (decimal min, decimal max) GetEosinophilsReference(int ageYears, string gender);
        (decimal min, decimal max) GetBasophilsReference(int ageYears, string gender);
        (decimal min, decimal max) GetPlateletsReference(int ageYears, string gender);
        (decimal min, decimal max) GetMPVReference(int ageYears, string gender);
        (decimal min, decimal max) GetPDWReference(int ageYears, string gender);
        (decimal min, decimal max) GetPCTReference(int ageYears, string gender);
        (decimal min, decimal max) GetReticulocytesReference(int ageYears, string gender);
        (decimal min, decimal max) GetESRReference(int ageYears, string gender);
        (decimal min, decimal max) GetCRPReference(int ageYears, string gender);

        // Lipid Profile Reference Values
        (decimal optimal, decimal borderline, decimal high, decimal veryHigh) GetTotalCholesterolReference(int ageYears, string gender);
        (decimal low, decimal normal, decimal high) GetHDLReference(int ageYears, string gender);
        (decimal optimal, decimal nearOptimal, decimal borderline, decimal high, decimal veryHigh) GetLDLReference(int ageYears, string gender);
        (decimal normal, decimal borderline, decimal high, decimal veryHigh) GetTriglyceridesReference(int ageYears, string gender);

        // Glucose Reference Values
        (decimal normal, decimal prediabetes, decimal diabetes) GetFastingGlucoseReference(int ageYears, string gender);
        (decimal normal, decimal prediabetes, decimal diabetes) GetRandomGlucoseReference(int ageYears, string gender);
        (decimal normal, decimal prediabetes, decimal diabetes) GetHbA1cReference(int ageYears, string gender);

        // Kidney Function Reference Values
        (decimal min, decimal max) GetCreatinineReference(int ageYears, string gender);
        (decimal min, decimal max) GetBUNReference(int ageYears, string gender);
        (decimal min, decimal max) GetUricAcidReference(int ageYears, string gender);

        // Liver Function Reference Values
        (decimal min, decimal max) GetALTReference(int ageYears, string gender);
        (decimal min, decimal max) GetASTReference(int ageYears, string gender);
        (decimal min, decimal max) GetAlkalinePhosphataseReference(int ageYears, string gender);
        (decimal min, decimal max) GetBilirubinTotalReference(int ageYears, string gender);
        (decimal min, decimal max) GetBilirubinDirectReference(int ageYears, string gender);

        // Thyroid Function Reference Values
        (decimal min, decimal max) GetTSHReference(int ageYears, string gender);
        (decimal min, decimal max) GetT3Reference(int ageYears, string gender);
        (decimal min, decimal max) GetT4Reference(int ageYears, string gender);

        // Electrolytes Reference Values
        (decimal min, decimal max) GetSodiumReference(int ageYears, string gender);
        (decimal min, decimal max) GetPotassiumReference(int ageYears, string gender);
        (decimal min, decimal max) GetChlorideReference(int ageYears, string gender);
        (decimal min, decimal max) GetCalciumReference(int ageYears, string gender);
        (decimal min, decimal max) GetPhosphorusReference(int ageYears, string gender);
        (decimal min, decimal max) GetMagnesiumReference(int ageYears, string gender);

        // Helper methods
        string GetTestStatus(decimal value, decimal min, decimal max);
        string GetLipidStatus(decimal value, params decimal[] thresholds);
        string GetGlucoseStatus(decimal value, decimal normal, decimal prediabetes, decimal diabetes);
        
        // Update test result with proper reference ranges
        void UpdateCBCReferenceRanges(CBCTestResult cbcResult, Patient patient);
    }
}