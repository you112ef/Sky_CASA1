# دليل المساهمة في MedicalLabAnalyzer

شكراً لاهتمامك بالمساهمة في مشروع **MedicalLabAnalyzer**! هذا الدليل سيساعدك على فهم كيفية المساهمة في المشروع.

## 🚀 كيفية البدء

### 1. Fork المشروع
1. اذهب إلى [GitHub Repository](https://github.com/you112ef/Sky_CASA1)
2. اضغط على زر "Fork" في أعلى الصفحة
3. سيتم إنشاء نسخة من المشروع في حسابك

### 2. Clone المشروع
```bash
git clone https://github.com/YOUR_USERNAME/Sky_CASA1.git
cd Sky_CASA1
```

### 3. إعداد بيئة التطوير
```bash
# تثبيت .NET 8.0 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# استعادة الحزم
dotnet restore

# بناء المشروع
dotnet build --configuration Release
```

## 📝 أنواع المساهمات

### 🐛 إصلاح الأخطاء
- تحديد المشكلة بوضوح
- إرفاق خطوات لإعادة الإنتاج
- إرفاق ملفات السجل إن أمكن
- اختبار الحل قبل الإرسال

### ✨ إضافة ميزات جديدة
- مناقشة الميزة في Issue أولاً
- التأكد من عدم وجود ميزة مشابهة
- اتباع معايير الكود
- إضافة اختبارات للميزة الجديدة

### 📚 تحسين التوثيق
- تحديث README.md
- إضافة تعليقات للكود
- تحسين دليل المستخدم
- إضافة أمثلة للاستخدام

### 🎨 تحسينات واجهة المستخدم
- اتباع تصميم Material Design
- دعم اللغة العربية والـ RTL
- تحسين تجربة المستخدم
- اختبار على أحجام شاشات مختلفة

## 🔧 معايير الكود

### تسمية الملفات والفئات
```csharp
// ✅ صحيح
public class PatientService { }
public class CASAAnalyzer { }
public class UrineTestResult { }

// ❌ خطأ
public class patientService { }
public class CASA_analyzer { }
public class urine_test_result { }
```

### تسمية المتغيرات
```csharp
// ✅ صحيح
private string _patientName;
private int _examCount;
private readonly ILogger<PatientService> _logger;

// ❌ خطأ
private string patientName;
private int exam_count;
private readonly ILogger<PatientService> logger;
```

### التعليقات
```csharp
/// <summary>
/// يحلل نتائج فحص CBC للمريض
/// </summary>
/// <param name="patientId">معرف المريض</param>
/// <param name="cbcData">بيانات CBC</param>
/// <returns>نتائج التحليل</returns>
public async Task<CBCAnalysisResult> AnalyzeCBCAsync(int patientId, Dictionary<string, double> cbcData)
{
    // التحقق من صحة البيانات
    if (cbcData == null || !cbcData.Any())
    {
        throw new ArgumentException("بيانات CBC مطلوبة", nameof(cbcData));
    }
    
    // باقي الكود...
}
```

### معالجة الأخطاء
```csharp
try
{
    var result = await _analyzer.AnalyzeAsync(data);
    return result;
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "بيانات غير صحيحة: {Message}", ex.Message);
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "خطأ غير متوقع في التحليل");
    throw new ApplicationException("فشل في تحليل البيانات", ex);
}
```

## 🧪 الاختبارات

### إضافة اختبارات جديدة
```csharp
[Test]
public async Task AnalyzeCBC_WithValidData_ReturnsCorrectResult()
{
    // Arrange
    var analyzer = new CBCAnalyzer();
    var testData = new Dictionary<string, double>
    {
        ["WBC"] = 7.5,
        ["RBC"] = 4.8,
        ["Hemoglobin"] = 14.2
    };
    
    // Act
    var result = await analyzer.AnalyzeCBCResults(testData);
    
    // Assert
    Assert.That(result.Status, Is.EqualTo("Normal"));
    Assert.That(result.TotalParameters, Is.EqualTo(3));
}
```

### تشغيل الاختبارات
```bash
# تشغيل جميع الاختبارات
dotnet test

# تشغيل اختبارات محددة
dotnet test --filter "TestCategory=CASA"

# تشغيل مع تقرير مفصل
dotnet test --logger "console;verbosity=detailed"
```

## 📋 عملية إرسال التغييرات

### 1. إنشاء Branch جديد
```bash
git checkout -b feature/your-feature-name
# أو
git checkout -b fix/your-bug-fix
```

### 2. إجراء التغييرات
```bash
# تعديل الملفات المطلوبة
# إضافة ملفات جديدة
git add .

# حفظ التغييرات
git commit -m "Add new feature: description of changes"
```

### 3. إرسال التغييرات
```bash
git push origin feature/your-feature-name
```

### 4. إنشاء Pull Request
1. اذهب إلى GitHub Repository
2. اضغط على "Compare & pull request"
3. املأ النموذج:
   - **العنوان**: وصف مختصر للتغييرات
   - **الوصف**: تفاصيل التغييرات والسبب
   - **نوع التغيير**: Bug fix / Feature / Documentation
   - **اختبار**: ما تم اختباره

## 📋 قالب Pull Request

```markdown
## نوع التغيير
- [ ] Bug fix
- [ ] ميزة جديدة
- [ ] تحسين التوثيق
- [ ] تحسين الأداء
- [ ] إصلاح أمان

## الوصف
وصف مختصر للتغييرات...

## المشكلة
إذا كان إصلاح خطأ، اشرح المشكلة...

## الحل
اشرح كيف تم حل المشكلة...

## الاختبارات
- [ ] تم اختبار التغييرات محلياً
- [ ] تم تشغيل الاختبارات بنجاح
- [ ] تم اختبار واجهة المستخدم

## لقطات شاشة
إذا كان التغيير يؤثر على واجهة المستخدم...

## ملاحظات إضافية
أي معلومات إضافية...
```

## 🎯 معايير المراجعة

### الكود
- [ ] يتبع معايير التسمية
- [ ] يحتوي على تعليقات كافية
- [ ] يعالج الأخطاء بشكل صحيح
- [ ] لا يحتوي على كود مكرر

### الاختبارات
- [ ] يحتوي على اختبارات كافية
- [ ] تغطي الحالات الحدية
- [ ] تعمل بنجاح

### التوثيق
- [ ] تم تحديث README إذا لزم الأمر
- [ ] تم إضافة تعليقات للكود الجديد
- [ ] تم تحديث CHANGELOG

### الأمان
- [ ] لا يحتوي على ثغرات أمنية
- [ ] يتبع أفضل الممارسات الأمنية
- [ ] يحمي البيانات الحساسة

## 🏷️ إصدارات Git

### أنواع الـ Tags
- `v1.0.0` - إصدار رئيسي
- `v1.1.0` - إصدار ثانوي
- `v1.0.1` - إصلاح خطأ

### إنشاء Tag
```bash
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

## 📞 الحصول على المساعدة

### إنشاء Issue
1. اذهب إلى [Issues](https://github.com/you112ef/Sky_CASA1/issues)
2. اضغط على "New Issue"
3. اختر نوع الـ Issue المناسب
4. املأ النموذج بالتفاصيل

### قوالب Issues
- **Bug Report**: للإبلاغ عن أخطاء
- **Feature Request**: لطلب ميزات جديدة
- **Documentation**: لتحسين التوثيق
- **Question**: للأسئلة العامة

## 🎉 الاعتراف بالمساهمات

سيتم إضافة جميع المساهمين إلى:
- قائمة المساهمين في README.md
- ملف CONTRIBUTORS.md
- صفحة GitHub Contributors

## 📄 الترخيص

بالمساهمة في هذا المشروع، فإنك توافق على أن مساهماتك ستكون مرخصة تحت نفس رخصة المشروع (MIT).

---

**شكراً لك على المساهمة في MedicalLabAnalyzer!** 🎉

للمزيد من المعلومات، راجع:
- [README.md](README.md)
- [QuickStart.md](QuickStart.md)
- [BuildInstructions.md](BuildInstructions.md)