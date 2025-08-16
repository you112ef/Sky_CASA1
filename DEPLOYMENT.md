# دليل النشر والتوزيع - MedicalLabAnalyzer

## 🚀 نظرة عامة

هذا الدليل يوضح كيفية نشر وتوزيع تطبيق **MedicalLabAnalyzer** في بيئات مختلفة.

## 📦 أنواع النشر

### 1. نشر محلي (Local Deployment)
- للتطوير والاختبار
- على جهاز واحد
- قاعدة بيانات محلية

### 2. نشر مؤسسي (Enterprise Deployment)
- للمستشفيات والمختبرات
- على شبكة محلية
- قاعدة بيانات مشتركة

### 3. نشر سحابي (Cloud Deployment)
- للاستخدام عبر الإنترنت
- على خوادم سحابية
- قاعدة بيانات سحابية

## 🛠️ متطلبات النشر

### متطلبات النظام
- **Windows 10/11** (64-bit)
- **.NET 8.0 Runtime**
- **4 GB RAM** كحد أدنى
- **2 GB مساحة خالية**
- **SQLite** (مدمج)

### متطلبات الشبكة (للنشر المؤسسي)
- **شبكة محلية** (LAN)
- **خادم ملفات** مشترك
- **إعدادات أمان** مناسبة

### متطلبات السحابة (للنشر السحابي)
- **خادم سحابي** (Azure, AWS, GCP)
- **قاعدة بيانات سحابية**
- **شهادة SSL**
- **نطاق** (Domain)

## 📋 خطوات النشر المحلي

### 1. تحضير البيئة
```bash
# تثبيت .NET 8.0 Runtime
# https://dotnet.microsoft.com/download/dotnet/8.0

# فحص التثبيت
dotnet --version
```

### 2. بناء التطبيق
```bash
# استنساخ المشروع
git clone https://github.com/you112ef/Sky_CASA1.git
cd Sky_CASA1

# استعادة الحزم
dotnet restore

# بناء التطبيق
dotnet build --configuration Release
```

### 3. نشر التطبيق
```bash
# إنشاء حزمة النشر
dotnet publish --configuration Release --output ./dist --self-contained false

# أو استخدام سكريبت البناء
.\build_offline.ps1
```

### 4. تشغيل التطبيق
```bash
# تشغيل التطبيق
.\dist\MedicalLabAnalyzer.exe

# أو من الكود المصدري
dotnet run --project src/MedicalLabAnalyzer --configuration Release
```

## 🏢 نشر مؤسسي

### 1. إعداد الخادم
```bash
# إنشاء مجلد مشترك
mkdir C:\MedicalLabAnalyzer
mkdir C:\MedicalLabAnalyzer\Database
mkdir C:\MedicalLabAnalyzer\Reports
mkdir C:\MedicalLabAnalyzer\Logs

# تعيين الصلاحيات
icacls C:\MedicalLabAnalyzer /grant "Users:(OI)(CI)F"
```

### 2. نشر التطبيق
```bash
# نسخ الملفات إلى الخادم
xcopy .\dist\* C:\MedicalLabAnalyzer\ /E /I /Y

# إنشاء اختصار على سطح المكتب
mklink "%USERPROFILE%\Desktop\MedicalLabAnalyzer.lnk" "C:\MedicalLabAnalyzer\MedicalLabAnalyzer.exe"
```

### 3. إعداد قاعدة البيانات
```sql
-- إنشاء قاعدة البيانات المشتركة
-- سيتم إنشاؤها تلقائياً عند أول تشغيل
```

### 4. إعداد المستخدمين
```sql
-- إنشاء المستخدمين الافتراضيين
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, IsActive)
VALUES 
('admin', '$2a$11$...', 'Admin', 'مدير النظام', 'admin@hospital.com', 1),
('labtech', '$2a$11$...', 'LabTechnician', 'فني المختبر', 'lab@hospital.com', 1),
('reception', '$2a$11$...', 'Receptionist', 'مستقبل', 'reception@hospital.com', 1);
```

## ☁️ نشر سحابي

### 1. إعداد الخادم السحابي

#### Azure
```bash
# إنشاء App Service
az appservice plan create --name MedicalLabPlan --resource-group MedicalLabRG --sku B1

# إنشاء Web App
az webapp create --name medicallabanalyzer --resource-group MedicalLabRG --plan MedicalLabPlan

# نشر التطبيق
az webapp deployment source config-zip --resource-group MedicalLabRG --name medicallabanalyzer --src ./dist.zip
```

#### AWS
```bash
# إنشاء EC2 Instance
aws ec2 run-instances --image-id ami-12345678 --count 1 --instance-type t2.medium

# رفع الملفات
aws s3 cp ./dist s3://medicallab-bucket/dist --recursive

# نشر على EC2
aws ssm send-command --instance-ids i-12345678 --document-name "AWS-RunShellScript" --parameters commands=["cd /opt/medicallab && ./MedicalLabAnalyzer"]
```

