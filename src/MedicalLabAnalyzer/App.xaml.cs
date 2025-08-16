using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Windows;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Views;

namespace MedicalLabAnalyzer
{
    public partial class App : Application
    {
        private IHost _host;
        private ILogger<App> _logger;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                    .WriteTo.Debug()
                    .CreateLogger();

                // Build host with dependency injection
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // Configure logging
                        services.AddLogging(builder =>
                        {
                            builder.AddSerilog();
                        });

                        // Register services
                        services.AddScoped<DatabaseService>();
                        services.AddScoped<AuthService>();
                        services.AddScoped<CalibrationService>();
                        services.AddScoped<ImageAnalysisService>();
                        services.AddScoped<ReportService>();

                        // Register Views
                        services.AddTransient<MainWindow>();
                        services.AddTransient<CalibrationView>();
                    })
                    .Build();

                // Initialize database
                using (var scope = _host.Services.CreateScope())
                {
                    var dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
                    
                    // Database will be created automatically by DatabaseService
                    // AuthService will create initial admin user if needed
                }

                // Get logger
                _logger = _host.Services.GetRequiredService<ILogger<App>>();
                _logger.LogInformation("Application started successfully");

                // Show main window
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start application");
                MessageBox.Show($"Failed to start application: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                _logger?.LogInformation("Application shutting down");
                
                if (_host != null)
                {
                    await _host.StopAsync();
                    _host.Dispose();
                }

                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during shutdown");
            }

            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError(e.Exception, "Unhandled exception occurred");
            
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }
    }
}