# Medical Lab Analyzer - نظام تحليل المختبر الطبي

A comprehensive medical laboratory management system with video analysis capabilities, built using WPF, .NET 8, Entity Framework Core, and EmguCV.

نظام شامل لإدارة المختبر الطبي مع إمكانيات تحليل الفيديو، مبني باستخدام WPF و .NET 8 و Entity Framework Core و EmguCV.

## 🚀 Features - المميزات

### Core Functionality - الوظائف الأساسية
- **Patient Management** - إدارة المرضى
- **Exam Management** - إدارة الفحوصات
- **Video Analysis** - تحليل الفيديو باستخدام EmguCV
- **Reporting System** - نظام التقارير (Crystal Reports + PDF fallback)
- **User Authentication & Authorization** - نظام المصادقة والتفويض
- **Backup & Restore** - النسخ الاحتياطي والاستعادة
- **Multi-language Support** - دعم متعدد اللغات (English/Arabic)

### Technical Features - المميزات التقنية
- **Modern WPF MVVM Architecture** - هيكل WPF MVVM حديث
- **Entity Framework Core** - قاعدة بيانات SQLite
- **Material Design UI** - واجهة مستخدم Material Design
- **Dependency Injection** - حقن التبعيات
- **Comprehensive Logging** - تسجيل شامل
- **Async/Await Pattern** - نمط البرمجة غير المتزامن
- **Error Handling** - معالجة الأخطاء

## 🛠️ Prerequisites - المتطلبات الأساسية

### Required Software - البرامج المطلوبة
- **Visual Studio 2022/2025** (Community/Professional/Enterprise)
- **.NET 8.0 SDK**
- **Windows 10/11** (x64)

### Optional Software - البرامج الاختيارية
- **SAP Crystal Reports** (for advanced reporting)
- **SQLite Browser** (for database management)

## 📦 Installation - التثبيت

### 1. Clone/Download - استنساخ/تحميل المشروع
```bash
git clone https://github.com/yourusername/MedicalLabAnalyzer.git
cd MedicalLabAnalyzer
```

### 2. Build & Run - البناء والتشغيل
1. Open `MedicalLabAnalyzer.sln` in Visual Studio
2. Restore NuGet packages
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

### 3. First Launch - التشغيل الأول
- Default admin credentials: **admin / Admin@123**
- Database will be automatically created at `Database/medical_lab.db`
- Sample data will be seeded automatically

## 🏗️ Project Structure - هيكل المشروع

```
MedicalLabAnalyzer/
├── src/
│   └── MedicalLabAnalyzer/
│       ├── Models/           # Entity models
│       ├── ViewModels/       # MVVM ViewModels
│       ├── Views/            # WPF Views
│       ├── Services/         # Business logic services
│       ├── Data/             # Entity Framework context
│       ├── Resources/        # Images, icons, styles
│       ├── Reports/          # Crystal Reports (.rpt files)
│       └── Utils/            # Utility classes
├── tests/                    # Unit tests
├── install/                  # Inno Setup installer
├── Database/                 # SQLite database
└── logs/                     # Application logs
```

## 🔧 Configuration - الإعدادات

### App Settings - إعدادات التطبيق
Edit `appsettings.json` to configure:
- Database connection string
- Logging levels
- Video analysis parameters
- Report settings

### Crystal Reports - تقارير Crystal
1. Install SAP Crystal Reports
2. Add `CRYSTAL` to Build symbols
3. Place `.rpt` files in `Reports/` folder
4. Rebuild solution

## 📊 Usage Guide - دليل الاستخدام

### Patients Management - إدارة المرضى
1. Navigate to **Patients** section
2. Click **Add New Patient** to create patient records
3. Fill in patient information (Name, National ID, Date of Birth, etc.)
4. Save patient data

### Exams Management - إدارة الفحوصات
1. Go to **Exams** section
2. Select a patient for the exam
3. Enter exam details (Type, Name, Description)
4. Set exam status and assign technician/doctor
5. Add video files if applicable

