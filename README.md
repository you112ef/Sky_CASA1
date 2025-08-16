# MedicalLabAnalyzer - نظام إدارة المختبرات الطبية المتقدم
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
=======
# MedicalLabAnalyzer - نظام إدارة المختبرات الطبية المتقدم

## 🏥 نظرة عامة

**MedicalLabAnalyzer** هو نظام إدارة مختبر طبي متقدم ومتكامل يعمل بدون إنترنت، مصمم خصيصاً للمختبرات الطبية والمستشفيات. النظام يدعم اللغة العربية بالكامل مع واجهة RTL، ويوفر تحليلات طبية متقدمة مع نظام صلاحيات متدرج.

## ✨ المميزات الرئيسية

### 🔬 التحليلات الطبية المتقدمة
- **CASA (تحليل الحيوانات المنوية)**: تحليل فيديو متقدم مع خوارزميات Kalman Filter + Hungarian Algorithm
- **CBC (تحليل الدم الشامل)**: تحليل شامل مع قيم مرجعية وتحديد الأنماط المرضية
- **تحليل البول**: فحص شامل للبول مع تحليل مجهري وكيميائي
- **تحليل البراز**: فحص البراز مع تحليل الطفيليات والالتهابات

### 🎯 نظام الصلاحيات المتدرج
- **مدير النظام**: صلاحيات كاملة على جميع الوظائف
- **فني المختبر**: تحليل العينات، إدخال النتائج، طباعة التقارير
- **مستقبل**: إدارة المرضى وحجز المواعيد فقط

### 📊 نظام التقارير المتقدم
- **تقارير PDF**: إنشاء تقارير احترافية باللغة العربية
- **تصدير Excel**: تصدير البيانات إلى ملفات Excel
- **أرشفة تلقائية**: حفظ نسخ من جميع التقارير
- **إحصائيات شاملة**: تحليلات إحصائية للبيانات

### 🔒 الأمان والتدقيق
- **نظام AuditLog**: تسجيل كامل لجميع العمليات
- **تشفير كلمات المرور**: استخدام BCrypt للتشفير
- **إدارة الجلسات**: نظام جلسات آمن
- **نسخ احتياطية**: نظام نسخ احتياطي تلقائي

## 🛠️ المتطلبات التقنية

### متطلبات النظام
- **Windows 10/11** أو أحدث
- **.NET 8.0 Desktop Runtime**
- **4 GB RAM** كحد أدنى
- **2 GB مساحة خالية** على القرص الصلب

### المكتبات المطلوبة
```
Emgu.CV 4.8.1.5350 (معالجة الفيديو والصور)
System.Data.SQLite.Core 1.0.118 (قاعدة البيانات)
Dapper 2.1.15 (ORM)
BCrypt.Net-Next 4.0.3 (تشفير كلمات المرور)
PdfSharp-MigraDoc 1.50.5147 (إنشاء PDF)
EPPlus 7.0.5 (تصدير Excel)
Microsoft.Extensions.Logging 8.0.0 (التسجيل)
Serilog 3.1.1 (تسجيل متقدم)
FluentValidation 11.8.1 (التحقق من البيانات)
AutoMapper 12.0.1 (تحويل البيانات)
```

## 🚀 التثبيت والتشغيل

### 1. تحميل وتثبيت .NET 8.0
```bash
# تحميل .NET 8.0 Desktop Runtime من Microsoft
# https://dotnet.microsoft.com/download/dotnet/8.0
```

### 2. بناء المشروع
```powershell
# فتح PowerShell في مجلد المشروع
cd C:\MedicalLabAnalyzer

# بناء المشروع
dotnet build --configuration Release

# أو استخدام سكربت البناء
.\build_offline.ps1
```

### 3. تشغيل التطبيق
```bash
# تشغيل التطبيق
dotnet run --configuration Release

# أو تشغيل الملف التنفيذي مباشرة
.\bin\Release\net8.0-windows\MedicalLabAnalyzer.exe
```

## 👥 المستخدمين الافتراضيين

| المستخدم | كلمة المرور | الدور | الصلاحيات |
|-----------|-------------|-------|------------|
| `admin` | `admin` | مدير النظام | جميع الصلاحيات |
| `lab` | `123` | فني المختبر | تحليل العينات والتقارير |
| `reception` | `123` | مستقبل | إدارة المرضى والمواعيد |

