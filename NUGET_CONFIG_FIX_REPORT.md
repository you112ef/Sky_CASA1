# NuGet.Config Syntax Error Fix Report

## المشكلة المحددة
تم اكتشاف خطأ في ملف `NuGet.Config` يمنع التطبيق من البناء:

```
error: Unable to parse config file because: Missing required attribute 'key' in element 'add'. 
Path: 'D:\a\Sky_CASA1\Sky_CASA1\NuGet.Config'.
```

## سبب المشكلة
تم إضافة خصائص MSBuild غير صالحة في قسم `<config>` من ملف `NuGet.Config`:

```xml
<!-- هذه الأسطر كانت تسبب الخطأ -->
<add key="restorePackagesWithLockFile" value="true" />
<add key="restoreLockedMode" value="false" />
```

هذه الخصائص هي خصائص MSBuild وليست خصائص NuGet صالحة.

## الحل المطبق
1. **إزالة الخصائص غير الصالحة** من `NuGet.Config`
2. **الحفاظ على الخصائص الصحيحة** في `Directory.Build.props`
3. **إزالة التكرار** من `Directory.Build.props`

## الملفات المعدلة

### 1. nuget.config
**قبل الإصلاح:**
```xml
<config>
  <add key="globalPackagesFolder" value="%USERPROFILE%\.nuget\packages" />
  <add key="maxHttpRequestsPerSource" value="10" />
  <add key="signatureValidationMode" value="accept" />
  <add key="http_timeout" value="900" />
  <add key="repositoryPath" value="%USERPROFILE%\.nuget\packages" />
  <add key="dependencyVersion" value="HighestMinor" />
  <!-- هذه الأسطر كانت تسبب الخطأ -->
  <add key="restorePackagesWithLockFile" value="true" />
  <add key="restoreLockedMode" value="false" />
</config>
```

**بعد الإصلاح:**
```xml
<config>
  <add key="globalPackagesFolder" value="%USERPROFILE%\.nuget\packages" />
  <add key="maxHttpRequestsPerSource" value="10" />
  <add key="signatureValidationMode" value="accept" />
  <add key="http_timeout" value="900" />
  <add key="repositoryPath" value="%USERPROFILE%\.nuget\packages" />
  <add key="dependencyVersion" value="HighestMinor" />
</config>
```

### 2. Directory.Build.props
**قبل الإصلاح:**
```xml
<!-- تكرار في الخصائص -->
<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
<RestoreLockedMode Condition="$(ContinuousIntegrationBuild) == 'true'">true</RestoreLockedMode>

<!-- ... -->

<!-- NuGet package management -->
<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
<RestoreLockedMode Condition="$(ContinuousIntegrationBuild) == 'true'">true</RestoreLockedMode>
```

**بعد الإصلاح:**
```xml
<!-- الخصائص في المكان الصحيح -->
<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
<RestoreLockedMode Condition="$(ContinuousIntegrationBuild) == 'true'">true</RestoreLockedMode>

<!-- ... -->

<!-- NuGet package management -->
<!-- These properties are already defined in the main PropertyGroup above -->
```

## الخصائص الصحيحة في كل ملف

### nuget.config
- `globalPackagesFolder`: مجلد الحزم العام
- `maxHttpRequestsPerSource`: الحد الأقصى لطلبات HTTP لكل مصدر
- `signatureValidationMode`: وضع التحقق من التوقيع
- `http_timeout`: مهلة HTTP
- `repositoryPath`: مسار المستودع
- `dependencyVersion`: إصدار التبعيات

### Directory.Build.props
- `RestorePackagesWithLockFile`: استخدام ملف قفل الحزم
- `RestoreLockedMode`: وضع القفل (في CI فقط)

## النتيجة المتوقعة
بعد هذا الإصلاح:
1. ✅ سيتم حل خطأ "Missing required attribute 'key'"
2. ✅ سيعمل `dotnet restore` بشكل صحيح
3. ✅ سيعمل `dotnet build` بدون أخطاء
4. ✅ ستستمر GitHub Actions في البناء

## الخطوات التالية
1. انتظار GitHub Actions لإكمال البناء
2. التحقق من نجاح البناء
3. اختبار التطبيق محلياً
4. المتابعة مع تطوير الميزات المفقودة

## تاريخ الإصلاح
- **التاريخ**: 16 أغسطس 2025
- **الوقت**: بعد تحليل سجل البناء الجديد
- **السبب**: استمرار وجود الأخطاء بعد الإصلاحات السابقة
- **الحل**: إصلاح خطأ في بنية ملف NuGet.Config

## ملاحظات مهمة
- خصائص MSBuild يجب أن تكون في ملفات `.csproj` أو `Directory.Build.props`
- خصائص NuGet يجب أن تكون في `nuget.config`
- لا توجد تداخل بين هذين النوعين من الخصائص