### 2. إعداد قاعدة البيانات السحابية

#### Azure SQL Database
```sql
-- إنشاء قاعدة البيانات
CREATE DATABASE MedicalLabDB;

-- إنشاء الجداول
-- سيتم إنشاؤها تلقائياً من التطبيق
```

#### AWS RDS
```bash
# إنشاء RDS Instance
aws rds create-db-instance --db-instance-identifier medicallab-db --db-instance-class db.t3.micro --engine sqlserver-se --allocated-storage 20
```

### 3. إعداد النطاق والشهادة
```bash
# شراء نطاق
# إعداد DNS
# شراء شهادة SSL
# تكوين HTTPS
```

## 🔧 إعدادات النشر

### ملف التكوين
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./Database/MedicalLab.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Security": {
    "JwtSecret": "your-secret-key",
    "SessionTimeout": 30
  },
  "Database": {
    "AutoMigrate": true,
    "BackupInterval": 24
  }
}
```

### متغيرات البيئة
```bash
# Windows
set MEDICALLAB_ENVIRONMENT=Production
set MEDICALLAB_DATABASE_PATH=C:\MedicalLabAnalyzer\Database
set MEDICALLAB_REPORTS_PATH=C:\MedicalLabAnalyzer\Reports

# Linux
export MEDICALLAB_ENVIRONMENT=Production
export MEDICALLAB_DATABASE_PATH=/opt/medicallab/database
export MEDICALLAB_REPORTS_PATH=/opt/medicallab/reports
```

## 🔒 إعدادات الأمان

### جدار الحماية
```bash
# Windows Firewall
netsh advfirewall firewall add rule name="MedicalLabAnalyzer" dir=in action=allow protocol=TCP localport=5000

# Linux iptables
iptables -A INPUT -p tcp --dport 5000 -j ACCEPT
```

### تشفير البيانات
```csharp
// تشفير قاعدة البيانات
// تشفير الملفات الحساسة
// تشفير الاتصالات
```

### النسخ الاحتياطي
```bash
# نسخ احتياطي تلقائي
# نسخ احتياطي يدوي
# استعادة البيانات
```

## 📊 مراقبة الأداء

### أدوات المراقبة
- **Application Insights** (Azure)
- **CloudWatch** (AWS)
- **Prometheus** (مفتوح المصدر)
- **Grafana** (عرض البيانات)

### مؤشرات الأداء
- **وقت الاستجابة**
- **معدل الأخطاء**
- **استخدام الموارد**
- **عدد المستخدمين**

## 🚨 استكشاف الأخطاء

### مشاكل شائعة
1. **خطأ في الاتصال بقاعدة البيانات**
   - فحص مسار قاعدة البيانات
   - فحص الصلاحيات
   - فحص الاتصال بالشبكة

2. **خطأ في الوصول للملفات**
   - فحص مسارات الملفات
   - فحص صلاحيات المستخدم
   - فحص إعدادات الأمان

3. **خطأ في الأداء**
   - فحص استخدام الذاكرة
   - فحص استخدام المعالج
   - فحص قاعدة البيانات

### أدوات التشخيص
```bash
# فحص النظام
systeminfo
wmic computersystem get TotalPhysicalMemory
wmic cpu get NumberOfCores

# فحص التطبيق
dotnet-counters monitor
dotnet-trace collect
dotnet-dump collect
```

## 📈 التوسع

### التوسع الأفقي
- **Load Balancer**
- **Multiple Instances**
- **Database Clustering**

### التوسع العمودي
- **زيادة الموارد**
- **تحسين الأداء**
- **تحسين قاعدة البيانات**

## 🔄 التحديثات

### تحديث التطبيق
```bash
# إيقاف التطبيق
taskkill /F /IM MedicalLabAnalyzer.exe

# نسخ الملفات الجديدة
xcopy .\dist\* C:\MedicalLabAnalyzer\ /E /I /Y

# إعادة تشغيل التطبيق
start C:\MedicalLabAnalyzer\MedicalLabAnalyzer.exe
```

### تحديث قاعدة البيانات
```sql
-- نسخ احتياطي
-- تطبيق التحديثات
-- التحقق من البيانات
```

## 📞 الدعم

### للدعم الفني
- **البريد الإلكتروني**: deployment@medicallabanalyzer.com
- **GitHub Issues**: [مشاكل النشر](https://github.com/you112ef/Sky_CASA1/issues)
- **التوثيق**: [دليل الدعم](SUPPORT.md)

### للمساعدة
- **المجتمع**: [GitHub Discussions](https://github.com/you112ef/Sky_CASA1/discussions)
- **التدريب**: [فيديوهات تعليمية](https://youtube.com/medicallabanalyzer)
- **المنتدى**: [منتدى المستخدمين](https://forum.medicallabanalyzer.com)

---

**آخر تحديث**: ديسمبر 2024  
**الإصدار**: 1.0.0  
**فريق النشر**: MedicalLabAnalyzer Deployment Team