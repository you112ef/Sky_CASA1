using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Models;
using MedicalLabAnalyzer.Views;
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
                txtUserRole.Text = $"{_currentUser.RoleName} - {GetRoleInArabic(_currentUser.RoleName)}";
                
                // Update button permissions based on user role
                UpdateButtonPermissions();
            }
        }

        private string GetRoleInArabic(string role)
        {
            return role switch
            {
                "Admin" => "مدير",
                "LabTech" => "فني مختبر",
                "Reception" => "استقبال",
                _ => role
            };
        }

        private void UpdateButtonPermissions()
        {
            if (_currentUser == null) return;

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
            }
        }

        private async Task<bool> ShowLoginDialogAsync()
        {
            try
            {
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
                return false;
            }
        }

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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error navigating to page: {pageName}");
                MessageBox.Show($"Navigation error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
            contentFrame.Content = new PatientManagementView();
        }

        private void ShowExams()
        {
            contentFrame.Content = new ExamManagementView();
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
            contentFrame.Content = new ReportsView();
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
        }
    }
}