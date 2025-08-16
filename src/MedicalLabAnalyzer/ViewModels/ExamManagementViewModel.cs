using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.ViewModels
{
    public class ExamManagementViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private readonly VideoAnalysisService _videoAnalysisService;
        private readonly CBCAnalyzer _cbcAnalyzer;
        private readonly UrineAnalyzer _urineAnalyzer;
        private readonly StoolAnalyzer _stoolAnalyzer;
        private readonly AuditLogger _auditLogger;
        
        // Tab Selection Properties
        private bool _isCASASelected = true;
        private bool _isCBCSelected;
        private bool _isUrineSelected;
        private bool _isStoolSelected;
        
        // Common Properties
        private ObservableCollection<Patient> _patients;
        private Patient _selectedPatient;
        private bool _isAnalyzing;
        
        // CASA Properties
        private string _videoFilePath;
        
        // CBC Properties
        private CBCTestData _cbcData;
        
        // Urine Properties
        private UrineTestData _urineData;
        
        // Stool Properties
        private StoolTestData _stoolData;
        
        public ExamManagementViewModel()
        {
            // Initialize services
            _patientService = new PatientService();
            _videoAnalysisService = new VideoAnalysisService();
            _cbcAnalyzer = new CBCAnalyzer();
            _urineAnalyzer = new UrineAnalyzer();
            _stoolAnalyzer = new StoolAnalyzer();
            _auditLogger = new AuditLogger();
            
            // Initialize collections
            Patients = new ObservableCollection<Patient>();
            
            // Initialize test data objects
            CBCData = new CBCTestData();
            UrineData = new UrineTestData();
            StoolData = new StoolTestData();
            
            // Initialize commands
            SelectCASACommand = new RelayCommand(() => SelectTab("CASA"));
            SelectCBCCommand = new RelayCommand(() => SelectTab("CBC"));
            SelectUrineCommand = new RelayCommand(() => SelectTab("Urine"));
            SelectStoolCommand = new RelayCommand(() => SelectTab("Stool"));
            
            SelectVideoCommand = new RelayCommand(SelectVideoFile);
            AnalyzeCASACommand = new RelayCommand(async () => await AnalyzeCASAAsync(), () => CanAnalyzeCASA());
            AnalyzeCBCCommand = new RelayCommand(async () => await AnalyzeCBCAsync(), () => CanAnalyzeCBC());
            AnalyzeUrineCommand = new RelayCommand(async () => await AnalyzeUrineAsync(), () => CanAnalyzeUrine());
            AnalyzeStoolCommand = new RelayCommand(async () => await AnalyzeStoolAsync(), () => CanAnalyzeStool());
            
            // Load initial data
            _ = LoadPatientsAsync();
        }
        
        #region Properties
        
        public bool IsCASASelected
        {
            get => _isCASASelected;
            set
            {
                _isCASASelected = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsCBCSelected
        {
            get => _isCBCSelected;
            set
            {
                _isCBCSelected = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsUrineSelected
        {
            get => _isUrineSelected;
            set
            {
                _isUrineSelected = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsStoolSelected
        {
            get => _isStoolSelected;
            set
            {
                _isStoolSelected = value;
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
        
        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                _selectedPatient = value;
                OnPropertyChanged();
                // Update command states
                OnPropertyChanged(nameof(CanAnalyzeCASA));
                OnPropertyChanged(nameof(CanAnalyzeCBC));
                OnPropertyChanged(nameof(CanAnalyzeUrine));
                OnPropertyChanged(nameof(CanAnalyzeStool));
            }
        }
        
        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set
            {
                _isAnalyzing = value;
                OnPropertyChanged();
            }
        }
        
        public string VideoFilePath
        {
            get => _videoFilePath;
            set
            {
                _videoFilePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanAnalyzeCASA));
            }
        }
        
        public CBCTestData CBCData
        {
            get => _cbcData;
            set
            {
                _cbcData = value;
                OnPropertyChanged();
            }
        }
        
        public UrineTestData UrineData
        {
            get => _urineData;
            set
            {
                _urineData = value;
                OnPropertyChanged();
            }
        }
        
        public StoolTestData StoolData
        {
            get => _stoolData;
            set
            {
                _stoolData = value;
                OnPropertyChanged();
            }
        }
        
        #endregion
        
        #region Commands
        
        public ICommand SelectCASACommand { get; }
        public ICommand SelectCBCCommand { get; }
        public ICommand SelectUrineCommand { get; }
        public ICommand SelectStoolCommand { get; }
        public ICommand SelectVideoCommand { get; }
        public ICommand AnalyzeCASACommand { get; }
        public ICommand AnalyzeCBCCommand { get; }
        public ICommand AnalyzeUrineCommand { get; }
        public ICommand AnalyzeStoolCommand { get; }
        
        #endregion
        
        #region Methods
        
        private void SelectTab(string tabName)
        {
            // Reset all tabs
            IsCASASelected = false;
            IsCBCSelected = false;
            IsUrineSelected = false;
            IsStoolSelected = false;
            
            // Set selected tab
            switch (tabName)
            {
                case "CASA":
                    IsCASASelected = true;
                    break;
                case "CBC":
                    IsCBCSelected = true;
                    break;
                case "Urine":
                    IsUrineSelected = true;
                    break;
                case "Stool":
                    IsStoolSelected = true;
                    break;
            }
        }
        
        private void SelectVideoFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "اختر ملف الفيديو",
                Filter = "ملفات الفيديو (*.mp4;*.avi;*.mov;*.wmv)|*.mp4;*.avi;*.mov;*.wmv|جميع الملفات (*.*)|*.*",
                FilterIndex = 1
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                VideoFilePath = openFileDialog.FileName;
            }
        }
        
        private async Task LoadPatientsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                Patients.Clear();
                foreach (var patient in patients)
                {
                    Patients.Add(patient);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في تحميل المرضى: {ex.Message}", "System", 0);
            }
        }
        
        private async Task AnalyzeCASAAsync()
        {
            if (!CanAnalyzeCASA()) return;
            
            IsAnalyzing = true;
            
            try
            {
                // إجراء تحليل CASA
                var result = await _videoAnalysisService.AnalyzeVideoAsync(VideoFilePath);
                
                if (result != null && result.IsValid)
                {
                    // إنشاء فحص جديد
                    var exam = new Exam
                    {
                        PatientId = SelectedPatient.Id,
                        ExamType = ExamType.CASA,
                        CreatedDate = DateTime.Now,
                        Status = ExamStatus.Completed
                    };
                    
                    // حفظ الفحص والنتيجة
                    await _patientService.CreateExamAsync(exam);
                    await _patientService.SaveCASAResultAsync(exam.Id, result);
                    
                    // تسجيل العملية
                    await _auditLogger.LogAsync(
                        EventType.ExamCreated,
                        $"تم إجراء تحليل CASA للمريض: {SelectedPatient.FullName}",
                        "User",
                        SelectedPatient.Id
                    );
                    
                    MessageBox.Show("تم إجراء تحليل CASA بنجاح", "نجح التحليل", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("فشل في تحليل الفيديو. تأكد من صحة الملف.", "خطأ في التحليل", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحليل CASA: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في تحليل CASA: {ex.Message}", "System", SelectedPatient?.Id ?? 0);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }
        
        private async Task AnalyzeCBCAsync()
        {
            if (!CanAnalyzeCBC()) return;
            
            IsAnalyzing = true;
            
            try
            {
                // تحويل البيانات المدخلة إلى CBC_Result
                var cbcResult = new CBC_Result
                {
                    WBC = CBCData.WBC,
                    RBC = CBCData.RBC,
                    Hemoglobin = CBCData.Hemoglobin,
                    Hematocrit = CBCData.Hematocrit,
                    Platelets = CBCData.Platelets,
                    MCV = CBCData.MCV,
                    MCH = CBCData.MCH,
                    MCHC = CBCData.MCHC,
                    Neutrophils = CBCData.Neutrophils,
                    Lymphocytes = CBCData.Lymphocytes,
                    Monocytes = CBCData.Monocytes,
                    Eosinophils = CBCData.Eosinophils,
                    Basophils = CBCData.Basophils
                };
                
                // تحليل النتائج
                var analysis = await _cbcAnalyzer.AnalyzeAsync(cbcResult);
                
                // إنشاء فحص جديد
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = ExamType.CBC,
                    CreatedDate = DateTime.Now,
                    Status = ExamStatus.Completed
                };
                
                // حفظ الفحص والنتيجة
                await _patientService.CreateExamAsync(exam);
                await _patientService.SaveCBCResultAsync(exam.Id, cbcResult);
                
                // تسجيل العملية
                await _auditLogger.LogAsync(
                    EventType.ExamCreated,
                    $"تم إجراء تحليل CBC للمريض: {SelectedPatient.FullName}",
                    "User",
                    SelectedPatient.Id
                );
                
                MessageBox.Show($"تم إجراء تحليل CBC بنجاح\n{analysis}", "نجح التحليل", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحليل CBC: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في تحليل CBC: {ex.Message}", "System", SelectedPatient?.Id ?? 0);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }
        
        private async Task AnalyzeUrineAsync()
        {
            if (!CanAnalyzeUrine()) return;
            
            IsAnalyzing = true;
            
            try
            {
                // تحويل البيانات المدخلة إلى UrineTestResult
                var urineResult = new UrineTestResult
                {
                    Color = UrineData.Color,
                    Clarity = UrineData.Clarity,
                    SpecificGravity = UrineData.SpecificGravity,
                    pH = UrineData.pH,
                    Protein = UrineData.Protein,
                    Glucose = UrineData.Glucose,
                    Ketones = UrineData.Ketones,
                    Blood = UrineData.Blood,
                    Nitrite = UrineData.Nitrite,
                    LeukocyteEsterase = UrineData.LeukocyteEsterase,
                    WBC = UrineData.WBC,
                    RBC = UrineData.RBC,
                    EpithelialCells = UrineData.EpithelialCells,
                    Bacteria = UrineData.Bacteria,
                    Crystals = UrineData.Crystals
                };
                
                // تحليل النتائج
                var analysis = await _urineAnalyzer.AnalyzeAsync(urineResult);
                
                // إنشاء فحص جديد
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = ExamType.Urine,
                    CreatedDate = DateTime.Now,
                    Status = ExamStatus.Completed
                };
                
                // حفظ الفحص والنتيجة
                await _patientService.CreateExamAsync(exam);
                await _patientService.SaveUrineResultAsync(exam.Id, urineResult);
                
                // تسجيل العملية
                await _auditLogger.LogAsync(
                    EventType.ExamCreated,
                    $"تم إجراء تحليل البول للمريض: {SelectedPatient.FullName}",
                    "User",
                    SelectedPatient.Id
                );
                
                MessageBox.Show($"تم إجراء تحليل البول بنجاح\n{analysis}", "نجح التحليل", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحليل البول: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في تحليل البول: {ex.Message}", "System", SelectedPatient?.Id ?? 0);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }
        
        private async Task AnalyzeStoolAsync()
        {
            if (!CanAnalyzeStool()) return;
            
            IsAnalyzing = true;
            
            try
            {
                // تحويل البيانات المدخلة إلى StoolTestResult
                var stoolResult = new StoolTestResult
                {
                    Color = StoolData.Color,
                    Consistency = StoolData.Consistency,
                    BloodVisible = StoolData.BloodVisible,
                    BloodOccult = StoolData.BloodOccult,
                    WBC = StoolData.WBC,
                    RBC = StoolData.RBC,
                    Parasites = StoolData.Parasites,
                    Bacteria = StoolData.Bacteria,
                    Yeast = StoolData.Yeast,
                    FatGlobules = StoolData.FatGlobules
                };
                
                // تحليل النتائج
                var analysis = await _stoolAnalyzer.AnalyzeAsync(stoolResult);
                
                // إنشاء فحص جديد
                var exam = new Exam
                {
                    PatientId = SelectedPatient.Id,
                    ExamType = ExamType.Stool,
                    CreatedDate = DateTime.Now,
                    Status = ExamStatus.Completed
                };
                
                // حفظ الفحص والنتيجة
                await _patientService.CreateExamAsync(exam);
                await _patientService.SaveStoolResultAsync(exam.Id, stoolResult);
                
                // تسجيل العملية
                await _auditLogger.LogAsync(
                    EventType.ExamCreated,
                    $"تم إجراء تحليل البراز للمريض: {SelectedPatient.FullName}",
                    "User",
                    SelectedPatient.Id
                );
                
                MessageBox.Show($"تم إجراء تحليل البراز بنجاح\n{analysis}", "نجح التحليل", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحليل البراز: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                await _auditLogger.LogAsync(EventType.SystemError, $"خطأ في تحليل البراز: {ex.Message}", "System", SelectedPatient?.Id ?? 0);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }
        
        private bool CanAnalyzeCASA()
        {
            return SelectedPatient != null && !string.IsNullOrEmpty(VideoFilePath) && !IsAnalyzing;
        }
        
        private bool CanAnalyzeCBC()
        {
            return SelectedPatient != null && !IsAnalyzing;
        }
        
        private bool CanAnalyzeUrine()
        {
            return SelectedPatient != null && !IsAnalyzing;
        }
        
        private bool CanAnalyzeStool()
        {
            return SelectedPatient != null && !IsAnalyzing;
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
    
    // Helper classes for test data binding
    public class CBCTestData : INotifyPropertyChanged
    {
        private double _wbc, _rbc, _hemoglobin, _hematocrit, _platelets, _mcv, _mch, _mchc;
        private double _neutrophils, _lymphocytes, _monocytes, _eosinophils, _basophils;
        
        public double WBC { get => _wbc; set { _wbc = value; OnPropertyChanged(); } }
        public double RBC { get => _rbc; set { _rbc = value; OnPropertyChanged(); } }
        public double Hemoglobin { get => _hemoglobin; set { _hemoglobin = value; OnPropertyChanged(); } }
        public double Hematocrit { get => _hematocrit; set { _hematocrit = value; OnPropertyChanged(); } }
        public double Platelets { get => _platelets; set { _platelets = value; OnPropertyChanged(); } }
        public double MCV { get => _mcv; set { _mcv = value; OnPropertyChanged(); } }
        public double MCH { get => _mch; set { _mch = value; OnPropertyChanged(); } }
        public double MCHC { get => _mchc; set { _mchc = value; OnPropertyChanged(); } }
        public double Neutrophils { get => _neutrophils; set { _neutrophils = value; OnPropertyChanged(); } }
        public double Lymphocytes { get => _lymphocytes; set { _lymphocytes = value; OnPropertyChanged(); } }
        public double Monocytes { get => _monocytes; set { _monocytes = value; OnPropertyChanged(); } }
        public double Eosinophils { get => _eosinophils; set { _eosinophils = value; OnPropertyChanged(); } }
        public double Basophils { get => _basophils; set { _basophils = value; OnPropertyChanged(); } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class UrineTestData : INotifyPropertyChanged
    {
        private string _color, _clarity, _protein, _glucose, _ketones, _blood, _nitrite, _leukocyteEsterase;
        private string _wbc, _rbc, _epithelialCells, _bacteria, _crystals;
        private double _specificGravity, _pH;
        
        public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }
        public string Clarity { get => _clarity; set { _clarity = value; OnPropertyChanged(); } }
        public double SpecificGravity { get => _specificGravity; set { _specificGravity = value; OnPropertyChanged(); } }
        public double pH { get => _pH; set { _pH = value; OnPropertyChanged(); } }
        public string Protein { get => _protein; set { _protein = value; OnPropertyChanged(); } }
        public string Glucose { get => _glucose; set { _glucose = value; OnPropertyChanged(); } }
        public string Ketones { get => _ketones; set { _ketones = value; OnPropertyChanged(); } }
        public string Blood { get => _blood; set { _blood = value; OnPropertyChanged(); } }
        public string Nitrite { get => _nitrite; set { _nitrite = value; OnPropertyChanged(); } }
        public string LeukocyteEsterase { get => _leukocyteEsterase; set { _leukocyteEsterase = value; OnPropertyChanged(); } }
        public string WBC { get => _wbc; set { _wbc = value; OnPropertyChanged(); } }
        public string RBC { get => _rbc; set { _rbc = value; OnPropertyChanged(); } }
        public string EpithelialCells { get => _epithelialCells; set { _epithelialCells = value; OnPropertyChanged(); } }
        public string Bacteria { get => _bacteria; set { _bacteria = value; OnPropertyChanged(); } }
        public string Crystals { get => _crystals; set { _crystals = value; OnPropertyChanged(); } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class StoolTestData : INotifyPropertyChanged
    {
        private string _color, _consistency, _parasites, _bacteria, _yeast, _fatGlobules;
        private string _wbc, _rbc;
        private bool _bloodVisible, _bloodOccult;
        
        public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }
        public string Consistency { get => _consistency; set { _consistency = value; OnPropertyChanged(); } }
        public bool BloodVisible { get => _bloodVisible; set { _bloodVisible = value; OnPropertyChanged(); } }
        public bool BloodOccult { get => _bloodOccult; set { _bloodOccult = value; OnPropertyChanged(); } }
        public string WBC { get => _wbc; set { _wbc = value; OnPropertyChanged(); } }
        public string RBC { get => _rbc; set { _rbc = value; OnPropertyChanged(); } }
        public string Parasites { get => _parasites; set { _parasites = value; OnPropertyChanged(); } }
        public string Bacteria { get => _bacteria; set { _bacteria = value; OnPropertyChanged(); } }
        public string Yeast { get => _yeast; set { _yeast = value; OnPropertyChanged(); } }
        public string FatGlobules { get => _fatGlobules; set { _fatGlobules = value; OnPropertyChanged(); } }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        
        public void Execute(object parameter) => _execute();
    }
}