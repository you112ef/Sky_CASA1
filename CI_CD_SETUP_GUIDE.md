# 🚀 دليل إعداد CI/CD - MedicalLabAnalyzer

## 📋 نظرة عامة

هذا الدليل يوضح كيفية إعداد وتشغيل نظام CI/CD الكامل لتطبيق MedicalLabAnalyzer باستخدام GitHub Actions.

## 🛠️ الملفات المطلوبة

### 1. ملفات Workflow الأساسية

#### `simple-test.yml` - اختبار سريع
```yaml
# للاختبار السريع والتحقق من البناء
- Build only
- Basic tests
- No external dependencies
```

#### `build-test.yml` - بناء واختبار شامل
```yaml
# للبناء والاختبار مع VC++ Redistributables
- Full build process
- Comprehensive testing
- VC++ Redistributables installation
```

#### `sonarqube.yml` - تحليل جودة الكود
```yaml
# لتحليل جودة الكود باستخدام SonarQube
- Code quality analysis
- Coverage reporting
- Security scanning
```

#### `package.yml` - إنشاء الحزم
```yaml
# لإنشاء حزم التوزيع
- Offline package creation
- ZIP file generation
- Installation scripts
```

#### `security.yml` - الفحص الأمني
```yaml
# لفحص الأمان والتبعيات
- Vulnerability scanning
- Dependency analysis
- License compliance
```

### 2. ملفات Scripts المحلية

#### `scripts/BuildDeploy.ps1`
```powershell
# لبناء التطبيق محلياً
.\scripts\BuildDeploy.ps1 -Configuration Release -CreatePackage
```

#### `scripts/TestRunner.ps1`
```powershell
# لتشغيل الاختبارات محلياً
.\scripts\TestRunner.ps1 -Coverage -GenerateReport
```

## 🔧 إعداد GitHub Secrets

### 1. SonarQube Token
```bash
# في GitHub Repository Settings > Secrets and variables > Actions
SONAR_TOKEN = your_sonarcloud_token_here
```

### 2. Slack Webhook (اختياري)
```bash
# لإرسال إشعارات Slack
SLACK_WEBHOOK_URL = your_slack_webhook_url_here
```

## 🚀 كيفية التشغيل

### 1. اختبار سريع
```bash
# تشغيل workflow مبسط للاختبار السريع
git push origin main
# سيتم تشغيل simple-test.yml تلقائياً
```

### 2. بناء شامل
```bash
# تشغيل workflow البناء والاختبار الشامل
git push origin develop
# سيتم تشغيل build-test.yml تلقائياً
```

### 3. تحليل جودة الكود
```bash
# تشغيل تحليل SonarQube
git push origin main
# سيتم تشغيل sonarqube.yml تلقائياً
```

### 4. إنشاء حزمة
```bash
# إنشاء حزمة توزيع
git tag v2.0.0
git push origin v2.0.0
# سيتم تشغيل package.yml تلقائياً
```

### 5. فحص أمني
```bash
# تشغيل فحص أمني
git push origin main
# سيتم تشغيل security.yml تلقائياً
```

## 📊 مراقبة النتائج

### 1. GitHub Actions Dashboard
```
https://github.com/your-username/your-repo/actions
```

### 2. SonarCloud Dashboard
```
https://sonarcloud.io/project/overview?id=your-project-key
```

### 3. Artifacts
```
- Test Results: Available in workflow artifacts
- Build Artifacts: Available in workflow artifacts
- Packages: Available in workflow artifacts
```

## 🔍 استكشاف الأخطاء

### 1. مشاكل البناء
```bash
# فحص ملف الحل
ls -la *.sln

# فحص التبعيات
dotnet restore --verbosity detailed

# فحص البناء
dotnet build --verbosity detailed
```

### 2. مشاكل الاختبارات
```bash
# تشغيل الاختبارات محلياً
.\scripts\TestRunner.ps1 -Verbose

# فحص نتائج الاختبارات
ls -la TestResults/
```

### 3. مشاكل SonarQube
```bash
# فحص Token
echo $SONAR_TOKEN

# تشغيل SonarQube محلياً
dotnet sonarscanner begin /k:"your-project-key"
dotnet build
dotnet sonarscanner end
```

### 4. مشاكل الحزم
```bash
# تشغيل script البناء محلياً
.\scripts\BuildDeploy.ps1 -CreatePackage -Verbose

# فحص مجلد التوزيع
ls -la Dist/
```

## 📝 أفضل الممارسات

### 1. إدارة Branches
```bash
# main: للإنتاج
# develop: للتطوير
# feature/*: للميزات الجديدة
# release/*: للإصدارات
```

### 2. إدارة Tags
```bash
# إنشاء tag للإصدار
git tag v2.0.0
git push origin v2.0.0

# حذف tag
git tag -d v2.0.0
git push origin --delete v2.0.0
```

### 3. مراجعة الكود
```bash
# إنشاء Pull Request
git checkout -b feature/new-feature
git push origin feature/new-feature
# إنشاء PR في GitHub
```

### 4. مراقبة الأداء
```bash
# فحص وقت البناء
# مراقبة استخدام الموارد
# تحسين التبعيات
```

## 🎯 معايير النجاح

### 1. نجاح البناء
- [ ] Build completes without errors
- [ ] All tests pass
- [ ] Code coverage > 80%
- [ ] No security vulnerabilities

### 2. نجاح التحليل
- [ ] SonarQube analysis completes
- [ ] Quality gate passes
- [ ] Code smells < 10
- [ ] Duplications < 5%

### 3. نجاح التعبئة
- [ ] Package creation succeeds
- [ ] ZIP file generated
- [ ] Installer created (optional)
- [ ] Artifacts uploaded

### 4. نجاح الإشعارات
- [ ] Slack notifications work
- [ ] GitHub summaries generated
- [ ] Status badges updated
- [ ] Error reporting functional

## 🔧 التخصيص

### 1. تعديل إعدادات Workflow
```yaml
# في كل workflow file
env:
  DOTNET_VERSION: '8.0.100'  # تغيير إصدار .NET
  SOLUTION_FILE: 'MedicalLabAnalyzer.sln'  # تغيير ملف الحل
```

### 2. إضافة خطوات جديدة
```yaml
- name: Custom Step
  run: |
    echo "Custom step execution"
    # Add your custom logic here
```

### 3. تعديل Triggers
```yaml
on:
  push:
    branches: [ main, develop ]  # تغيير الفروع
  pull_request:
    branches: [ main ]  # تغيير فروع PR
```

## 📞 الدعم والمساعدة

### 1. المشاكل الشائعة
- **Build fails**: فحص التبعيات وملف الحل
- **Tests fail**: تشغيل الاختبارات محلياً
- **SonarQube fails**: فحص Token والإعدادات
- **Package creation fails**: فحص مسارات الملفات

### 2. مصادر المساعدة
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [SonarCloud Documentation](https://docs.sonarcloud.io/)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)

### 3. التواصل
- إنشاء Issue في GitHub
- مراجعة Workflow logs
- فحص Artifacts للتفاصيل

## 🎉 الخلاصة

بعد اتباع هذا الدليل، سيكون لديك:
- ✅ نظام CI/CD مستقر وموثوق
- ✅ بناء سريع وفعال
- ✅ اختبارات شاملة
- ✅ تحليل جودة الكود
- ✅ حزم توزيع جاهزة
- ✅ إشعارات فورية
- ✅ توثيق شامل

---

*تم إنشاء هذا الدليل لـ MedicalLabAnalyzer v2.0.0*