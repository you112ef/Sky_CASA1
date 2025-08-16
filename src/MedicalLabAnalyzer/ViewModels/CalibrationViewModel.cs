using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MedicalLabAnalyzer.ViewModels
{
    public partial class CalibrationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedDevice = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private bool _showCalibrationDialog = false;

        [ObservableProperty]
        private DateTime _lastCalibrationDate = DateTime.Now;

        public ObservableCollection<string> AvailableDevices { get; } = new()
        {
            "Microscope - المجهر",
            "Centrifuge - الطرد المركزي",
            "Incubator - الحاضنة",
            "Analyzers - المحللات",
            "CASA System - نظام CASA"
        };

        public ObservableCollection<CalibrationRecord> CalibrationHistory { get; } = new();

        [RelayCommand]
        private async Task LoadCalibrationData()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل بيانات المعايرة...";
                
                // Simulate loading time
                await Task.Delay(1000);
                
                // Add sample calibration records
                CalibrationHistory.Clear();
                CalibrationHistory.Add(new CalibrationRecord 
                { 
                    DeviceName = "Microscope - المجهر",
                    CalibrationDate = DateTime.Now.AddDays(-30),
                    Technician = "أحمد محمد",
                    Status = "Valid - صالح"
                });
                CalibrationHistory.Add(new CalibrationRecord 
                { 
                    DeviceName = "Centrifuge - الطرد المركزي",
                    CalibrationDate = DateTime.Now.AddDays(-15),
                    Technician = "فاطمة علي",
                    Status = "Valid - صالح"
                });
                
                StatusMessage = $"تم تحميل {CalibrationHistory.Count} سجل معايرة بنجاح";
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
        private void StartCalibration()
        {
            if (string.IsNullOrWhiteSpace(SelectedDevice))
            {
                StatusMessage = "الرجاء اختيار جهاز للمعايرة";
                return;
            }

            ShowCalibrationDialog = true;
            StatusMessage = $"بدء معايرة الجهاز: {SelectedDevice}";
        }

        [RelayCommand]
        private void ViewCalibrationHistory()
        {
            StatusMessage = "عرض سجل المعايرة";
            // Implement view history logic here
        }

        [RelayCommand]
        private void ExportCalibrationReport()
        {
            StatusMessage = "تصدير تقرير المعايرة";
            // Implement export logic here
        }

        public CalibrationViewModel()
        {
            // Load initial data
            _ = LoadCalibrationDataCommand.ExecuteAsync(null);
        }
    }

    public class CalibrationRecord
    {
        public int Id { get; set; }
        public string DeviceName { get; set; } = "";
        public DateTime CalibrationDate { get; set; }
        public string Technician { get; set; } = "";
        public string Status { get; set; } = "";
        public string Notes { get; set; } = "";
        public DateTime NextCalibrationDate { get; set; }
    }
}