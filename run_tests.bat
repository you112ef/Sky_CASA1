@echo off
echo ========================================
echo MedicalLabAnalyzer Test Runner
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

REM Check if project exists
if not exist "src\MedicalLabAnalyzer\MedicalLabAnalyzer.csproj" (
    echo ERROR: Project file not found
    echo Please run this script from the project root directory
    pause
    exit /b 1
)

REM Check if tests exist
if not exist "tests\MedicalLabAnalyzer.Tests\MedicalLabAnalyzer.Tests.csproj" (
    echo ERROR: Test project not found
    echo Please run this script from the project root directory
    pause
    exit /b 1
)

echo Project structure verified.
echo.

REM Clean and restore
echo Cleaning and restoring packages...
dotnet clean
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

echo.
echo ========================================
echo Running All Tests
echo ========================================
echo.

REM Run all tests
echo Running all tests...
dotnet test --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo.
    echo WARNING: Some tests failed
    echo.
)

echo.
echo ========================================
echo Running Specific Test Categories
echo ========================================
echo.

REM Run CASA tests
echo Running CASA analysis tests...
dotnet test --configuration Release --filter "Category=CASA" --verbosity minimal
if %errorlevel% neq 0 (
    echo WARNING: Some CASA tests failed
)

REM Run CBC tests
echo Running CBC analysis tests...
dotnet test --configuration Release --filter "Category=CBC" --verbosity minimal
if %errorlevel% neq 0 (
    echo WARNING: Some CBC tests failed
)

REM Run Urine tests
echo Running Urine analysis tests...
dotnet test --configuration Release --filter "Category=Urine" --verbosity minimal
if %errorlevel% neq 0 (
    echo WARNING: Some Urine tests failed
)

REM Run Stool tests
echo Running Stool analysis tests...
dotnet test --configuration Release --filter "Category=Stool" --verbosity minimal
if %errorlevel% neq 0 (
    echo WARNING: Some Stool tests failed
)

REM Run Integration tests
echo Running Integration tests...
dotnet test --configuration Release --filter "Category=Integration" --verbosity minimal
if %errorlevel% neq 0 (
    echo WARNING: Some Integration tests failed
)

echo.
echo ========================================
echo Running Real-World Tests
echo ========================================
echo.

REM Check if sample data exists
if exist "Samples\sperm_sample.mp4" (
    echo Sample video found. Running real CASA test...
    echo.
    echo Note: This test requires a real video file and may take several minutes.
    echo.
    
    REM Run real CASA test
    dotnet test --configuration Release --filter "CasaAnalysisRealTest" --verbosity normal
    if %errorlevel% neq 0 (
        echo WARNING: Real CASA test failed
    )
) else (
    echo No sample video found. Skipping real CASA test.
    echo To run real tests, place a sample video file in the Samples folder.
)

echo.
echo ========================================
echo Test Summary
echo ========================================
echo.
echo All tests completed.
echo.
echo To run specific tests:
echo   dotnet test --filter "Category=CASA"
echo   dotnet test --filter "Category=CBC"
echo   dotnet test --filter "Category=Urine"
echo   dotnet test --filter "Category=Stool"
echo   dotnet test --filter "Category=Integration"
echo.
echo To run tests with detailed output:
echo   dotnet test --verbosity detailed
echo.
echo To run tests and generate coverage report:
echo   dotnet test --collect:"XPlat Code Coverage"
echo.

pause