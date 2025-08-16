using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.ViewModels
{
    public class PatientManagementViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private readonly AuditLogger _auditLogger;
        
        private string _searchText;
        private Patient _selectedPatient;
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Patient> _allPatients; // للبحث
        private bool _isLoading;
        
        public PatientManagementViewModel()
        {
            // Initialize services
            _patientService = new PatientService();
            _auditLogger = new AuditLogger();
            
            // Initialize collections
            Patients = new ObservableCollection<Patient>();
            _allPatients = new ObservableCollection<Patient>();
            
            // Initialize commands
            SearchCommand = new RelayCommand(SearchPatients);
            AddPatientCommand = new RelayCommand(AddPatient);
            EditPatientCommand = new RelayCommand<Patient>(EditPatient, CanEditPatient);
            DeletePatientCommand = new RelayCommand<Patient>(async (patient) => await DeletePatientAsync(patient), CanDeletePatient);
            ViewPatientExamsCommand = new RelayCommand<Patient>(ViewPatientExams, CanViewPatientExams);
            
            // Load initial data
            _ = LoadPatientsAsync();
        }
        
        #region Properties
        
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                SearchPatients(); // البحث التلقائي
            }
        }
        
        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set
            {
                _patients = value;
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
        
        #endregion
        
        #region Commands
        
        public ICommand SearchCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand EditPatientCommand { get; }
        public ICommand DeletePatientCommand { get; }
        public ICommand ViewPatientExamsCommand { get; }
        
        #endregion
        
        #region Methods
        
        private async Task LoadPatientsAsync()
        {
            IsLoading = true;
            
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                
                _allPatients.Clear();
                Patients.Clear();
                
                foreach (var patient in patients)
                {
                    // حساب عدد الفحوصات لكل مريض
                    var examCount = await _patientService.GetPatientExamCountAsync(patient.Id);
                    patient.ExamCount = examCount;
                    
                    _allPatients.Add(patient);
                    Patients.Add(patient);
                }
                
                await _auditLogger.LogAsync(
                    EventType.DataAccess,
                    $"تم تحميل قائمة المرضى - العدد: {patients.Count()}",
                    "User",
                    0
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في تحميل المرضى: {ex.Message}", "System", 0);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private void SearchPatients()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // إظهار جميع المرضى
                Patients.Clear();
                foreach (var patient in _allPatients)
                {
                    Patients.Add(patient);
                }
            }
            else
            {
                // البحث في الاسم ورقم الهاتف
                var searchTerm = SearchText.Trim().ToLower();
                var filteredPatients = _allPatients.Where(p =>
                    p.FullName.ToLower().Contains(searchTerm) ||
                    p.PhoneNumber?.ToLower().Contains(searchTerm) == true ||
                    p.Id.ToString().Contains(searchTerm)
                ).ToList();
                
                Patients.Clear();
                foreach (var patient in filteredPatients)
                {
                    Patients.Add(patient);
                }
            }
        }
        
        private void AddPatient()
        {
            try
            {
                // فتح نافذة إضافة مريض جديد
                var addPatientWindow = new Views.AddPatientWindow();
                if (addPatientWindow.ShowDialog() == true)
                {
                    // تم إضافة مريض جديد، إعادة تحميل القائمة
                    _ = LoadPatientsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إضافة مريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void EditPatient(Patient patient)
        {
            if (patient == null) return;
            
            try
            {
                // فتح نافذة تعديل المريض
                var editPatientWindow = new Views.EditPatientWindow(patient);
                if (editPatientWindow.ShowDialog() == true)
                {
                    // تم تعديل المريض، إعادة تحميل القائمة
                    _ = LoadPatientsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تعديل المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task DeletePatientAsync(Patient patient)
        {
            if (patient == null) return;
            
            var result = MessageBox.Show(
                $"هل تريد حقاً حذف المريض: {patient.FullName}؟\nسيتم حذف جميع فحوصاته أيضاً.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _patientService.DeletePatientAsync(patient.Id);
                    
                    // إزالة المريض من القوائم
                    Patients.Remove(patient);
                    _allPatients.Remove(patient);
                    
                    // تسجيل العملية
                    await _auditLogger.LogAsync(
                        EventType.PatientDeleted,
                        $"تم حذف المريض: {patient.FullName}",
                        "User",
                        patient.Id
                    );
                    
                    MessageBox.Show("تم حذف المريض بنجاح", "نجح الحذف", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في حذف المريض: {ex.Message}", "System", patient.Id);
                }
            }
        }
        
        private void ViewPatientExams(Patient patient)
        {
            if (patient == null) return;
            
            try
            {
                // فتح نافذة عرض فحوصات المريض
                var patientExamsWindow = new Views.PatientExamsWindow(patient);
                patientExamsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض فحوصات المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private bool CanEditPatient(Patient patient)
        {
            return patient != null && !IsLoading;
        }
        
        private bool CanDeletePatient(Patient patient)
        {
            return patient != null && !IsLoading;
        }
        
        private bool CanViewPatientExams(Patient patient)
        {
            return patient != null;
        }
        
        #endregion
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion
    }
    
    // Generic RelayCommand for parameterized commands
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }
        
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}