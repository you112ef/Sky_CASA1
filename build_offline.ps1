# Medical Lab Analyzer - Offline Build Script
# This script builds the application in an offline environment with all dependencies

param(
    [string]$Configuration = "Release",
    [string]$Platform = "AnyCPU",
    [string]$OutputPath = ".\install",
    [switch]$Clean,
    [switch]$SkipTests,
    [switch]$CreateInstaller
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Colors for output
$Colors = @{
    Info = "Cyan"
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
}

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Colors[$Color]
}

function Test-Command {
    param([string]$Command)
    try {
        Get-Command $Command -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Test-DotNet {
    if (-not (Test-Command "dotnet")) {
        Write-ColorOutput "❌ .NET SDK not found. Please install .NET 8.0 SDK." "Error"
        exit 1
    }
    
    $version = dotnet --version
    Write-ColorOutput "✅ .NET SDK found: $version" "Success"
}

function Test-NuGet {
    if (-not (Test-Command "nuget")) {
        Write-ColorOutput "⚠️  NuGet CLI not found. Installing..." "Warning"
        
        # Download NuGet CLI
        $nugetUrl = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
        $nugetPath = ".\tools\nuget.exe"
        
        if (-not (Test-Path ".\tools")) {
            New-Item -ItemType Directory -Path ".\tools" | Out-Null
        }
        
        Invoke-WebRequest -Uri $nugetUrl -OutFile $nugetPath
        $env:PATH += ";$PWD\tools"
        
        Write-ColorOutput "✅ NuGet CLI installed" "Success"
    }
    else {
        Write-ColorOutput "✅ NuGet CLI found" "Success"
    }
}

function Initialize-Environment {
    Write-ColorOutput "🔧 Initializing build environment..." "Info"
    
    # Create necessary directories
    $directories = @(
        ".\tools",
        ".\packages",
        ".\local-nuget",
        ".\Database",
        ".\Samples",
        $OutputPath
    )
    
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir | Out-Null
            Write-ColorOutput "   Created directory: $dir" "Info"
        }
    }
    
    # Create local NuGet feed
    if (-not (Test-Path ".\local-nuget\nuget.config")) {
        @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="Local" value=".\local-nuget" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@ | Out-File -FilePath ".\local-nuget\nuget.config" -Encoding UTF8
    }
}

function Download-Packages {
    Write-ColorOutput "📦 Downloading NuGet packages..." "Info"
    
    # Create packages.config for offline download
    $packagesConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <!-- WPF and UI Dependencies -->
  <package id="Microsoft.Xaml.Behaviors.Wpf" version="1.1.77" targetFramework="net8.0-windows" />
  <package id="MaterialDesignThemes" version="4.9.0" targetFramework="net8.0-windows" />
  <package id="MaterialDesignColors" version="2.1.4" targetFramework="net8.0-windows" />
  
  <!-- EmguCV for Video Analysis -->
  <package id="Emgu.CV" version="4.8.1.5350" targetFramework="net8.0-windows" />
  <package id="Emgu.CV.runtime.windows" version="4.8.1.5350" targetFramework="net8.0-windows" />
  
  <!-- Database and ORM -->
  <package id="System.Data.SQLite.Core" version="1.0.118" targetFramework="net8.0-windows" />
  <package id="Dapper" version="2.1.15" targetFramework="net8.0-windows" />
  
  <!-- Password Hashing -->
  <package id="BCrypt.Net-Next" version="4.0.3" targetFramework="net8.0-windows" />
  
  <!-- PDF Generation -->
  <package id="PdfSharp-MigraDoc" version="1.50.5147" targetFramework="net8.0-windows" />
  
  <!-- JSON Support -->
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net8.0-windows" />
  
  <!-- Logging and Configuration -->
  <package id="Microsoft.Extensions.Logging" version="8.0.0" targetFramework="net8.0-windows" />
  <package id="Microsoft.Extensions.Configuration" version="8.0.0" targetFramework="net8.0-windows" />
  <package id="Microsoft.Extensions.Configuration.Json" version="8.0.0" targetFramework="net8.0-windows" />
  <package id="Serilog" version="3.1.1" targetFramework="net8.0-windows" />
  <package id="Serilog.Extensions.Logging" version="8.0.0" targetFramework="net8.0-windows" />
  <package id="Serilog.Sinks.File" version="5.0.0" targetFramework="net8.0-windows" />
  
  <!-- Validation and Utilities -->
  <package id="FluentValidation" version="11.8.1" targetFramework="net8.0-windows" />
  <package id="AutoMapper" version="12.0.1" targetFramework="net8.0-windows" />
  
  <!-- Excel Export -->
  <package id="EPPlus" version="7.0.5" targetFramework="net8.0-windows" />
  
  <!-- MVVM Framework -->
  <package id="CommunityToolkit.Mvvm" version="8.2.2" targetFramework="net8.0-windows" />
  
  <!-- CSV Helper for Export -->
  <package id="CsvHelper" version="30.0.1" targetFramework="net8.0-windows" />
</packages>
"@
    
    $packagesConfig | Out-File -FilePath ".\packages\packages.config" -Encoding UTF8
    
    # Download packages
    nuget restore ".\packages\packages.config" -PackagesDirectory ".\packages" -ConfigFile ".\nuget.config"
    
    Write-ColorOutput "✅ Packages downloaded successfully" "Success"
}

