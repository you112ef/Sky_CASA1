# دليل الدعم - MedicalLabAnalyzer

## 🆘 الحصول على المساعدة

مرحباً بك في دليل الدعم لمشروع **MedicalLabAnalyzer**! نحن هنا لمساعدتك في حل أي مشاكل أو استفسارات.

## 📞 قنوات الدعم

### 🐛 الإبلاغ عن الأخطاء
إذا واجهت خطأ في التطبيق:

1. **GitHub Issues**: [إنشاء Issue جديد](https://github.com/you112ef/Sky_CASA1/issues/new)
2. **البريد الإلكتروني**: bugs@medicallabanalyzer.com

### 💡 طلب ميزات جديدة
إذا كنت ترغب في اقتراح ميزة جديدة:

1. **GitHub Discussions**: [مناقشة الميزة](https://github.com/you112ef/Sky_CASA1/discussions)
2. **Feature Request Issue**: [إنشاء طلب ميزة](https://github.com/you112ef/Sky_CASA1/issues/new?template=feature_request.md)

### ❓ الأسئلة العامة
للاستفسارات العامة:

1. **GitHub Discussions**: [الأسئلة العامة](https://github.com/you112ef/Sky_CASA1/discussions/categories/q-a)
2. **البريد الإلكتروني**: support@medicallabanalyzer.com

## 🔧 المشاكل الشائعة وحلولها

### مشاكل التثبيت

#### خطأ: "dotnet command not found"
**الحل**:
```bash
# تثبيت .NET 8.0 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### خطأ: "Package restore failed"
**الحل**:
```bash
# تنظيف الحزم
dotnet clean
dotnet restore --force
```

#### خطأ: "Build failed"
**الحل**:
```bash
# فحص المتطلبات
dotnet --version
dotnet list package

# إعادة البناء
dotnet clean
dotnet restore
dotnet build --verbosity detailed
```

### مشاكل التشغيل

#### التطبيق لا يفتح
**الحلول**:
1. فحص تثبيت .NET 8.0 Runtime
2. تشغيل من Command Prompt كمدير
3. فحص ملفات السجل في `logs/`

#### خطأ في قاعدة البيانات
**الحلول**:
1. فحص وجود مجلد `Database/`
2. حذف ملف `*.db-journal` إذا وجد
3. إعادة تشغيل التطبيق

#### مشاكل في تحليل الفيديو
**الحلول**:
1. فحص تثبيت EmguCV
2. التأكد من صحة ملف الفيديو
3. فحص مساحة القرص المتاحة

### مشاكل تسجيل الدخول

#### نسيان كلمة المرور
**الحل**:
```sql
-- إعادة تعيين كلمة مرور المدير
UPDATE Users SET PasswordHash = '$2a$11$...' WHERE Username = 'admin';
```

#### خطأ في الصلاحيات
**الحلول**:
1. التأكد من اختيار نوع المستخدم الصحيح
2. فحص قاعدة البيانات
3. إعادة إنشاء المستخدم

## 📋 قوالب الإبلاغ

### قالب الإبلاغ عن خطأ
```markdown
## وصف الخطأ
وصف مفصل للخطأ...

## خطوات إعادة الإنتاج
1. الخطوة الأولى
2. الخطوة الثانية
3. الخطوة الثالثة

## السلوك المتوقع
ما كان يجب أن يحدث...

## السلوك الفعلي
ما حدث بالفعل...

## معلومات النظام
- نظام التشغيل: Windows 10/11
- إصدار .NET: 8.0.x
- إصدار التطبيق: 1.0.0
- الذاكرة المتاحة: X GB

## ملفات السجل
```
[إرفاق ملفات السجل من مجلد logs/]
```

## لقطات شاشة
[إرفاق لقطات شاشة إن أمكن]
```

### قالب طلب ميزة
```markdown
## وصف الميزة
وصف مفصل للميزة المطلوبة...

## المشكلة التي تحلها
كيف ستساعد هذه الميزة...

## الحلول البديلة
هل فكرت في حلول أخرى...

## أمثلة
أمثلة على الميزة في تطبيقات أخرى...

## أولوية الميزة
- [ ] عالية (ضرورية للعمل)
- [ ] متوسطة (مفيدة)
- [ ] منخفضة (تحسين)
```

## 🛠️ أدوات التشخيص

### فحص النظام
```bash
# فحص إصدار .NET
dotnet --version

# فحص الحزم المثبتة
dotnet list package

# فحص مساحة القرص
dir C:\

# فحص الذاكرة
wmic computersystem get TotalPhysicalMemory
```

### فحص التطبيق
```bash
# تشغيل مع تفاصيل أكثر
dotnet run --verbosity detailed

# تشغيل الاختبارات
dotnet test --logger "console;verbosity=detailed"

# فحص ملفات السجل
type logs\app.log
```

### فحص قاعدة البيانات
```bash
# فحص وجود قاعدة البيانات
dir Database\

# فحص حجم قاعدة البيانات
dir Database\*.db
```

## 📚 الموارد المفيدة

### التوثيق
- [README.md](README.md) - دليل شامل
- [QuickStart.md](QuickStart.md) - دليل البدء السريع
- [BuildInstructions.md](BuildInstructions.md) - تعليمات البناء
- [CONTRIBUTING.md](CONTRIBUTING.md) - دليل المساهمة

### الفيديوهات التعليمية
- [دليل التثبيت](https://youtube.com/watch?v=...)
- [دليل الاستخدام](https://youtube.com/watch?v=...)
- [حل المشاكل الشائعة](https://youtube.com/watch?v=...)

### المنتديات والمجتمعات
- [GitHub Discussions](https://github.com/you112ef/Sky_CASA1/discussions)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/medicallabanalyzer)
- [Reddit Community](https://reddit.com/r/medicallabanalyzer)

## ⏰ أوقات الاستجابة

### الأولوية العالية (أخطاء حرجة)
- **الرد الأولي**: خلال 4-8 ساعات
- **الحل**: خلال 24-48 ساعة

### الأولوية المتوسطة (أخطاء عادية)
- **الرد الأولي**: خلال 24 ساعة
- **الحل**: خلال 3-5 أيام

### الأولوية المنخفضة (طلبات ميزات)
- **الرد الأولي**: خلال 48 ساعة
- **التقييم**: خلال أسبوع
- **التنفيذ**: حسب الأولوية

## 🎯 نطاق الدعم

### ما نغطيه
- مشاكل التثبيت والتشغيل
- أخطاء في التطبيق
- استفسارات حول الاستخدام
- طلبات ميزات جديدة
- مشاكل الأداء

### ما لا نغطيه
- مشاكل في الأجهزة
- مشاكل في نظام التشغيل
- طلبات تخصيص خاصة
- دعم الإصدارات القديمة
- مشاكل في المكتبات الخارجية

## 🤝 المساهمة في الدعم

### مساعدة الآخرين
- الإجابة على الأسئلة في GitHub Discussions
- مشاركة الحلول للمشاكل الشائعة
- تحسين التوثيق
- إنشاء فيديوهات تعليمية

### تحسين الدعم
- اقتراح تحسينات للتوثيق
- الإبلاغ عن أخطاء في الدليل
- اقتراح أدوات تشخيص جديدة
- مشاركة أفضل الممارسات

## 📞 معلومات التواصل

### البريد الإلكتروني
- **الدعم العام**: support@medicallabanalyzer.com
- **الأخطاء**: bugs@medicallabanalyzer.com
- **الميزات**: features@medicallabanalyzer.com
- **الأمان**: security@medicallabanalyzer.com

### GitHub
- **Repository**: https://github.com/you112ef/Sky_CASA1
- **Issues**: https://github.com/you112ef/Sky_CASA1/issues
- **Discussions**: https://github.com/you112ef/Sky_CASA1/discussions

### وسائل التواصل الاجتماعي
- **Twitter**: @MedicalLabAnalyzer
- **LinkedIn**: MedicalLabAnalyzer
- **YouTube**: MedicalLabAnalyzer Channel

## 🙏 شكر وتقدير

نشكر جميع المستخدمين والمساهمين الذين يساعدون في تحسين المشروع من خلال:
- الإبلاغ عن الأخطاء
- اقتراح الميزات
- مساعدة المستخدمين الآخرين
- تحسين التوثيق

---

**آخر تحديث**: ديسمبر 2024  
**الإصدار**: 1.0.0  
**فريق الدعم**: MedicalLabAnalyzer Support Team