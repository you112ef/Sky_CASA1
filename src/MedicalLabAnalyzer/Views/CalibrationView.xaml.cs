using System.Windows;
using MedicalLabAnalyzer.ViewModels;

namespace MedicalLabAnalyzer.Views
{
    public partial class CalibrationView : Window
    {
        private CalibrationViewModel _viewModel;
        
        public CalibrationView()
        {
            InitializeComponent();
            
            _viewModel = new CalibrationViewModel();
            DataContext = _viewModel;
            
            // Subscribe to close request
            _viewModel.RequestClose += OnRequestClose;
        }
        
        private void OnRequestClose(object sender, bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }
        
        protected override void OnClosed(System.EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            if (_viewModel != null)
            {
                _viewModel.RequestClose -= OnRequestClose;
            }
            base.OnClosed(e);
        }
    }
}