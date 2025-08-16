using System;
using System.Windows;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private readonly AuthService _authService;

        public LoginView()
        {
            InitializeComponent();
            
            // Initialize auth service (in a real app, this would come from DI)
            _authService = new AuthService();
            
            // Set default values for testing
            UsernameTextBox.Text = "admin";
            PasswordBox.Password = "admin123";
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable login button
                LoginButton.IsEnabled = false;
                StatusTextBlock.Visibility = Visibility.Collapsed;

                // Get credentials
                string username = UsernameTextBox.Text?.Trim();
                string password = PasswordBox.Password;

                // Validate input
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ShowError("يرجى إدخال اسم المستخدم وكلمة المرور");
                    return;
                }

                // Attempt login
                bool loginSuccess = _authService.AuthenticateUser(username, password);

                if (loginSuccess)
                {
                    // Login successful - show main window
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("اسم المستخدم أو كلمة المرور غير صحيحة");
                }
            }
            catch (Exception ex)
            {
                ShowError($"خطأ في تسجيل الدخول: {ex.Message}");
            }
            finally
            {
                // Re-enable login button
                LoginButton.IsEnabled = true;
            }
        }

        private void ShowError(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Visibility = Visibility.Visible;
        }

        private void ShowSuccess(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            StatusTextBlock.Visibility = Visibility.Visible;
        }
    }
}