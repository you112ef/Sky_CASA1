using System;
using System.Windows;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.Views
{
    public partial class CalibrationView : Window
    {
        private readonly CalibrationService _cal;
        public CalibrationView()
        {
            InitializeComponent();
            var db = new DatabaseService();
            _cal = new CalibrationService(db);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtMicronsPerPixel.Text, out var mpp) || mpp <= 0)
            {
                MessageBox.Show("ادخل قيمة صحيحة لـ Microns/Pixel", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!double.TryParse(txtFPS.Text, out var fps) || fps <= 0)
            {
                MessageBox.Show("ادخل قيمة صحيحة لـ FPS", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _cal.SaveCalibration(mpp, fps, txtCameraName.Text, Environment.UserName, null);
            MessageBox.Show("تم حفظ المعايرة", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}