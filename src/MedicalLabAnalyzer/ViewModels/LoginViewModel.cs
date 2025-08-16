using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedicalLabAnalyzer.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly AuditLogger _auditLogger;
        
        private string _username = "";
        private string _password = "";
        private string _errorMessage = "";
        private bool _hasError = false;
        private bool _isLoading = false;
        private UserRole _selectedRole;
        
        public LoginViewModel(IConfiguration configuration = null, ILogger<LoginViewModel> logger = null)
        {
            // إنشاء database connection factories
            var connectionFactory = new DatabaseConnectionFactory(configuration, null);
            var contextFactory = new MedicalLabContextFactory(configuration, null);
            var dbConnection = connectionFactory.CreateConnection();
            var context = contextFactory.CreateContext();
            
            _authService = new AuthService(null, context);
            _userService = new UserService(null, dbConnection);
            _auditLogger = new AuditLogger(null, dbConnection);
            
            LoginCommand = new RelayCommand(async () => await LoginAsync(), () => CanLogin);
            AvailableRoles = new ObservableCollection<UserRole>
            {
                new UserRole { RoleType = UserRoleType.Admin, DisplayName = "مدير النظام" },
                new UserRole { RoleType = UserRoleType.LabTechnician, DisplayName = "فني مختبر" },
                new UserRole { RoleType = UserRoleType.Receptionist, DisplayName = "مستقبل" }
            };
            
            SelectedRole = AvailableRoles[1]; // Default to Lab Technician
        }
        
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanLogin));
            }
        }
        
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanLogin));
            }
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                HasError = !string.IsNullOrEmpty(value);
            }
        }
        
        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
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
                OnPropertyChanged(nameof(CanLogin));
            }
        }
        
        public UserRole SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<UserRole> AvailableRoles { get; }
        
        public bool CanLogin => !string.IsNullOrEmpty(Username) && 
                               !string.IsNullOrEmpty(Password) && 
                               SelectedRole != null && 
                               !IsLoading;
        
        public ICommand LoginCommand { get; }
        
        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = "";
                
                // محاولة تسجيل الدخول
                var loginResult = await _authService.LoginAsync(Username, Password, SelectedRole.RoleType);
                
                if (loginResult.IsSuccess)
                {
                    // تسجيل الحدث في AuditLog
                    await _auditLogger.LogUserActionAsync(
                        loginResult.User?.Id.ToString() ?? "0",
                        Username,
                        "UserLogin",
                        $"تسجيل دخول ناجح للمستخدم: {Username}"
                    );
                    
                    // فتح النافذة الرئيسية
                    var mainWindow = new Views.MainWindow(loginResult.User);
                    mainWindow.Show();
                    
                    // إغلاق نافذة تسجيل الدخول
                    Application.Current.MainWindow?.Close();
                    Application.Current.MainWindow = mainWindow;
                }
                else
                {
                    ErrorMessage = loginResult.ErrorMessage;
                    
                    // تسجيل محاولة تسجيل دخول فاشلة
                    await _auditLogger.LogUserActionAsync(
                        "0",
                        Username,
                        "LoginFailed",
                        $"محاولة تسجيل دخول فاشلة للمستخدم: {Username}"
                    );
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في النظام: {ex.Message}";
                
                await _auditLogger.LogSystemEventAsync(
                    "0",
                    Username,
                    "SystemError",
                    $"خطأ في تسجيل الدخول: {ex.Message}"
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
    
    public class UserRole
    {
        public UserRoleType RoleType { get; set; }
        public string DisplayName { get; set; }
    }
    
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