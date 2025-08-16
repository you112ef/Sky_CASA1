using System.Windows;

namespace MedicalLabAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // إنشاء النافذة الرئيسية
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}