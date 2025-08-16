@echo off
echo ========================================
echo Medical Lab Analyzer - Build Script
echo ========================================
echo.

REM Check if .NET 8 is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 8 SDK is not installed!
    echo Please install .NET 8 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo .NET Version: 
dotnet --version
echo.

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj
if %errorlevel% neq 0 (
    echo ERROR: Failed to clean project
    pause
    exit /b 1
)

REM Restore packages
echo Restoring NuGet packages...
dotnet restore src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

REM Build in Debug mode
echo Building in Debug mode...
dotnet build src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj -c Debug
if %errorlevel% neq 0 (
    echo ERROR: Debug build failed
    pause
    exit /b 1
)

REM Build in Release mode
echo Building in Release mode...
dotnet build src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj -c Release
if %errorlevel% neq 0 (
    echo ERROR: Release build failed
    pause
    exit /b 1
)

REM Run tests
echo Running tests...
dotnet test tests/MedicalLabAnalyzer.Tests/MedicalLabAnalyzer.Tests.csproj
if %errorlevel% neq 0 (
    echo WARNING: Some tests failed
    echo.
)

REM Publish for Windows x64
echo Publishing for Windows x64...
dotnet publish src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj ^
    -c Release ^
    -r win-x64 ^
    -p:PublishSingleFile=true ^
    --self-contained true ^
    -o publish/win-x64

if %errorlevel% neq 0 (
    echo ERROR: Publishing failed
    pause
    exit /b 1
)

REM Create deployment package
echo Creating deployment package...
if not exist "deploy" mkdir deploy
xcopy /E /I /Y "publish\win-x64" "deploy\MedicalLabAnalyzer"
xcopy /E /I /Y "Database" "deploy\MedicalLabAnalyzer\Database"
xcopy /E /I /Y "src\MedicalLabAnalyzer\Reports" "deploy\MedicalLabAnalyzer\Reports"
copy "src\MedicalLabAnalyzer\appsettings.json" "deploy\MedicalLabAnalyzer\"
copy "README.md" "deploy\MedicalLabAnalyzer\"
copy "LICENSE" "deploy\MedicalLabAnalyzer\"

REM Create ZIP package
echo Creating ZIP package...
powershell -Command "Compress-Archive -Path 'deploy\MedicalLabAnalyzer' -DestinationPath 'deploy\MedicalLabAnalyzer-v1.0.0.zip' -Force"

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Output locations:
echo - Debug build: src\MedicalLabAnalyzer\bin\Debug\net8.0-windows\
echo - Release build: src\MedicalLabAnalyzer\bin\Release\net8.0-windows\
echo - Published: publish\win-x64\
echo - Deployment package: deploy\MedicalLabAnalyzer\
echo - ZIP package: deploy\MedicalLabAnalyzer-v1.0.0.zip
echo.
echo To run the application:
echo - Debug: dotnet run --project src\MedicalLabAnalyzer\MedicalLabAnalyzer.csproj
echo - Release: deploy\MedicalLabAnalyzer\MedicalLabAnalyzer.exe
echo.
pause