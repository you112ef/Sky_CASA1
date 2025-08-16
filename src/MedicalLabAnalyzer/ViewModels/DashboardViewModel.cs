using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly PatientService _patientService;
        private readonly UserService _userService;
        private readonly AuditLogger _auditLogger;
        
        private int _totalPatients;
        private int _totalExams;
        private int _todayExams;
        private int _pendingResults;
        private int _casaExams;
        private int _cbcExams;
        private int _urineExams;
        private int _stoolExams;
        
        public DashboardViewModel()
        {
            _patientService = new PatientService();
            _userService = new UserService();
            _auditLogger = new AuditLogger();
            
            RecentActivity = new ObservableCollection<ActivityItem>();
            RefreshCommand = new RelayCommand(async () => await LoadDashboardDataAsync());
            
            // تحميل البيانات عند الإنشاء
            _ = LoadDashboardDataAsync();
        }
        
        public int TotalPatients
        {
            get => _totalPatients;
            set
            {
                _totalPatients = value;
                OnPropertyChanged();
            }
        }
        
        public int TotalExams
        {
            get => _totalExams;
            set
            {
                _totalExams = value;
                OnPropertyChanged();
            }
        }
        
        public int TodayExams
        {
            get => _todayExams;
            set
            {
                _todayExams = value;
                OnPropertyChanged();
            }
        }
        
        public int PendingResults
        {
            get => _pendingResults;
            set
            {
                _pendingResults = value;
                OnPropertyChanged();
            }
        }
        
        public int CASAExams
        {
            get => _casaExams;
            set
            {
                _casaExams = value;
                OnPropertyChanged();
            }
        }
        
        public int CBCExams
        {
            get => _cbcExams;
            set
            {
                _cbcExams = value;
                OnPropertyChanged();
            }
        }
        
        public int UrineExams
        {
            get => _urineExams;
            set
            {
                _urineExams = value;
                OnPropertyChanged();
            }
        }
        
        public int StoolExams
        {
            get => _stoolExams;
            set
            {
                _stoolExams = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<ActivityItem> RecentActivity { get; }
        
        public ICommand RefreshCommand { get; }
        
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // تحميل إحصائيات المرضى
                var patientStats = await _patientService.GetPatientStatisticsAsync();
                TotalPatients = patientStats.TotalPatients;
                
                // تحميل إحصائيات الفحوصات
                var examStats = await _patientService.GetExamStatisticsAsync();
                TotalExams = examStats.TotalExams;
                TodayExams = examStats.TodayExams;
                PendingResults = examStats.PendingResults;
                
                // تحميل إحصائيات أنواع التحاليل
                CASAExams = examStats.CASAExams;
                CBCExams = examStats.CBCExams;
                UrineExams = examStats.UrineExams;
                StoolExams = examStats.StoolExams;
                
                // تحميل النشاط الأخير
                await LoadRecentActivityAsync();
                
                // تسجيل الوصول للوحة التحكم
                await _auditLogger.LogAsync(
                    EventType.DashboardAccess,
                    "تم الوصول للوحة التحكم الرئيسية",
                    "System",
                    0
                );
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                await _auditLogger.LogAsync(
                    EventType.SystemError,
                    $"خطأ في تحميل بيانات لوحة التحكم: {ex.Message}",
                    "System",
                    0
                );
            }
        }
        
        private async Task LoadRecentActivityAsync()
        {
            try
            {
                RecentActivity.Clear();
                
                // الحصول على آخر 10 أنشطة من AuditLog
                var recentLogs = await _auditLogger.GetRecentLogsAsync(10);
                
                foreach (var log in recentLogs)
                {
                    RecentActivity.Add(new ActivityItem
                    {
                        Description = log.Description,
                        Timestamp = log.Timestamp
                    });
                }
            }
            catch (Exception ex)
            {
                // في حالة الخطأ، إضافة رسالة خطأ
                RecentActivity.Add(new ActivityItem
                {
                    Description = $"خطأ في تحميل النشاط الأخير: {ex.Message}",
                    Timestamp = DateTime.Now
                });
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class ActivityItem
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
    }
}