# MedicalLabAnalyzer - دليل التثبيت والبناء الشامل

## 🚀 الخطوة التالية: تثبيت وتشغيل المشروع

### 📋 المتطلبات الأساسية

#### 1. تثبيت .NET 8.0 SDK
```bash
# تحميل .NET 8.0 SDK من Microsoft
# https://dotnet.microsoft.com/download/dotnet/8.0

# للتحقق من التثبيت
dotnet --version
# يجب أن يظهر: 8.0.x
```

#### 2. تثبيت Visual Studio 2022 (اختياري للتطوير)
```bash
# تحميل Visual Studio 2022 Community (مجاني)
# https://visualstudio.microsoft.com/downloads/

# أو استخدام Visual Studio Code
# https://code.visualstudio.com/
```

### 🔧 خطوات البناء والتشغيل

#### الخطوة 1: استنساخ المشروع
```bash
# استنساخ المشروع من GitHub
git clone https://github.com/you112ef/Sky_CASA1.git
cd Sky_CASA1

# أو إذا كان المشروع موجود محلياً
cd MedicalLabAnalyzer
```

#### الخطوة 2: استعادة الحزم
```bash
# استعادة جميع حزم NuGet
dotnet restore

# أو استعادة مشروع محدد
dotnet restore MedicalLabAnalyzer.csproj
```

#### الخطوة 3: بناء المشروع
```bash
# بناء المشروع في وضع Debug
dotnet build

# بناء المشروع في وضع Release
dotnet build --configuration Release

# بناء مع معلومات مفصلة
dotnet build --verbosity detailed
```

#### الخطوة 4: تشغيل التطبيق
```bash
# تشغيل التطبيق مباشرة
dotnet run

# تشغيل من مجلد محدد
dotnet run --project MedicalLabAnalyzer.csproj

# تشغيل في وضع Release
dotnet run --configuration Release
```

### 🏗️ البناء المتقدم باستخدام PowerShell

#### استخدام سكربت البناء الشامل
```powershell
# فتح PowerShell كمدير
# الانتقال لمجلد المشروع
cd C:\path\to\MedicalLabAnalyzer

# بناء كامل مع ZIP
.\BuildDeploy.ps1 -CreateZip

# بناء مع مثبت
.\BuildDeploy.ps1 -CreateInstaller -CreateZip

# تخطي الاختبارات للبناء السريع
.\BuildDeploy.ps1 -SkipTests -CreateZip
```

#### بناء يدوي خطوة بخطوة
```powershell
# تنظيف البناء السابق
dotnet clean

# استعادة الحزم
dotnet restore

# بناء المشروع
dotnet build --configuration Release

# نشر التطبيق
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output publish/win-x64
```

### 🧪 تشغيل الاختبارات

#### تشغيل جميع الاختبارات
```bash
# تشغيل الاختبارات
dotnet test

# تشغيل اختبارات محددة
dotnet test --filter "Category=CASA"
dotnet test --filter "Category=CBC"
```

#### استخدام سكربت الاختبارات
```bash
# تشغيل سكربت الاختبارات
.\run_tests.bat
```

### 📦 إنشاء حزمة التوزيع

#### إنشاء ZIP للتوزيع
```powershell
# استخدام سكربت البناء
.\BuildDeploy.ps1 -CreateZip

# أو يدوياً
$publishPath = "publish/win-x64"
$distPath = "dist"
$packageName = "MedicalLabAnalyzer_v1.0.0_win-x64"

# إنشاء مجلد التوزيع
New-Item -ItemType Directory -Path "$distPath/$packageName" -Force

# نسخ الملفات المنشورة
Copy-Item -Path "$publishPath/*" -Destination "$distPath/$packageName/" -Recurse

# إنشاء ZIP
Compress-Archive -Path "$distPath/$packageName/*" -DestinationPath "$distPath/${packageName}.zip" -Force
```

### 🔍 استكشاف الأخطاء

#### مشاكل شائعة وحلولها

##### 1. خطأ "dotnet command not found"
```bash
# حل: تثبيت .NET 8.0 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# للتحقق من التثبيت
dotnet --version
```

##### 2. خطأ في استعادة الحزم
```bash
# تنظيف ذاكرة التخزين المؤقت
dotnet nuget locals all --clear

# إعادة استعادة الحزم
dotnet restore --force
```

##### 3. خطأ في البناء
```bash
# تنظيف البناء
dotnet clean

# إعادة البناء
dotnet build --verbosity detailed
```

##### 4. خطأ في تشغيل التطبيق
```bash
# التحقق من وجود الملفات
ls bin/Debug/net8.0-windows/
ls bin/Release/net8.0-windows/

# تشغيل مع معلومات مفصلة
dotnet run --verbosity detailed
```

### 🎯 اختبار التطبيق

#### 1. تسجيل الدخول
```
المستخدم: admin
كلمة المرور: admin
```

#### 2. اختبار الوظائف الأساسية
- ✅ إضافة مريض جديد
- ✅ إضافة فحص CASA
- ✅ تحليل فيديو
- ✅ إنشاء تقرير
- ✅ تصدير PDF

#### 3. اختبار التحاليل المختلفة
- ✅ CBC - تحليل الدم
- ✅ Urine - تحليل البول
- ✅ Stool - تحليل البراز
- ✅ CASA - تحليل الحيوانات المنوية

### 📊 مراقبة الأداء

#### ملفات السجل
```bash
# سجلات التطبيق
logs/app.log

# سجلات الأخطاء
logs/error.log

# سجلات قاعدة البيانات
Database/medical_lab.db
```

#### مراقبة الأداء
```bash
# تشغيل مع مراقبة الأداء
dotnet run --configuration Release

# مراقبة استخدام الذاكرة
# مراقبة استخدام CPU
# مراقبة استجابة الواجهة
```

### 🚀 النشر والإنتاج

#### إعداد البيئة الإنتاجية
```bash
# 1. تثبيت .NET 8.0 Runtime
# 2. نسخ ملفات التطبيق
# 3. إعداد قاعدة البيانات
# 4. تكوين التقارير
# 5. اختبار الوظائف
```

#### النسخ الاحتياطية
```bash
# نسخ احتياطي لقاعدة البيانات
cp Database/medical_lab.db Database/Backup/medical_lab_$(date +%Y%m%d_%H%M%S).db

# نسخ احتياطي للتقارير
cp -r Reports/Archive/* Database/Backup/Reports/
```

### 📞 الدعم والمساعدة

#### في حالة وجود مشاكل
1. **راجع ملفات السجل** في مجلد `logs/`
2. **تحقق من قاعدة البيانات** في مجلد `Database/`
3. **راجع التوثيق** في ملف `README.md`
4. **أبلغ عن المشكلة** في GitHub Issues

#### معلومات الاتصال
- **GitHub Issues:** للإبلاغ عن الأخطاء
- **Email:** support@medicallabsolutions.com
- **Documentation:** راجع ملفات التوثيق

---

## 🎉 تهانينا! المشروع جاهز للاستخدام

بعد اتباع هذه الخطوات، سيكون لديك:
- ✅ نظام إدارة مختبر طبي متقدم
- ✅ واجهة عربية RTL كاملة
- ✅ تحليلات طبية متقدمة
- ✅ نظام تقارير احترافي
- ✅ أمان وامتثال طبي

**استمتع باستخدام MedicalLabAnalyzer! 🏥✨**