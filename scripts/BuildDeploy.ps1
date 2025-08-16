# MedicalLabAnalyzer Build and Deploy Script
# This script builds the application and creates a distribution package

param(
    [string]$Configuration = "Release",
    [string]$Version = "2.0.0",
    [switch]$SkipTests,
    [switch]$CreatePackage,
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Function to check if command exists
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

# Function to check prerequisites
function Test-Prerequisites {
    Write-ColorOutput "=== Checking Prerequisites ===" "Cyan"
    
    # Check .NET SDK
    if (Test-Command "dotnet") {
        $dotnetVersion = dotnet --version
        Write-ColorOutput "✅ .NET SDK found: $dotnetVersion" "Green"
    } else {
        Write-ColorOutput "❌ .NET SDK not found. Please install .NET 8.0 SDK" "Red"
        exit 1
    }
    
    # Check if solution file exists
    if (Test-Path "MedicalLabAnalyzer.sln") {
        Write-ColorOutput "✅ Solution file found: MedicalLabAnalyzer.sln" "Green"
    } else {
        Write-ColorOutput "❌ Solution file not found: MedicalLabAnalyzer.sln" "Red"
        exit 1
    }
    
    Write-ColorOutput "✅ All prerequisites met" "Green"
}

# Function to clean build artifacts
function Clear-BuildArtifacts {
    Write-ColorOutput "=== Cleaning Build Artifacts ===" "Cyan"
    
    $pathsToClean = @("bin", "obj", "TestResults", "Dist")
    
    foreach ($path in $pathsToClean) {
        if (Test-Path $path) {
            Remove-Item $path -Recurse -Force
            Write-ColorOutput "✅ Cleaned: $path" "Green"
        }
    }
    
    # Clear NuGet cache
    dotnet nuget locals all --clear
    Write-ColorOutput "✅ NuGet cache cleared" "Green"
}

# Function to restore packages
function Restore-Packages {
    Write-ColorOutput "=== Restoring Packages ===" "Cyan"
    
    $restoreArgs = @("restore", "MedicalLabAnalyzer.sln")
    if ($Verbose) {
        $restoreArgs += "--verbosity", "detailed"
    } else {
        $restoreArgs += "--verbosity", "normal"
    }
    
    dotnet $restoreArgs
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "✅ Packages restored successfully" "Green"
    } else {
        Write-ColorOutput "❌ Package restore failed" "Red"
        exit 1
    }
}

# Function to build solution
function Build-Solution {
    Write-ColorOutput "=== Building Solution ===" "Cyan"
    
    $buildArgs = @("build", "MedicalLabAnalyzer.sln", "--configuration", $Configuration, "--no-restore")
    if ($Verbose) {
        $buildArgs += "--verbosity", "detailed"
    } else {
        $buildArgs += "--verbosity", "normal"
    }
    
    dotnet $buildArgs
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "✅ Solution built successfully" "Green"
    } else {
        Write-ColorOutput "❌ Build failed" "Red"
        exit 1
    }
}

# Function to run tests
function Run-Tests {
    if ($SkipTests) {
        Write-ColorOutput "=== Skipping Tests ===" "Yellow"
        return
    }
    
    Write-ColorOutput "=== Running Tests ===" "Cyan"
    
    # Create TestResults directory
    New-Item -ItemType Directory -Path "TestResults" -Force | Out-Null
    
    $testArgs = @("test", "MedicalLabAnalyzer.sln", "--configuration", $Configuration, "--no-build")
    if ($Verbose) {
        $testArgs += "--verbosity", "detailed"
    } else {
        $testArgs += "--verbosity", "normal"
    }
    $testArgs += "--logger", "trx;LogFileName=test_results.trx"
    $testArgs += "--results-directory", "./TestResults"
    
    dotnet $testArgs
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "✅ Tests completed successfully" "Green"
    } else {
        Write-ColorOutput "❌ Tests failed" "Red"
        exit 1
    }
}

