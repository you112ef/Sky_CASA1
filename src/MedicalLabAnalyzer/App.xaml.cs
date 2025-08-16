using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Windows;
using MedicalLabAnalyzer.Services;
using MedicalLabAnalyzer.Data;
using MedicalLabAnalyzer.ViewModels;
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

                        // Configure Entity Framework
                        services.AddDbContext<MedicalLabContext>(options =>
                        {
                            options.UseSqlite("Data Source=Database/medical_lab.db");
                        });

                        // Register services
                        services.AddScoped<DatabaseService>();
                        services.AddScoped<AuthService>();
                        services.AddScoped<CalibrationService>();
                        services.AddScoped<ImageAnalysisService>();
                        services.AddScoped<ReportService>();

                        // Register ViewModels
                        services.AddTransient<MainViewModel>();
                        services.AddTransient<PatientsViewModel>();
                        services.AddTransient<ExamsViewModel>();
                        services.AddTransient<VideoAnalysisViewModel>();
                        services.AddTransient<ReportsViewModel>();

                        // Register Views
                        services.AddTransient<MainWindow>();
                        services.AddTransient<PatientsView>();
                        services.AddTransient<ExamsView>();
                        services.AddTransient<VideoAnalysisView>();
                        services.AddTransient<ReportsView>();
                    })
                    .Build();

                // Initialize database
                using (var scope = _host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<MedicalLabContext>();
                    await context.Database.EnsureCreatedAsync();
                    
                    // Seed initial data
                    var seeder = new DatabaseSeeder(context);
                    await seeder.SeedAsync();
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