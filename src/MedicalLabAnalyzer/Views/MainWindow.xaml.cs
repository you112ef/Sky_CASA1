using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using MedicalLabAnalyzer.ViewModels;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Models;
using System.Threading.Tasks;

namespace MedicalLabAnalyzer.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainWindow> _logger;
        private readonly IAuthenticationService _authService;
        private User? _currentUser;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<MainWindow>>();
            _authService = serviceProvider.GetRequiredService<IAuthenticationService>();

            Loaded += MainWindow_Loaded;
            InitializeNavigation();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading(true);
                
                // Check if user is authenticated
                _currentUser = await _authService.GetCurrentUserAsync();
                if (_currentUser == null)
                {
                    // Show login dialog
                    var loginResult = await ShowLoginDialogAsync();
                    if (!loginResult)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                    _currentUser = await _authService.GetCurrentUserAsync();
                }

                UpdateUserInfo();
                NavigateToPage("Dashboard");
                ShowLoading(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during window initialization");
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void InitializeNavigation()
        {
            // Set default button states
            btnDashboard.IsEnabled = true;
            btnPatients.IsEnabled = true;
            btnExams.IsEnabled = true;
            btnVideoAnalysis.IsEnabled = true;
            btnReports.IsEnabled = true;
            btnSettings.IsEnabled = true;
            btnBackup.IsEnabled = true;
        }

        private void UpdateUserInfo()
        {
            if (_currentUser != null)
            {
                txtUserName.Text = $"Welcome {_currentUser.FullName} - مرحباً {_currentUser.FullName}";
                txtUserRole.Text = $"{_currentUser.Role} - {GetRoleInArabic(_currentUser.Role)}";
                
                // Update button permissions based on user role
                UpdateButtonPermissions();
            }
        }

        private string GetRoleInArabic(string role)
        {
            return role switch
            {
                "Admin" => "مدير",
                "Doctor" => "طبيب",
                "Technician" => "فني",
                "User" => "مستخدم",
                _ => role
            };
        }

        private void UpdateButtonPermissions()
        {
            if (_currentUser == null) return;

            btnPatients.IsEnabled = _currentUser.HasPermission("ViewPatients");
            btnExams.IsEnabled = _currentUser.HasPermission("ViewExams");
            btnVideoAnalysis.IsEnabled = _currentUser.HasPermission("VideoAnalysis");
            btnReports.IsEnabled = _currentUser.HasPermission("ViewReports");
            btnSettings.IsEnabled = _currentUser.IsAdmin;
            btnBackup.IsEnabled = _currentUser.IsAdmin;
        }

        private async void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                await NavigateToPageAsync(pageName);
            }
        }

        private async Task NavigateToPageAsync(string pageName)
        {
            try
            {
                ShowLoading(true);
                await NavigateToPage(pageName);
                ShowLoading(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to page: {PageName}", pageName);
                MessageBox.Show($"Error navigating to page: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ShowLoading(false);
            }
        }

        private async Task NavigateToPage(string pageName)
        {
            try
            {
                txtPageTitle.Text = GetPageTitle(pageName);
                
                switch (pageName.ToLower())
                {
                    case "dashboard":
                        var dashboardView = _serviceProvider.GetRequiredService<DashboardView>();
                        MainFrame.Navigate(dashboardView);
                        break;

                    case "patients":
                        var patientsView = _serviceProvider.GetRequiredService<PatientsView>();
                        MainFrame.Navigate(patientsView);
                        break;

                    case "exams":
                        var examsView = _serviceProvider.GetRequiredService<ExamsView>();
                        MainFrame.Navigate(examsView);
                        break;

                    case "videoanalysis":
                        var videoAnalysisView = _serviceProvider.GetRequiredService<VideoAnalysisView>();
                        MainFrame.Navigate(videoAnalysisView);
                        break;

                    case "reports":
                        var reportsView = _serviceProvider.GetRequiredService<ReportsView>();
                        MainFrame.Navigate(reportsView);
                        break;

                    case "settings":
                        var settingsView = _serviceProvider.GetRequiredService<SettingsView>();
                        MainFrame.Navigate(settingsView);
                        break;

                    case "backup":
                        var backupView = _serviceProvider.GetRequiredService<BackupView>();
                        MainFrame.Navigate(backupView);
                        break;

                    default:
                        var dashboardViewDefault = _serviceProvider.GetRequiredService<DashboardView>();
                        MainFrame.Navigate(dashboardViewDefault);
                        break;
                }

                // Update navigation button states
                UpdateNavigationButtonStates(pageName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to page: {PageName}", pageName);
                throw;
            }
        }

        private string GetPageTitle(string pageName)
        {
            return pageName.ToLower() switch
            {
                "dashboard" => "Dashboard - لوحة التحكم",
                "patients" => "Patients Management - إدارة المرضى",
                "exams" => "Exams Management - إدارة الفحوصات",
                "videoanalysis" => "Video Analysis - تحليل الفيديو",
                "reports" => "Reports - التقارير",
                "settings" => "Settings - الإعدادات",
                "backup" => "Backup & Restore - النسخ الاحتياطي",
                _ => "Dashboard - لوحة التحكم"
            };
        }

        private void UpdateNavigationButtonStates(string activePage)
        {
            // Reset all buttons
            btnDashboard.Style = FindResource("NavigationButtonStyle") as Style;
            btnPatients.Style = FindResource("NavigationButtonStyle") as Style;
            btnExams.Style = FindResource("NavigationButtonStyle") as Style;
            btnVideoAnalysis.Style = FindResource("NavigationButtonStyle") as Style;
            btnReports.Style = FindResource("NavigationButtonStyle") as Style;
            btnSettings.Style = FindResource("NavigationButtonStyle") as Style;
            btnBackup.Style = FindResource("NavigationButtonStyle") as Style;

            // Highlight active button
            var activeButton = activePage.ToLower() switch
            {
                "dashboard" => btnDashboard,
                "patients" => btnPatients,
                "exams" => btnExams,
                "videoanalysis" => btnVideoAnalysis,
                "reports" => btnReports,
                "settings" => btnSettings,
                "backup" => btnBackup,
                _ => btnDashboard
            };

            if (activeButton != null)
            {
                activeButton.Style = FindResource("MaterialDesignRaisedButton") as Style;
            }
        }

        private async Task<bool> ShowLoginDialogAsync()
        {
            try
            {
                var loginView = _serviceProvider.GetRequiredService<LoginView>();
                var loginWindow = new Window
                {
                    Title = "Login - تسجيل الدخول",
                    Content = loginView,
                    Width = 400,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.ToolWindow
                };

                var result = loginWindow.ShowDialog();
                return result == true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing login dialog");
                return false;
            }
        }

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to logout? - هل أنت متأكد من تسجيل الخروج؟", 
                    "Confirm Logout - تأكيد تسجيل الخروج", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _authService.LogoutAsync();
                    _currentUser = null;
                    
                    // Show login dialog again
                    var loginResult = await ShowLoginDialogAsync();
                    if (!loginResult)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                    
                    _currentUser = await _authService.GetCurrentUserAsync();
                    UpdateUserInfo();
                    NavigateToPage("Dashboard");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnClosed(EventArgs e)
        {
            _logger.LogInformation("Main window closed");
            base.OnClosed(e);
        }
    }
}