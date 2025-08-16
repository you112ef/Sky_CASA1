using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows;
using System.Windows.Controls;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
            
            // إنشاء database connection factories
            var connectionFactory = new DatabaseConnectionFactory(null, null);
            var contextFactory = new MedicalLabContextFactory(null, null);
            var dbConnection = connectionFactory.CreateConnection();
            var context = contextFactory.CreateContext();
            
            _authService = new AuthService(null, context);
            _auditLogger = new AuditLogger(null, dbConnection);

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
                var roleName = _currentUser.RoleName ?? _currentUser.Role?.ToString() ?? "User";
                txtUserRole.Text = $"{roleName} - {GetRoleInArabic(roleName)}";
                
                // Update button permissions based on user role
                UpdateButtonPermissions();
            }
        }

        private string GetRoleInArabic(string role)
        {
            return role?.ToLower() switch
            {
                "admin" => "مدير",
                "doctor" => "طبيب",
                "labtech" => "فني مختبر",
                "labtechnician" => "فني مختبر",
                "technician" => "فني",
                "reception" => "استقبال",
                "receptionist" => "استقبال",
                "user" => "مستخدم",
                _ => role ?? "Unknown"
            };
        }

        private void UpdateButtonPermissions()
        {
            if (_currentUser == null) return;

            var roleName = (_currentUser.RoleName ?? _currentUser.Role?.ToString() ?? "User").ToLower();
            
            // Admin has access to everything
            if (roleName == "admin")
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
            if (roleName == "labtech" || roleName == "labtechnician" || roleName == "technician")
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
            if (roleName == "reception" || roleName == "receptionist")
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

            // Default user permissions
            btnDashboard.IsEnabled = true;
            btnPatients.IsEnabled = false;
            btnExams.IsEnabled = false;
            btnVideoAnalysis.IsEnabled = false;
            btnReports.IsEnabled = false;
            btnSettings.IsEnabled = false;
            btnBackup.IsEnabled = false;
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
                await NavigateToPageAsync_Internal(pageName);
                ShowLoading(false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error navigating to page: {PageName}", pageName);
                MessageBox.Show($"Error navigating to page: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ShowLoading(false);
            }
        }

        private async Task NavigateToPageAsync_Internal(string pageName)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    NavigateToPage(pageName);
                });
            });
        }

        private string GetPageTitle(string pageName)
        {
            return pageName?.ToLower() switch
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
            try
            {
                // Reset all buttons to default style
                var defaultStyle = FindResource("MaterialDesignFlatButton") as Style;
                var activeStyle = FindResource("MaterialDesignRaisedButton") as Style;
                
                if (defaultStyle != null)
                {
                    btnDashboard.Style = defaultStyle;
                    btnPatients.Style = defaultStyle;
                    btnExams.Style = defaultStyle;
                    btnVideoAnalysis.Style = defaultStyle;
                    btnReports.Style = defaultStyle;
                    btnSettings.Style = defaultStyle;
                    btnBackup.Style = defaultStyle;
                }

                // Highlight active button
                var activeButton = activePage?.ToLower() switch
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

                if (activeButton != null && activeStyle != null)
                {
                    activeButton.Style = activeStyle;
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
                var loginWindow = new LoginDialog();
                var result = loginWindow.ShowDialog();
                
                if (result == true)
                {
                    var user = await _authService.AuthenticateAsync(loginWindow.Username, loginWindow.Password);
                    if (user != null)
                    {
                        _currentUser = user;
                        await _auditLogger?.LogUserEvent(_currentUser.Id.ToString(), _currentUser.FullName, "User_Login", "Authentication", 
                            new { LoginTime = DateTime.Now, IPAddress = "Local" });
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة\nInvalid username or password", "فشل تسجيل الدخول\nLogin Failed", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return await ShowLoginDialogAsync();
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during login");
                MessageBox.Show($"خطأ في تسجيل الدخول: {ex.Message}\nLogin error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private void NavigateToPage(string pageName)
        {
            try
            {
                // Update page title
                if (txtPageTitle != null)
                {
                    txtPageTitle.Text = GetPageTitle(pageName);
                }
                
                // Navigate to appropriate content
                switch (pageName?.ToLower())
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
                        break; release/v1.0.0
                }
                
                // Update navigation button states
                UpdateNavigationButtonStates(pageName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error navigating to page: {PageName}", pageName);
                MessageBox.Show($"خطأ في التنقل: {ex.Message}\nNavigation error: {ex.Message}", "Error", 
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
            try
            {
                // Create exam management interface
                var examPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Orientation = Orientation.Vertical
                };

                // Title and new exam button
                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var title = new TextBlock
                {
                    Text = "إدارة الفحوصات\nExams Management",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(title, 0);
                headerGrid.Children.Add(title);

                var newExamButton = new Button
                {
                    Content = "فحص جديد\nNew Exam",
                    Margin = new Thickness(10, 0, 0, 0),
                    Padding = new Thickness(15, 8, 15, 8)
                };
                newExamButton.Click += (s, e) => ShowNewExamOptions();
                Grid.SetColumn(newExamButton, 1);
                headerGrid.Children.Add(newExamButton);
                
                examPanel.Children.Add(headerGrid);

                // Exam type buttons
                var typeGrid = new Grid
                {
                    Margin = new Thickness(0, 20, 0, 20)
                };
                
                for (int i = 0; i < 4; i++)
                {
                    typeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                var casaBtn = CreateExamTypeButton("CASA\nتحليل السائل المنوي", "#4CAF50");
                var cbcBtn = CreateExamTypeButton("CBC\nصورة الدم الكاملة", "#2196F3");
                var urineBtn = CreateExamTypeButton("Urine\nتحليل البول", "#FF9800");
                var stoolBtn = CreateExamTypeButton("Stool\nتحليل البراز", "#9C27B0");

                casaBtn.Click += (s, e) => MessageBox.Show("سيتم فتح واجهة فحص CASA\nCASA exam interface will open", "فحص CASA", MessageBoxButton.OK, MessageBoxImage.Information);
                cbcBtn.Click += (s, e) => MessageBox.Show("سيتم فتح واجهة فحص CBC\nCBC exam interface will open", "فحص CBC", MessageBoxButton.OK, MessageBoxImage.Information);
                urineBtn.Click += (s, e) => MessageBox.Show("سيتم فتح واجهة فحص البول\nUrine exam interface will open", "فحص البول", MessageBoxButton.OK, MessageBoxImage.Information);
                stoolBtn.Click += (s, e) => MessageBox.Show("سيتم فتح واجهة فحص البراز\nStool exam interface will open", "فحص البراز", MessageBoxButton.OK, MessageBoxImage.Information);

                Grid.SetColumn(casaBtn, 0);
                Grid.SetColumn(cbcBtn, 1);
                Grid.SetColumn(urineBtn, 2);
                Grid.SetColumn(stoolBtn, 3);

                typeGrid.Children.Add(casaBtn);
                typeGrid.Children.Add(cbcBtn);
                typeGrid.Children.Add(urineBtn);
                typeGrid.Children.Add(stoolBtn);
                
                examPanel.Children.Add(typeGrid);

                // Recent exams list
                var recentTitle = new TextBlock
                {
                    Text = "الفحوصات الأخيرة\nRecent Exams",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 20, 0, 10)
                };
                examPanel.Children.Add(recentTitle);

                var examsBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Height = 200,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                
                var examsContent = new TextBlock
                {
                    Text = "لا توجد فحوصات مسجلة\nNo exams recorded\n\nاستخدم أزرار أنواع الفحوصات أعلاه لبدء فحص جديد\nUse exam type buttons above to start a new exam",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 14
                };
                
                examsBorder.Child = examsContent;
                examPanel.Children.Add(examsBorder);

                if (contentFrame != null)
                {
                    contentFrame.Content = examPanel;
                }
                else if (MainFrame != null)
                {
                    MainFrame.Content = examPanel;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing exams");
                ShowSimpleContent("Exams - الفحوصات", "خطأ في تحميل واجهة الفحوصات\nError loading exams interface");
            }
        }

        private void ShowVideoAnalysis()
        {
            try
            {
                // Create video analysis interface
                var videoPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Orientation = Orientation.Vertical
                };

                var title = new TextBlock
                {
                    Text = "تحليل الفيديو - CASA\nVideo Analysis - CASA",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 30)
                };
                videoPanel.Children.Add(title);

                // Video input section
                var inputSection = new GroupBox
                {
                    Header = "تحميل الفيديو\nVideo Input",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var inputPanel = new StackPanel();
                
                var uploadButton = new Button
                {
                    Content = "اختيار ملف فيديو\nSelect Video File",
                    Height = 40,
                    Width = 200,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                uploadButton.Click += (s, e) => MessageBox.Show("سيتم فتح متصفح الملفات لاختيار الفيديو\nFile browser will open to select video", "اختيار فيديو", MessageBoxButton.OK, MessageBoxImage.Information);
                
                var supportedFormats = new TextBlock
                {
                    Text = "الصيغ المدعومة: MP4, AVI, MOV, WMV\nSupported formats: MP4, AVI, MOV, WMV",
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 12,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                
                inputPanel.Children.Add(uploadButton);
                inputPanel.Children.Add(supportedFormats);
                inputSection.Content = inputPanel;
                videoPanel.Children.Add(inputSection);

                // Analysis options
                var optionsSection = new GroupBox
                {
                    Header = "خيارات التحليل\nAnalysis Options",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var optionsGrid = new Grid();
                optionsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                optionsGrid.ColumnDefinitions.Add(new ColumnDefinition());
                optionsGrid.RowDefinitions.Add(new RowDefinition());
                optionsGrid.RowDefinitions.Add(new RowDefinition());

                var countCheck = new CheckBox { Content = "عدد الحيوانات المنوية\nSperm Count", IsChecked = true, Margin = new Thickness(5) };
                var motilityCheck = new CheckBox { Content = "الحركية\nMotility", IsChecked = true, Margin = new Thickness(5) };
                var morphologyCheck = new CheckBox { Content = "الشكل\nMorphology", IsChecked = true, Margin = new Thickness(5) };
                var velocityCheck = new CheckBox { Content = "السرعة\nVelocity", IsChecked = true, Margin = new Thickness(5) };

                Grid.SetRow(countCheck, 0); Grid.SetColumn(countCheck, 0);
                Grid.SetRow(motilityCheck, 0); Grid.SetColumn(motilityCheck, 1);
                Grid.SetRow(morphologyCheck, 1); Grid.SetColumn(morphologyCheck, 0);
                Grid.SetRow(velocityCheck, 1); Grid.SetColumn(velocityCheck, 1);

                optionsGrid.Children.Add(countCheck);
                optionsGrid.Children.Add(motilityCheck);
                optionsGrid.Children.Add(morphologyCheck);
                optionsGrid.Children.Add(velocityCheck);
                
                optionsSection.Content = optionsGrid;
                videoPanel.Children.Add(optionsSection);

                // Analysis button
                var analyzeButton = new Button
                {
                    Content = "بدء التحليل\nStart Analysis",
                    Height = 45,
                    Width = 150,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                analyzeButton.Click += (s, e) => MessageBox.Show("سيتم بدء تحليل الفيديو\nVideo analysis will start\n\nالمعالجة قد تستغرق عدة دقائق\nProcessing may take several minutes", "تحليل الفيديو", MessageBoxButton.OK, MessageBoxImage.Information);
                
                videoPanel.Children.Add(analyzeButton);

                // Results preview
                var previewBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Height = 150,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                
                var previewContent = new TextBlock
                {
                    Text = "معاينة النتائج\nResults Preview\n\nستظهر النتائج هنا بعد التحليل\nResults will appear here after analysis",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 14
                };
                
                previewBorder.Child = previewContent;
                videoPanel.Children.Add(previewBorder);

                if (contentFrame != null)
                {
                    contentFrame.Content = videoPanel;
                }
                else if (MainFrame != null)
                {
                    MainFrame.Content = videoPanel;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing video analysis");
                ShowSimpleContent("Video Analysis - تحليل الفيديو", "خطأ في تحميل واجهة تحليل الفيديو\nError loading video analysis interface");
            }
        }

        private void ShowReports()
        {
            try
            {
                // Create reports interface
                var reportsPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Orientation = Orientation.Vertical
                };

                var title = new TextBlock
                {
                    Text = "التقارير والإحصائيات\nReports & Statistics",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 30)
                };
                reportsPanel.Children.Add(title);

                // Report types grid
                var typesGrid = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 20)
                };
                
                for (int i = 0; i < 3; i++)
                {
                    typesGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                var dailyBtn = CreateReportTypeButton("التقرير اليومي\nDaily Report", "#4CAF50");
                var monthlyBtn = CreateReportTypeButton("التقرير الشهري\nMonthly Report", "#2196F3");
                var statisticsBtn = CreateReportTypeButton("الإحصائيات\nStatistics", "#FF9800");

                dailyBtn.Click += (s, e) => ShowReportDialog("Daily Report");
                monthlyBtn.Click += (s, e) => ShowReportDialog("Monthly Report");
                statisticsBtn.Click += (s, e) => ShowReportDialog("Statistics");

                Grid.SetColumn(dailyBtn, 0);
                Grid.SetColumn(monthlyBtn, 1);
                Grid.SetColumn(statisticsBtn, 2);

                typesGrid.Children.Add(dailyBtn);
                typesGrid.Children.Add(monthlyBtn);
                typesGrid.Children.Add(statisticsBtn);
                
                reportsPanel.Children.Add(typesGrid);

                // Date range selection
                var dateSection = new GroupBox
                {
                    Header = "نطاق التاريخ\nDate Range",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var datePanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var fromLabel = new Label { Content = "من:\nFrom:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
                var fromDate = new DatePicker { Width = 120, SelectedDate = DateTime.Today.AddDays(-30), Margin = new Thickness(5, 0, 20, 0) };
                var toLabel = new Label { Content = "إلى:\nTo:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
                var toDate = new DatePicker { Width = 120, SelectedDate = DateTime.Today, Margin = new Thickness(5, 0, 0, 0) };
                
                datePanel.Children.Add(fromLabel);
                datePanel.Children.Add(fromDate);
                datePanel.Children.Add(toLabel);
                datePanel.Children.Add(toDate);
                
                dateSection.Content = datePanel;
                reportsPanel.Children.Add(dateSection);

                // Export options
                var exportSection = new GroupBox
                {
                    Header = "تصدير التقرير\nExport Report",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var exportPanel = new StackPanel { Orientation = Orientation.Horizontal };
                
                var pdfBtn = new Button { Content = "PDF", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
                var excelBtn = new Button { Content = "Excel", Width = 80, Height = 30, Margin = new Thickness(0, 0, 10, 0) };
                var printBtn = new Button { Content = "طباعة\nPrint", Width = 80, Height = 30 };
                
                pdfBtn.Click += (s, e) => MessageBox.Show("سيتم تصدير التقرير كملف PDF\nReport will be exported as PDF", "تصدير PDF", MessageBoxButton.OK, MessageBoxImage.Information);
                excelBtn.Click += (s, e) => MessageBox.Show("سيتم تصدير التقرير كملف Excel\nReport will be exported as Excel", "تصدير Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                printBtn.Click += (s, e) => MessageBox.Show("سيتم طباعة التقرير\nReport will be printed", "طباعة", MessageBoxButton.OK, MessageBoxImage.Information);
                
                exportPanel.Children.Add(pdfBtn);
                exportPanel.Children.Add(excelBtn);
                exportPanel.Children.Add(printBtn);
                
                exportSection.Content = exportPanel;
                reportsPanel.Children.Add(exportSection);

                // Quick stats
                var statsTitle = new TextBlock
                {
                    Text = "إحصائيات سريعة\nQuick Statistics",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                reportsPanel.Children.Add(statsTitle);

                var statsBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(15),
                    Margin = new Thickness(0, 5, 0, 0)
                };
                
                var statsContent = new TextBlock
                {
                    Text = "إجمالي الفحوصات هذا الشهر: 0\nTotal exams this month: 0\n\nفحوصات اليوم: 0\nToday's exams: 0\n\nفحوصات معلقة: 0\nPending exams: 0",
                    FontSize = 14,
                    LineHeight = 20
                };
                
                statsBorder.Child = statsContent;
                reportsPanel.Children.Add(statsBorder);

                if (contentFrame != null)
                {
                    contentFrame.Content = reportsPanel;
                }
                else if (MainFrame != null)
                {
                    MainFrame.Content = reportsPanel;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing reports");
                ShowSimpleContent("Reports - التقارير", "خطأ في تحميل واجهة التقارير\nError loading reports interface");
            }
        }
        
        private Button CreateReportTypeButton(string text, string color)
        {
            var button = new Button
            {
                Content = text,
                Height = 60,
                Margin = new Thickness(5),
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)),
                Foreground = System.Windows.Media.Brushes.White
            };
            return button;
        }
        
        private void ShowReportDialog(string reportType)
        {
            MessageBox.Show(
                $"سيتم إنشاء {reportType}\n{reportType} will be generated\n\nقد يستغرق الأمر عدة ثوانٍ\nThis may take a few seconds",
                $"إنشاء تقرير\nGenerating {reportType}",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ShowSettings()
        {
            try
            {
                // Create settings interface
                var settingsPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Orientation = Orientation.Vertical
                };

                var title = new TextBlock
                {
                    Text = "إعدادات النظام\nSystem Settings",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 30)
                };
                settingsPanel.Children.Add(title);

                // User Management
                var userSection = new GroupBox
                {
                    Header = "إدارة المستخدمين\nUser Management",
                    Margin = new Thickness(0, 0, 0, 15),
                    Padding = new Thickness(15)
                };
                
                var userPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var addUserBtn = new Button { Content = "إضافة مستخدم\nAdd User", Width = 120, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
                var editUserBtn = new Button { Content = "تعديل مستخدم\nEdit User", Width = 120, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
                var deleteUserBtn = new Button { Content = "حذف مستخدم\nDelete User", Width = 120, Height = 35 };
                
                addUserBtn.Click += (s, e) => MessageBox.Show("سيتم فتح نافذة إضافة مستخدم جديد\nAdd new user dialog will open", "إضافة مستخدم", MessageBoxButton.OK, MessageBoxImage.Information);
                editUserBtn.Click += (s, e) => MessageBox.Show("سيتم فتح نافذة تعديل المستخدم\nEdit user dialog will open", "تعديل مستخدم", MessageBoxButton.OK, MessageBoxImage.Information);
                deleteUserBtn.Click += (s, e) => MessageBox.Show("سيتم فتح نافذة حذف المستخدم\nDelete user dialog will open", "حذف مستخدم", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                userPanel.Children.Add(addUserBtn);
                userPanel.Children.Add(editUserBtn);
                userPanel.Children.Add(deleteUserBtn);
                
                userSection.Content = userPanel;
                settingsPanel.Children.Add(userSection);

                // System Configuration
                var systemSection = new GroupBox
                {
                    Header = "إعدادات النظام\nSystem Configuration",
                    Margin = new Thickness(0, 0, 0, 15),
                    Padding = new Thickness(15)
                };
                
                var systemGrid = new Grid();
                systemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                systemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                systemGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                systemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                systemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                
                var dbLabel = new Label { Content = "قاعدة البيانات:\nDatabase:", Margin = new Thickness(0, 0, 10, 5) };
                var dbPath = new TextBox { Text = "./Database/medical_lab.db", IsReadOnly = true, Margin = new Thickness(0, 0, 0, 10) };
                
                var langLabel = new Label { Content = "اللغة:\nLanguage:", Margin = new Thickness(0, 0, 10, 5) };
                var langCombo = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
                langCombo.Items.Add("العربية - Arabic");
                langCombo.Items.Add("English - إنجليزي");
                langCombo.SelectedIndex = 0;
                
                var themeLabel = new Label { Content = "المظهر:\nTheme:", Margin = new Thickness(0, 0, 10, 5) };
                var themeCombo = new ComboBox();
                themeCombo.Items.Add("Light - فاتح");
                themeCombo.Items.Add("Dark - داكن");
                themeCombo.SelectedIndex = 0;
                
                Grid.SetRow(dbLabel, 0); Grid.SetColumn(dbLabel, 0);
                Grid.SetRow(dbPath, 0); Grid.SetColumn(dbPath, 1);
                Grid.SetRow(langLabel, 1); Grid.SetColumn(langLabel, 0);
                Grid.SetRow(langCombo, 1); Grid.SetColumn(langCombo, 1);
                Grid.SetRow(themeLabel, 2); Grid.SetColumn(themeLabel, 0);
                Grid.SetRow(themeCombo, 2); Grid.SetColumn(themeCombo, 1);
                
                systemGrid.Children.Add(dbLabel);
                systemGrid.Children.Add(dbPath);
                systemGrid.Children.Add(langLabel);
                systemGrid.Children.Add(langCombo);
                systemGrid.Children.Add(themeLabel);
                systemGrid.Children.Add(themeCombo);
                
                systemSection.Content = systemGrid;
                settingsPanel.Children.Add(systemSection);

                // CASA Settings
                var casaSection = new GroupBox
                {
                    Header = "إعدادات CASA\nCASA Settings",
                    Margin = new Thickness(0, 0, 0, 15),
                    Padding = new Thickness(15)
                };
                
                var casaGrid = new Grid();
                casaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                casaGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                casaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                casaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                
                var frameRateLabel = new Label { Content = "معدل الإطارات:\nFrame Rate:", Margin = new Thickness(0, 0, 10, 5) };
                var frameRateBox = new TextBox { Text = "30", Width = 100, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10) };
                
                var qualityLabel = new Label { Content = "جودة التحليل:\nAnalysis Quality:", Margin = new Thickness(0, 0, 10, 5) };
                var qualityCombo = new ComboBox { Width = 150, HorizontalAlignment = HorizontalAlignment.Left };
                qualityCombo.Items.Add("عالية - High");
                qualityCombo.Items.Add("متوسطة - Medium");
                qualityCombo.Items.Add("منخفضة - Low");
                qualityCombo.SelectedIndex = 0;
                
                Grid.SetRow(frameRateLabel, 0); Grid.SetColumn(frameRateLabel, 0);
                Grid.SetRow(frameRateBox, 0); Grid.SetColumn(frameRateBox, 1);
                Grid.SetRow(qualityLabel, 1); Grid.SetColumn(qualityLabel, 0);
                Grid.SetRow(qualityCombo, 1); Grid.SetColumn(qualityCombo, 1);
                
                casaGrid.Children.Add(frameRateLabel);
                casaGrid.Children.Add(frameRateBox);
                casaGrid.Children.Add(qualityLabel);
                casaGrid.Children.Add(qualityCombo);
                
                casaSection.Content = casaGrid;
                settingsPanel.Children.Add(casaSection);

                // Save button
                var saveButton = new Button
                {
                    Content = "حفظ الإعدادات\nSave Settings",
                    Height = 40,
                    Width = 150,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                saveButton.Click += (s, e) => 
                {
                    MessageBox.Show("تم حفظ الإعدادات بنجاح\nSettings saved successfully", "حفظ الإعدادات", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                
                settingsPanel.Children.Add(saveButton);

                if (contentFrame != null)
                {
                    contentFrame.Content = settingsPanel;
                }
                else if (MainFrame != null)
                {
                    MainFrame.Content = settingsPanel;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing settings");
                ShowSimpleContent("Settings - الإعدادات", "خطأ في تحميل واجهة الإعدادات\nError loading settings interface");
            }
        }

        private void ShowBackup()
        {
            try
            {
                // Create backup interface
                var backupPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    Orientation = Orientation.Vertical
                };

                var title = new TextBlock
                {
                    Text = "النسخ الاحتياطي والاستعادة\nBackup & Restore",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 30)
                };
                backupPanel.Children.Add(title);

                // Backup section
                var backupSection = new GroupBox
                {
                    Header = "إنشاء نسخة احتياطية\nCreate Backup",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var backupContent = new StackPanel();
                
                var backupDesc = new TextBlock
                {
                    Text = "إنشاء نسخة احتياطية من قاعدة البيانات والإعدادات\nCreate backup of database and settings",
                    Margin = new Thickness(0, 0, 0, 15),
                    TextWrapping = TextWrapping.Wrap
                };
                
                var backupButtons = new StackPanel { Orientation = Orientation.Horizontal };
                
                var fullBackupBtn = new Button
                {
                    Content = "نسخة كاملة\nFull Backup",
                    Width = 120,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                fullBackupBtn.Click += (s, e) => ShowBackupDialog("Full Backup");
                
                var dataOnlyBtn = new Button
                {
                    Content = "البيانات فقط\nData Only",
                    Width = 120,
                    Height = 35
                };
                dataOnlyBtn.Click += (s, e) => ShowBackupDialog("Data Only Backup");
                
                backupButtons.Children.Add(fullBackupBtn);
                backupButtons.Children.Add(dataOnlyBtn);
                
                backupContent.Children.Add(backupDesc);
                backupContent.Children.Add(backupButtons);
                
                backupSection.Content = backupContent;
                backupPanel.Children.Add(backupSection);

                // Restore section
                var restoreSection = new GroupBox
                {
                    Header = "استعادة النسخة الاحتياطية\nRestore Backup",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var restoreContent = new StackPanel();
                
                var restoreDesc = new TextBlock
                {
                    Text = "استعادة قاعدة البيانات من نسخة احتياطية سابقة\nRestore database from previous backup",
                    Margin = new Thickness(0, 0, 0, 15),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = System.Windows.Media.Brushes.DarkRed
                };
                
                var warning = new TextBlock
                {
                    Text = "تحذير: سيتم استبدال جميع البيانات الحالية\nWarning: All current data will be replaced",
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Red,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                
                var restoreButtons = new StackPanel { Orientation = Orientation.Horizontal };
                
                var selectFileBtn = new Button
                {
                    Content = "اختيار ملف\nSelect File",
                    Width = 120,
                    Height = 35,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                selectFileBtn.Click += (s, e) => MessageBox.Show("سيتم فتح متصفح الملفات لاختيار النسخة الاحتياطية\nFile browser will open to select backup file", "اختيار نسخة احتياطية", MessageBoxButton.OK, MessageBoxImage.Information);
                
                var restoreBtn = new Button
                {
                    Content = "استعادة\nRestore",
                    Width = 120,
                    Height = 35,
                    IsEnabled = false
                };
                restoreBtn.Click += (s, e) => ShowRestoreDialog();
                
                restoreButtons.Children.Add(selectFileBtn);
                restoreButtons.Children.Add(restoreBtn);
                
                restoreContent.Children.Add(restoreDesc);
                restoreContent.Children.Add(warning);
                restoreContent.Children.Add(restoreButtons);
                
                restoreSection.Content = restoreContent;
                backupPanel.Children.Add(restoreSection);

                // Auto backup settings
                var autoSection = new GroupBox
                {
                    Header = "النسخ الاحتياطي التلقائي\nAutomatic Backup",
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(15)
                };
                
                var autoContent = new StackPanel();
                
                var enableAuto = new CheckBox
                {
                    Content = "تفعيل النسخ الاحتياطي التلقائي\nEnable automatic backup",
                    Margin = new Thickness(0, 0, 0, 10)
                };
                
                var frequencyPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };
                var freqLabel = new Label { Content = "التكرار:\nFrequency:", Width = 100 };
                var freqCombo = new ComboBox { Width = 150 };
                freqCombo.Items.Add("يومياً - Daily");
                freqCombo.Items.Add("أسبوعياً - Weekly");
                freqCombo.Items.Add("شهرياً - Monthly");
                freqCombo.SelectedIndex = 1;
                
                frequencyPanel.Children.Add(freqLabel);
                frequencyPanel.Children.Add(freqCombo);
                
                autoContent.Children.Add(enableAuto);
                autoContent.Children.Add(frequencyPanel);
                
                autoSection.Content = autoContent;
                backupPanel.Children.Add(autoSection);

                // Recent backups
                var recentTitle = new TextBlock
                {
                    Text = "النسخ الاحتياطية الأخيرة\nRecent Backups",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 10, 0, 10)
                };
                backupPanel.Children.Add(recentTitle);

                var recentBorder = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Height = 120,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                
                var recentContent = new TextBlock
                {
                    Text = "لا توجد نسخ احتياطية\nNo backups found\n\nقم بإنشاء نسخة احتياطية أولاً\nCreate a backup first",
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Gray,
                    FontSize = 14
                };
                
                recentBorder.Child = recentContent;
                backupPanel.Children.Add(recentBorder);

                if (contentFrame != null)
                {
                    contentFrame.Content = backupPanel;
                }
                else if (MainFrame != null)
                {
                    MainFrame.Content = backupPanel;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing backup");
                ShowSimpleContent("Backup - النسخ الاحتياطي", "خطأ في تحميل واجهة النسخ الاحتياطي\nError loading backup interface");
            }
        }
        
        private void ShowBackupDialog(string backupType)
        {
            var result = MessageBox.Show(
                $"هل تريد إنشاء {backupType}؟\nDo you want to create {backupType}?\n\nقد يستغرق الأمر عدة دقائق\nThis may take several minutes",
                "تأكيد النسخ الاحتياطي\nConfirm Backup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "تم إنشاء النسخة الاحتياطية بنجاح\nBackup created successfully",
                    "نجح النسخ الاحتياطي\nBackup Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
        
        private void ShowRestoreDialog()
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من استعادة النسخة الاحتياطية؟\nAre you sure you want to restore the backup?\n\nسيتم فقدان جميع البيانات الحالية!\nAll current data will be lost!",
                "تأكيد الاستعادة\nConfirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "تمت استعادة النسخة الاحتياطية بنجاح\nBackup restored successfully\n\nيجب إعادة تشغيل التطبيق\nApplication must be restarted",
                    "نجحت الاستعادة\nRestore Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }

        private void ShowLoading(bool show)
        {
            try
            {
                // Try multiple possible loading overlay names
                var overlayNames = new[] { "LoadingOverlay", "loadingOverlay", "ProgressOverlay" };
                
                foreach (var overlayName in overlayNames)
                {
                    try
                    {
                        var overlay = FindName(overlayName) as FrameworkElement;
                        if (overlay != null)
                        {
                            overlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                            return;
                        }
                    }
                    catch
                    {
                        // Continue to next overlay name
                    }
                }
                
                // If no overlay found, just log it
                _logger?.LogDebug("Loading overlay not found, continuing without loading indicator");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error showing/hiding loading overlay");
            }
        }
        
        /// <summary>
        /// Display simple content when full interface fails to load
        /// </summary>
        private void ShowSimpleContent(string title, string message)
        {
            var content = new StackPanel
            {
                Margin = new Thickness(20),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            
            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            
            var messageBlock = new TextBlock
            {
                Text = message,
                FontSize = 14,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            
            content.Children.Add(titleBlock);
            content.Children.Add(messageBlock);
            
            if (contentFrame != null)
            {
                contentFrame.Content = content;
            }
            else if (MainFrame != null)
            {
                MainFrame.Content = content;
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

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من تسجيل الخروج؟\nAre you sure you want to logout?", 
                    "تأكيد تسجيل الخروج\nConfirm Logout", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Log the logout event
                    await _auditLogger?.LogUserEvent(_currentUser?.Id.ToString() ?? "Unknown", _currentUser?.FullName ?? "Unknown", "User_Logout", "Authentication", 
                        new { LogoutTime = DateTime.Now, IPAddress = "Local" });
                    
                    await _authService.LogoutAsync();
                    _currentUser = null;
                    
                    // Show login dialog again
                    var loginResult = await ShowLoginDialogAsync();
                    if (!loginResult)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                    
                    _currentUser = _authService.CurrentUser;
                    UpdateUserInfo();
                    NavigateToPage("Dashboard");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during logout");
                MessageBox.Show($"خطأ في تسجيل الخروج: {ex.Message}\nLogout error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                _logger?.LogInformation("Main window closed");
                
                // Log application close event
                if (_currentUser != null)
                {
                    _auditLogger?.LogUserEvent(_currentUser.Id.ToString(), _currentUser.FullName, "Application_Closed", "System", 
                        new { CloseTime = DateTime.Now });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during window close");
            }
            finally
            {
                base.OnClosed(e);
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