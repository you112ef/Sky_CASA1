# Medical Lab Analyzer Security Setup Script
# تحذير: هذا السكريبت يساعد في إعداد النظام بشكل آمن للإنتاج

param(
    [switch]$Production,
    [switch]$Development,
    [switch]$CheckOnly
)

Write-Host "========================================" -ForegroundColor Yellow
Write-Host "Medical Lab Analyzer Security Setup" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

# Check current security status
function Check-SecurityStatus {
    Write-Host "🔍 فحص الحالة الأمنية الحالية..." -ForegroundColor Cyan
    
    $securityIssues = @()
    
    # Check for test data in database seeder
    $seederPath = "src/MedicalLabAnalyzer/Data/DatabaseSeeder.cs"
    if (Test-Path $seederPath) {
        $seederContent = Get-Content $seederPath -Raw
        if ($seederContent -match "Admin@123" -or $seederContent -match "Password@123") {
            $securityIssues += "❌ كلمات مرور افتراضية موجودة في DatabaseSeeder"
        }
    }
    
    # Check for hardcoded credentials in AuthService
    $authPath = "src/MedicalLabAnalyzer/Services/AuthService.cs"
    if (Test-Path $authPath) {
        $authContent = Get-Content $authPath -Raw
        if ($authContent -match "_fallbackUsers") {
            $securityIssues += "⚠️ مستخدمون احتياطيون موجودون - تأكد من تأمينهم"
        }
    }
    
    # Check for test credentials in UI
    $loginPath = "src/MedicalLabAnalyzer/Views/LoginView.xaml.cs"
    if (Test-Path $loginPath) {
        $loginContent = Get-Content $loginPath -Raw
        if ($loginContent -match "admin" -and $loginContent -match "password") {
            $securityIssues += "❌ بيانات تجريبية في واجهة تسجيل الدخول"
        } else {
            Write-Host "✅ تم إزالة البيانات التجريبية من واجهة تسجيل الدخول" -ForegroundColor Green
        }
    }
    
    # Check database file
    if (Test-Path "src/MedicalLabAnalyzer/Database/medical_lab.db") {
        $securityIssues += "⚠️ قاعدة بيانات موجودة - تأكد من عدم وجود بيانات تجريبية"
    }
    
    return $securityIssues
}

# Generate secure passwords
function Generate-SecurePassword {
    $length = 12
    $chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*"
    $password = ""
    for ($i = 0; $i -lt $length; $i++) {
        $password += $chars[(Get-Random -Maximum $chars.Length)]
    }
    return $password
}

# Production setup
function Setup-Production {
    Write-Host "🔐 إعداد النظام للإنتاج..." -ForegroundColor Red
    Write-Host ""
    
    Write-Host "تحذير هام: " -ForegroundColor Red -NoNewline
    Write-Host "أنت تقوم بإعداد النظام للإنتاج. هذا سيؤثر على الأمان والبيانات." -ForegroundColor Yellow
    $confirm = Read-Host "هل تريد المتابعة؟ (y/N)"
    
    if ($confirm -ne "y" -and $confirm -ne "Y") {
        Write-Host "تم الإلغاء بواسطة المستخدم." -ForegroundColor Yellow
        return
    }
    
    # Generate secure passwords
    $adminPassword = Generate-SecurePassword
    $emergencyPassword = Generate-SecurePassword
    
    Write-Host ""
    Write-Host "🔑 كلمات مرور آمنة تم إنشاؤها:" -ForegroundColor Green
    Write-Host "Admin Password: " -NoNewline -ForegroundColor Yellow
    Write-Host $adminPassword -ForegroundColor Red
    Write-Host "Emergency Password: " -NoNewline -ForegroundColor Yellow
    Write-Host $emergencyPassword -ForegroundColor Red
    Write-Host ""
    Write-Host "⚠️ احفظ هذه كلمات المرور في مكان آمن!" -ForegroundColor Red
    Write-Host ""
    
    # Create production config
    $productionConfig = @{
        "Database" = @{
            "ConnectionString" = "Data Source=Database/production.db;Version=3;Password=$emergencyPassword"
        }
        "Security" = @{
            "RequireStrongPasswords" = $true
            "MinPasswordLength" = 8
            "RequirePasswordChange" = $true
            "SessionTimeout" = 30
        }
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Warning"
                "Microsoft" = "Error"
                "MedicalLabAnalyzer" = "Information"
            }
            "AuditLogging" = $true
        }
        "Production" = @{
            "Environment" = "Production"
            "DebugMode" = $false
            "SeedDatabase" = $false
        }
    }
    
    $configJson = $productionConfig | ConvertTo-Json -Depth 4
    $configJson | Out-File "src/MedicalLabAnalyzer/appsettings.Production.json" -Encoding UTF8
    
    Write-Host "✅ تم إنشاء ملف إعدادات الإنتاج" -ForegroundColor Green
    
    # Create security checklist
    Write-Host ""
    Write-Host "📋 قائمة فحص الأمان:" -ForegroundColor Cyan
    Write-Host "□ تغيير كلمات المرور الافتراضية" -ForegroundColor Yellow
    Write-Host "□ حذف البيانات التجريبية" -ForegroundColor Yellow
    Write-Host "□ تشفير قاعدة البيانات" -ForegroundColor Yellow
    Write-Host "□ إعداد النسخ الاحتياطية" -ForegroundColor Yellow
    Write-Host "□ اختبار النظام" -ForegroundColor Yellow
    Write-Host "□ مراجعة السجلات الأمنية" -ForegroundColor Yellow
}

