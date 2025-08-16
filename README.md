# Medical Lab Analyzer - محلل المختبر الطبي

A comprehensive medical laboratory analysis system with CASA (Computer-Assisted Sperm Analysis) capabilities, built with WPF .NET 8, SQLite, and EmguCV.

نظام شامل لتحليل المختبرات الطبية مع إمكانيات CASA (تحليل الحيوانات المنوية بمساعدة الحاسوب)، مبني بـ WPF .NET 8 و SQLite و EmguCV.

## ✨ New Features - المميزات الجديدة

### 🔧 Calibration System - نظام المعايرة
- **CalibrationView.xaml** - واجهة معايرة الكاميرا/العدسة
- **Database_Init.sql** - جداول المعايرة والسجلات
- **AuditLogger.cs** - نظام تسجيل شامل لجميع العمليات

### 🧪 Testing & Validation - الاختبار والتحقق
- **CasaAnalysisTest.cs** - اختبار حقيقي لتحليل CASA
- **SimpleTracker.cs** - خوارزمية تتبع متقدمة
- **ImageAnalysisService.cs** - خدمة تحليل الصور والفيديو

## 🚀 Quick Start - بدء سريع

### 1. Prerequisites - المتطلبات الأساسية
```bash
# .NET 8.0 SDK
# Visual Studio 2022/2025 or VS Code
# SQLite (included)
# EmguCV packages (auto-restored)
```

### 2. Build & Run - البناء والتشغيل
```bash
# Clone and navigate
git clone <repository>
cd MedicalLabAnalyzer

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj
```

### 3. Database Setup - إعداد قاعدة البيانات
```bash
# The database will be created automatically on first run
# Or manually run:
sqlite3 Database/medical_lab.db < Database/init_schema_full.sql
sqlite3 Database/medical_lab.db < Database/Database_Init.sql
```

## 🎯 Core Functionality - الوظائف الأساسية

### Patient Management - إدارة المرضى
- Add, edit, and search patients
- إضافة وتعديل وبحث المرضى
- Medical record numbers (MRN)
- أرقام السجلات الطبية

### Exam Management - إدارة الفحوصات
- CASA (Computer-Assisted Sperm Analysis)
- CBC (Complete Blood Count)
- Urine Analysis
- Stool Analysis

### CASA Analysis - تحليل الحيوانات المنوية
- **Real-time video tracking** - تتبع الفيديو في الوقت الفعلي
- **VCL, VSL, VAP, ALH, BCF calculations** - حساب المقاييس
- **Calibration system** - نظام المعايرة
- **Audit logging** - تسجيل العمليات

### Video Analysis - تحليل الفيديو
- MP4/AVI support
- Background subtraction
- Object detection and tracking
- Frame-by-frame analysis

### Reporting System - نظام التقارير
- PDF generation with PdfSharp
- Crystal Reports support (optional)
- Customizable templates
- Multi-language support

## 🏗️ Project Structure - هيكل المشروع

```
MedicalLabAnalyzer/
├── Database/
│   ├── init_schema_full.sql      # Complete database schema
│   └── Database_Init.sql         # Calibration & Audit tables
├── src/MedicalLabAnalyzer/
│   ├── Models/                   # POCO models
│   ├── Services/                 # Business logic
│   │   ├── DatabaseService.cs    # SQLite operations
│   │   ├── AuthService.cs        # Authentication
│   │   ├── AuditLogger.cs        # Comprehensive logging
│   │   ├── CalibrationService.cs # Camera calibration
│   │   ├── ImageAnalysisService.cs # Video analysis
│   │   └── ReportService.cs      # PDF generation
│   ├── Views/                    # WPF UI
│   │   ├── MainWindow.xaml       # Main application
│   │   └── CalibrationView.xaml  # Calibration interface
│   ├── Helpers/                  # Utility classes
│   │   └── SimpleTracker.cs      # Object tracking
│   └── Tests/                    # Unit tests
│       └── CasaAnalysisTest.cs   # CASA validation
├── install/                       # Installer scripts
├── build.bat                     # Windows build script
├── build_offline.ps1             # PowerShell offline build
└── nuget.config                  # Package sources
```

## 🔧 Configuration - الإعدادات

### Database Connection - اتصال قاعدة البيانات
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Database/medical_lab.db;Version=3;"
  },
  "Calibration": {
    "DefaultMicronsPerPixel": 0.5,
    "DefaultFPS": 25.0
  }
}
```

### Video Analysis Settings - إعدادات تحليل الفيديو
```json
{
  "VideoAnalysis": {
    "MinBlobArea": 10,
    "MaxMatchDistance": 40,
    "MaxMissedFrames": 6
  }
}
```

## 🧪 Testing - الاختبار

### Run CASA Test - تشغيل اختبار CASA
```csharp
// In your application
using MedicalLabAnalyzer.Tests;

// Test with video file
CasaAnalysisTest.Run("path/to/video.mp4");

// Simple test without video
CasaAnalysisTest.RunSimpleTest();
```

### Test Output - مخرجات الاختبار
```
=== CASA Analysis Test ===
اختبار تحليل CASA

Using pixelsPerMicron = 0.5 µm/px
يتم استخدام معامل المعايرة = 0.5 ميكرومتر/بكسل

Extracted 15 tracks, video FPS=25
تم استخراج 15 مسار، معدل الإطار = 25

