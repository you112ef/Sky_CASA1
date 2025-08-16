using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;

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
        
        public DashboardViewModel(IConfiguration configuration = null, ILogger<DashboardViewModel> logger = null)
        {
            // إنشاء database connection factory
            var connectionFactory = new DatabaseConnectionFactory(configuration, null);
            var dbConnection = connectionFactory.CreateConnection();
            
            _patientService = new PatientService(null, dbConnection);
            _userService = new UserService(null, dbConnection);
            _auditLogger = new AuditLogger(null, dbConnection);
            
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
                var allPatients = await _patientService.GetAllPatientsAsync();
                TotalPatients = allPatients?.Count ?? 0;
                
                // إحصائيات مؤقتة - يمكن تطويرها لاحقاً
                TotalExams = 0;
                TodayExams = 0;
                PendingResults = 0;
                
                // إحصائيات أنواع التحاليل - مؤقتة
                CASAExams = 0;
                CBCExams = 0;
                UrineExams = 0;
                StoolExams = 0;
                
                // تحميل النشاط الأخير
                await LoadRecentActivityAsync();
                
                // تسجيل الوصول للوحة التحكم
                await _auditLogger.LogUserActionAsync(
                    "system",
                    "System",
                    "DashboardAccess",
                    "تم الوصول للوحة التحكم الرئيسية"
                );
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                await _auditLogger.LogSystemEventAsync(
                    "system",
                    "System",
                    "SystemError",
                    $"خطأ في تحميل بيانات لوحة التحكم: {ex.Message}"
                );
            }
        }
        
        private async Task LoadRecentActivityAsync()
        {
            try
            {
                RecentActivity.Clear();
                
                // الحصول على آخر 10 أنشطة من AuditLog
                var recentLogs = await _auditLogger.GetAuditLogsAsync();
                
                if (recentLogs != null)
                {
                    foreach (var log in recentLogs.Take(10))
                    {
                        RecentActivity.Add(new ActivityItem
                        {
                            Description = log.Action + ": " + log.Details,
                            Timestamp = log.Timestamp
                        });
                    }
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