# Development setup
function Setup-Development {
    Write-Host "🛠️ إعداد النظام للتطوير..." -ForegroundColor Blue
    
    $devConfig = @{
        "Database" = @{
            "ConnectionString" = "Data Source=Database/development.db;Version=3;"
        }
        "Security" = @{
            "RequireStrongPasswords" = $false
            "MinPasswordLength" = 4
            "RequirePasswordChange" = $false
            "SessionTimeout" = 480
        }
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Debug"
                "Microsoft" = "Information"
                "MedicalLabAnalyzer" = "Debug"
            }
            "AuditLogging" = $true
        }
        "Development" = @{
            "Environment" = "Development"
            "DebugMode" = $true
            "SeedDatabase" = $true
        }
    }
    
    $configJson = $devConfig | ConvertTo-Json -Depth 4
    $configJson | Out-File "src/MedicalLabAnalyzer/appsettings.Development.json" -Encoding UTF8
    
    Write-Host "✅ تم إنشاء ملف إعدادات التطوير" -ForegroundColor Green
    Write-Host ""
    Write-Host "⚠️ تذكر: هذا للتطوير فقط - لا تستخدم في الإنتاج!" -ForegroundColor Yellow
}

# Main execution
Write-Host "المعاملات المتاحة:" -ForegroundColor White
Write-Host "  -CheckOnly    : فحص الحالة الأمنية فقط" -ForegroundColor Gray
Write-Host "  -Development  : إعداد للتطوير" -ForegroundColor Gray
Write-Host "  -Production   : إعداد للإنتاج" -ForegroundColor Gray
Write-Host ""

# Check security status
$issues = Check-SecurityStatus

if ($issues.Count -eq 0) {
    Write-Host "✅ لا توجد مشاكل أمنية واضحة" -ForegroundColor Green
} else {
    Write-Host "⚠️ مشاكل أمنية موجودة:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  $issue" -ForegroundColor Yellow
    }
}

Write-Host ""

if ($CheckOnly) {
    Write-Host "تم الانتهاء من فحص الأمان." -ForegroundColor Cyan
    return
}

if ($Production) {
    Setup-Production
} elseif ($Development) {
    Setup-Development
} else {
    Write-Host "اختر نوع الإعداد:" -ForegroundColor White
    Write-Host "1. التطوير (Development)" -ForegroundColor Gray
    Write-Host "2. الإنتاج (Production)" -ForegroundColor Gray
    Write-Host "3. فحص فقط (Check Only)" -ForegroundColor Gray
    
    $choice = Read-Host "اختر (1/2/3)"
    
    switch ($choice) {
        "1" { Setup-Development }
        "2" { Setup-Production }
        "3" { Write-Host "انتهى الفحص." -ForegroundColor Cyan }
        default { Write-Host "اختيار غير صحيح." -ForegroundColor Red }
    }
}

Write-Host ""
Write-Host "📚 للمزيد من المعلومات، راجع ملف SECURITY_WARNING.md" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Yellow