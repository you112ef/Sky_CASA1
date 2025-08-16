# Medical Lab Analyzer - Offline Build Script
# This script builds and packages the application without internet connection

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipTests,
    [switch]$CreateInstaller
)

Write-Host "=== Medical Lab Analyzer - Offline Build Script ===" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Runtime: $Runtime" -ForegroundColor Yellow
Write-Host "Skip Tests: $SkipTests" -ForegroundColor Yellow
Write-Host "Create Installer: $CreateInstaller" -ForegroundColor Yellow
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Cyan

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Check if required packages are available locally
$nugetPackages = @(
    "Emgu.CV",
    "Emgu.CV.runtime.windows", 
    "System.Data.SQLite.Core",
    "Dapper",
    "BCrypt.Net-Next",
    "PdfSharp-MigraDoc",
    "Newtonsoft.Json"
)

Write-Host "Checking NuGet packages..." -ForegroundColor Cyan
foreach ($package in $nugetPackages) {
    $packagePath = "$env:USERPROFILE\.nuget\packages\$package"
    if (Test-Path $packagePath) {
        Write-Host "✓ $package found" -ForegroundColor Green
    } else {
        Write-Host "✗ $package not found locally" -ForegroundColor Red
        Write-Host "  Please download packages while online or use local NuGet feed" -ForegroundColor Yellow
    }
}

Write-Host ""

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Cyan
dotnet clean src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "Warning: Clean failed, continuing..." -ForegroundColor Yellow
}

# Restore packages (will use local cache if offline)
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Package restore failed" -ForegroundColor Red
    exit 1
}

# Build the solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}

# Run tests (unless skipped)
if (-not $SkipTests) {
    Write-Host "Running tests..." -ForegroundColor Cyan
    dotnet test tests/MedicalLabAnalyzer.Tests/MedicalLabAnalyzer.Tests.csproj
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Warning: Some tests failed" -ForegroundColor Yellow
    }
}

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Cyan
$publishPath = "publish/$Runtime"
dotnet publish src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj `
    -c $Configuration `
    -r $Runtime `
    -p:PublishSingleFile=true `
    --self-contained true `
    -o $publishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Publishing failed" -ForegroundColor Red
    exit 1
}

# Create deployment package
Write-Host "Creating deployment package..." -ForegroundColor Cyan
$deployPath = "deploy/MedicalLabAnalyzer"
if (Test-Path $deployPath) {
    Remove-Item $deployPath -Recurse -Force
}
New-Item -ItemType Directory -Path $deployPath -Force | Out-Null

# Copy published files
Copy-Item "$publishPath/*" $deployPath -Recurse -Force

# Copy additional files
if (Test-Path "Database") {
    Copy-Item "Database" $deployPath -Recurse -Force
}
if (Test-Path "src/MedicalLabAnalyzer/Reports") {
    Copy-Item "src/MedicalLabAnalyzer/Reports" $deployPath -Recurse -Force
}
if (Test-Path "src/MedicalLabAnalyzer/appsettings.json") {
    Copy-Item "src/MedicalLabAnalyzer/appsettings.json" $deployPath
}
Copy-Item "README.md" $deployPath
Copy-Item "LICENSE" $deployPath

# Create ZIP package
Write-Host "Creating ZIP package..." -ForegroundColor Cyan
$zipPath = "deploy/MedicalLabAnalyzer-v1.0.0-$Configuration.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path $deployPath -DestinationPath $zipPath -Force

# Create installer if requested
if ($CreateInstaller) {
    Write-Host "Creating installer..." -ForegroundColor Cyan
    if (Test-Path "install/MedicalLabAnalyzer.iss") {
        # Check if Inno Setup is available
        $innoPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
        if (Test-Path $innoPath) {
            & $innoPath "install/MedicalLabAnalyzer.iss"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Installer created successfully" -ForegroundColor Green
            } else {
                Write-Host "✗ Installer creation failed" -ForegroundColor Red
            }
        } else {
            Write-Host "✗ Inno Setup not found. Installer not created." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Inno Setup script not found" -ForegroundColor Yellow
    }
}

# Summary
Write-Host ""
Write-Host "=== Build Summary ===" -ForegroundColor Green
Write-Host "✓ Build completed successfully" -ForegroundColor Green
Write-Host ""
Write-Host "Output locations:" -ForegroundColor Cyan
Write-Host "  - Published: $publishPath" -ForegroundColor White
Write-Host "  - Deployment: $deployPath" -ForegroundColor White
Write-Host "  - ZIP Package: $zipPath" -ForegroundColor White
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  - Debug: dotnet run --project src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj" -ForegroundColor White
Write-Host "  - Release: $deployPath/MedicalLabAnalyzer.exe" -ForegroundColor White
Write-Host ""
Write-Host "Default credentials: admin / admin123" -ForegroundColor Yellow
Write-Host ""

# Check for potential issues
Write-Host "=== Potential Issues Check ===" -ForegroundColor Cyan

# Check for EmguCV runtime files
$emguFiles = @("opencv_videoio_ffmpeg*.dll", "opencv_world*.dll")
foreach ($pattern in $emguFiles) {
    $files = Get-ChildItem $deployPath -Name $pattern
    if ($files.Count -eq 0) {
        Write-Host "⚠ Warning: EmguCV runtime files not found ($pattern)" -ForegroundColor Yellow
        Write-Host "  These are required for video analysis functionality" -ForegroundColor Yellow
    }
}

# Check database
if (-not (Test-Path "$deployPath/Database/medical_lab.db")) {
    Write-Host "⚠ Warning: Database file not found" -ForegroundColor Yellow
    Write-Host "  Database will be created automatically on first run" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
=======
# ===================================================================
# MedicalLabAnalyzer - Advanced Medical Laboratory Management System
# Offline Build and Deployment Script
# Version: 1.0.0
# ===================================================================

param(
    [string]$Configuration = "Release",
    [string]$Platform = "AnyCPU",
    [switch]$CreateInstaller = $false,
    [switch]$CreateOfflinePackage = $true,
    [switch]$RunTests = $true,
    [switch]$CleanBuild = $false,
    [string]$OutputPath = ".\Dist"
)

# ===================================================================
# Configuration and Variables
# ===================================================================

$ProjectName = "MedicalLabAnalyzer"
$SolutionFile = "$ProjectName.sln"
$ProjectFile = "src\$ProjectName\$ProjectName.csproj"
$TargetFramework = "net8.0-windows"
$Version = "1.0.0"

# Colors for output
$Colors = @{
    Success = "Green"
    Error = "Red"
    Warning = "Yellow"
    Info = "Cyan"
    Header = "Magenta"
}

# ===================================================================
# Helper Functions
# ===================================================================

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Colors[$Color]
}

function Write-Header {
    param([string]$Message)
    Write-ColorOutput "`n" -Color Info
    Write-ColorOutput "=" * 80 -Color Header
    Write-ColorOutput " $Message" -Color Header
    Write-ColorOutput "=" * 80 -Color Header
    Write-ColorOutput "`n" -Color Info
}

