# MedicalLabAnalyzer - Advanced Medical Laboratory Management System
# Complete Build and Deployment Script
# Version: 1.0.0
# Author: Medical Lab Solutions

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipTests,
    [switch]$CreateInstaller,
    [switch]$CreateZip,
    [switch]$DeployOnly
)

# Configuration
$ProjectName = "MedicalLabAnalyzer"
$SolutionFile = "$ProjectName.sln"
$ProjectFile = "$ProjectName.csproj"
$Version = "1.0.0"
$Company = "Medical Lab Solutions"
$ProductName = "MedicalLabAnalyzer - نظام إدارة المختبرات الطبية المتقدم"

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"
$Magenta = "Magenta"
$White = "White"

# Banner
Write-Host "==================================================================" -ForegroundColor $Magenta
Write-Host "$ProductName" -ForegroundColor $Magenta
Write-Host "Complete Build and Deployment Script v$Version" -ForegroundColor $Magenta
Write-Host "==================================================================" -ForegroundColor $Magenta
Write-Host ""

# Display parameters
Write-Host "🔧 Build Configuration:" -ForegroundColor $Cyan
Write-Host "   Configuration: $Configuration" -ForegroundColor $White
Write-Host "   Runtime: $Runtime" -ForegroundColor $White
Write-Host "   Skip Tests: $SkipTests" -ForegroundColor $White
Write-Host "   Create Installer: $CreateInstaller" -ForegroundColor $White
Write-Host "   Create ZIP: $CreateZip" -ForegroundColor $White
Write-Host "   Deploy Only: $DeployOnly" -ForegroundColor $White
Write-Host ""

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor $Cyan

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor $Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor $Red
    exit 1
}

# Check if solution file exists
if (-not (Test-Path $SolutionFile)) {
    Write-Host "❌ Solution file not found: $SolutionFile" -ForegroundColor $Red
    exit 1
}

Write-Host "✅ Solution file found: $SolutionFile" -ForegroundColor $Green
Write-Host ""

# Create directories
Write-Host "📁 Creating directories..." -ForegroundColor $Cyan
$directories = @(
    "publish",
    "dist",
    "Database",
    "Reports",
    "Reports/Archive",
    "Reports/Templates",
    "Resources",
    "logs"
)

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
        Write-Host "   Created: $dir" -ForegroundColor $White
    }
}
Write-Host ""

# Clean previous builds
Write-Host "🧹 Cleaning previous builds..." -ForegroundColor $Cyan
dotnet clean --configuration $Configuration --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️ Clean failed, continuing..." -ForegroundColor $Yellow
}
Write-Host "✅ Clean completed" -ForegroundColor $Green
Write-Host ""

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor $Cyan
dotnet restore --configuration $Configuration --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed" -ForegroundColor $Red
    exit 1
}
Write-Host "✅ Packages restored" -ForegroundColor $Green
Write-Host ""

# Build the solution
Write-Host "🔨 Building solution..." -ForegroundColor $Cyan
dotnet build --configuration $Configuration --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor $Red
    exit 1
}
Write-Host "✅ Build completed" -ForegroundColor $Green
Write-Host ""

# Run tests (unless skipped)
if (-not $SkipTests) {
    Write-Host "🧪 Running tests..." -ForegroundColor $Cyan
    dotnet test --configuration $Configuration --verbosity quiet --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Some tests failed" -ForegroundColor $Yellow
    } else {
        Write-Host "✅ All tests passed" -ForegroundColor $Green
    }
    Write-Host ""
}

# Publish the application
Write-Host "📤 Publishing application..." -ForegroundColor $Cyan
$publishPath = "publish/$Runtime"
$publishArgs = @(
    "publish",
    "--configuration", $Configuration,
    "--runtime", $Runtime,
    "--self-contained", "true",
    "--output", $publishPath,
    "--verbosity", "quiet"
)

dotnet $publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Publishing failed" -ForegroundColor $Red
    exit 1
}
Write-Host "✅ Application published to: $publishPath" -ForegroundColor $Green
Write-Host ""

# Copy additional files
Write-Host "📋 Copying additional files..." -ForegroundColor $Cyan

# Copy database files
if (Test-Path "Database") {
    Copy-Item -Path "Database/*" -Destination "$publishPath/Database/" -Recurse -Force
    Write-Host "   Database files copied" -ForegroundColor $White
}

# Copy report templates
if (Test-Path "Reports/Templates") {
    Copy-Item -Path "Reports/Templates/*" -Destination "$publishPath/Reports/Templates/" -Recurse -Force
    Write-Host "   Report templates copied" -ForegroundColor $White
}

# Copy resources
if (Test-Path "Resources") {
    Copy-Item -Path "Resources/*" -Destination "$publishPath/Resources/" -Recurse -Force
    Write-Host "   Resources copied" -ForegroundColor $White
}

