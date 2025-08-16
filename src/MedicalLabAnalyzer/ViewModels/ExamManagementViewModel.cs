using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MedicalLabAnalyzer.ViewModels
{
    public partial class ExamManagementViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _selectedTab = "CASA";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        public ObservableCollection<string> AvailableTabs { get; } = new()
        {
            "CASA",
            "CBC", 
            "Urine",
            "Stool"
        };

        [RelayCommand]
        private void SelectCASA()
        {
            SelectedTab = "CASA";
            StatusMessage = "تم اختيار تحليل CASA";
        }

        [RelayCommand]
        private void SelectCBC()
        {
            SelectedTab = "CBC";
            StatusMessage = "تم اختيار تحليل CBC";
        }

        [RelayCommand]
        private void SelectUrine()
        {
            SelectedTab = "Urine";
            StatusMessage = "تم اختيار تحليل البول";
        }

        [RelayCommand]
        private void SelectStool()
        {
            SelectedTab = "Stool";
            StatusMessage = "تم اختيار تحليل البراز";
        }

        [RelayCommand]
        private async Task LoadExamData()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل بيانات الفحوصات...";
                
                // Simulate loading time
                await Task.Delay(1000);
                
                StatusMessage = "تم تحميل بيانات الفحوصات بنجاح";
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

        public ExamManagementViewModel()
        {
            // Load initial data
            _ = LoadExamDataCommand.ExecuteAsync(null);
        }
    }
}