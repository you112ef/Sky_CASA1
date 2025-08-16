using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace MedicalLabAnalyzer.ViewModels
{
    public class PatientManagementViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private readonly AuditLogger _auditLogger;
        
        private Patient _selectedPatient;
        private string _searchText = "";
        private bool _isLoading = false;
        
        public PatientManagementViewModel(IConfiguration configuration = null, ILogger<PatientManagementViewModel> logger = null)
        {
            // إنشاء database connection factory
            var connectionFactory = new DatabaseConnectionFactory(configuration, null);
            var dbConnection = connectionFactory.CreateConnection();
            
            _patientService = new PatientService(null, dbConnection);
            _auditLogger = new AuditLogger(null, dbConnection);
            
            Patients = new ObservableCollection<Patient>();
            
            LoadPatientsCommand = new RelayCommand(async () => await LoadPatientsAsync(), () => !IsLoading);
            AddPatientCommand = new RelayCommand(async () => await AddPatientAsync(), () => !IsLoading);
            EditPatientCommand = new RelayCommand(async () => await EditPatientAsync(), () => SelectedPatient != null && !IsLoading);
            DeletePatientCommand = new RelayCommand(async () => await DeletePatientAsync(), () => SelectedPatient != null && !IsLoading);
            SearchPatientsCommand = new RelayCommand(async () => await SearchPatientsAsync(), () => !IsLoading);
            
            // تحميل المرضى عند الإنشاء
            _ = LoadPatientsAsync();
        }
        
        public ObservableCollection<Patient> Patients { get; }
        
        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedPatient));
            }
        }
        
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
        
        public bool HasSelectedPatient => SelectedPatient != null;
        
        public ICommand LoadPatientsCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand EditPatientCommand { get; }
        public ICommand DeletePatientCommand { get; }
        public ICommand SearchPatientsCommand { get; }
        
        private async Task LoadPatientsAsync()
        {
            try
            {
                IsLoading = true;
                var patients = await _patientService.GetAllPatientsAsync();
                
                Patients.Clear();
                if (patients != null)
                {
                    foreach (var patient in patients)
                    {
                        Patients.Add(patient);
                    }
                }
                
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "LoadPatients", $"تم تحميل {Patients.Count} مريض"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في تحميل المرضى: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task AddPatientAsync()
        {
            // TODO: إنشاء نافذة إضافة مريض جديد
            await Task.CompletedTask;
        }
        
        private async Task EditPatientAsync()
        {
            if (SelectedPatient == null) return;
            
            // TODO: إنشاء نافذة تعديل بيانات المريض
            await Task.CompletedTask;
        }
        
        private async Task DeletePatientAsync()
        {
            if (SelectedPatient == null) return;
            
            try
            {
                IsLoading = true;
                var result = await _patientService.DeletePatientAsync(SelectedPatient.Id);
                
                if (result)
                {
                    Patients.Remove(SelectedPatient);
                    await _auditLogger.LogUserActionAsync(
                        "system", "System", "DeletePatient", 
                        $"تم حذف المريض: {SelectedPatient.FirstName} {SelectedPatient.LastName}"
                    );
                    SelectedPatient = null;
                }
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في حذف المريض: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task SearchPatientsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadPatientsAsync();
                return;
            }
            
            try
            {
                IsLoading = true;
                var patients = await _patientService.SearchPatientsAsync(SearchText);
                
                Patients.Clear();
                if (patients != null)
                {
                    foreach (var patient in patients)
                    {
                        Patients.Add(patient);
                    }
                }
                
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "SearchPatients", 
                    $"بحث عن المرضى بالكلمة: {SearchText}, النتائج: {Patients.Count}"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في البحث عن المرضى: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}