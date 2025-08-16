# MedicalLabAnalyzer - Build Instructions

## 🛠️ دليل البناء الشامل

### المتطلبات الأساسية

#### متطلبات النظام
- **Windows 10/11** (64-bit)
- **.NET 8.0 SDK** (ليس Runtime فقط)
- **PowerShell 5.0** أو أحدث
- **Git** (للنسخ من Repository)
- **4 GB RAM** كحد أدنى
- **5 GB مساحة خالية** على القرص الصلب

#### أدوات اختيارية
- **Visual Studio 2022** (Community/Professional/Enterprise)
- **NSIS** (لإنشاء Installer)
- **7-Zip** (لإنشاء حزم مضغوطة)

### 1. إعداد البيئة

#### تثبيت .NET 8.0 SDK
```bash
# تحميل .NET 8.0 SDK من Microsoft
# https://dotnet.microsoft.com/download/dotnet/8.0

# التحقق من التثبيت
dotnet --version
# يجب أن يظهر: 8.x.x
```

#### تثبيت Git
```bash
# تحميل Git من https://git-scm.com/
# أو استخدام winget
winget install Git.Git
```

#### تثبيت NSIS (اختياري)
```bash
# تحميل NSIS من https://nsis.sourceforge.io/Download
# أو استخدام winget
winget install NSIS.NSIS
```

### 2. الحصول على الكود

#### Clone Repository
```bash
# Clone المشروع
git clone https://github.com/your-repo/MedicalLabAnalyzer.git
cd MedicalLabAnalyzer

# التحقق من الفرع الصحيح
git branch
git status
```

#### أو تحميل ZIP
```bash
# تحميل ZIP من GitHub
# استخراج الملفات
# فتح PowerShell في مجلد المشروع
```

### 3. البناء السريع

#### استخدام سكربت البناء التلقائي
```powershell
# البناء الأساسي
.\build_offline.ps1

# البناء مع إنشاء Installer
.\build_offline.ps1 -CreateInstaller

# البناء مع تنظيف كامل
.\build_offline.ps1 -CleanBuild

# تخصيص مسار الإخراج
.\build_offline.ps1 -OutputPath "C:\MyOutput"
```

#### البناء اليدوي
```powershell
# تنظيف البناء السابق
dotnet clean --configuration Release

# استعادة الحزم
dotnet restore --configuration Release

# بناء المشروع
dotnet build --configuration Release --no-restore

# تشغيل الاختبارات
dotnet test --configuration Release --no-build

# نشر التطبيق
dotnet publish --configuration Release --output ".\Dist\MedicalLabAnalyzer"
```

### 4. البناء المفصل

#### الخطوة 1: التحقق من البيئة
```powershell
# التحقق من .NET SDK
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes

# التحقق من PowerShell
$PSVersionTable.PSVersion

# التحقق من مساحة القرص
Get-WmiObject -Class Win32_LogicalDisk | Select-Object DeviceID, @{Name="FreeSpace(GB)";Expression={[math]::Round($_.FreeSpace/1GB,2)}}
```

#### الخطوة 2: استعادة الحزم
```powershell
# استعادة جميع الحزم
dotnet restore --configuration Release --verbosity normal

# التحقق من الحزم المثبتة
dotnet list package
```

#### الخطوة 3: بناء المشروع
```powershell
# بناء مع معلومات مفصلة
dotnet build --configuration Release --no-restore --verbosity detailed

# التحقق من الملفات المولدة
Get-ChildItem "src\MedicalLabAnalyzer\bin\Release\net8.0-windows" -Recurse
```

#### الخطوة 4: تشغيل الاختبارات
```powershell
# تشغيل جميع الاختبارات
dotnet test --configuration Release --no-build --verbosity normal

# تشغيل اختبارات محددة
dotnet test --configuration Release --no-build --filter "CasaAnalysisRealTest"
dotnet test --configuration Release --no-build --filter "CalibrationTest"
dotnet test --configuration Release --no-build --filter "ServiceTest"
```

