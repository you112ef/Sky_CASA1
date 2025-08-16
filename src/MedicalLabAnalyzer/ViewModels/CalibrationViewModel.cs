using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.ViewModels
{
    public class CalibrationViewModel : INotifyPropertyChanged
    {
        private readonly CalibrationService _calibrationService;
        private readonly AuditLogger _auditLogger;
        
        private double _micronsPerPixel;
        private double _fps;
        private string _userName;
        private bool _isLoading;
        private bool _isSaving;
        
        public CalibrationViewModel()
        {
            // Initialize services
            _calibrationService = new CalibrationService();
            _auditLogger = new AuditLogger();
            
            // Initialize commands
            SaveCommand = new RelayCommand(async () => await SaveCalibrationAsync(), CanSave);
            CancelCommand = new RelayCommand(Cancel);
            
            // Set default user name
            UserName = Environment.UserName;
            
            // Load current calibration settings
            _ = LoadCalibrationAsync();
        }
        
        #region Properties
        
        public double MicronsPerPixel
        {
            get => _micronsPerPixel;
            set
            {
                _micronsPerPixel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }
        
        public double FPS
        {
            get => _fps;
            set
            {
                _fps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }
        
        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
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
        
        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
            }
        }
        
        public bool CanSave => !string.IsNullOrWhiteSpace(UserName) && 
                              MicronsPerPixel > 0 && 
                              FPS > 0 && 
                              !IsSaving && 
                              !IsLoading;
        
        #endregion
        
        #region Commands
        
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        
        #endregion
        
        #region Events
        
        public event EventHandler<bool> RequestClose;
        
        #endregion
        
        #region Methods
        
        private async Task LoadCalibrationAsync()
        {
            IsLoading = true;
            
            try
            {
                var calibration = await _calibrationService.GetLatestCalibrationAsync();
                
                if (calibration != null)
                {
                    MicronsPerPixel = calibration.MicronsPerPixel;
                    FPS = calibration.FPS;
                }
                else
                {
                    // استخدام القيم الافتراضية إذا لم يتم العثور على معايرة سابقة
                    MicronsPerPixel = 0.5;
                    FPS = 25.0;
                }
            }
            catch (Exception ex)
            {
                // استخدام القيم الافتراضية في حالة الخطأ
                MicronsPerPixel = 0.5;
                FPS = 25.0;
                
                MessageBox.Show($"تعذر تحميل إعدادات المعايرة السابقة: {ex.Message}\nسيتم استخدام القيم الافتراضية.", 
                              "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task SaveCalibrationAsync()
        {
            if (!CanSave) return;
            
            // التحقق من صحة القيم
            if (MicronsPerPixel <= 0)
            {
                MessageBox.Show("الرجاء إدخال قيمة صحيحة لـ Microns/Pixel (أكبر من صفر)", 
                              "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (FPS <= 0)
            {
                MessageBox.Show("الرجاء إدخال قيمة صحيحة لـ FPS (أكبر من صفر)", 
                              "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("الرجاء إدخال اسم المستخدم", 
                              "خطأ في البيانات", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            IsSaving = true;
            
            try
            {
                var calibration = new Calibration
                {
                    MicronsPerPixel = MicronsPerPixel,
                    FPS = FPS,
                    UserName = UserName,
                    CreatedAt = DateTime.Now
                };
                
                await _calibrationService.SaveCalibrationAsync(calibration);
                
                // تسجيل العملية في نظام المراجعة
                await _auditLogger.LogAsync(
                    EventType.CalibrationUpdated,
                    $"تم حفظ إعدادات المعايرة - MicronsPerPixel: {MicronsPerPixel:F3}, FPS: {FPS:F1}",
                    UserName,
                    0
                );
                
                MessageBox.Show("تم حفظ إعدادات المعايرة بنجاح", 
                              "نجح الحفظ", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // إغلاق النافذة بنجاح
                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ إعدادات المعايرة: {ex.Message}", 
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // تسجيل الخطأ
                await _auditLogger.LogAsync(
                    EventType.SystemError,
                    $"خطأ في حفظ إعدادات المعايرة: {ex.Message}",
                    UserName ?? "Unknown",
                    0
                );
            }
            finally
            {
                IsSaving = false;
            }
        }
        
        private void Cancel()
        {
            // إغلاق النافذة بدون حفظ
            RequestClose?.Invoke(this, false);
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
}