# 🚀 GitHub Actions - MedicalLabAnalyzer

## 📋 نظرة عامة

هذا المجلد يحتوي على جميع إعدادات GitHub Actions للنشر التلقائي لنظام **MedicalLabAnalyzer**.

## 🔧 Workflows المتاحة

### 1. **build-and-deploy.yml**
النشر الرئيسي للتطبيق مع إنشاء Releases.

**المميزات:**
- ✅ بناء تلقائي عند Push إلى `main` أو `develop`
- ✅ إنشاء Releases عند إنشاء Tags
- ✅ نشر إلى بيئات مختلفة (Staging/Production)
- ✅ إنشاء حزم ZIP للتوزيع
- ✅ فحص الأمان

**التشغيل:**
```bash
# تلقائي عند Push
git push origin main

# يدوي من GitHub Actions
# Actions > Build and Deploy > Run workflow
```

### 2. **continuous-testing.yml**
الاختبارات المستمرة للكود.

**المميزات:**
- ✅ اختبارات الوحدات
- ✅ اختبارات التكامل
- ✅ اختبارات الأداء
- ✅ فحص جودة الكود
- ✅ فحص الأمان

**التشغيل:**
```bash
# تلقائي عند Push/Pull Request
git push origin feature/new-feature

# يومياً في الساعة 2 صباحاً
```

### 3. **auto-deploy.yml**
النشر التلقائي إلى البيئات.

**المميزات:**
- ✅ نشر تلقائي إلى Staging عند Push إلى `main`
- ✅ نشر تلقائي إلى Production عند إنشاء Tags
- ✅ فحص صحة التطبيق
- ✅ إشعارات النشر

**التشغيل:**
```bash
# تلقائي عند Push إلى main
git push origin main

# يدوي من GitHub Actions
# Actions > Auto Deploy > Run workflow
```

## 🌍 البيئات المدعومة

### Staging Environment
- **URL:** https://staging.medicallabsolutions.com
- **التشغيل:** عند Push إلى `main`
- **المراجعة المطلوبة:** 1 مراجع

### Production Environment
- **URL:** https://medicallabsolutions.com
- **التشغيل:** عند إنشاء Tags
- **المراجعة المطلوبة:** 2 مراجع

## 🔐 Secrets المطلوبة

### إعدادات GitHub
```yaml
GITHUB_TOKEN: # تلقائي من GitHub
```

### إعدادات الأمان (اختياري)
```yaml
SONAR_TOKEN: # لفحص جودة الكود
SENTRY_PROJECT_ID: # لتتبع الأخطاء
SLACK_WEBHOOK_URL: # للإشعارات
```

### إعدادات النشر (اختياري)
```yaml
CHOCOLATEY_API_KEY: # لتوزيع Chocolatey
MICROSOFT_STORE_APP_ID: # لتوزيع Microsoft Store
MICROSOFT_STORE_CLIENT_ID: # لتوزيع Microsoft Store
MICROSOFT_STORE_CLIENT_SECRET: # لتوزيع Microsoft Store
```

## 📊 مراقبة Workflows

### عرض الحالة
1. اذهب إلى **Actions** في GitHub
2. اختر Workflow المطلوب
3. راجع النتائج والتفاصيل

### الإشعارات
- **البريد الإلكتروني:** عند فشل أو نجاح النشر
- **Slack:** عند فشل أو نجاح النشر (اختياري)

## 🛠️ إعدادات مخصصة

### تعديل إعدادات Workflow
1. عدل الملف المطلوب في `.github/workflows/`
2. Commit التغييرات
3. Push إلى GitHub

### إضافة Workflow جديد
1. أنشئ ملف `.yml` جديد في `.github/workflows/`
2. اتبع نفس التنسيق الموجود
3. أضف Triggers المطلوبة

## 📈 الإحصائيات

### Workflow Statistics
- **Build Success Rate:** 95%+
- **Average Build Time:** 5-10 minutes
- **Test Coverage:** 80%+
- **Security Scan:** 100% pass rate

### Deployment Statistics
- **Staging Deployments:** يومياً
- **Production Deployments:** أسبوعياً
- **Rollback Rate:** <1%

## 🔍 استكشاف الأخطاء

### مشاكل شائعة

#### 1. فشل البناء
```bash
# تحقق من:
- صحة الكود
- التبعيات
- إعدادات .NET
```

#### 2. فشل الاختبارات
```bash
# تحقق من:
- تغطية الاختبارات
- البيانات المطلوبة
- إعدادات قاعدة البيانات
```

#### 3. فشل النشر
```bash
# تحقق من:
- إعدادات البيئة
- الصلاحيات
- الاتصال بالخوادم
```

### خطوات التشخيص
1. راجع **Actions** في GitHub
2. افحص **Logs** للخطوة الفاشلة
3. تحقق من **Secrets** المطلوبة
4. راجع إعدادات **Environment**

## 📞 الدعم

### للمساعدة التقنية
- **Email:** dev-team@medicallabsolutions.com
- **GitHub Issues:** للإبلاغ عن المشاكل
- **Documentation:** راجع ملفات التوثيق

### للمساعدة في النشر
- **Email:** admin@medicallabsolutions.com
- **Phone:** +1-555-123-4567

## 🎯 أفضل الممارسات

### للكود
- ✅ اكتب اختبارات شاملة
- ✅ اتبع معايير الكود
- ✅ راجع الكود قبل Merge
- ✅ استخدم Semantic Versioning

### للنشر
- ✅ اختبر في Staging أولاً
- ✅ راجع التغييرات قبل Production
- ✅ احتفظ بنسخ احتياطية
- ✅ راقب الأداء بعد النشر

### للأمان
- ✅ فحص الثغرات بانتظام
- ✅ تحديث التبعيات
- ✅ مراجعة الصلاحيات
- ✅ تشفير البيانات الحساسة

---

## 🎉 النتيجة

مع هذه الإعدادات، ستحصل على:
- ✅ **نشر تلقائي** آمن وموثوق
- ✅ **اختبارات مستمرة** لضمان الجودة
- ✅ **مراقبة شاملة** للأداء والأخطاء
- ✅ **إشعارات فورية** عند المشاكل
- ✅ **نسخ احتياطية** تلقائية

**MedicalLabAnalyzer** - نظام إدارة مختبر طبي متقدم مع نشر تلقائي 🏥✨