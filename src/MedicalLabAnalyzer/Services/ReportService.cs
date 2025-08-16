using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using Dapper;

namespace MedicalLabAnalyzer.Services
{
    public class ReportService
    {
<<<<<<< HEAD
        private readonly string _outFolder;
        public ReportService(string outFolder = null)
        {
            _outFolder = outFolder ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "Output");
            if (!Directory.Exists(_outFolder)) Directory.CreateDirectory(_outFolder);
        }

        public string GenerateCASAReport(Patient p, CASA_Result res)
        {
            var doc = new PdfDocument();
            doc.Info.Title = $"CASA Report - {p.FullName}";
            var page = doc.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var fontH = new XFont("Segoe UI", 16, XFontStyle.Bold);
            var font = new XFont("Segoe UI", 12);
            gfx.DrawString("تقرير تحليل الحيوانات المنوية (CASA)", fontH, XBrushes.Black, new XRect(0, 20, page.Width, 40), XStringFormats.Center);

            gfx.DrawString($"الاسم: {p.FullName}", font, XBrushes.Black, new XRect(40, 80, 400, 20), XStringFormats.TopLeft);
            gfx.DrawString($"MRN: {p.MRN}", font, XBrushes.Black, new XRect(40, 110, 400, 20), XStringFormats.TopLeft);

            int y = 150;
            gfx.DrawString($"VCL: {res.VCL:F2} µm/s", font, XBrushes.Black, new XRect(40, y, 400, 20), XStringFormats.TopLeft); y += 25;
            gfx.DrawString($"VSL: {res.VSL:F2} µm/s", font, XBrushes.Black, new XRect(40, y, 400, 20), XStringFormats.TopLeft); y += 25;
            gfx.DrawString($"VAP: {res.VAP:F2} µm/s", font, XBrushes.Black, new XRect(40, y, 400, 20), XStringFormats.TopLeft); y += 25;
            gfx.DrawString($"ALH: {res.ALH:F2} µm", font, XBrushes.Black, new XRect(40, y, 400, 20), XStringFormats.TopLeft); y += 25;
            gfx.DrawString($"BCF: {res.BCF:F2} Hz", font, XBrushes.Black, new XRect(40, y, 400, 20), XStringFormats.TopLeft); y += 25;
            gfx.DrawString($"Motility: {res.MotilityPercent:F1} %", font, XBrushes.Black, new XRect(40, y, 400, 20), XStringFormats.TopLeft);

            var file = Path.Combine(_outFolder, $"CASA_{p.MRN}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            doc.Save(file);
            doc.Close();
            AuditLogger.Log("Report.Generate", $"Generated CASA report: {file}");
            return file;
        }
    }
=======
        private readonly IDbConnection _db;
        private readonly ILogger<ReportService> _logger;
        private readonly AuditLogger _auditLogger;
        private readonly string _reportsDirectory;

        public ReportService(IDbConnection db, ILogger<ReportService> logger = null, AuditLogger auditLogger = null)
        {
            _db = db;
            _logger = logger;
            _auditLogger = auditLogger;
            _reportsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            
            // إنشاء مجلد التقارير إذا لم يكن موجوداً
            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }
            
