using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System;
using System.IO;
using MedicalLabAnalyzer.Models;

namespace MedicalLabAnalyzer.Services
{
    public class ReportService
    {
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
}