### Video Analysis - تحليل الفيديو
1. Navigate to **Video Analysis**
2. Select an exam with video
3. Choose video file (MP4, AVI supported)
4. Click **Analyze Video** to start processing
5. View analysis results (motion detection, object count, brightness)

### Reports Generation - إنشاء التقارير
1. Go to **Reports** section
2. Select report type and parameters
3. Choose export format (PDF/Crystal)
4. Generate and download report

## 🧪 Testing - الاختبار

### Unit Tests - اختبارات الوحدات
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/MedicalLabAnalyzer.Tests/
```

### Integration Tests - اختبارات التكامل
- Database operations
- Service interactions
- Video analysis workflows

## 📦 Deployment - النشر

### Publishing - النشر
```bash
# Publish for Windows x64
dotnet publish src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj \
    -c Release \
    -r win-x64 \
    -p:PublishSingleFile=true \
    --self-contained true
```

### Installer Creation - إنشاء المثبت
1. Install Inno Setup
2. Open `install/MedicalLabAnalyzer.iss`
3. Build installer
4. Distribute generated `.exe` file

## 🔒 Security - الأمان

### Authentication - المصادقة
- SHA256 password hashing
- Session management
- Role-based access control

### Data Protection - حماية البيانات
- Input validation
- SQL injection prevention
- Secure file handling

## 📝 Logging - التسجيل

### Log Levels - مستويات التسجيل
- **Information**: General application events
- **Warning**: Potential issues
- **Error**: Errors and exceptions
- **Debug**: Detailed debugging information

### Log Files - ملفات التسجيل
- Location: `logs/app.log`
- Rolling daily logs
- Structured logging with Serilog

## 🐛 Troubleshooting - حل المشاكل

### Common Issues - المشاكل الشائعة

#### Build Errors - أخطاء البناء
```bash
# Clean solution
dotnet clean
# Restore packages
dotnet restore
# Rebuild
dotnet build
```

#### Database Issues - مشاكل قاعدة البيانات
- Delete `Database/medical_lab.db`
- Restart application
- Database will be recreated automatically

#### Video Analysis Errors - أخطاء تحليل الفيديو
- Ensure video file is valid MP4/AVI
- Check EmguCV runtime files
- Verify video codec compatibility

#### Crystal Reports Issues - مشاكل Crystal Reports
- Verify Crystal Reports installation
- Check `CRYSTAL` build symbol
- Ensure `.rpt` files are in correct location

## 🤝 Contributing - المساهمة

### Development Guidelines - إرشادات التطوير
1. Follow MVVM pattern
2. Use async/await for I/O operations
3. Implement proper error handling
4. Add unit tests for new features
5. Follow C# coding conventions

### Code Style - أسلوب الكود
- Use meaningful variable names
- Add XML documentation
- Implement proper logging
- Handle exceptions gracefully

## 📄 License - الترخيص

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

هذا المشروع مرخص تحت رخصة MIT - راجع ملف [LICENSE](LICENSE) للتفاصيل.

## 🙏 Acknowledgments - الشكر والتقدير

- **EmguCV** for computer vision capabilities
- **Material Design** for modern UI components
- **Entity Framework Core** for data access
- **Serilog** for structured logging
- **Community contributors** for feedback and suggestions

## 📞 Support - الدعم

### Getting Help - الحصول على المساعدة
- **Issues**: Create GitHub issue
- **Documentation**: Check project wiki
- **Community**: Join discussions

### Contact - التواصل
- **Email**: support@medicallab.com
- **GitHub**: [Project Repository](https://github.com/yourusername/MedicalLabAnalyzer)

---

**Note**: This application is designed for medical laboratory use. Ensure compliance with local healthcare regulations and data protection laws.

**ملاحظة**: تم تصميم هذا التطبيق للاستخدام في المختبرات الطبية. تأكد من الامتثال للوائح الرعاية الصحية المحلية وقوانين حماية البيانات.