function Write-Step {
    param([string]$Message)
    Write-ColorOutput "🔹 $Message" -Color Info
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✅ $Message" -Color Success
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "❌ $Message" -Color Error
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput "⚠️ $Message" -Color Warning
}

# ===================================================================
# Environment Validation
# ===================================================================

function Test-Environment {
    Write-Header "Environment Validation"
    
    # Check .NET SDK
    Write-Step "Checking .NET 8.0 SDK..."
    try {
        $dotnetVersion = dotnet --version
        if ($dotnetVersion -like "8.*") {
            Write-Success ".NET SDK $dotnetVersion found"
        } else {
            Write-Error ".NET 8.0 SDK not found. Please install .NET 8.0 SDK"
            exit 1
        }
    } catch {
        Write-Error ".NET SDK not found. Please install .NET 8.0 SDK"
        exit 1
    }
    
    # Check PowerShell version
    Write-Step "Checking PowerShell version..."
    $psVersion = $PSVersionTable.PSVersion
    if ($psVersion.Major -ge 5) {
        Write-Success "PowerShell $psVersion found"
    } else {
        Write-Warning "PowerShell 5.0 or higher recommended"
    }
    
    # Check available disk space
    Write-Step "Checking available disk space..."
    $drive = Get-WmiObject -Class Win32_LogicalDisk -Filter "DeviceID='C:'"
    $freeSpaceGB = [math]::Round($drive.FreeSpace / 1GB, 2)
    if ($freeSpaceGB -gt 2) {
        Write-Success "Available disk space: $freeSpaceGB GB"
    } else {
        Write-Warning "Low disk space: $freeSpaceGB GB (2 GB recommended)"
    }
}

# ===================================================================
# Clean and Restore
# ===================================================================

