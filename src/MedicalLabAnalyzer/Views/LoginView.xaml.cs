using System.Windows;
using MedicalLabAnalyzer.ViewModels;

namespace MedicalLabAnalyzer.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            
            // ربط PasswordBox مع ViewModel
            txtPassword.PasswordChanged += (s, e) =>
            {
                if (DataContext is LoginViewModel viewModel)
                {
                    viewModel.Password = txtPassword.Password;
                }
            };
        }
    }
}