function Build-Project {
    Write-ColorOutput "🔨 Building project..." "Info"
    
    if ($Clean) {
        Write-ColorOutput "   Cleaning previous build..." "Info"
        dotnet clean --configuration $Configuration
    }
    
    # Restore packages
    Write-ColorOutput "   Restoring packages..." "Info"
    dotnet restore --configfile ".\nuget.config"
    
    # Build project
    Write-ColorOutput "   Building with configuration: $Configuration" "Info"
    dotnet build --configuration $Configuration --no-restore
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ Build failed!" "Error"
        exit 1
    }
    
    Write-ColorOutput "✅ Build completed successfully" "Success"
}

function Run-Tests {
    if ($SkipTests) {
        Write-ColorOutput "⏭️  Skipping tests" "Warning"
        return
    }
    
    Write-ColorOutput "🧪 Running tests..." "Info"
    
    # Run unit tests
    dotnet test --configuration $Configuration --no-build --verbosity normal
    
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput "❌ Tests failed!" "Error"
        exit 1
    }
    
    Write-ColorOutput "✅ All tests passed" "Success"
}

function Create-Database {
    Write-ColorOutput "🗄️  Initializing database..." "Info"
    
    $dbPath = ".\Database\medical_lab.db"
    
    # Create database directory if it doesn't exist
    if (-not (Test-Path ".\Database")) {
        New-Item -ItemType Directory -Path ".\Database" | Out-Null
    }
    
    # Initialize database using the application
    $exePath = ".\src\MedicalLabAnalyzer\bin\$Configuration\net8.0-windows\MedicalLabAnalyzer.exe"
    
    if (Test-Path $exePath) {
        Write-ColorOutput "   Database will be initialized on first run" "Info"
    }
    else {
        Write-ColorOutput "⚠️  Application executable not found. Database will be created on first run." "Warning"
    }
}

function Copy-Files {
    Write-ColorOutput "📋 Copying files to output directory..." "Info"
    
    $sourceDir = ".\src\MedicalLabAnalyzer\bin\$Configuration\net8.0-windows"
    $targetDir = $OutputPath
    
    if (-not (Test-Path $sourceDir)) {
        Write-ColorOutput "❌ Build output not found: $sourceDir" "Error"
        exit 1
    }
    
    # Copy application files
    Copy-Item -Path "$sourceDir\*" -Destination $targetDir -Recurse -Force
    
    # Copy additional files
    $additionalFiles = @(
        ".\README.md",
        ".\LICENSE",
        ".\CHANGELOG.txt"
    )
    
    foreach ($file in $additionalFiles) {
        if (Test-Path $file) {
            Copy-Item -Path $file -Destination $targetDir -Force
        }
    }
    
    # Create Samples directory
    $samplesDir = Join-Path $targetDir "Samples"
    if (-not (Test-Path $samplesDir)) {
        New-Item -ItemType Directory -Path $samplesDir | Out-Null
    }
    
    # Create placeholder for sample video
    $sampleVideoPath = Join-Path $samplesDir "sperm_sample.mp4"
    if (-not (Test-Path $sampleVideoPath)) {
        @"
# Sample Video File
# Place your sperm analysis video file here
# Supported formats: MP4, AVI, MOV
# Recommended: 25-30 FPS, 640x480 or higher resolution
"@ | Out-File -FilePath "$sampleVideoPath.txt" -Encoding UTF8
    }
    
    Write-ColorOutput "✅ Files copied successfully" "Success"
}