# Function to create distribution package
function Create-DistributionPackage {
    if (-not $CreatePackage) {
        Write-ColorOutput "=== Skipping Package Creation ===" "Yellow"
        return
    }
    
    Write-ColorOutput "=== Creating Distribution Package ===" "Cyan"
    
    $distFolder = "Dist/MedicalLabAnalyzer-v$Version"
    $zipFile = "Dist/MedicalLabAnalyzer-v$Version.zip"
    
    # Create distribution folder structure
    $folders = @(
        $distFolder,
        "$distFolder/App",
        "$distFolder/Database",
        "$distFolder/Reports",
        "$distFolder/Logs"
    )
    
    foreach ($folder in $folders) {
        New-Item -ItemType Directory -Path $folder -Force | Out-Null
    }
    
    Write-ColorOutput "✅ Distribution folder structure created" "Green"
    
    # Copy application files
    $sourcePath = "bin/$Configuration/net8.0-windows"
    $destPath = "$distFolder/App"
    
    if (Test-Path $sourcePath) {
        Copy-Item -Path "$sourcePath/*" -Destination $destPath -Recurse -Force
        Write-ColorOutput "✅ Application files copied" "Green"
    } else {
        Write-ColorOutput "❌ Source path not found: $sourcePath" "Red"
        exit 1
    }
    
    # Copy documentation files
    $docFiles = @("README.md", "CHANGELOG.txt", "LICENSE", "INSTALLATION_GUIDE.md", "QUICK_START.md")
    foreach ($file in $docFiles) {
        if (Test-Path $file) {
            Copy-Item -Path $file -Destination $distFolder -Force
            Write-ColorOutput "✅ Copied: $file" "Green"
        } else {
            Write-ColorOutput "⚠️ File not found: $file" "Yellow"
        }
    }
    
    # Create database template
    $dbFile = "$distFolder/Database/medical_lab.db"
    if (-not (Test-Path $dbFile)) {
        New-Item -ItemType File -Path $dbFile -Force | Out-Null
        Write-ColorOutput "✅ Database template created" "Green"
    }
    
    # Create configuration file
    $appSettings = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=../Database/medical_lab.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Reports": {
    "OutputPath": "../Reports",
    "ArchivePath": "../Reports/Archive"
  },
  "Security": {
    "SessionTimeout": 30,
    "MaxLoginAttempts": 3
  }
}
"@
    
    $appSettings | Out-File -FilePath "$distFolder/App/appsettings.json" -Encoding UTF8
    Write-ColorOutput "✅ Configuration file created" "Green"
    
    # Create installation script
    $installScript = @"
@echo off
echo Installing MedicalLabAnalyzer v$Version...
echo.

REM Check if .NET 8.0 is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET 8.0 is not installed.
    echo Please install .NET 8.0 from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

REM Create desktop shortcut
echo Creating desktop shortcut...
powershell -Command "`$WshShell = New-Object -comObject WScript.Shell; `$Shortcut = `$WshShell.CreateShortcut(`"`$env:USERPROFILE\Desktop\MedicalLabAnalyzer.lnk`"); `$Shortcut.TargetPath = `"%~dp0App\MedicalLabAnalyzer.exe`"; `$Shortcut.Save()"

echo.
echo Installation completed successfully!
echo You can now run MedicalLabAnalyzer from the desktop shortcut.
pause
"@
    
    $installScript | Out-File -FilePath "$distFolder/install.bat" -Encoding ASCII
    Write-ColorOutput "✅ Installation script created" "Green"
    
    # Create ZIP package
    if (Test-Path $zipFile) {
        Remove-Item $zipFile -Force
    }
    
    Compress-Archive -Path $distFolder -DestinationPath $zipFile -Force
    $zipSize = (Get-Item $zipFile).Length
    Write-ColorOutput "✅ ZIP package created: $zipFile ($([math]::Round($zipSize / 1MB, 2)) MB)" "Green"
}

# Function to display build summary
function Show-BuildSummary {
    Write-ColorOutput "=== Build Summary ===" "Cyan"
    Write-ColorOutput "Configuration: $Configuration" "White"
    Write-ColorOutput "Version: $Version" "White"
    Write-ColorOutput "Tests: $(if ($SkipTests) { 'Skipped' } else { 'Run' })" "White"
    Write-ColorOutput "Package: $(if ($CreatePackage) { 'Created' } else { 'Skipped' })" "White"
    
    if ($CreatePackage) {
        $zipFile = "Dist/MedicalLabAnalyzer-v$Version.zip"
        if (Test-Path $zipFile) {
            $zipSize = (Get-Item $zipFile).Length
            Write-ColorOutput "Package Size: $([math]::Round($zipSize / 1MB, 2)) MB" "White"
        }
    }
    
    Write-ColorOutput "✅ Build completed successfully!" "Green"
}

# Main execution
try {
    Write-ColorOutput "🏥 MedicalLabAnalyzer Build and Deploy Script" "Magenta"
    Write-ColorOutput "Version: $Version" "Magenta"
    Write-ColorOutput "Configuration: $Configuration" "Magenta"
    Write-ColorOutput ""
    
    Test-Prerequisites
    Clear-BuildArtifacts
    Restore-Packages
    Build-Solution
    Run-Tests
    Create-DistributionPackage
    Show-BuildSummary
}
catch {
    Write-ColorOutput "❌ Build failed with error: $($_.Exception.Message)" "Red"
    exit 1
}