Aggregated CASA Results:
نتائج CASA المجمعة:
Tracks: 15
VCL: 45.23 µm/s
VSL: 38.67 µm/s
VAP: 42.15 µm/s
ALH: 3.45 µm
BCF: 12.34 Hz
Motility%: 86.7 %
Progressive%: 73.3 %
```

## 📊 Calibration - المعايرة

### Access Calibration - الوصول للمعايرة
1. Open the application
2. Navigate to **Tools > Calibration**
3. Enter calibration values:
   - **Microns per Pixel**: Camera/lens calibration factor
   - **FPS**: Video frame rate
   - **User Name**: Technician performing calibration

### Calibration Values - قيم المعايرة
- **Typical values**: 0.1 - 2.0 µm/px
- **High magnification**: Lower values (0.1-0.5)
- **Low magnification**: Higher values (1.0-2.0)
- **FPS**: Usually 25, 30, or 60

## 📝 Audit Logging - تسجيل العمليات

### Automatic Logging - التسجيل التلقائي
The system automatically logs:
- User logins/logouts
- CASA analysis operations
- Report generation
- Calibration changes
- System errors

### View Audit Logs - عرض سجلات التدقيق
```sql
-- View recent audit logs
SELECT Action, Description, CreatedAt 
FROM AuditLogs 
ORDER BY CreatedAt DESC 
LIMIT 50;

-- Filter by action type
SELECT * FROM AuditLogs 
WHERE Action = 'CASA_Analysis';
```

## 🚀 Deployment - النشر

### Build for Production - البناء للإنتاج
```bash
# Windows x64
dotnet publish -c Release -r win-x64 --self-contained true

# Create installer
.\build_offline.ps1 -CreateInstaller
```

### Offline Build - البناء بدون إنترنت
```bash
# Use local NuGet packages
.\build_offline.ps1 -SkipTests
```

## 🔒 Security - الأمان

### Authentication - المصادقة
- **Default admin**: `admin` / `admin123`
- **Password hashing**: BCrypt
- **Role-based access**: Admin, LabTech, Reception

### Data Protection - حماية البيانات
- SQLite database encryption (optional)
- Audit trail for all operations
- User activity logging

## 🆘 Troubleshooting - حل المشاكل

### Common Issues - المشاكل الشائعة

#### Video Analysis Fails - فشل تحليل الفيديو
```bash
# Check video format
ffmpeg -i video.mp4 -f null -

# Verify calibration values
sqlite3 Database/medical_lab.db "SELECT * FROM Calibration ORDER BY CreatedAt DESC LIMIT 1;"
```

#### Database Connection Error - خطأ اتصال قاعدة البيانات
```bash
# Check file permissions
ls -la Database/

# Verify SQLite installation
sqlite3 --version
```

#### EmguCV Runtime Error - خطأ EmguCV
```bash
# Install Visual C++ Redistributable
# Check DLL dependencies
drmemory MedicalLabAnalyzer.exe
```

### Log Files - ملفات السجل
- **Application logs**: `Logs/app_YYYYMMDD.log`
- **Audit logs**: Database table `AuditLogs`
- **Error logs**: `Logs/error_YYYYMMDD.log`

## 📚 API Reference - مرجع API

### Key Services - الخدمات الرئيسية

#### ImageAnalysisService
```csharp
public class ImageAnalysisService
{
    // Extract tracks from video
    List<List<TrackPoint>> ExtractTracksFromVideo(string videoPath, double pixelsPerMicron, out double fps);
    
    // Analyze CASA metrics
    CASA_Result AnalyzeCASAFromTracks(List<List<TrackPoint>> tracks);
}
```

#### AuditLogger
```csharp
public static class AuditLogger
{
    // Basic logging
    static void Log(string action, string description);
    
    // CASA analysis logging
    static void LogCasaAnalysis(int examId, string videoPath, int userId);
    
    // User authentication logging
    static void LogLogin(string username, bool success);
}
```

#### CalibrationService
```csharp
public class CalibrationService
{
    // Save calibration data
    void SaveCalibration(double micronsPerPixel, double fps, string userName);
    
    // Get latest calibration
    Calibration GetLatestCalibration(string cameraName = null);
}
```

## 🤝 Contributing - المساهمة

### Development Setup - إعداد التطوير
1. Fork the repository
2. Create feature branch
3. Make changes
4. Add tests
5. Submit pull request

### Code Standards - معايير الكود
- Follow C# coding conventions
- Add XML documentation
- Include unit tests
- Use Arabic comments where appropriate

## 📄 License - الترخيص

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

هذا المشروع مرخص تحت رخصة MIT - راجع ملف [LICENSE](LICENSE) للتفاصيل.

## 🙏 Acknowledgments - الشكر والتقدير

- **EmguCV** - Computer vision library
- **SQLite** - Embedded database
- **PdfSharp** - PDF generation
- **Material Design** - UI framework
- **BCrypt.Net-Next** - Password hashing

## 📞 Support - الدعم

### Documentation - التوثيق
- [User Manual](docs/UserManual.md)
- [API Reference](docs/API.md)
- [Troubleshooting Guide](docs/Troubleshooting.md)

### Contact - التواصل
- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions
- **Email**: support@medicallabanalyzer.com

---

**⚠️ Important Notice**: This software is for educational and research purposes. For clinical use, proper validation and regulatory approval are required.

**⚠️ ملاحظة مهمة**: هذا البرنامج مخصص للأغراض التعليمية والبحثية. للاستخدام السريري، يلزم التحقق والموافقة التنظيمية المناسبة.