function Create-Installer {
    if (-not $CreateInstaller) {
        return
    }
    
    Write-ColorOutput "📦 Creating installer..." "Info"
    
    # Create NSIS script for installer
    $nsisScript = @"
!include "MUI2.nsh"

Name "Medical Lab Analyzer"
OutFile "MedicalLabAnalyzer-Setup.exe"
InstallDir "$PROGRAMFILES\Medical Lab Analyzer"
RequestExecutionLevel admin

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

Section "Install"
    SetOutPath "$INSTDIR"
    File /r "$OutputPath\*"
    
    CreateDirectory "$SMPROGRAMS\Medical Lab Analyzer"
    CreateShortCut "$SMPROGRAMS\Medical Lab Analyzer\Medical Lab Analyzer.lnk" "$INSTDIR\MedicalLabAnalyzer.exe"
    CreateShortCut "$DESKTOP\Medical Lab Analyzer.lnk" "$INSTDIR\MedicalLabAnalyzer.exe"
    
    WriteUninstaller "$INSTDIR\Uninstall.exe"
    
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MedicalLabAnalyzer" "DisplayName" "Medical Lab Analyzer"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MedicalLabAnalyzer" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MedicalLabAnalyzer" "DisplayIcon" "$INSTDIR\MedicalLabAnalyzer.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MedicalLabAnalyzer" "Publisher" "Medical Lab Solutions"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MedicalLabAnalyzer" "DisplayVersion" "1.0.0"
SectionEnd

Section "Uninstall"
    Delete "$SMPROGRAMS\Medical Lab Analyzer\Medical Lab Analyzer.lnk"
    RMDir "$SMPROGRAMS\Medical Lab Analyzer"
    Delete "$DESKTOP\Medical Lab Analyzer.lnk"
    
    RMDir /r "$INSTDIR"
    
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MedicalLabAnalyzer"
SectionEnd
"@
    
    $nsisScript | Out-File -FilePath ".\installer.nsi" -Encoding UTF8
    
    # Check if NSIS is available
    if (Test-Command "makensis") {
        makensis installer.nsi
        Write-ColorOutput "✅ Installer created: MedicalLabAnalyzer-Setup.exe" "Success"
    }
    else {
        Write-ColorOutput "⚠️  NSIS not found. Installer script created: installer.nsi" "Warning"
    }
}