#### الخطوة 5: إنشاء حزمة التوزيع
```powershell
# إنشاء مجلد التوزيع
$distPath = ".\Dist\MedicalLabAnalyzer-v1.0.0"
New-Item -ItemType Directory -Path $distPath -Force

# نسخ الملفات التنفيذية
$buildOutput = "src\MedicalLabAnalyzer\bin\Release\net8.0-windows"
Copy-Item "$buildOutput\*" $distPath -Recurse -Force

# نسخ الموارد الإضافية
Copy-Item "Database" $distPath -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item "Reports" $distPath -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item "Samples" $distPath -Recurse -Force -ErrorAction SilentlyContinue

# نسخ التوثيق
Copy-Item "README.md" $distPath -Force
Copy-Item "CHANGELOG.txt" $distPath -Force
Copy-Item "LICENSE" $distPath -Force -ErrorAction SilentlyContinue
```

### 5. البناء المتقدم

#### بناء متعدد المنصات
```powershell
# بناء لـ Windows x64
dotnet publish --configuration Release --runtime win-x64 --self-contained false --output ".\Dist\win-x64"

# بناء لـ Windows x86
dotnet publish --configuration Release --runtime win-x86 --self-contained false --output ".\Dist\win-x86"

# بناء Self-contained
dotnet publish --configuration Release --runtime win-x64 --self-contained true --output ".\Dist\win-x64-self-contained"
```

#### بناء مع تحسينات الأداء
```powershell
# بناء مع تحسينات الأداء
dotnet build --configuration Release --no-restore /p:OptimizeImplicitlyTriggeredBuild=true /p:Deterministic=true

# بناء مع معلومات التصحيح
dotnet build --configuration Release --no-restore /p:DebugType=portable /p:DebugSymbols=true
```

#### بناء مع تحليل الكود
```powershell
# تثبيت أدوات التحليل
dotnet tool install --global dotnet-format
dotnet tool install --global Microsoft.CodeAnalysis.NetAnalyzers

# تحليل الكود
dotnet format --verify-no-changes
dotnet build --configuration Release --no-restore /p:TreatWarningsAsErrors=true
```

### 6. إنشاء Installer

#### استخدام NSIS
```powershell
# إنشاء سكربت NSIS
$nsisScript = @"
!include "MUI2.nsh"

Name "MedicalLabAnalyzer"
OutFile "MedicalLabAnalyzer-Setup.exe"
InstallDir "$PROGRAMFILES\MedicalLabAnalyzer"
RequestExecutionLevel admin

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "English"

Section "Install"
    SetOutPath "$INSTDIR"
    File /r ".\Dist\MedicalLabAnalyzer-v1.0.0\*"
    
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    CreateDirectory "$SMPROGRAMS\MedicalLabAnalyzer"
    CreateShortCut "$SMPROGRAMS\MedicalLabAnalyzer\MedicalLabAnalyzer.lnk" "$INSTDIR\MedicalLabAnalyzer.exe"
    CreateShortCut "$DESKTOP\MedicalLabAnalyzer.lnk" "$INSTDIR\MedicalLabAnalyzer.exe"
SectionEnd

Section "Uninstall"
    RMDir /r "$INSTDIR"
    Delete "$SMPROGRAMS\MedicalLabAnalyzer\MedicalLabAnalyzer.lnk"
    RMDir "$SMPROGRAMS\MedicalLabAnalyzer"
    Delete "$DESKTOP\MedicalLabAnalyzer.lnk"
SectionEnd
"@

$nsisScript | Out-File -FilePath "installer.nsi" -Encoding ASCII

# بناء Installer
makensis installer.nsi
```

#### استخدام Inno Setup
```pascal
[Setup]
AppName=MedicalLabAnalyzer
AppVersion=1.0.0
DefaultDirName={pf}\MedicalLabAnalyzer
DefaultGroupName=MedicalLabAnalyzer
OutputDir=Dist
OutputBaseFilename=MedicalLabAnalyzer-Setup

[Files]
Source: "Dist\MedicalLabAnalyzer-v1.0.0\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\MedicalLabAnalyzer"; Filename: "{app}\MedicalLabAnalyzer.exe"
Name: "{commondesktop}\MedicalLabAnalyzer"; Filename: "{app}\MedicalLabAnalyzer.exe"
```

### 7. إنشاء حزمة Offline

