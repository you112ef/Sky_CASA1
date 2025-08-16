using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MedicalLabAnalyzer.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddDays(-30);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private string _selectedReportType = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private bool _showReportPreview = false;

        public ObservableCollection<string> AvailableReportTypes { get; } = new()
        {
            "Patient Reports - تقارير المرضى",
            "Exam Results - نتائج الفحوصات",
            "Financial Reports - التقارير المالية",
            "Quality Control - مراقبة الجودة",
            "CASA Analysis - تحليل CASA",
            "Calibration Reports - تقارير المعايرة"
        };

        public ObservableCollection<ReportTemplate> ReportTemplates { get; } = new();

        [RelayCommand]
        private async Task LoadReportTemplates()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل قوالب التقارير...";
                
                // Simulate loading time
                await Task.Delay(1000);
                
                // Add sample report templates
                ReportTemplates.Clear();
                ReportTemplates.Add(new ReportTemplate 
                { 
                    Name = "تقرير المريض الشامل",
                    Description = "تقرير شامل لجميع فحوصات المريض",
                    Category = "Patient Reports"
                });
                ReportTemplates.Add(new ReportTemplate 
                { 
                    Name = "تقرير نتائج الفحوصات",
                    Description = "تقرير لنتائج الفحوصات في فترة محددة",
                    Category = "Exam Results"
                });
                
                StatusMessage = $"تم تحميل {ReportTemplates.Count} قالب تقرير بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في تحميل البيانات: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SelectReportType(string reportType)
        {
            SelectedReportType = reportType;
            StatusMessage = $"تم اختيار: {reportType}";
        }

        [RelayCommand]
        private async Task GenerateReport()
        {
            if (string.IsNullOrWhiteSpace(SelectedReportType))
            {
                StatusMessage = "الرجاء اختيار نوع التقرير";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"جاري إنشاء التقرير: {SelectedReportType}";
                
                // Simulate report generation time
                await Task.Delay(2000);
                
                ShowReportPreview = true;
                StatusMessage = "تم إنشاء التقرير بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ في إنشاء التقرير: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ExportReport()
        {
            StatusMessage = "تصدير التقرير";
            // Implement export logic here
        }

        [RelayCommand]
        private void PrintReport()
        {
            StatusMessage = "طباعة التقرير";
            // Implement print logic here
        }

        [RelayCommand]
        private void EmailReport()
        {
            StatusMessage = "إرسال التقرير بالبريد الإلكتروني";
            // Implement email logic here
        }

        public ReportsViewModel()
        {
            // Load initial data
            _ = LoadReportTemplatesCommand.ExecuteAsync(null);
        }
    }

    public class ReportTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}