## 📋 دليل الاستخدام

### 🔬 تحليل CASA (الحيوانات المنوية)

1. **تسجيل الدخول** كفني مختبر أو مدير
2. **فتح شاشة المعايرة** من القائمة الجانبية
3. **تحميل فيديو العينة** (صيغ مدعومة: MP4, AVI)
4. **إدخال معامل المعايرة** (ميكرون/بكسل)
5. **بدء التحليل** - النظام سيقوم بـ:
   - استخراج المسارات باستخدام Kalman Filter
   - تطبيق Hungarian Algorithm للتتبع
   - حساب VCL, VSL, VAP, ALH, BCF
   - إنشاء تقرير مفصل

### 🩸 تحليل CBC (الدم الشامل)

1. **إضافة فحص جديد** من إدارة الفحوصات
2. **اختيار نوع الفحص**: CBC
3. **إدخال القيم**:
   - WBC, RBC, Hemoglobin, Hematocrit, Platelets
   - MCV, MCH, MCHC, RDW
   - Neutrophils, Lymphocytes, Monocytes, Eosinophils, Basophils
4. **حفظ النتائج** - النظام سيقوم بـ:
   - التحقق من القيم المرجعية
   - تحديد الحالة (Normal/Abnormal/Critical)
   - تحليل الأنماط المرضية
   - إنشاء تقرير شامل

### 🧪 تحليل البول

1. **إضافة فحص بول جديد**
2. **إدخال الخصائص الفيزيائية**:
   - اللون، الشفافية، pH، الكثافة النوعية
3. **إدخال الفحوصات الكيميائية**:
   - البروتين، الجلوكوز، الكيتونات، الدم، الكريات البيضاء
4. **إدخال الفحص المجهري**:
   - RBC, WBC, الخلايا الظهارية، القوالب، البلورات
5. **حفظ النتائج** - النظام سيقوم بـ:
   - تحديد وجود UTI
   - تحليل البيلة الدموية
   - تحديد البيلة البروتينية
   - إنشاء تقرير مفصل

### 💩 تحليل البراز

1. **إضافة فحص براز جديد**
2. **إدخال الخصائص الفيزيائية**:
   - اللون، التماسك، الشكل، الوزن، الرائحة
3. **إدخال الفحوصات الكيميائية**:
   - الدم الخفي، pH، المواد المختزلة، محتوى الدهون
4. **إدخال الفحص المجهري**:
   - المخاط، الطعام غير المهضوم، الألياف العضلية
   - النشا، كريات الدهون، الطفيليات، البويضات
5. **إدخال الفحوصات الإضافية**:
   - Calprotectin, Lactoferrin, Alpha1-Antitrypsin
6. **حفظ النتائج** - النظام سيقوم بـ:
   - تحديد النزيف الهضمي
   - تحليل العدوى الطفيلية
   - تحديد التهاب الأمعاء
   - تحليل سوء الامتصاص

## 📊 التقارير والإحصائيات

### أنواع التقارير
- **تقرير CASA**: VCL, VSL, VAP, ALH, BCF مع الرسوم البيانية
- **تقرير CBC**: جميع مؤشرات الدم مع القيم المرجعية
- **تقرير البول**: الفحوصات الكيميائية والمجهري
- **تقرير البراز**: الفحوصات الشاملة مع تحليل الطفيليات
- **التقرير الإحصائي**: إحصائيات شاملة للفترة المحددة

### تصدير البيانات
- **PDF**: تقارير احترافية باللغة العربية
- **Excel**: بيانات منظمة قابلة للتحليل
- **CSV**: بيانات خام للتحليل الإحصائي

## 🔧 الإعدادات والتكوين

### إعداد قاعدة البيانات
```sql
-- إنشاء قاعدة البيانات تلقائياً
-- الملف: Database/medical_lab.db
-- النسخ الاحتياطي: Database/Backup/
```

### إعداد التسجيل
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs/app-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

### إعداد التقارير
```
Reports/
├── Templates/          # قوالب التقارير
├── Archive/           # أرشيف التقارير
└── Output/            # التقارير المولدة
```