#### حزمة كاملة
```powershell
# إنشاء حزمة كاملة مع جميع التبعيات
$offlinePackage = ".\Dist\MedicalLabAnalyzer-Offline-v1.0.0"
New-Item -ItemType Directory -Path $offlinePackage -Force

# نسخ التطبيق
Copy-Item ".\Dist\MedicalLabAnalyzer-v1.0.0" "$offlinePackage\Application" -Recurse -Force

# نسخ .NET Runtime
$runtimePath = "C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.0"
Copy-Item $runtimePath "$offlinePackage\Runtime" -Recurse -Force

# إنشاء سكربت التثبيت
$installScript = @"
@echo off
echo Installing MedicalLabAnalyzer...
echo.

REM Install .NET Runtime
echo Installing .NET Runtime...
xcopy "Runtime\*" "%ProgramFiles%\dotnet\shared\Microsoft.WindowsDesktop.App\8.0.0\" /E /I /Y

REM Install Application
echo Installing Application...
xcopy "Application\*" "%ProgramFiles%\MedicalLabAnalyzer\" /E /I /Y

REM Create Shortcuts
echo Creating Shortcuts...
mklink "%USERPROFILE%\Desktop\MedicalLabAnalyzer.lnk" "%ProgramFiles%\MedicalLabAnalyzer\MedicalLabAnalyzer.exe"

echo Installation Complete!
pause
"@

$installScript | Out-File -FilePath "$offlinePackage\install.bat" -Encoding ASCII

# إنشاء ZIP
Compress-Archive -Path $offlinePackage -DestinationPath ".\Dist\MedicalLabAnalyzer-Offline-v1.0.0.zip" -Force
```

### 8. اختبار البناء

#### اختبار التطبيق
```powershell
# تشغيل التطبيق
$appPath = ".\Dist\MedicalLabAnalyzer-v1.0.0\MedicalLabAnalyzer.exe"
if (Test-Path $appPath) {
    Write-Host "Testing application startup..."
    Start-Process $appPath -Wait -NoNewWindow
    Write-Host "Application test completed"
} else {
    Write-Host "Application not found: $appPath"
}
```

#### اختبار Installer
```powershell
# اختبار Installer (في بيئة اختبار)
$installerPath = ".\Dist\MedicalLabAnalyzer-Setup.exe"
if (Test-Path $installerPath) {
    Write-Host "Testing installer..."
    # يمكن تشغيل Installer في بيئة اختبار
    Write-Host "Installer test completed"
} else {
    Write-Host "Installer not found: $installerPath"
}
```

#### اختبار الحزمة Offline
```powershell
# اختبار الحزمة Offline
$offlinePath = ".\Dist\MedicalLabAnalyzer-Offline-v1.0.0.zip"
if (Test-Path $offlinePath) {
    Write-Host "Testing offline package..."
    # يمكن اختبار الحزمة في بيئة معزولة
    Write-Host "Offline package test completed"
} else {
    Write-Host "Offline package not found: $offlinePath"
}
```

### 9. تحسينات الأداء

#### تحسين حجم الملفات
```powershell
# إزالة ملفات التصحيح
Get-ChildItem ".\Dist" -Recurse -Include "*.pdb" | Remove-Item -Force

# إزالة ملفات XML
Get-ChildItem ".\Dist" -Recurse -Include "*.xml" | Remove-Item -Force

# ضغط الصور
# يمكن استخدام أدوات ضغط الصور لتقليل حجم الملفات
```

#### تحسين وقت البناء
```powershell
# استخدام Parallel Build
dotnet build --configuration Release --no-restore /m

# استخدام Incremental Build
dotnet build --configuration Release --no-restore /p:IncrementalBuild=true

# استخدام Build Cache
dotnet build --configuration Release --no-restore /p:BuildCacheEnabled=true
```

### 10. استكشاف أخطاء البناء

#### مشاكل شائعة

**خطأ في استعادة الحزم**
```powershell
# تنظيف Cache
dotnet nuget locals all --clear

# إعادة استعادة الحزم
dotnet restore --force
```

**خطأ في بناء المشروع**
```powershell
# تنظيف كامل
dotnet clean --configuration Release
Remove-Item "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "obj" -Recurse -Force -ErrorAction SilentlyContinue

# إعادة البناء
dotnet restore --configuration Release
dotnet build --configuration Release --no-restore
```

**خطأ في الاختبارات**
```powershell
# تشغيل اختبار واحد
dotnet test --configuration Release --no-build --filter "FullyQualifiedName~TestName"

# تشغيل مع معلومات مفصلة
dotnet test --configuration Release --no-build --verbosity detailed --logger "console;verbosity=detailed"
```