            if (!Directory.Exists(Path.Combine(_reportsDirectory, "Archive")))
            {
                Directory.CreateDirectory(Path.Combine(_reportsDirectory, "Archive"));
            }
        }

        /// <summary>
        /// إنشاء تقرير CASA كامل
        /// </summary>
        public async Task<string> GenerateCASAReportAsync(int examId, string userId, string userName)
        {
            try
            {
                var exam = await GetExamWithPatientAsync(examId);
                if (exam == null)
                    throw new ArgumentException($"Exam with ID {examId} not found");

                var casaResult = await GetCASAResultAsync(examId);
                if (casaResult == null)
                    throw new ArgumentException($"CASA result for exam {examId} not found");

                var reportData = new CASAReportData
                {
                    Exam = exam,
                    CASAResult = casaResult,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };

                var reportPath = await GeneratePDFReportAsync("CASA", reportData, examId);
                
                // تسجيل في AuditLog
                _auditLogger?.LogCASAnalysis(userId, userName, "", 0, $"CASA_Report_{examId}", 
                    new { ExamId = examId, ReportPath = reportPath }, 
                    new { ReportGenerated = true, ReportPath = reportPath });

                return reportPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating CASA report for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// إنشاء تقرير CBC كامل
        /// </summary>
        public async Task<string> GenerateCBCReportAsync(int examId, string userId, string userName)
        {
            try
            {
                var exam = await GetExamWithPatientAsync(examId);
                if (exam == null)
                    throw new ArgumentException($"Exam with ID {examId} not found");

                var cbcResult = await GetCBCResultAsync(examId);
                if (cbcResult == null)
                    throw new ArgumentException($"CBC result for exam {examId} not found");

                var reportData = new CBCReportData
                {
                    Exam = exam,
                    CBCResult = cbcResult,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName,
                    ReferenceRanges = GetCBCReferenceRanges()
                };

                var reportPath = await GeneratePDFReportAsync("CBC", reportData, examId);
                
                _auditLogger?.LogSystemEvent(userId, userName, "CBC_Report_Generated", "Reports", 
                    new { ExamId = examId, ReportPath = reportPath });

                return reportPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating CBC report for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// إنشاء تقرير البول
        /// </summary>
        public async Task<string> GenerateUrineReportAsync(int examId, string userId, string userName)
        {
            try
            {
                var exam = await GetExamWithPatientAsync(examId);
                var urineResult = await GetUrineResultAsync(examId);
                
                if (exam == null || urineResult == null)
                    throw new ArgumentException($"Exam or Urine result not found for ID {examId}");

                var reportData = new UrineReportData
                {
                    Exam = exam,
                    UrineResult = urineResult,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };

                var reportPath = await GeneratePDFReportAsync("Urine", reportData, examId);
                
                _auditLogger?.LogSystemEvent(userId, userName, "Urine_Report_Generated", "Reports", 
                    new { ExamId = examId, ReportPath = reportPath });

                return reportPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating Urine report for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// إنشاء تقرير البراز
        /// </summary>
        public async Task<string> GenerateStoolReportAsync(int examId, string userId, string userName)
        {
            try
            {
                var exam = await GetExamWithPatientAsync(examId);
                var stoolResult = await GetStoolResultAsync(examId);
                
                if (exam == null || stoolResult == null)
                    throw new ArgumentException($"Exam or Stool result not found for ID {examId}");

                var reportData = new StoolReportData
                {
                    Exam = exam,
                    StoolResult = stoolResult,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };

                var reportPath = await GeneratePDFReportAsync("Stool", reportData, examId);
                
                _auditLogger?.LogSystemEvent(userId, userName, "Stool_Report_Generated", "Reports", 
                    new { ExamId = examId, ReportPath = reportPath });

                return reportPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating Stool report for exam {ExamId}", examId);
                throw;
            }
        }

        /// <summary>
        /// إنشاء تقرير إحصائي شامل
        /// </summary>
        public async Task<string> GenerateStatisticsReportAsync(DateTime startDate, DateTime endDate, 
            string userId, string userName)
        {
            try
            {
                var stats = await GetStatisticsAsync(startDate, endDate);
                var reportData = new StatisticsReportData
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    Statistics = stats,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = userName
                };

                var reportPath = await GeneratePDFReportAsync("Statistics", reportData, 0);
                
                _auditLogger?.LogSystemEvent(userId, userName, "Statistics_Report_Generated", "Reports", 
                    new { StartDate = startDate, EndDate = endDate, ReportPath = reportPath });

                return reportPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating statistics report from {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// تصدير البيانات إلى Excel
        /// </summary>
        public async Task<string> ExportToExcelAsync(string reportType, object data, int examId = 0)
        {
            try
            {
                var fileName = $"{reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var filePath = Path.Combine(_reportsDirectory, fileName);

                // استخدام EPPlus لإنشاء ملف Excel
                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Report");
                    
                    // إضافة البيانات حسب نوع التقرير
                    switch (reportType.ToUpper())
                    {
                        case "CASA":
                            await ExportCASAToExcelAsync(worksheet, data as CASAReportData);
                            break;
                        case "CBC":
                            await ExportCBCToExcelAsync(worksheet, data as CBCReportData);
                            break;
                        case "URINE":
                            await ExportUrineToExcelAsync(worksheet, data as UrineReportData);
                            break;
                        case "STOOL":
                            await ExportStoolToExcelAsync(worksheet, data as StoolReportData);
                            break;
                    }

                    package.SaveAs(new FileInfo(filePath));
                }

                return filePath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting {ReportType} to Excel", reportType);
                throw;
            }
        }

        /// <summary>
        /// الحصول على قائمة التقارير المحفوظة
        /// </summary>
        public List<ReportInfo> GetSavedReports()
        {
            var reports = new List<ReportInfo>();
            var archivePath = Path.Combine(_reportsDirectory, "Archive");
            
            if (Directory.Exists(archivePath))
            {
                var files = Directory.GetFiles(archivePath, "*.pdf");
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    reports.Add(new ReportInfo
                    {
                        FileName = fileInfo.Name,
                        FilePath = file,
                        CreatedDate = fileInfo.CreationTime,
                        FileSize = fileInfo.Length
                    });
                }
            }

            return reports.OrderByDescending(r => r.CreatedDate).ToList();
        }

        /// <summary>
        /// حذف تقرير محفوظ
        /// </summary>
        public bool DeleteReport(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger?.LogInformation("Report deleted: {FilePath}", filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting report: {FilePath}", filePath);
                return false;
            }
        }

        #region Private Methods

        private async Task<string> GeneratePDFReportAsync(string reportType, object data, int examId)
        {
            var fileName = $"{reportType}_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(_reportsDirectory, fileName);
            var archivePath = Path.Combine(_reportsDirectory, "Archive", fileName);

            // إنشاء PDF باستخدام PdfSharp
            using (var document = new PdfSharp.Pdf.PdfDocument())
            {
                var page = document.AddPage();
                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
                var font = new PdfSharp.Drawing.XFont("Arial", 12);

                // إضافة المحتوى حسب نوع التقرير
                switch (reportType.ToUpper())
                {
                    case "CASA":
                        await GenerateCASAPDFAsync(gfx, font, data as CASAReportData);
                        break;
                    case "CBC":
                        await GenerateCBCPDFAsync(gfx, font, data as CBCReportData);
                        break;
                    case "URINE":
                        await GenerateUrinePDFAsync(gfx, font, data as UrineReportData);
                        break;
                    case "STOOL":
                        await GenerateStoolPDFAsync(gfx, font, data as StoolReportData);
                        break;
                    case "STATISTICS":
                        await GenerateStatisticsPDFAsync(gfx, font, data as StatisticsReportData);
                        break;
                }

                document.Save(filePath);
            }

            // نسخ إلى الأرشيف
            File.Copy(filePath, archivePath, true);
            
            return filePath;
        }

        private async Task<ExamWithPatient> GetExamWithPatientAsync(int examId)
        {
            var sql = @"
                SELECT e.*, p.FullName as PatientName, p.Age, p.Gender 
                FROM Exams e 
                JOIN Patients p ON e.PatientId = p.Id 
                WHERE e.Id = @ExamId";
            
            return await _db.QueryFirstOrDefaultAsync<ExamWithPatient>(sql, new { ExamId = examId });
        }

        private async Task<CASA_Result> GetCASAResultAsync(int examId)
        {
            var sql = "SELECT * FROM CASAResults WHERE ExamId = @ExamId";
            return await _db.QueryFirstOrDefaultAsync<CASA_Result>(sql, new { ExamId = examId });
        }

        private async Task<CBCTestResult> GetCBCResultAsync(int examId)
        {
            var sql = "SELECT * FROM CBCTestResults WHERE ExamId = @ExamId";
            return await _db.QueryFirstOrDefaultAsync<CBCTestResult>(sql, new { ExamId = examId });
        }

        private async Task<UrineTestResult> GetUrineResultAsync(int examId)
        {
            var sql = "SELECT * FROM UrineTestResults WHERE ExamId = @ExamId";
            return await _db.QueryFirstOrDefaultAsync<UrineTestResult>(sql, new { ExamId = examId });
        }

        private async Task<StoolTestResult> GetStoolResultAsync(int examId)
        {
            var sql = "SELECT * FROM StoolTestResults WHERE ExamId = @ExamId";
            return await _db.QueryFirstOrDefaultAsync<StoolTestResult>(sql, new { ExamId = examId });
        }

        private async Task<StatisticsData> GetStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            var sql = @"
                SELECT 
                    COUNT(*) as TotalExams,
                    COUNT(CASE WHEN ExamType = 'CASA' THEN 1 END) as CASAExams,
                    COUNT(CASE WHEN ExamType = 'CBC' THEN 1 END) as CBCExams,
                    COUNT(CASE WHEN ExamType = 'Urine' THEN 1 END) as UrineExams,
                    COUNT(CASE WHEN ExamType = 'Stool' THEN 1 END) as StoolExams
                FROM Exams 
                WHERE ExamDate BETWEEN @StartDate AND @EndDate";

            var stats = await _db.QueryFirstAsync<StatisticsData>(sql, new { StartDate = startDate, EndDate = endDate });
            return stats;
        }

        private Dictionary<string, (double Min, double Max)> GetCBCReferenceRanges()
        {
            return new Dictionary<string, (double Min, double Max)>
            {
                { "WBC", (4.0, 11.0) },
                { "RBC", (4.5, 5.5) },
                { "Hemoglobin", (13.5, 17.5) },
                { "Hematocrit", (41.0, 50.0) },
                { "Platelets", (150, 450) }
            };
        }

        // PDF Generation Methods
        private async Task GenerateCASAPDFAsync(PdfSharp.Drawing.XGraphics gfx, PdfSharp.Drawing.XFont font, CASAReportData data)
        {
            var y = 50;
            gfx.DrawString("تقرير تحليل الحيوانات المنوية (CASA)", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"المريض: {data.Exam.PatientName}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"التاريخ: {data.Exam.ExamDate:dd/MM/yyyy}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            
            gfx.DrawString($"VCL: {data.CASAResult.VCL:F2} µm/s", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"VSL: {data.CASAResult.VSL:F2} µm/s", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"VAP: {data.CASAResult.VAP:F2} µm/s", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"ALH: {data.CASAResult.ALH:F2} µm", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"BCF: {data.CASAResult.BCF:F2} Hz", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
        }

        private async Task GenerateCBCPDFAsync(PdfSharp.Drawing.XGraphics gfx, PdfSharp.Drawing.XFont font, CBCReportData data)
        {
            var y = 50;
            gfx.DrawString("تقرير تحليل الدم الشامل (CBC)", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"المريض: {data.Exam.PatientName}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"التاريخ: {data.Exam.ExamDate:dd/MM/yyyy}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            
            gfx.DrawString($"WBC: {data.CBCResult.WBC:F2} K/µL", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"RBC: {data.CBCResult.RBC:F2} M/µL", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"Hemoglobin: {data.CBCResult.Hemoglobin:F2} g/dL", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"Hematocrit: {data.CBCResult.Hematocrit:F2} %", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"Platelets: {data.CBCResult.Platelets:F0} K/µL", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
        }

        private async Task GenerateUrinePDFAsync(PdfSharp.Drawing.XGraphics gfx, PdfSharp.Drawing.XFont font, UrineReportData data)
        {
            var y = 50;
            gfx.DrawString("تقرير تحليل البول", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"المريض: {data.Exam.PatientName}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"التاريخ: {data.Exam.ExamDate:dd/MM/yyyy}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            
            gfx.DrawString($"اللون: {data.UrineResult.Color}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"pH: {data.UrineResult.pH:F1}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"الكثافة النوعية: {data.UrineResult.SpecificGravity:F3}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"البروتين: {data.UrineResult.Protein}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"الجلوكوز: {data.UrineResult.Glucose}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
        }

        private async Task GenerateStoolPDFAsync(PdfSharp.Drawing.XGraphics gfx, PdfSharp.Drawing.XFont font, StoolReportData data)
        {
            var y = 50;
            gfx.DrawString("تقرير تحليل البراز", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"المريض: {data.Exam.PatientName}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"التاريخ: {data.Exam.ExamDate:dd/MM/yyyy}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            
            gfx.DrawString($"اللون: {data.StoolResult.Color}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"التماسك: {data.StoolResult.Consistency}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"الدم الخفي: {data.StoolResult.OccultBlood}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"الطفيليات: {data.StoolResult.Parasites}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
        }

        private async Task GenerateStatisticsPDFAsync(PdfSharp.Drawing.XGraphics gfx, PdfSharp.Drawing.XFont font, StatisticsReportData data)
        {
            var y = 50;
            gfx.DrawString("التقرير الإحصائي", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            gfx.DrawString($"الفترة: من {data.StartDate:dd/MM/yyyy} إلى {data.EndDate:dd/MM/yyyy}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 30;
            
            gfx.DrawString($"إجمالي الفحوصات: {data.Statistics.TotalExams}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"فحوصات CASA: {data.Statistics.CASAExams}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"فحوصات CBC: {data.Statistics.CBCExams}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"فحوصات البول: {data.Statistics.UrineExams}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
            y += 20;
            gfx.DrawString($"فحوصات البراز: {data.Statistics.StoolExams}", font, PdfSharp.Drawing.XBrushes.Black, 50, y);
        }

        // Excel Export Methods
        private async Task ExportCASAToExcelAsync(OfficeOpenXml.ExcelWorksheet worksheet, CASAReportData data)
        {
            worksheet.Cells[1, 1].Value = "تقرير تحليل الحيوانات المنوية (CASA)";
            worksheet.Cells[3, 1].Value = "المريض:";
            worksheet.Cells[3, 2].Value = data.Exam.PatientName;
            worksheet.Cells[4, 1].Value = "التاريخ:";
            worksheet.Cells[4, 2].Value = data.Exam.ExamDate.ToString("dd/MM/yyyy");
            
            worksheet.Cells[6, 1].Value = "المعامل";
            worksheet.Cells[6, 2].Value = "القيمة";
            worksheet.Cells[6, 3].Value = "الوحدة";
            
            worksheet.Cells[7, 1].Value = "VCL";
            worksheet.Cells[7, 2].Value = data.CASAResult.VCL;
            worksheet.Cells[7, 3].Value = "µm/s";
            
            worksheet.Cells[8, 1].Value = "VSL";
            worksheet.Cells[8, 2].Value = data.CASAResult.VSL;
            worksheet.Cells[8, 3].Value = "µm/s";
            
            worksheet.Cells[9, 1].Value = "VAP";
            worksheet.Cells[9, 2].Value = data.CASAResult.VAP;
            worksheet.Cells[9, 3].Value = "µm/s";
            
            worksheet.Cells[10, 1].Value = "ALH";
            worksheet.Cells[10, 2].Value = data.CASAResult.ALH;
            worksheet.Cells[10, 3].Value = "µm";
            
            worksheet.Cells[11, 1].Value = "BCF";
            worksheet.Cells[11, 2].Value = data.CASAResult.BCF;
            worksheet.Cells[11, 3].Value = "Hz";
        }

        private async Task ExportCBCToExcelAsync(OfficeOpenXml.ExcelWorksheet worksheet, CBCReportData data)
        {
            worksheet.Cells[1, 1].Value = "تقرير تحليل الدم الشامل (CBC)";
            worksheet.Cells[3, 1].Value = "المريض:";
            worksheet.Cells[3, 2].Value = data.Exam.PatientName;
            worksheet.Cells[4, 1].Value = "التاريخ:";
            worksheet.Cells[4, 2].Value = data.Exam.ExamDate.ToString("dd/MM/yyyy");
            
            worksheet.Cells[6, 1].Value = "المعامل";
            worksheet.Cells[6, 2].Value = "القيمة";
            worksheet.Cells[6, 3].Value = "الوحدة";
            worksheet.Cells[6, 4].Value = "النطاق المرجعي";
            
            worksheet.Cells[7, 1].Value = "WBC";
            worksheet.Cells[7, 2].Value = data.CBCResult.WBC;
            worksheet.Cells[7, 3].Value = "K/µL";
            worksheet.Cells[7, 4].Value = "4.0-11.0";
            
            worksheet.Cells[8, 1].Value = "RBC";
            worksheet.Cells[8, 2].Value = data.CBCResult.RBC;
            worksheet.Cells[8, 3].Value = "M/µL";
            worksheet.Cells[8, 4].Value = "4.5-5.5";
            
            worksheet.Cells[9, 1].Value = "Hemoglobin";
            worksheet.Cells[9, 2].Value = data.CBCResult.Hemoglobin;
            worksheet.Cells[9, 3].Value = "g/dL";
            worksheet.Cells[9, 4].Value = "13.5-17.5";
        }

        private async Task ExportUrineToExcelAsync(OfficeOpenXml.ExcelWorksheet worksheet, UrineReportData data)
        {
            worksheet.Cells[1, 1].Value = "تقرير تحليل البول";
            worksheet.Cells[3, 1].Value = "المريض:";
            worksheet.Cells[3, 2].Value = data.Exam.PatientName;
            worksheet.Cells[4, 1].Value = "التاريخ:";
            worksheet.Cells[4, 2].Value = data.Exam.ExamDate.ToString("dd/MM/yyyy");
            
            worksheet.Cells[6, 1].Value = "المعامل";
            worksheet.Cells[6, 2].Value = "القيمة";
            
            worksheet.Cells[7, 1].Value = "اللون";
            worksheet.Cells[7, 2].Value = data.UrineResult.Color;
            
            worksheet.Cells[8, 1].Value = "pH";
            worksheet.Cells[8, 2].Value = data.UrineResult.pH;
            
            worksheet.Cells[9, 1].Value = "الكثافة النوعية";
            worksheet.Cells[9, 2].Value = data.UrineResult.SpecificGravity;
            
            worksheet.Cells[10, 1].Value = "البروتين";
            worksheet.Cells[10, 2].Value = data.UrineResult.Protein;
            
            worksheet.Cells[11, 1].Value = "الجلوكوز";
            worksheet.Cells[11, 2].Value = data.UrineResult.Glucose;
        }

        private async Task ExportStoolToExcelAsync(OfficeOpenXml.ExcelWorksheet worksheet, StoolReportData data)
        {
            worksheet.Cells[1, 1].Value = "تقرير تحليل البراز";
            worksheet.Cells[3, 1].Value = "المريض:";
            worksheet.Cells[3, 2].Value = data.Exam.PatientName;
            worksheet.Cells[4, 1].Value = "التاريخ:";
            worksheet.Cells[4, 2].Value = data.Exam.ExamDate.ToString("dd/MM/yyyy");
            
            worksheet.Cells[6, 1].Value = "المعامل";
            worksheet.Cells[6, 2].Value = "القيمة";
            
            worksheet.Cells[7, 1].Value = "اللون";
            worksheet.Cells[7, 2].Value = data.StoolResult.Color;
            
            worksheet.Cells[8, 1].Value = "التماسك";
            worksheet.Cells[8, 2].Value = data.StoolResult.Consistency;
            
            worksheet.Cells[9, 1].Value = "الدم الخفي";
            worksheet.Cells[9, 2].Value = data.StoolResult.OccultBlood;
            
            worksheet.Cells[10, 1].Value = "الطفيليات";
            worksheet.Cells[10, 2].Value = data.StoolResult.Parasites;
        }

        #endregion
    }

    #region Data Classes

    public class CASAReportData
    {
        public ExamWithPatient Exam { get; set; }
        public CASA_Result CASAResult { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class CBCReportData
    {
        public ExamWithPatient Exam { get; set; }
        public CBCTestResult CBCResult { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
        public Dictionary<string, (double Min, double Max)> ReferenceRanges { get; set; }
    }

    public class UrineReportData
    {
        public ExamWithPatient Exam { get; set; }
        public UrineTestResult UrineResult { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class StoolReportData
    {
        public ExamWithPatient Exam { get; set; }
        public StoolTestResult StoolResult { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class StatisticsReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public StatisticsData Statistics { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; }
    }

    public class ExamWithPatient
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string ExamType { get; set; }
        public DateTime ExamDate { get; set; }
        public string Notes { get; set; }
        public string PatientName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }

    public class StatisticsData
    {
        public int TotalExams { get; set; }
        public int CASAExams { get; set; }
        public int CBCExams { get; set; }
        public int UrineExams { get; set; }
        public int StoolExams { get; set; }
    }

    public class ReportInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public long FileSize { get; set; }
    }

    #endregion
>>>>>>> release/v1.0.0
}