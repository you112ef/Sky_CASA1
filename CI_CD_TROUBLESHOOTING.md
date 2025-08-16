# 🔧 استكشاف أخطاء خط أنابيب CI/CD - MedicalLabAnalyzer

## 📋 نظرة عامة

هذا الدليل يساعدك في حل مشاكل خط أنابيب CI/CD وإصلاح أخطاء `exit code 1` التي تحدث في GitHub Actions.

## 🚨 المشاكل الشائعة وحلولها

### 1. خطأ `Process completed with exit code 1`

#### الأسباب المحتملة:
- ملف الحل `.sln` غير موجود أو في مسار خاطئ
- مشاكل في تبعيات NuGet
- أخطاء في الكود أو مراجع مفقودة
- عدم توافق مكتبات EmguCV/Crystal Reports

#### الحلول:

##### أ) التحقق من ملف الحل
```yaml
- name: Verify solution file
  run: |
    Write-Host "=== Verifying solution file ==="
    if (Test-Path "${{ env.SOLUTION_FILE }}") {
      Write-Host "✅ Solution file found: ${{ env.SOLUTION_FILE }}"
    } else {
      Write-Host "❌ Solution file not found: ${{ env.SOLUTION_FILE }}"
      Get-ChildItem -Name "*.sln"
      exit 1
    }
```

##### ب) إصلاح تبعيات NuGet
```yaml
- name: Clear NuGet cache
  run: |
    Write-Host "=== Clearing NuGet cache ==="
    dotnet nuget locals all --clear
    Write-Host "✅ NuGet cache cleared"

- name: Restore packages
  run: |
    Write-Host "=== Restoring NuGet packages ==="
    dotnet restore "${{ env.SOLUTION_FILE }}" --verbosity normal
    if ($LASTEXITCODE -eq 0) {
      Write-Host "✅ NuGet packages restored successfully"
    } else {
      Write-Host "❌ NuGet restore failed"
      exit 1
    }
```

##### ج) تثبيت VC++ Redistributables
```yaml
- name: Install VC++ Redistributables
  run: |
    Write-Host "=== Installing VC++ Redistributables ==="
    choco install vcredist2019 -y --no-progress
    choco install vcredist2022 -y --no-progress
    Write-Host "✅ VC++ Redistributables installed"
```

### 2. فشل اختبارات الوحدة

#### الأسباب المحتملة:
- اختبارات غير موجودة
- مشاكل في تكوين الاختبارات
- تبعيات مفقودة للاختبارات

#### الحلول:

##### أ) إنشاء مجلد TestResults
```yaml
- name: Create TestResults directory
  run: |
    Write-Host "=== Creating TestResults directory ==="
    New-Item -ItemType Directory -Path "TestResults" -Force
    Write-Host "✅ TestResults directory created"
```

##### ب) تشغيل الاختبارات مع معالجة الأخطاء
```yaml
- name: Run tests
  run: |
    Write-Host "=== Running tests ==="
    dotnet test "${{ env.SOLUTION_FILE }}" --configuration Release --no-build --verbosity normal --logger "console;verbosity=normal" --results-directory ./TestResults
    if ($LASTEXITCODE -eq 0) {
      Write-Host "✅ Tests completed successfully"
    } else {
      Write-Host "❌ Tests failed"
      exit 1
    }
```

### 3. فشل تحليل جودة الكود (SonarQube)

#### الأسباب المحتملة:
- SONAR_TOKEN غير صحيح أو مفقود
- مشاكل في تكوين SonarCloud
- فشل في البناء أو الاختبارات

#### الحلول:

##### أ) التحقق من SONAR_TOKEN
```yaml
- name: Begin SonarCloud analysis
  run: |
    echo "=== Starting SonarCloud analysis ==="
    dotnet sonarscanner begin \
      /k:"my-org:medical-analyzer" \
      /o:"my-org" \
      /d:sonar.login="${{ secrets.SONAR_TOKEN }}" \
      /d:sonar.host.url="https://sonarcloud.io"
```

##### ب) تشغيل SonarQube بعد نجاح البناء
```yaml
code-quality:
  needs: build-and-test
  if: success() && github.event_name != 'pull_request'
```