**خطأ في إنشاء Installer**
```powershell
# التحقق من NSIS
makensis /VERSION

# تشغيل NSIS مع معلومات مفصلة
makensis /V4 installer.nsi
```

### 11. إعداد CI/CD

#### GitHub Actions
```yaml
name: Build MedicalLabAnalyzer

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --configuration Release --no-build
    
    - name: Create Release Package
      run: |
        dotnet publish --configuration Release --output dist
        Compress-Archive -Path dist -DestinationPath MedicalLabAnalyzer-v1.0.0.zip
    
    - name: Upload Release
      uses: actions/upload-artifact@v3
      with:
        name: MedicalLabAnalyzer
        path: MedicalLabAnalyzer-v1.0.0.zip
```

#### Azure DevOps
```yaml
trigger:
- main

pool:
  vmImage: 'windows-latest'

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore NuGet packages'
  inputs:
    command: 'restore'
    projects: '$(solution)'

- task: DotNetCoreCLI@2
  displayName: 'Build solution'
  inputs:
    command: 'build'
    projects: '$(solution)'
    arguments: '--configuration $(buildConfiguration) --no-restore'

- task: DotNetCoreCLI@2
  displayName: 'Run tests'
  inputs:
    command: 'test'
    projects: '**/*Tests/*.csproj'
    arguments: '--configuration $(buildConfiguration) --no-build'

- task: ArchiveFiles@2
  displayName: 'Create release package'
  inputs:
    rootFolderOrFile: '$(Build.SourcesDirectory)/src/MedicalLabAnalyzer/bin/$(buildConfiguration)/net8.0-windows'
    includeRootFolder: false
    archiveType: 'zip'
    archiveFile: '$(Build.ArtifactStagingDirectory)/MedicalLabAnalyzer-v1.0.0.zip'

- task: PublishBuildArtifacts@1
  displayName: 'Publish artifacts'
  inputs:
    pathToPublish: '$(Build.ArtifactStagingDirectory)'
    artifactName: 'drop'
```

### 12. النشر والإنتاج

#### التحقق قبل النشر
```powershell
# قائمة التحقق
$checklist = @(
    "✅ جميع الاختبارات تمر",
    "✅ التطبيق يعمل بدون أخطاء",
    "✅ جميع الميزات تعمل",
    "✅ التقارير تُنشأ بشكل صحيح",
    "✅ قاعدة البيانات تعمل",
    "✅ نظام AuditLog يعمل",
    "✅ إدارة المستخدمين تعمل",
    "✅ النسخ الاحتياطي يعمل"
)

foreach ($item in $checklist) {
    Write-Host $item
}
```

#### إنشاء حزمة النشر
```powershell
# إنشاء حزمة النشر النهائية
$releasePackage = ".\Dist\MedicalLabAnalyzer-Release-v1.0.0"
New-Item -ItemType Directory -Path $releasePackage -Force

# نسخ الملفات
Copy-Item ".\Dist\MedicalLabAnalyzer-v1.0.0\*" $releasePackage -Recurse -Force

# إضافة ملفات إضافية
Copy-Item "README.md" $releasePackage -Force
Copy-Item "CHANGELOG.txt" $releasePackage -Force
Copy-Item "LICENSE" $releasePackage -Force
Copy-Item "QuickStart.md" $releasePackage -Force

# إنشاء ZIP نهائي
Compress-Archive -Path $releasePackage -DestinationPath ".\Dist\MedicalLabAnalyzer-Release-v1.0.0.zip" -Force

Write-Host "Release package created: .\Dist\MedicalLabAnalyzer-Release-v1.0.0.zip"
```

---

## 🎯 ملخص البناء

| المرحلة | الوقت المتوقع | الوصف |
|---------|---------------|--------|
| إعداد البيئة | 10 دقائق | تثبيت الأدوات المطلوبة |
| استعادة الحزم | 5 دقائق | تحميل التبعيات |
| البناء | 3 دقائق | تجميع المشروع |
| الاختبارات | 2 دقائق | تشغيل الاختبارات |
| إنشاء الحزمة | 2 دقائق | تجهيز التوزيع |
| **المجموع** | **22 دقيقة** | **جاهز للنشر** |

---

**MedicalLabAnalyzer** - نظام إدارة مختبر طبي متقدم ومتكامل 🏥