function Create-OfflinePackage {
    Write-ColorOutput "📦 Creating offline package..." "Info"
    
    $packageName = "MedicalLabAnalyzer-Offline-$Configuration"
    $packagePath = ".\$packageName"
    
    if (Test-Path $packagePath) {
        Remove-Item -Path $packagePath -Recurse -Force
    }
    
    # Create package structure
    New-Item -ItemType Directory -Path $packagePath | Out-Null
    New-Item -ItemType Directory -Path "$packagePath\Application" | Out-Null
    New-Item -ItemType Directory -Path "$packagePath\Packages" | Out-Null
    New-Item -ItemType Directory -Path "$packagePath\Tools" | Out-Null
    
    # Copy application
    Copy-Item -Path "$OutputPath\*" -Destination "$packagePath\Application" -Recurse -Force
    
    # Copy packages
    Copy-Item -Path ".\packages\*" -Destination "$packagePath\Packages" -Recurse -Force
    
    # Copy tools
    if (Test-Path ".\tools") {
        Copy-Item -Path ".\tools\*" -Destination "$packagePath\Tools" -Recurse -Force
    }
    
    # Create installation script
    $installScript = @"
@echo off
echo Medical Lab Analyzer - Offline Installation
echo ===========================================

echo Installing packages...
nuget restore ".\Packages\packages.config" -PackagesDirectory ".\Packages"

echo Copying application...
xcopy ".\Application\*" "%PROGRAMFILES%\Medical Lab Analyzer\" /E /I /Y

echo Creating shortcuts...
mklink "%USERPROFILE%\Desktop\Medical Lab Analyzer.lnk" "%PROGRAMFILES%\Medical Lab Analyzer\MedicalLabAnalyzer.exe"

echo Installation complete!
pause
"@
    
    $installScript | Out-File -FilePath "$packagePath\install.bat" -Encoding ASCII
    
    # Create README
    $readme = @"
Medical Lab Analyzer - Offline Package
=====================================

This package contains everything needed to install Medical Lab Analyzer in an offline environment.

Contents:
- Application: Complete application files
- Packages: All required NuGet packages
- Tools: Build and installation tools

Installation:
1. Run install.bat as Administrator
2. The application will be installed to Program Files
3. A desktop shortcut will be created

System Requirements:
- Windows 10 or later
- .NET 8.0 Runtime
- 4GB RAM minimum
- 2GB free disk space

For support, contact: support@medicallabsolutions.com
"@
    
    $readme | Out-File -FilePath "$packagePath\README.txt" -Encoding UTF8
    
    # Create ZIP archive
    $zipPath = "$packageName.zip"
    if (Test-Path $zipPath) {
        Remove-Item -Path $zipPath -Force
    }
    
    Compress-Archive -Path $packagePath -DestinationPath $zipPath
    
    Write-ColorOutput "✅ Offline package created: $zipPath" "Success"
}

function Show-Summary {
    Write-ColorOutput "`n🎉 Build Summary" "Success"
    Write-ColorOutput "===============" "Success"
    Write-ColorOutput "Configuration: $Configuration" "Info"
    Write-ColorOutput "Platform: $Platform" "Info"
    Write-ColorOutput "Output Directory: $OutputPath" "Info"
    Write-ColorOutput "Clean Build: $Clean" "Info"
    Write-ColorOutput "Tests Skipped: $SkipTests" "Info"
    Write-ColorOutput "Installer Created: $CreateInstaller" "Info"
    
    Write-ColorOutput "`n📁 Generated Files:" "Info"
    Write-ColorOutput "   Application: $OutputPath" "Info"
    if ($CreateInstaller) {
        Write-ColorOutput "   Installer: MedicalLabAnalyzer-Setup.exe" "Info"
    }
    Write-ColorOutput "   Offline Package: MedicalLabAnalyzer-Offline-$Configuration.zip" "Info"
    
    Write-ColorOutput "`n🚀 Next Steps:" "Info"
    Write-ColorOutput "   1. Test the application: $OutputPath\MedicalLabAnalyzer.exe" "Info"
    Write-ColorOutput "   2. Place sample videos in: $OutputPath\Samples\" "Info"
    Write-ColorOutput "   3. Configure calibration settings" "Info"
    Write-ColorOutput "   4. Run CASA analysis tests" "Info"
}

# Main execution
try {
    Write-ColorOutput "🏥 Medical Lab Analyzer - Offline Build Script" "Success"
    Write-ColorOutput "===============================================" "Success"
    Write-ColorOutput "Configuration: $Configuration" "Info"
    Write-ColorOutput "Platform: $Platform" "Info"
    Write-ColorOutput "Output Path: $OutputPath" "Info"
    Write-ColorOutput ""
    
    Test-DotNet
    Test-NuGet
    Initialize-Environment
    Download-Packages
    Build-Project
    Run-Tests
    Create-Database
    Copy-Files
    Create-Installer
    Create-OfflinePackage
    Show-Summary
    
    Write-ColorOutput "`n✅ Build completed successfully!" "Success"
}
catch {
    Write-ColorOutput "`n❌ Build failed: $($_.Exception.Message)" "Error"
    Write-ColorOutput "Stack trace: $($_.ScriptStackTrace)" "Error"
    exit 1
}