function Initialize-Build {
    Write-Header "Build Initialization"
    
    if ($CleanBuild) {
        Write-Step "Cleaning previous build artifacts..."
        dotnet clean --configuration $Configuration
        if (Test-Path $OutputPath) {
            Remove-Item $OutputPath -Recurse -Force
        }
        Write-Success "Clean completed"
    }
    
    Write-Step "Restoring NuGet packages..."
    dotnet restore --configuration $Configuration
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Package restore completed"
    } else {
        Write-Error "Package restore failed"
        exit 1
    }
}

# ===================================================================
# Build Process
# ===================================================================

function Build-Project {
    Write-Header "Building Project"
    
    Write-Step "Building solution with configuration: $Configuration"
    dotnet build --configuration $Configuration --no-restore
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build completed successfully"
    } else {
        Write-Error "Build failed"
        exit 1
    }
    
    # Verify build output
    $buildOutput = "src\$ProjectName\bin\$Configuration\$TargetFramework"
    if (Test-Path $buildOutput) {
        Write-Success "Build output verified at: $buildOutput"
    } else {
        Write-Error "Build output not found"
        exit 1
    }
}

# ===================================================================
# Testing
# ===================================================================

function Run-TestSuite {
    if (-not $RunTests) {
        Write-Warning "Skipping tests as requested"
        return
    }
    
    Write-Header "Running Test Suite"
    
    Write-Step "Running unit tests..."
    dotnet test --configuration $Configuration --no-build --verbosity normal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "All tests passed"
    } else {
        Write-Warning "Some tests failed - continuing with build"
    }
    
    # Run specific test suites
    Write-Step "Running CASA analysis tests..."
    dotnet test --configuration $Configuration --no-build --filter "CasaAnalysisRealTest"
    
    Write-Step "Running calibration tests..."
    dotnet test --configuration $Configuration --no-build --filter "CalibrationTest"
}

# ===================================================================
# Package Creation
# ===================================================================

function Create-DistributionPackage {
    Write-Header "Creating Distribution Package"
    
    # Create output directory
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    }
    
    $buildOutput = "src\$ProjectName\bin\$Configuration\$TargetFramework"
    $distPath = Join-Path $OutputPath "$ProjectName-v$Version"
    
    Write-Step "Creating distribution directory..."
    if (Test-Path $distPath) {
        Remove-Item $distPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $distPath -Force | Out-Null
    
    # Copy application files
    Write-Step "Copying application files..."
    Copy-Item "$buildOutput\*" $distPath -Recurse -Force
    
    # Copy additional resources
    Write-Step "Copying additional resources..."
    
    # Database
    if (Test-Path "Database") {
        Copy-Item "Database" $distPath -Recurse -Force
    }
    
    # Reports
    if (Test-Path "Reports") {
        Copy-Item "Reports" $distPath -Recurse -Force
    }
    
    # Samples
    if (Test-Path "Samples") {
        Copy-Item "Samples" $distPath -Recurse -Force
    }
    
    # Documentation
    $docs = @("README.md", "CHANGELOG.txt", "LICENSE")
    foreach ($doc in $docs) {
        if (Test-Path $doc) {
            Copy-Item $doc $distPath -Force
        }
    }
    
    # Create startup script
    Write-Step "Creating startup script..."
    $startupScript = @"
@echo off
echo Starting MedicalLabAnalyzer...
echo.
cd /d "%~dp0"
start "" "$ProjectName.exe"
"@
    $startupScript | Out-File -FilePath (Join-Path $distPath "Start.bat") -Encoding ASCII
    
    Write-Success "Distribution package created at: $distPath"
    return $distPath
}

# ===================================================================
# Installer Creation
# ===================================================================

