using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Models;
using System.Threading.Tasks;

namespace MedicalLabAnalyzer.Views
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainWindow> _logger;
        private readonly AuthService _authService;
        private readonly AuditLogger _auditLogger;
        private User? _currentUser;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<MainWindow>>();
            _authService = serviceProvider.GetRequiredService<AuthService>();
            _auditLogger = serviceProvider.GetRequiredService<AuditLogger>();

            Loaded += MainWindow_Loaded;
            InitializeNavigation();
        }

        // Constructor overload for direct user injection (used by LoginViewModel)
        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _authService = new AuthService();
            _auditLogger = new AuditLogger();

            Loaded += MainWindow_Loaded;
            InitializeNavigation();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading(true);
                
                // Check if user is authenticated
                _currentUser = _authService.CurrentUser;
                if (_currentUser == null)
                {
                    // Show login dialog
                    var loginResult = await ShowLoginDialogAsync();
                    if (!loginResult)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                    _currentUser = _authService.CurrentUser;
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
<<<<<<< HEAD
                txtUserRole.Text = $"{_currentUser.Role} - {GetRoleInArabic(_currentUser.Role)}";
=======
                txtUserRole.Text = $"{_currentUser.RoleName} - {GetRoleInArabic(_currentUser.RoleName)}";
>>>>>>> release/v1.0.0
                
                // Update button permissions based on user role
                UpdateButtonPermissions();
            }
        }

        private string GetRoleInArabic(string role)
        {
            return role switch
            {
                "Admin" => "مدير",
<<<<<<< HEAD
                "Doctor" => "طبيب",
                "Technician" => "فني",
                "User" => "مستخدم",
=======
                "LabTech" => "فني مختبر",
                "Reception" => "استقبال",
>>>>>>> release/v1.0.0
                _ => role
            };
        }

        private void UpdateButtonPermissions()
        {
            if (_currentUser == null) return;

<<<<<<< HEAD
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
=======
            // إخفاء جميع الأزرار أولاً
            btnPatients.Visibility = Visibility.Collapsed;
            btnExams.Visibility = Visibility.Collapsed;
            btnVideoAnalysis.Visibility = Visibility.Collapsed;
            btnReports.Visibility = Visibility.Collapsed;
            btnSettings.Visibility = Visibility.Collapsed;
            btnBackup.Visibility = Visibility.Collapsed;

            // إظهار الأزرار حسب نوع المستخدم
            switch (_currentUser.Role)
            {
                case UserRoleType.Admin:
                    // المدير: جميع الصلاحيات
                    btnPatients.Visibility = Visibility.Visible;
                    btnExams.Visibility = Visibility.Visible;
                    btnVideoAnalysis.Visibility = Visibility.Visible;
                    btnReports.Visibility = Visibility.Visible;
                    btnSettings.Visibility = Visibility.Visible;
                    btnBackup.Visibility = Visibility.Visible;
                    break;

                case UserRoleType.LabTechnician:
                    // فني المختبر: إدارة الفحوصات والتحليل والتقارير
                    btnExams.Visibility = Visibility.Visible;
                    btnVideoAnalysis.Visibility = Visibility.Visible;
                    btnReports.Visibility = Visibility.Visible;
                    break;

                case UserRoleType.Receptionist:
                    // المستقبل: إدارة المرضى فقط
                    btnPatients.Visibility = Visibility.Visible;
                    break;
            }

            var role = _currentUser.RoleName?.ToLower();
            
            // Admin has access to everything
            if (role == "admin")
            {
                btnDashboard.IsEnabled = true;
                btnPatients.IsEnabled = true;
                btnExams.IsEnabled = true;
                btnVideoAnalysis.IsEnabled = true;
                btnReports.IsEnabled = true;
                btnSettings.IsEnabled = true;
                btnBackup.IsEnabled = true;
                return;
            }

            // LabTech permissions
            if (role == "labtech")
            {
                btnDashboard.IsEnabled = true;
                btnPatients.IsEnabled = true;
                btnExams.IsEnabled = true;
                btnVideoAnalysis.IsEnabled = true;
                btnReports.IsEnabled = true;
                btnSettings.IsEnabled = false;
                btnBackup.IsEnabled = false;
                return;
            }

            // Reception permissions
            if (role == "reception")
            {
                btnDashboard.IsEnabled = true;
                btnPatients.IsEnabled = true;
                btnExams.IsEnabled = true;
                btnVideoAnalysis.IsEnabled = false;
                btnReports.IsEnabled = false;
                btnSettings.IsEnabled = false;
                btnBackup.IsEnabled = false;
                return;
>>>>>>> release/v1.0.0
            }
        }

        private async Task<bool> ShowLoginDialogAsync()
        {
            try
            {
<<<<<<< HEAD
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
=======
                // Simple login dialog for now
                var loginWindow = new LoginDialog();
                var result = loginWindow.ShowDialog();
                
                if (result == true)
                {
                    var user = _authService.Authenticate(loginWindow.Username, loginWindow.Password);
                    if (user != null)
                    {
                        _currentUser = user;
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Invalid username or password", "Login Failed", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return await ShowLoginDialogAsync();
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                MessageBox.Show($"Login error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
>>>>>>> release/v1.0.0
                return false;
            }
        }

<<<<<<< HEAD
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
=======
        private void NavigateToPage(string pageName)
        {
            try
            {
                // Hide all content frames
                contentFrame.Content = null;
                
                switch (pageName.ToLower())
                {
                    case "dashboard":
                        ShowDashboard();
                        break;
                    case "patients":
                        ShowPatients();
                        break;
                    case "exams":
                        ShowExams();
                        break;
                    case "videoanalysis":
                        ShowVideoAnalysis();
                        break;
                    case "reports":
                        ShowReports();
                        break;
                    case "settings":
                        ShowSettings();
                        break;
                    case "backup":
                        ShowBackup();
                        break;
                    default:
                        ShowDashboard();
                        break;
>>>>>>> release/v1.0.0
                }
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
                _logger.LogError(ex, "Error during logout");
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", 
=======
                _logger.LogError(ex, $"Error navigating to page: {pageName}");
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", 
>>>>>>> release/v1.0.0
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

<<<<<<< HEAD
        private void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnClosed(EventArgs e)
        {
            _logger.LogInformation("Main window closed");
            base.OnClosed(e);
=======
        private void ShowDashboard()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Dashboard - لوحة التحكم\n\nWelcome to Medical Lab Analyzer\nمرحباً بك في محلل المختبر الطبي",
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowPatients()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Patients Management - إدارة المرضى\n\nPatient management interface will be implemented here\nسيتم تنفيذ واجهة إدارة المرضى هنا",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowExams()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Exams Management - إدارة الفحوصات\n\nExam management interface will be implemented here\nسيتم تنفيذ واجهة إدارة الفحوصات هنا",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowVideoAnalysis()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Video Analysis - تحليل الفيديو\n\nVideo analysis interface will be implemented here\nسيتم تنفيذ واجهة تحليل الفيديو هنا",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowReports()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Reports - التقارير\n\nReports interface will be implemented here\nسيتم تنفيذ واجهة التقارير هنا",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowSettings()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Settings - الإعدادات\n\nSettings interface will be implemented here\nسيتم تنفيذ واجهة الإعدادات هنا",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowBackup()
        {
            contentFrame.Content = new TextBlock
            {
                Text = "Backup & Restore - النسخ الاحتياطي والاستعادة\n\nBackup interface will be implemented here\nسيتم تنفيذ واجهة النسخ الاحتياطي هنا",
                FontSize = 16,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void ShowLoading(bool show)
        {
            if (loadingOverlay != null)
            {
                loadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Navigation button click handlers
        private void btnDashboard_Click(object sender, RoutedEventArgs e) => NavigateToPage("Dashboard");
        private void btnPatients_Click(object sender, RoutedEventArgs e) => NavigateToPage("Patients");
        private void btnExams_Click(object sender, RoutedEventArgs e) => NavigateToPage("Exams");
        private void btnVideoAnalysis_Click(object sender, RoutedEventArgs e) => NavigateToPage("VideoAnalysis");
        private void btnReports_Click(object sender, RoutedEventArgs e) => NavigateToPage("Reports");
        private void btnSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage("Settings");
        private void btnBackup_Click(object sender, RoutedEventArgs e) => NavigateToPage("Backup");

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _authService.Logout();
                _currentUser = null;
                
                // Show login dialog again
                ShowLoginDialogAsync().ContinueWith(async task =>
                {
                    if (await task)
                    {
                        _currentUser = _authService.CurrentUser;
                        UpdateUserInfo();
                        NavigateToPage("Dashboard");
                    }
                    else
                    {
                        Application.Current.Shutdown();
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                MessageBox.Show($"Logout error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // Simple login dialog
    public class LoginDialog : Window
    {
        public string Username { get; private set; } = "";
        public string Password { get; private set; } = "";

        public LoginDialog()
        {
            Title = "Login - تسجيل الدخول";
            Width = 300;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Margin = new Thickness(20);

            // Username
            var usernameLabel = new Label { Content = "Username - اسم المستخدم:" };
            var usernameBox = new TextBox { Name = "txtUsername" };
            Grid.SetRow(usernameLabel, 0);
            Grid.SetRow(usernameBox, 1);
            grid.Children.Add(usernameLabel);
            grid.Children.Add(usernameBox);

            // Password
            var passwordLabel = new Label { Content = "Password - كلمة المرور:", Margin = new Thickness(0, 10, 0, 0) };
            var passwordBox = new PasswordBox { Name = "txtPassword" };
            Grid.SetRow(passwordLabel, 2);
            Grid.SetRow(passwordBox, 3);
            grid.Children.Add(passwordLabel);
            grid.Children.Add(passwordBox);

            // Buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            var loginButton = new Button { Content = "Login - دخول", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Cancel - إلغاء", Width = 80 };

            loginButton.Click += (s, e) =>
            {
                Username = usernameBox.Text;
                Password = passwordBox.Password;
                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Please enter username and password", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            cancelButton.Click += (s, e) =>
            {
                DialogResult = false;
                Close();
            };

            buttonPanel.Children.Add(loginButton);
            buttonPanel.Children.Add(cancelButton);
            grid.Children.Add(buttonPanel);

            Content = grid;
>>>>>>> release/v1.0.0
        }
    }
}