using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace MedicalLabAnalyzer.Services
{
    public class ReportService
    {
        private readonly ILogger<ReportService> _logger;
        private readonly IConfiguration _configuration;

        public ReportService(ILogger<ReportService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GenerateCBCReportAsync(CBCTestResult cbcResult, Exam exam, Patient patient)
        {
            try
            {
                _logger.LogInformation("Generating CBC report for exam {ExamId}", exam.Id);

                // Create document
                var document = new Document();
                document.Info.Title = "تقرير تحليل الدم الشامل";
                document.Info.Author = "MedicalLabAnalyzer";
                document.Info.Subject = $"CBC Report - {patient.FullName}";

                // Add styles
                DefineStyles(document);

                // Add content
                AddCBCContent(document, cbcResult, exam, patient);

                // Generate PDF
                var renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();

                // Save file
                var reportsPath = _configuration["Reports:OutputPath"] ?? "Reports/GeneratedReports";
                if (!Directory.Exists(reportsPath))
                {
                    Directory.CreateDirectory(reportsPath);
                }

                var fileName = $"CBC_Report_{patient.NationalId}_{exam.ExamDate:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(reportsPath, fileName);

                renderer.PdfDocument.Save(filePath);
                renderer.PdfDocument.Dispose();

                _logger.LogInformation("CBC report generated successfully: {FilePath}", filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating CBC report for exam {ExamId}", exam.Id);
                throw;
            }
        }

        private void DefineStyles(Document document)
        {
            // Get the predefined style Normal
            var style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 10;

            // Heading1 style
            style = document.Styles["Heading1"];
            style.Font.Name = "Arial";
            style.Font.Size = 16;
            style.Font.Bold = true;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            style.ParagraphFormat.SpaceAfter = 10;

            // Heading2 style
            style = document.Styles["Heading2"];
            style.Font.Name = "Arial";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.ParagraphFormat.SpaceBefore = 10;
            style.ParagraphFormat.SpaceAfter = 5;

            // Table style
            style = document.Styles["Table"];
            style.Font.Name = "Arial";
            style.Font.Size = 9;
        }

        private void AddCBCContent(Document document, CBCTestResult cbcResult, Exam exam, Patient patient)
        {
            // Add header
            var section = document.AddSection();
            section.PageSetup.RightMargin = 20;
            section.PageSetup.LeftMargin = 20;
            section.PageSetup.TopMargin = 20;
            section.PageSetup.BottomMargin = 20;

            // Title
            var title = section.AddParagraph("تقرير تحليل الدم الشامل");
            title.Style = "Heading1";

            // Patient Information
            var patientInfo = section.AddParagraph("معلومات المريض");
            patientInfo.Style = "Heading2";

            var patientTable = section.AddTable();
            patientTable.Style = "Table";
            patientTable.Borders.Width = 0.5;

            // Add columns
            patientTable.AddColumn("3cm");
            patientTable.AddColumn("6cm");
            patientTable.AddColumn("3cm");
            patientTable.AddColumn("6cm");

            // Add rows
            var row = patientTable.AddRow();
            row.Cells[0].AddParagraph("الاسم:");
            row.Cells[1].AddParagraph(patient.FullName);
            row.Cells[2].AddParagraph("رقم الهوية:");
            row.Cells[3].AddParagraph(patient.NationalId);

            row = patientTable.AddRow();
            row.Cells[0].AddParagraph("تاريخ الميلاد:");
            row.Cells[1].AddParagraph(patient.DateOfBirth.ToString("dd/MM/yyyy"));
            row.Cells[2].AddParagraph("العمر:");
            row.Cells[3].AddParagraph($"{patient.Age} سنة");

            row = patientTable.AddRow();
            row.Cells[0].AddParagraph("الجنس:");
            row.Cells[1].AddParagraph(patient.Gender);
            row.Cells[2].AddParagraph("فصيلة الدم:");
            row.Cells[3].AddParagraph(patient.BloodType ?? "غير محدد");

            row = patientTable.AddRow();
            row.Cells[0].AddParagraph("رقم الهاتف:");
            row.Cells[1].AddParagraph(patient.PhoneNumber ?? "غير محدد");
            row.Cells[2].AddParagraph("البريد الإلكتروني:");
            row.Cells[3].AddParagraph(patient.Email ?? "غير محدد");

            // Exam Information
            section.AddParagraph();
            var examInfo = section.AddParagraph("معلومات الفحص");
            examInfo.Style = "Heading2";

            var examTable = section.AddTable();
            examTable.Style = "Table";
            examTable.Borders.Width = 0.5;

            examTable.AddColumn("3cm");
            examTable.AddColumn("6cm");
            examTable.AddColumn("3cm");
            examTable.AddColumn("6cm");

            row = examTable.AddRow();
            row.Cells[0].AddParagraph("رقم الفحص:");
            row.Cells[1].AddParagraph(exam.Id.ToString());
            row.Cells[2].AddParagraph("تاريخ الفحص:");
            row.Cells[3].AddParagraph(exam.ExamDate.ToString("dd/MM/yyyy"));

            row = examTable.AddRow();
            row.Cells[0].AddParagraph("نوع الفحص:");
            row.Cells[1].AddParagraph(exam.ExamType);
            row.Cells[2].AddParagraph("حالة الفحص:");
            row.Cells[3].AddParagraph(exam.StatusDisplay);

            row = examTable.AddRow();
            row.Cells[0].AddParagraph("تاريخ التحليل:");
            row.Cells[1].AddParagraph(cbcResult.AnalysisDate.ToString("dd/MM/yyyy"));
            row.Cells[2].AddParagraph("تم التحليل بواسطة:");
            row.Cells[3].AddParagraph(cbcResult.AnalyzedBy);

            // CBC Results
            section.AddParagraph();
            var resultsTitle = section.AddParagraph("نتائج التحليل");
            resultsTitle.Style = "Heading2";

            // RBC Results
            var rbcTitle = section.AddParagraph("خلايا الدم الحمراء (RBC)");
            rbcTitle.Style = "Heading2";

            var rbcTable = section.AddTable();
            rbcTable.Style = "Table";
            rbcTable.Borders.Width = 0.5;

            rbcTable.AddColumn("4cm");
            rbcTable.AddColumn("2cm");
            rbcTable.AddColumn("2cm");
            rbcTable.AddColumn("2cm");
            rbcTable.AddColumn("2cm");

            // Header row
            row = rbcTable.AddRow();
            row.Cells[0].AddParagraph("الفحص");
            row.Cells[1].AddParagraph("النتيجة");
            row.Cells[2].AddParagraph("النطاق الطبيعي");
            row.Cells[3].AddParagraph("الوحدة");
            row.Cells[4].AddParagraph("الحالة");

            // RBC data
            AddResultRow(rbcTable, "RBC", cbcResult.RBC, cbcResult.RBCMin, cbcResult.RBCMax, "10^12/L", cbcResult.RBCStatus);
            AddResultRow(rbcTable, "الهيموغلوبين", cbcResult.Hemoglobin, cbcResult.HemoglobinMin, cbcResult.HemoglobinMax, "g/dL", cbcResult.HemoglobinStatus);
            AddResultRow(rbcTable, "الهيماتوكريت", cbcResult.Hematocrit, cbcResult.HematocritMin, cbcResult.HematocritMax, "%", cbcResult.HematocritStatus);
            AddResultRow(rbcTable, "MCV", cbcResult.MCV, cbcResult.MCVMin, cbcResult.MCVMax, "fL", cbcResult.MCVStatus);
            AddResultRow(rbcTable, "MCH", cbcResult.MCH, cbcResult.MCHMin, cbcResult.MCHMax, "pg", cbcResult.MCHStatus);
            AddResultRow(rbcTable, "MCHC", cbcResult.MCHC, cbcResult.MCHCMin, cbcResult.MCHCMax, "g/dL", cbcResult.MCHCStatus);
            AddResultRow(rbcTable, "RDW", cbcResult.RDW, cbcResult.RDWMin, cbcResult.RDWMax, "%", cbcResult.RDWStatus);

            // WBC Results
            section.AddParagraph();
            var wbcTitle = section.AddParagraph("خلايا الدم البيضاء (WBC)");
            wbcTitle.Style = "Heading2";

            var wbcTable = section.AddTable();
            wbcTable.Style = "Table";
            wbcTable.Borders.Width = 0.5;

            wbcTable.AddColumn("4cm");
            wbcTable.AddColumn("2cm");
            wbcTable.AddColumn("2cm");
            wbcTable.AddColumn("2cm");
            wbcTable.AddColumn("2cm");

            // Header row
            row = wbcTable.AddRow();
            row.Cells[0].AddParagraph("الفحص");
            row.Cells[1].AddParagraph("النتيجة");
            row.Cells[2].AddParagraph("النطاق الطبيعي");
            row.Cells[3].AddParagraph("الوحدة");
            row.Cells[4].AddParagraph("الحالة");

            // WBC data
            AddResultRow(wbcTable, "WBC", cbcResult.WBC, cbcResult.WBCMin, cbcResult.WBCMax, "10^9/L", cbcResult.WBCStatus);
            AddResultRow(wbcTable, "العدلات", cbcResult.Neutrophils, cbcResult.NeutrophilsMin, cbcResult.NeutrophilsMax, "%", cbcResult.NeutrophilsStatus);
            AddResultRow(wbcTable, "الخلايا الليمفاوية", cbcResult.Lymphocytes, cbcResult.LymphocytesMin, cbcResult.LymphocytesMax, "%", cbcResult.LymphocytesStatus);
            AddResultRow(wbcTable, "الوحيدات", cbcResult.Monocytes, cbcResult.MonocytesMin, cbcResult.MonocytesMax, "%", cbcResult.MonocytesStatus);
            AddResultRow(wbcTable, "الحمضات", cbcResult.Eosinophils, cbcResult.EosinophilsMin, cbcResult.EosinophilsMax, "%", cbcResult.EosinophilsStatus);
            AddResultRow(wbcTable, "الخلايا القاعدية", cbcResult.Basophils, cbcResult.BasophilsMin, cbcResult.BasophilsMax, "%", cbcResult.BasophilsStatus);

            // Platelets Results
            section.AddParagraph();
            var pltTitle = section.AddParagraph("الصفائح الدموية");
            pltTitle.Style = "Heading2";

            var pltTable = section.AddTable();
            pltTable.Style = "Table";
            pltTable.Borders.Width = 0.5;

            pltTable.AddColumn("4cm");
            pltTable.AddColumn("2cm");
            pltTable.AddColumn("2cm");
            pltTable.AddColumn("2cm");
            pltTable.AddColumn("2cm");

            // Header row
            row = pltTable.AddRow();
            row.Cells[0].AddParagraph("الفحص");
            row.Cells[1].AddParagraph("النتيجة");
            row.Cells[2].AddParagraph("النطاق الطبيعي");
            row.Cells[3].AddParagraph("الوحدة");
            row.Cells[4].AddParagraph("الحالة");

            // Platelets data
            AddResultRow(pltTable, "الصفائح الدموية", cbcResult.Platelets, cbcResult.PlateletsMin, cbcResult.PlateletsMax, "10^9/L", cbcResult.PlateletsStatus);
            AddResultRow(pltTable, "MPV", cbcResult.MPV, cbcResult.MPVMin, cbcResult.MPVMax, "fL", cbcResult.MPVStatus);
            AddResultRow(pltTable, "PDW", cbcResult.PDW, cbcResult.PDWMin, cbcResult.PDWMax, "%", cbcResult.PDWStatus);
            AddResultRow(pltTable, "PCT", cbcResult.PCT, cbcResult.PCTMin, cbcResult.PCTMax, "%", cbcResult.PCTStatus);

            // Additional Tests
            section.AddParagraph();
            var addTitle = section.AddParagraph("فحوصات إضافية");
            addTitle.Style = "Heading2";

            var addTable = section.AddTable();
            addTable.Style = "Table";
            addTable.Borders.Width = 0.5;

            addTable.AddColumn("4cm");
            addTable.AddColumn("2cm");
            addTable.AddColumn("2cm");
            addTable.AddColumn("2cm");
            addTable.AddColumn("2cm");

            // Header row
            row = addTable.AddRow();
            row.Cells[0].AddParagraph("الفحص");
            row.Cells[1].AddParagraph("النتيجة");
            row.Cells[2].AddParagraph("النطاق الطبيعي");
            row.Cells[3].AddParagraph("الوحدة");
            row.Cells[4].AddParagraph("الحالة");

            // Additional data
            AddResultRow(addTable, "الخلايا الشبكية", cbcResult.Reticulocytes, cbcResult.ReticulocytesMin, cbcResult.ReticulocytesMax, "%", cbcResult.ReticulocytesStatus);
            AddResultRow(addTable, "ESR", cbcResult.ESR, cbcResult.ESRMin, cbcResult.ESRMax, "mm/hr", cbcResult.ESRStatus);
            AddResultRow(addTable, "CRP", cbcResult.CRP, cbcResult.CRPMin, cbcResult.CRPMax, "mg/L", cbcResult.CRPStatus);

            // Interpretation
            section.AddParagraph();
            var interpretationTitle = section.AddParagraph("التفسير والتوصيات");
            interpretationTitle.Style = "Heading2";

            if (!string.IsNullOrEmpty(cbcResult.Interpretation))
            {
                var interpretation = section.AddParagraph(cbcResult.Interpretation);
                interpretation.Format.Font.Size = 10;
                interpretation.Format.SpaceAfter = 10;
            }

            // Footer
            section.AddParagraph();
            var footer = section.AddParagraph($"تم إنشاء هذا التقرير في {DateTime.Now:dd/MM/yyyy HH:mm}");
            footer.Format.Alignment = ParagraphAlignment.Center;
            footer.Format.Font.Size = 8;
            footer.Format.Font.Color = Colors.Gray;
        }

        private void AddResultRow(Table table, string testName, decimal? result, decimal? min, decimal? max, string unit, string status)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph(testName);
            row.Cells[1].AddParagraph(result?.ToString("F2") ?? "غير محدد");
            row.Cells[2].AddParagraph(min.HasValue && max.HasValue ? $"{min:F1} - {max:F1}" : "غير محدد");
            row.Cells[3].AddParagraph(unit);
            
            var statusCell = row.Cells[4].AddParagraph(GetStatusInArabic(status));
            if (status == "High")
                statusCell.Format.Font.Color = Colors.Red;
            else if (status == "Low")
                statusCell.Format.Font.Color = Colors.Blue;
            else if (status == "Normal")
                statusCell.Format.Font.Color = Colors.Green;
        }

        private string GetStatusInArabic(string status)
        {
            return status switch
            {
                "Normal" => "طبيعي",
                "Low" => "منخفض",
                "High" => "مرتفع",
                "Abnormal" => "غير طبيعي",
                "Critical" => "حرج",
                _ => status ?? "غير محدد"
            };
        }

        public async Task<string> GeneratePatientReportAsync(Patient patient, List<Exam> exams)
        {
            try
            {
                _logger.LogInformation("Generating patient report for {PatientId}", patient.Id);

                var document = new Document();
                document.Info.Title = "تقرير المريض";
                document.Info.Author = "MedicalLabAnalyzer";
                document.Info.Subject = $"Patient Report - {patient.FullName}";

                DefineStyles(document);

                var section = document.AddSection();
                section.PageSetup.RightMargin = 20;
                section.PageSetup.LeftMargin = 20;
                section.PageSetup.TopMargin = 20;
                section.PageSetup.BottomMargin = 20;

                // Title
                var title = section.AddParagraph("تقرير المريض");
                title.Style = "Heading1";

                // Patient Information
                var patientInfo = section.AddParagraph("معلومات المريض");
                patientInfo.Style = "Heading2";

                var patientTable = section.AddTable();
                patientTable.Style = "Table";
                patientTable.Borders.Width = 0.5;

                patientTable.AddColumn("3cm");
                patientTable.AddColumn("6cm");
                patientTable.AddColumn("3cm");
                patientTable.AddColumn("6cm");

                var row = patientTable.AddRow();
                row.Cells[0].AddParagraph("الاسم:");
                row.Cells[1].AddParagraph(patient.FullName);
                row.Cells[2].AddParagraph("رقم الهوية:");
                row.Cells[3].AddParagraph(patient.NationalId);

                row = patientTable.AddRow();
                row.Cells[0].AddParagraph("تاريخ الميلاد:");
                row.Cells[1].AddParagraph(patient.DateOfBirth.ToString("dd/MM/yyyy"));
                row.Cells[2].AddParagraph("العمر:");
                row.Cells[3].AddParagraph($"{patient.Age} سنة");

                row = patientTable.AddRow();
                row.Cells[0].AddParagraph("الجنس:");
                row.Cells[1].AddParagraph(patient.Gender);
                row.Cells[2].AddParagraph("فصيلة الدم:");
                row.Cells[3].AddParagraph(patient.BloodType ?? "غير محدد");

                // Exams Summary
                section.AddParagraph();
                var examsTitle = section.AddParagraph("ملخص الفحوصات");
                examsTitle.Style = "Heading2";

                if (exams.Any())
                {
                    var examsTable = section.AddTable();
                    examsTable.Style = "Table";
                    examsTable.Borders.Width = 0.5;

                    examsTable.AddColumn("2cm");
                    examsTable.AddColumn("3cm");
                    examsTable.AddColumn("3cm");
                    examsTable.AddColumn("3cm");
                    examsTable.AddColumn("3cm");

                    // Header
                    row = examsTable.AddRow();
                    row.Cells[0].AddParagraph("رقم الفحص");
                    row.Cells[1].AddParagraph("نوع الفحص");
                    row.Cells[2].AddParagraph("تاريخ الفحص");
                    row.Cells[3].AddParagraph("حالة الفحص");
                    row.Cells[4].AddParagraph("النتيجة");

                    // Data
                    foreach (var exam in exams.OrderByDescending(e => e.ExamDate))
                    {
                        row = examsTable.AddRow();
                        row.Cells[0].AddParagraph(exam.Id.ToString());
                        row.Cells[1].AddParagraph(exam.ExamType);
                        row.Cells[2].AddParagraph(exam.ExamDate.ToString("dd/MM/yyyy"));
                        row.Cells[3].AddParagraph(exam.StatusDisplay);
                        row.Cells[4].AddParagraph(exam.OverallStatus ?? "غير محدد");
                    }
                }
                else
                {
                    var noExams = section.AddParagraph("لا توجد فحوصات لهذا المريض");
                    noExams.Format.Font.Italic = true;
                }

                // Generate PDF
                var renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();

                var reportsPath = _configuration["Reports:OutputPath"] ?? "Reports/GeneratedReports";
                if (!Directory.Exists(reportsPath))
                {
                    Directory.CreateDirectory(reportsPath);
                }

                var fileName = $"Patient_Report_{patient.NationalId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(reportsPath, fileName);

                renderer.PdfDocument.Save(filePath);
                renderer.PdfDocument.Dispose();

                _logger.LogInformation("Patient report generated successfully: {FilePath}", filePath);

                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating patient report for {PatientId}", patient.Id);
                throw;
            }
        }
    }
}