function Create-Installer {
    param([string]$DistPath)
    
    if (-not $CreateInstaller) {
        Write-Warning "Skipping installer creation as requested"
        return
    }
    
    Write-Header "Creating Installer"
    
    # Check if NSIS is available
    $nsisPath = Get-Command makensis -ErrorAction SilentlyContinue
    if (-not $nsisPath) {
        Write-Warning "NSIS not found - skipping installer creation"
        Write-Warning "Install NSIS from: https://nsis.sourceforge.io/Download"
        return
    }
    
    Write-Step "Creating NSIS installer script..."
    $installerScript = @"
!include "MUI2.nsh"

; Basic settings
Name "MedicalLabAnalyzer"
OutFile "$OutputPath\$ProjectName-Setup-v$Version.exe"
InstallDir "`$PROGRAMFILES\$ProjectName"
InstallDirRegKey HKCU "Software\$ProjectName" ""

; Request application privileges
RequestExecutionLevel admin

; Interface settings
!define MUI_ABORTWARNING
!define MUI_ICON "src\$ProjectName\Resources\app.ico"
!define MUI_UNICON "src\$ProjectName\Resources\app.ico"

; Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE"
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

; Languages
!insertmacro MUI_LANGUAGE "English"

; Installation
Section "Install"
    SetOutPath "`$INSTDIR"
    File /r "$DistPath\*.*"
    
    ; Create uninstaller
    WriteUninstaller "`$INSTDIR\Uninstall.exe"
    
    ; Create start menu shortcut
    CreateDirectory "`$SMPROGRAMS\$ProjectName"
    CreateShortCut "`$SMPROGRAMS\$ProjectName\$ProjectName.lnk" "`$INSTDIR\$ProjectName.exe"
    CreateShortCut "`$SMPROGRAMS\$ProjectName\Uninstall.lnk" "`$INSTDIR\Uninstall.exe"
    
    ; Create desktop shortcut
    CreateShortCut "`$DESKTOP\$ProjectName.lnk" "`$INSTDIR\$ProjectName.exe"
    
    ; Registry information
    WriteRegStr HKCU "Software\$ProjectName" "" "`$INSTDIR"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\$ProjectName" "DisplayName" "$ProjectName"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\$ProjectName" "UninstallString" "`$INSTDIR\Uninstall.exe"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\$ProjectName" "DisplayVersion" "$Version"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\$ProjectName" "Publisher" "Medical Lab Solutions"
SectionEnd

; Uninstallation
Section "Uninstall"
    ; Remove files
    RMDir /r "`$INSTDIR"
    
    ; Remove shortcuts
    Delete "`$SMPROGRAMS\$ProjectName\$ProjectName.lnk"
    Delete "`$SMPROGRAMS\$ProjectName\Uninstall.lnk"
    RMDir "`$SMPROGRAMS\$ProjectName"
    Delete "`$DESKTOP\$ProjectName.lnk"
    
    ; Remove registry keys
    DeleteRegKey HKCU "Software\$ProjectName"
    DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\$ProjectName"
SectionEnd
"@
    
    $installerScriptPath = Join-Path $OutputPath "installer.nsi"
    $installerScript | Out-File -FilePath $installerScriptPath -Encoding ASCII
    
    Write-Step "Building installer with NSIS..."
    & makensis $installerScriptPath
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Installer created successfully"
    } else {
        Write-Error "Installer creation failed"
    }
}

# ===================================================================
# Offline Package Creation
# ===================================================================

function Create-OfflinePackage {
    param([string]$DistPath)
    
    if (-not $CreateOfflinePackage) {
        Write-Warning "Skipping offline package creation as requested"
        return
    }
    
    Write-Header "Creating Offline Package"
    
    Write-Step "Creating ZIP archive..."
    $zipPath = Join-Path $OutputPath "$ProjectName-Offline-v$Version.zip"
    
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    
    # Create ZIP using .NET
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($DistPath, $zipPath)
    
    if (Test-Path $zipPath) {
        $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
        Write-Success "Offline package created: $zipPath ($zipSize MB)"
    } else {
        Write-Error "Failed to create offline package"
    }
}

# ===================================================================
# Database Setup
# ===================================================================

function Initialize-Database {
    Write-Header "Database Initialization"
    
    Write-Step "Creating database directory..."
    $dbPath = "Database"
    if (-not (Test-Path $dbPath)) {
        New-Item -ItemType Directory -Path $dbPath -Force | Out-Null
    }
    
    Write-Step "Creating backup directory..."
    $backupPath = Join-Path $dbPath "Backup"
    if (-not (Test-Path $backupPath)) {
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
    }
    
    Write-Success "Database directories created"
}

# ===================================================================
# Reports Setup
# ===================================================================

function Initialize-Reports {
    Write-Header "Reports Initialization"
    
    Write-Step "Creating reports directory structure..."
    $reportsPath = "Reports"
    if (-not (Test-Path $reportsPath)) {
        New-Item -ItemType Directory -Path $reportsPath -Force | Out-Null
    }
    
    $subdirs = @("Templates", "Archive", "Output")
    foreach ($subdir in $subdirs) {
        $path = Join-Path $reportsPath $subdir
        if (-not (Test-Path $path)) {
            New-Item -ItemType Directory -Path $path -Force | Out-Null
        }
    }
    
    Write-Success "Reports directory structure created"
}

# ===================================================================
# Validation
# ===================================================================

