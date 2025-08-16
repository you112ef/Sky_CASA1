using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MedicalLabAnalyzer.ViewModels
{
    public partial class PatientManagementViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "";

        [ObservableProperty]
        private bool _showAddPatientDialog = false;

        public ObservableCollection<Patient> Patients { get; } = new();

        [RelayCommand]
        private async Task LoadPatients()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "جاري تحميل بيانات المرضى...";
                
                // Simulate loading time
                await Task.Delay(1000);
                
                // Add sample patients for demonstration
                Patients.Clear();
                Patients.Add(new Patient { Id = 1, FullName = "أحمد محمد", Age = 35, Gender = "ذكر", Phone = "0501234567" });
                Patients.Add(new Patient { Id = 2, FullName = "فاطمة علي", Age = 28, Gender = "أنثى", Phone = "0507654321" });
                Patients.Add(new Patient { Id = 3, FullName = "محمد حسن", Age = 42, Gender = "ذكر", Phone = "0509876543" });
                
                StatusMessage = $"تم تحميل {Patients.Count} مريض بنجاح";
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
        private void SearchPatients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadPatientsCommand.Execute(null);
                return;
            }

            StatusMessage = $"جاري البحث عن: {SearchText}";
            // Implement search logic here
        }

        [RelayCommand]
        private void AddPatient()
        {
            ShowAddPatientDialog = true;
            StatusMessage = "إضافة مريض جديد";
        }

        [RelayCommand]
        private void EditPatient(Patient patient)
        {
            if (patient != null)
            {
                StatusMessage = $"تعديل بيانات المريض: {patient.FullName}";
                // Implement edit logic here
            }
        }

        [RelayCommand]
        private void DeletePatient(Patient patient)
        {
            if (patient != null)
            {
                StatusMessage = $"حذف المريض: {patient.FullName}";
                Patients.Remove(patient);
                // Implement delete logic here
            }
        }

        public PatientManagementViewModel()
        {
            // Load initial data
            _ = LoadPatientsCommand.ExecuteAsync(null);
        }
    }

    public class Patient
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int Age { get; set; }
        public string Gender { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}