## 🛠️ خطوات التشخيص

### 1. تشغيل البناء محليًا
```bash
# في مجلد المشروع
dotnet restore MedicalLabAnalyzer.sln
dotnet build MedicalLabAnalyzer.sln --configuration Release
dotnet test MedicalLabAnalyzer.sln --configuration Release --no-build
```

### 2. فحص ملف الحل
```bash
# التحقق من وجود ملف الحل
ls -la *.sln

# عرض محتويات ملف الحل
cat MedicalLabAnalyzer.sln
```

### 3. فحص تبعيات NuGet
```bash
# عرض التبعيات
dotnet list package

# فحص التبعيات المعطلة
dotnet list package --vulnerable

# فحص التبعيات المحدثة
dotnet list package --outdated
```

## 📁 ملفات Workflow المتاحة

### 1. `basic-test.yml` - اختبار أساسي
- بناء واختبار بسيط
- بدون تبعيات معقدة
- مناسب للتحقق السريع

### 2. `enhanced-test.yml` - اختبار محسن
- تثبيت VC++ Redistributables
- معالجة أخطاء مفصلة
- تحقق من الملفات والأدلة

### 3. `fixed-medical-analyzer.yml` - خط أنابيب كامل
- جميع المميزات
- تحليل جودة الكود
- إنشاء الحزم
- إشعارات Slack

## 🔍 خطوات استكشاف الأخطاء

### الخطوة 1: تشغيل Basic Test
```bash
# في GitHub Actions
# تشغيل workflow: basic-test.yml
```

### الخطوة 2: فحص السجلات
- مراجعة سجلات GitHub Actions
- البحث عن رسائل الخطأ
- التحقق من exit codes

### الخطوة 3: تشغيل Enhanced Test
```bash
# في GitHub Actions
# تشغيل workflow: enhanced-test.yml
```

### الخطوة 4: إصلاح المشاكل
- إصلاح الأخطاء في الكود
- تحديث التبعيات
- تصحيح مسارات الملفات

### الخطوة 5: تشغيل Full Pipeline
```bash
# في GitHub Actions
# تشغيل workflow: fixed-medical-analyzer.yml
```

## 📊 مراقبة الأداء

### مؤشرات النجاح:
- ✅ جميع الخطوات تنتهي بـ `exit code 0`
- ✅ رسائل "✅" في السجلات
- ✅ رفع الأصول بنجاح
- ✅ إشعارات Slack (إذا مُعدة)

### مؤشرات الفشل:
- ❌ `exit code 1` في أي خطوة
- ❌ رسائل "❌" في السجلات
- ❌ فشل في رفع الأصول
- ❌ فشل في إشعارات Slack

## 🚀 نصائح للتحسين

### 1. تحسين الأداء
```yaml
# استخدام cache للتبعيات
- name: Cache NuGet packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

### 2. تحسين الأمان
```yaml
# فحص الثغرات
- name: Security scan
  run: |
    dotnet list package --vulnerable
    dotnet outdated --fail-on-updates
```

### 3. تحسين المراقبة
```yaml
# إشعارات مفصلة
- name: Notify Slack
  if: ${{ secrets.SLACK_WEBHOOK_URL != '' }}
  uses: slackapi/slack-github-action@v1.25.0
  with:
    payload: |
      {
        "text": "Pipeline Status",
        "attachments": [
          {
            "fields": [
              {"title": "Build", "value": "${{ needs.build.result }}", "short": true},
              {"title": "Tests", "value": "${{ needs.tests.result }}", "short": true}
            ]
          }
        ]
      }
```

## 📞 الدعم

إذا استمرت المشاكل:

1. **مراجعة السجلات** في GitHub Actions
2. **تشغيل البناء محليًا** للتحقق من الأخطاء
3. **فحص التبعيات** وتحديثها
4. **التحقق من تكوين SonarCloud**
5. **مراجعة إعدادات Secrets** في GitHub

---

**آخر تحديث**: ديسمبر 2024  
**الإصدار**: 2.1.0  
**الحالة**: جاهز للإنتاج