# Create appsettings.json if not exists
$appSettingsPath = "$publishPath/appsettings.json"
if (-not (Test-Path $appSettingsPath)) {
    $appSettings = @{
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Information"
                "Microsoft" = "Warning"
            }
        }
        "Database" = @{
            "ConnectionString" = "Data Source=Database/medical_lab.db;Version=3;"
        }
        "Reports" = @{
            "OutputPath" = "Reports"
            "ArchivePath" = "Reports/Archive"
        }
    } | ConvertTo-Json -Depth 10
    
    $appSettings | Out-File -FilePath $appSettingsPath -Encoding UTF8
    Write-Host "   appsettings.json created" -ForegroundColor $White
}

Write-Host "✅ Additional files copied" -ForegroundColor $Green
Write-Host ""

# Create deployment package
if ($CreateZip -or $CreateInstaller) {
    Write-Host "📦 Creating deployment package..." -ForegroundColor $Cyan
    
    $distPath = "dist"
    $packageName = "${ProjectName}_v${Version}_${Runtime}"
    $packagePath = "$distPath/$packageName"
    
    # Create package directory
    if (Test-Path $packagePath) {
        Remove-Item -Path $packagePath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $packagePath -Force | Out-Null
    
    # Copy published files
    Copy-Item -Path "$publishPath/*" -Destination $packagePath -Recurse -Force
    
    # Create README for package
    $readmeContent = @"
# MedicalLabAnalyzer v$Version

## نظام إدارة المختبرات الطبية المتقدم

### Installation Instructions - تعليمات التثبيت

1. Extract all files to a folder
2. Run MedicalLabAnalyzer.exe as Administrator
3. Default credentials:
   - Username: admin
   - Password: admin

### System Requirements - متطلبات النظام
- Windows 10/11 (x64)
- .NET 8.0 Runtime (included)
- Minimum 4GB RAM
- 2GB free disk space

### Features - المميزات
- Complete patient management
- Advanced medical analysis (CASA, CBC, Urine, Stool)
- Professional reporting system
- Arabic RTL interface
- Offline operation
- Comprehensive audit logging

### Support - الدعم
For support and documentation, visit the project repository.

---
Medical Lab Solutions - $((Get-Date).Year)
"@
    
    $readmeContent | Out-File -FilePath "$packagePath/README.txt" -Encoding UTF8
    
    # Create ZIP file
    if ($CreateZip) {
        $zipPath = "$distPath/${packageName}.zip"
        if (Test-Path $zipPath) {
            Remove-Item -Path $zipPath -Force
        }
        
        Compress-Archive -Path "$packagePath/*" -DestinationPath $zipPath -Force
        Write-Host "✅ ZIP package created: $zipPath" -ForegroundColor $Green
    }
    
    # Create installer (if WiX is available)
    if ($CreateInstaller) {
        Write-Host "🔧 Creating installer..." -ForegroundColor $Cyan
        # Note: This requires WiX Toolset to be installed
        # For now, we'll create a simple batch installer
        $installerContent = @"
@echo off
echo Installing MedicalLabAnalyzer v$Version...
echo.

REM Create program files directory
set "INSTALL_DIR=%ProgramFiles%\$Company\$ProjectName"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Copy files
xcopy /E /I /Y "%~dp0*" "%INSTALL_DIR%"

REM Create desktop shortcut
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT=%DESKTOP%\$ProjectName.lnk"

powershell -Command "`$WshShell = New-Object -comObject WScript.Shell; `$Shortcut = `$WshShell.CreateShortcut('%SHORTCUT%'); `$Shortcut.TargetPath = '%INSTALL_DIR%\MedicalLabAnalyzer.exe'; `$Shortcut.WorkingDirectory = '%INSTALL_DIR%'; `$Shortcut.Save()"

echo Installation completed!
echo.
pause
"@
        
        $installerContent | Out-File -FilePath "$packagePath/install.bat" -Encoding ASCII
        Write-Host "✅ Installer script created" -ForegroundColor $Green
    }
    
    Write-Host "✅ Deployment package created in: $packagePath" -ForegroundColor $Green
    Write-Host ""
}

# Final summary
Write-Host "==================================================================" -ForegroundColor $Magenta
Write-Host "🎉 Build and Deployment Completed Successfully!" -ForegroundColor $Green
Write-Host "==================================================================" -ForegroundColor $Magenta
Write-Host ""
Write-Host "📁 Published application: $publishPath" -ForegroundColor $White
if ($CreateZip) {
    Write-Host "📦 ZIP package: dist/${packageName}.zip" -ForegroundColor $White
}
if ($CreateInstaller) {
    Write-Host "🔧 Installer: dist/$packageName/install.bat" -ForegroundColor $White
}
Write-Host ""
Write-Host "🚀 Next steps:" -ForegroundColor $Cyan
Write-Host "   1. Test the application: $publishPath/MedicalLabAnalyzer.exe" -ForegroundColor $White
Write-Host "   2. Distribute the package to target systems" -ForegroundColor $White
Write-Host "   3. Run as Administrator for full functionality" -ForegroundColor $White
Write-Host ""
Write-Host "📞 For support, check the project documentation" -ForegroundColor $Yellow
Write-Host "==================================================================" -ForegroundColor $Magenta