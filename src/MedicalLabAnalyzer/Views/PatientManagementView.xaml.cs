using System.Windows.Controls;
using MedicalLabAnalyzer.ViewModels;

namespace MedicalLabAnalyzer.Views
{
    public partial class PatientManagementView : UserControl
    {
        public PatientManagementView()
        {
            InitializeComponent();
            DataContext = new PatientManagementViewModel();
        }
    }
}