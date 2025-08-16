using System;
using System.Data.SQLite;
using System.Windows;
using MedicalLabAnalyzer.Services;

namespace MedicalLabAnalyzer.Views
{
    public partial class CalibrationView : Window
    {
        private string _connectionString = "Data Source=Database/medical_lab.db;Version=3;";

        public CalibrationView()
        {
            InitializeComponent();
            LoadDefaultValues();
        }

        private void LoadDefaultValues()
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT MicronsPerPixel, FPS FROM Calibration ORDER BY CreatedAt DESC LIMIT 1";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtMicronsPerPixel.Text = reader["MicronsPerPixel"].ToString();
                                txtFPS.Text = reader["FPS"].ToString();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Database not accessible - user must provide calibration values
                // These are common medical microscopy standards but must be verified
                txtMicronsPerPixel.Text = ""; // Force user to enter correct value
                txtFPS.Text = ""; // Force user to enter correct value
            }
            
            txtUser.Text = Environment.UserName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMicronsPerPixel.Text) ||
                string.IsNullOrWhiteSpace(txtFPS.Text) ||
                string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("الرجاء إدخال جميع الحقول", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(txtMicronsPerPixel.Text, out double micronsPerPixel) || micronsPerPixel <= 0)
            {
                MessageBox.Show("الرجاء إدخال قيمة صحيحة لـ Microns/Pixel", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!double.TryParse(txtFPS.Text, out double fps) || fps <= 0)
            {
                MessageBox.Show("الرجاء إدخال قيمة صحيحة لـ FPS", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"INSERT INTO Calibration (MicronsPerPixel, FPS, UserName, CreatedAt)
                                   VALUES (@MicronsPerPixel, @FPS, @UserName, @CreatedAt)";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@MicronsPerPixel", micronsPerPixel);
                        cmd.Parameters.AddWithValue("@FPS", fps);
                        cmd.Parameters.AddWithValue("@UserName", txtUser.Text);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        cmd.ExecuteNonQuery();
                    }
                }

                AuditLogger.Log("Calibration", $"تم حفظ المعايرة من قبل {txtUser.Text} - MicronsPerPixel: {micronsPerPixel}, FPS: {fps}");

                MessageBox.Show("تم الحفظ بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}