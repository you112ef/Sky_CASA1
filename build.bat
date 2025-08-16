@echo off
echo ========================================
echo MedicalLabAnalyzer Build Script
echo ========================================
echo.

REM Check if .NET 8.0 is installed
echo Checking .NET 8.0 installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 8.0 is not installed or not in PATH
    echo Please install .NET 8.0 Desktop Runtime from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo .NET version found: 
dotnet --version
echo.

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean
if %errorlevel% neq 0 (
    echo ERROR: Failed to clean project
    pause
    exit /b 1
)

REM Restore packages
echo Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

REM Build in Release mode
echo Building project in Release mode...
dotnet build --configuration Release --no-restore
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

REM Run tests
echo Running tests...
dotnet test --configuration Release --no-build
if %errorlevel% neq 0 (
    echo WARNING: Some tests failed, but build completed
    echo.
)

echo.
echo ========================================
echo Build completed successfully!
echo ========================================
echo.
echo Output location: bin\Release\net8.0-windows\
echo.
echo To run the application:
echo   dotnet run --configuration Release
echo.
echo Or run the executable directly:
echo   bin\Release\net8.0-windows\MedicalLabAnalyzer.exe
echo.

pause