## 🧪 الاختبارات والتحقق

### تشغيل الاختبارات
```bash
# تشغيل جميع الاختبارات
dotnet test

# تشغيل اختبار CASA الحقيقي
dotnet test --filter "CasaAnalysisRealTest"

# تشغيل اختبار المعايرة
dotnet test --filter "CalibrationTest"
```

### اختبار تحليل الفيديو
1. **وضع فيديو عينة** في مجلد `Samples/`
2. **تسمية الملف**: `sperm_sample.mp4`
3. **تشغيل الاختبار**:
   ```csharp
   CasaAnalysisRealTest.RunReal();
   ```

## 🔒 الأمان والامتثال

### معايير الأمان
- **تشفير كلمات المرور**: BCrypt مع Salt
- **إدارة الجلسات**: رموز جلسات آمنة
- **تسجيل العمليات**: AuditLog شامل
- **نسخ احتياطية**: تلقائية ومشفرة

### الامتثال الطبي
- **IEC 62304**: دورة حياة برمجيات الأجهزة الطبية
- **ISO 13485**: أنظمة إدارة الجودة للأجهزة الطبية
- **ISO 14971**: إدارة المخاطر
- **HIPAA**: حماية البيانات الصحية (إذا كان مطلوباً)

## 🐛 استكشاف الأخطاء

### مشاكل شائعة وحلولها

#### التطبيق لا يعمل
```bash
# التحقق من تثبيت .NET 8.0
dotnet --version

# إعادة بناء المشروع
dotnet clean
dotnet restore
dotnet build
```

#### قاعدة البيانات لا تُحفظ
```bash
# التحقق من صلاحيات المجلد
# التأكد من وجود مجلد Database/
# التحقق من صلاحيات الكتابة
```

#### الفيديو لا يُحلل
```bash
# التحقق من صيغة الفيديو (MP4, AVI)
# التأكد من وجود EmguCV DLLs
# التحقق من معامل المعايرة
```

#### مشاكل Crystal Reports
```bash
# تثبيت Crystal Reports Runtime
# التحقق من إعدادات الطباعة
# التأكد من وجود قوالب التقارير
```

## 📞 الدعم والمساعدة

### التوثيق
- **دليل المستخدم**: `docs/UserGuide.md`
- **دليل المطور**: `docs/DeveloperGuide.md`
- **API Documentation**: `docs/API.md`

### الاتصال
- **البريد الإلكتروني**: support@medicallab.com
- **الهاتف**: +966-XX-XXXXXXX
- **الدعم الفني**: متاح 24/7

## 📄 الترخيص

هذا المشروع مرخص تحت **MIT License**. راجع ملف `LICENSE` للتفاصيل.

## ⚠️ تحذيرات مهمة

### تحذير طبي
```
هذا النظام مصمم للمساعدة في التحليل المختبري ولا يغني عن 
التشخيص الطبي المهني. جميع النتائج يجب مراجعتها من قبل 
طبيب مختص قبل اتخاذ أي قرارات علاجية.
```

### تحذير تقني
```
هذا النظام يتطلب تحققاً سريرياً شاملاً قبل الاستخدام 
في البيئة الإنتاجية. يرجى إجراء الاختبارات المطلوبة 
والتأكد من الامتثال للمعايير الطبية المحلية.
```

## 🔄 التحديثات والإصدارات

### الإصدار الحالي: v1.0.0
- ✅ نظام CASA متقدم مع Kalman + Hungarian
- ✅ تحليلات CBC, Urine, Stool شاملة
- ✅ نظام صلاحيات متدرج
- ✅ تقارير PDF/Excel متقدمة
- ✅ نظام AuditLog شامل
- ✅ واجهة عربية RTL كاملة

### التحديثات القادمة: v1.1.0
- 🔄 دعم تحليلات إضافية
- 🔄 واجهة ويب متقدمة
- 🔄 تكامل مع أنظمة المستشفيات
- 🔄 تحليلات ذكية باستخدام AI
- 🔄 تطبيق موبايل

---

**MedicalLabAnalyzer** - نظام إدارة مختبر طبي متقدم ومتكامل 🏥
>>>>>>> release/v1.0.0