function Test-BuildOutput {
    Write-Header "Build Output Validation"
    
    $distPath = Join-Path $OutputPath "$ProjectName-v$Version"
    
    Write-Step "Validating application files..."
    $requiredFiles = @(
        "$ProjectName.exe",
        "$ProjectName.dll",
        "System.Data.SQLite.dll",
        "Emgu.CV.dll"
    )
    
    foreach ($file in $requiredFiles) {
        $filePath = Join-Path $distPath $file
        if (Test-Path $filePath) {
            Write-Success "Found: $file"
        } else {
            Write-Warning "Missing: $file"
        }
    }
    
    Write-Step "Validating directories..."
    $requiredDirs = @("Database", "Reports", "Samples")
    foreach ($dir in $requiredDirs) {
        $dirPath = Join-Path $distPath $dir
        if (Test-Path $dirPath) {
            Write-Success "Found directory: $dir"
        } else {
            Write-Warning "Missing directory: $dir"
        }
    }
}

# ===================================================================
# Summary
# ===================================================================

function Show-BuildSummary {
    Write-Header "Build Summary"
    
    $distPath = Join-Path $OutputPath "$ProjectName-v$Version"
    $zipPath = Join-Path $OutputPath "$ProjectName-Offline-v$Version.zip"
    $installerPath = Join-Path $OutputPath "$ProjectName-Setup-v$Version.exe"
    
    Write-ColorOutput "🎉 Build completed successfully!" -Color Success
    Write-ColorOutput ""
    Write-ColorOutput "📦 Output Files:" -Color Info
    Write-ColorOutput "   • Distribution: $distPath" -Color Info
    if (Test-Path $zipPath) {
        $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
        Write-ColorOutput "   • Offline Package: $zipPath ($zipSize MB)" -Color Info
    }
    if (Test-Path $installerPath) {
        $installerSize = [math]::Round((Get-Item $installerPath).Length / 1MB, 2)
        Write-ColorOutput "   • Installer: $installerPath ($installerSize MB)" -Color Info
    }
    
    Write-ColorOutput ""
    Write-ColorOutput "🚀 Next Steps:" -Color Info
    Write-ColorOutput "   1. Test the application in the distribution folder" -Color Info
    Write-ColorOutput "   2. Run the installer on a clean system" -Color Info
    Write-ColorOutput "   3. Verify all features work correctly" -Color Info
    Write-ColorOutput "   4. Deploy to production environment" -Color Info
    
    Write-ColorOutput ""
    Write-ColorOutput "📋 Features Included:" -Color Info
    Write-ColorOutput "   ✅ Advanced CASA Analysis (Kalman + Hungarian)" -Color Success
    Write-ColorOutput "   ✅ CBC, Urine, Stool Analysis" -Color Success
    Write-ColorOutput "   ✅ Advanced Reporting System (PDF/Excel)" -Color Success
    Write-ColorOutput "   ✅ User Management & Authentication" -Color Success
    Write-ColorOutput "   ✅ Comprehensive AuditLog" -Color Success
    Write-ColorOutput "   ✅ Arabic RTL Support" -Color Success
    Write-ColorOutput "   ✅ Offline Operation" -Color Success
}

# ===================================================================
# Main Execution
# ===================================================================

function Main {
    Write-Header "MedicalLabAnalyzer v$Version - Advanced Medical Laboratory System"
    Write-ColorOutput "Build Configuration: $Configuration" -Color Info
    Write-ColorOutput "Platform: $Platform" -Color Info
    Write-ColorOutput "Create Installer: $CreateInstaller" -Color Info
    Write-ColorOutput "Create Offline Package: $CreateOfflinePackage" -Color Info
    Write-ColorOutput "Run Tests: $RunTests" -Color Info
    Write-ColorOutput ""
    
    try {
        # Environment validation
        Test-Environment
        
        # Initialize build
        Initialize-Build
        
        # Build project
        Build-Project
        
        # Run tests
        Run-TestSuite
        
        # Initialize directories
        Initialize-Database
        Initialize-Reports
        
        # Create distribution package
        $distPath = Create-DistributionPackage
        
        # Create installer
        Create-Installer -DistPath $distPath
        
        # Create offline package
        Create-OfflinePackage -DistPath $distPath
        
        # Validate build output
        Test-BuildOutput
        
        # Show summary
        Show-BuildSummary
        
    } catch {
        Write-Error "Build failed with error: $($_.Exception.Message)"
        exit 1
    }
}

# Execute main function
Main
>>>>>>> release/v1.0.0
