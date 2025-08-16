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

namespace MedicalLabAnalyzer.ViewModels
{
    public class ExamManagementViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private readonly AuditLogger _auditLogger;
        
        private Patient _selectedPatient;
        private Exam _selectedExam;
        private bool _isLoading = false;
        
        public ExamManagementViewModel(IConfiguration configuration = null, ILogger<ExamManagementViewModel> logger = null)
        {
            // إنشاء database connection factory
            var connectionFactory = new DatabaseConnectionFactory(configuration, null);
            var dbConnection = connectionFactory.CreateConnection();
            
            _patientService = new PatientService(null, dbConnection);
            _auditLogger = new AuditLogger(null, dbConnection);
            
            Patients = new ObservableCollection<Patient>();
            Exams = new ObservableCollection<Exam>();
            
            LoadPatientsCommand = new RelayCommand(async () => await LoadPatientsAsync(), () => !IsLoading);
            CreateCASAExamCommand = new RelayCommand(async () => await CreateCASAExamAsync(), () => SelectedPatient != null && !IsLoading);
            CreateCBCExamCommand = new RelayCommand(async () => await CreateCBCExamAsync(), () => SelectedPatient != null && !IsLoading);
            CreateUrineExamCommand = new RelayCommand(async () => await CreateUrineExamAsync(), () => SelectedPatient != null && !IsLoading);
            CreateStoolExamCommand = new RelayCommand(async () => await CreateStoolExamAsync(), () => SelectedPatient != null && !IsLoading);
            
            // تحميل المرضى عند الإنشاء
            _ = LoadPatientsAsync();
        }
        
        public ObservableCollection<Patient> Patients { get; }
        public ObservableCollection<Exam> Exams { get; }
        
        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedPatient));
                _ = LoadPatientExamsAsync();
            }
        }
        
        public Exam SelectedExam
        {
            get => _selectedExam;
            set
            {
                _selectedExam = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedExam));
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
        public bool HasSelectedExam => SelectedExam != null;
        
        public ICommand LoadPatientsCommand { get; }
        public ICommand CreateCASAExamCommand { get; }
        public ICommand CreateCBCExamCommand { get; }
        public ICommand CreateUrineExamCommand { get; }
        public ICommand CreateStoolExamCommand { get; }
        
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
        
        private async Task LoadPatientExamsAsync()
        {
            if (SelectedPatient == null)
            {
                Exams.Clear();
                return;
            }
            
            try
            {
                IsLoading = true;
                Exams.Clear();
                
                // TODO: تحميل فحوصات المريض من قاعدة البيانات
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "LoadPatientExams", 
                    $"تم تحميل فحوصات المريض: {SelectedPatient.FirstName} {SelectedPatient.LastName}"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في تحميل فحوصات المريض: {ex.Message}"
                );
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task CreateCASAExamAsync()
        {
            if (SelectedPatient == null) return;
            
            try
            {
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = "CASA",
                    RequestedDate = DateTime.Now,
                    Status = "Pending",
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };
                
                // TODO: حفظ الفحص في قاعدة البيانات
                Exams.Add(exam);
                
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "CreateCASAExam", 
                    $"تم إنشاء فحص CASA للمريض: {SelectedPatient.FirstName} {SelectedPatient.LastName}"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في إنشاء فحص CASA: {ex.Message}"
                );
            }
        }
        
        private async Task CreateCBCExamAsync()
        {
            if (SelectedPatient == null) return;
            
            try
            {
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = "CBC",
                    RequestedDate = DateTime.Now,
                    Status = "Pending",
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };
                
                // TODO: حفظ الفحص في قاعدة البيانات
                Exams.Add(exam);
                
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "CreateCBCExam", 
                    $"تم إنشاء فحص CBC للمريض: {SelectedPatient.FirstName} {SelectedPatient.LastName}"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في إنشاء فحص CBC: {ex.Message}"
                );
            }
        }
        
        private async Task CreateUrineExamAsync()
        {
            if (SelectedPatient == null) return;
            
            try
            {
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = "Urine",
                    RequestedDate = DateTime.Now,
                    Status = "Pending",
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };
                
                // TODO: حفظ الفحص في قاعدة البيانات
                Exams.Add(exam);
                
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "CreateUrineExam", 
                    $"تم إنشاء فحص البول للمريض: {SelectedPatient.FirstName} {SelectedPatient.LastName}"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في إنشاء فحص البول: {ex.Message}"
                );
            }
        }
        
        private async Task CreateStoolExamAsync()
        {
            if (SelectedPatient == null) return;
            
            try
            {
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = "Stool",
                    RequestedDate = DateTime.Now,
                    Status = "Pending",
                    CreatedBy = "System",
                    UpdatedBy = "System"
                };
                
                // TODO: حفظ الفحص في قاعدة البيانات
                Exams.Add(exam);
                
                await _auditLogger.LogUserActionAsync(
                    "system", "System", "CreateStoolExam", 
                    $"تم إنشاء فحص البراز للمريض: {SelectedPatient.FirstName} {SelectedPatient.LastName}"
                );
            }
            catch (Exception ex)
            {
                await _auditLogger.LogSystemEventAsync(
                    "system", "System", "SystemError", $"خطأ في إنشاء فحص البراز: {ex.Message}"
                );
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}