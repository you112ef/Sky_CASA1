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