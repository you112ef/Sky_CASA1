@echo off
REM ===================================================================
REM MedicalLabAnalyzer - Advanced Medical Laboratory Management System
REM Test Execution Script
REM Version: 1.0.0
REM ===================================================================

setlocal enabledelayedexpansion

REM Configuration
set PROJECT_NAME=MedicalLabAnalyzer
set CONFIGURATION=Release
set TARGET_FRAMEWORK=net8.0-windows
set SOLUTION_FILE=%PROJECT_NAME%.sln
set PROJECT_FILE=src\%PROJECT_NAME%\%PROJECT_NAME%.csproj

REM Colors for output
set "GREEN=[92m"
set "RED=[91m"
set "YELLOW=[93m"
set "CYAN=[96m"
set "MAGENTA=[95m"
set "RESET=[0m"

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%MedicalLabAnalyzer v1.0.0 - Advanced Medical Laboratory System%RESET%
echo %MAGENTA%Test Execution Script%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

REM Check if .NET SDK is available
echo %CYAN%🔹 Checking .NET 8.0 SDK...%RESET%
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo %RED%❌ .NET SDK not found. Please install .NET 8.0 SDK%RESET%
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo %GREEN%✅ .NET SDK %DOTNET_VERSION% found%RESET%
echo.

REM Check if solution file exists
if not exist "%SOLUTION_FILE%" (
    echo %RED%❌ Solution file not found: %SOLUTION_FILE%%RESET%
    pause
    exit /b 1
)

echo %CYAN%🔹 Solution file found: %SOLUTION_FILE%%RESET%
echo.

REM Clean previous build artifacts
echo %CYAN%🔹 Cleaning previous build artifacts...%RESET%
dotnet clean --configuration %CONFIGURATION% --verbosity quiet
if %ERRORLEVEL% neq 0 (
    echo %YELLOW%⚠️ Clean failed, continuing...%RESET%
)
echo %GREEN%✅ Clean completed%RESET%
echo.

REM Restore packages
echo %CYAN%🔹 Restoring NuGet packages...%RESET%
dotnet restore --configuration %CONFIGURATION% --verbosity quiet
if %ERRORLEVEL% neq 0 (
    echo %RED%❌ Package restore failed%RESET%
    pause
    exit /b 1
)
echo %GREEN%✅ Package restore completed%RESET%
echo.

REM Build the solution
echo %CYAN%🔹 Building solution with configuration: %CONFIGURATION%%RESET%
dotnet build --configuration %CONFIGURATION% --no-restore --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo %RED%❌ Build failed%RESET%
    pause
    exit /b 1
)
echo %GREEN%✅ Build completed successfully%RESET%
echo.

REM Verify build output
set BUILD_OUTPUT=src\%PROJECT_NAME%\bin\%CONFIGURATION%\%TARGET_FRAMEWORK%
if not exist "%BUILD_OUTPUT%" (
    echo %RED%❌ Build output not found: %BUILD_OUTPUT%%RESET%
    pause
    exit /b 1
)
echo %GREEN%✅ Build output verified at: %BUILD_OUTPUT%%RESET%
echo.

REM ===================================================================
REM Test Execution
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Running Test Suite%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

REM Run all unit tests
echo %CYAN%🔹 Running all unit tests...%RESET%
dotnet test --configuration %CONFIGURATION% --no-build --verbosity normal --logger "console;verbosity=normal"
set TEST_EXIT_CODE=%ERRORLEVEL%

if %TEST_EXIT_CODE% equ 0 (
    echo %GREEN%✅ All unit tests passed%RESET%
) else (
    echo %YELLOW%⚠️ Some unit tests failed - continuing with specific tests%RESET%
)
echo.

REM Run CASA analysis tests
echo %CYAN%🔹 Running CASA analysis tests...%RESET%
dotnet test --configuration %CONFIGURATION% --no-build --filter "CasaAnalysisRealTest" --verbosity normal
set CASA_TEST_EXIT_CODE=%ERRORLEVEL%

if %CASA_TEST_EXIT_CODE% equ 0 (
    echo %GREEN%✅ CASA analysis tests passed%RESET%
) else (
    echo %YELLOW%⚠️ CASA analysis tests failed%RESET%
)
echo.

REM Run calibration tests
echo %CYAN%🔹 Running calibration tests...%RESET%
dotnet test --configuration %CONFIGURATION% --no-build --filter "CalibrationTest" --verbosity normal
set CALIBRATION_TEST_EXIT_CODE=%ERRORLEVEL%

if %CALIBRATION_TEST_EXIT_CODE% equ 0 (
    echo %GREEN%✅ Calibration tests passed%RESET%
) else (
    echo %YELLOW%⚠️ Calibration tests failed%RESET%
)
echo.

REM Run service tests
echo %CYAN%🔹 Running service tests...%RESET%
dotnet test --configuration %CONFIGURATION% --no-build --filter "ServiceTest" --verbosity normal
set SERVICE_TEST_EXIT_CODE=%ERRORLEVEL%

if %SERVICE_TEST_EXIT_CODE% equ 0 (
    echo %GREEN%✅ Service tests passed%RESET%
) else (
    echo %YELLOW%⚠️ Service tests failed%RESET%
)
echo.

REM Run integration tests
echo %CYAN%🔹 Running integration tests...%RESET%
dotnet test --configuration %CONFIGURATION% --no-build --filter "IntegrationTest" --verbosity normal
set INTEGRATION_TEST_EXIT_CODE=%ERRORLEVEL%

if %INTEGRATION_TEST_EXIT_CODE% equ 0 (
    echo %GREEN%✅ Integration tests passed%RESET%
) else (
    echo %YELLOW%⚠️ Integration tests failed%RESET%
)
echo.

REM ===================================================================
REM Manual Test Execution
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Manual Test Execution%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

REM Check if sample video exists
set SAMPLE_VIDEO=Samples\sperm_sample.mp4
if exist "%SAMPLE_VIDEO%" (
    echo %GREEN%✅ Sample video found: %SAMPLE_VIDEO%%RESET%
    echo %CYAN%🔹 You can run manual CASA analysis test with this video%RESET%
) else (
    echo %YELLOW%⚠️ Sample video not found: %SAMPLE_VIDEO%%RESET%
    echo %CYAN%🔹 Please place a sample video file for manual testing%RESET%
)
echo.

REM Check if database exists
set DATABASE_PATH=Database\medical_lab.db
if exist "%DATABASE_PATH%" (
    echo %GREEN%✅ Database found: %DATABASE_PATH%%RESET%
) else (
    echo %YELLOW%⚠️ Database not found: %DATABASE_PATH%%RESET%
    echo %CYAN%🔹 Database will be created on first run%RESET%
)
echo.

REM Check if reports directory exists
if exist "Reports" (
    echo %GREEN%✅ Reports directory found%RESET%
) else (
    echo %YELLOW%⚠️ Reports directory not found%RESET%
    echo %CYAN%🔹 Creating reports directory...%RESET%
    mkdir Reports 2>nul
    mkdir Reports\Templates 2>nul
    mkdir Reports\Archive 2>nul
    mkdir Reports\Output 2>nul
    echo %GREEN%✅ Reports directory created%RESET%
)
echo.

REM ===================================================================
REM Performance Tests
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Performance Tests%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

REM Check application startup time
echo %CYAN%🔹 Testing application startup time...%RESET%
set APP_EXE=%BUILD_OUTPUT%\%PROJECT_NAME%.exe
if exist "%APP_EXE%" (
    echo %GREEN%✅ Application executable found: %APP_EXE%%RESET%
    echo %CYAN%🔹 You can manually test the application startup%RESET%
) else (
    echo %RED%❌ Application executable not found: %APP_EXE%%RESET%
)
echo.

REM Check memory usage
echo %CYAN%🔹 Checking system resources...%RESET%
wmic OS get TotalVisibleMemorySize,FreePhysicalMemory /format:value | find "=" > temp_memory.txt
for /f "tokens=2 delims==" %%a in ('findstr "FreePhysicalMemory" temp_memory.txt') do set FREE_MEMORY=%%a
for /f "tokens=2 delims==" %%a in ('findstr "TotalVisibleMemorySize" temp_memory.txt') do set TOTAL_MEMORY=%%a
del temp_memory.txt

set /a FREE_MEMORY_MB=%FREE_MEMORY% / 1024
set /a TOTAL_MEMORY_MB=%TOTAL_MEMORY% / 1024

echo %GREEN%✅ Available memory: %FREE_MEMORY_MB% MB / %TOTAL_MEMORY_MB% MB%RESET%

if %FREE_MEMORY_MB% gtr 2048 (
    echo %GREEN%✅ Sufficient memory available for testing%RESET%
) else (
    echo %YELLOW%⚠️ Low memory available - may affect performance%RESET%
)
echo.

REM ===================================================================
REM Test Summary
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Test Summary%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

set TOTAL_TESTS=0
set PASSED_TESTS=0
set FAILED_TESTS=0

if %TEST_EXIT_CODE% equ 0 set /a PASSED_TESTS+=1
if %TEST_EXIT_CODE% neq 0 set /a FAILED_TESTS+=1
set /a TOTAL_TESTS+=1

if %CASA_TEST_EXIT_CODE% equ 0 set /a PASSED_TESTS+=1
if %CASA_TEST_EXIT_CODE% neq 0 set /a FAILED_TESTS+=1
set /a TOTAL_TESTS+=1

if %CALIBRATION_TEST_EXIT_CODE% equ 0 set /a PASSED_TESTS+=1
if %CALIBRATION_TEST_EXIT_CODE% neq 0 set /a FAILED_TESTS+=1
set /a TOTAL_TESTS+=1

if %SERVICE_TEST_EXIT_CODE% equ 0 set /a PASSED_TESTS+=1
if %SERVICE_TEST_EXIT_CODE% neq 0 set /a FAILED_TESTS+=1
set /a TOTAL_TESTS+=1

if %INTEGRATION_TEST_EXIT_CODE% equ 0 set /a PASSED_TESTS+=1
if %INTEGRATION_TEST_EXIT_CODE% neq 0 set /a FAILED_TESTS+=1
set /a TOTAL_TESTS+=1

echo %CYAN%📊 Test Results:%RESET%
echo %CYAN%   • Total Tests: %TOTAL_TESTS%%RESET%
echo %GREEN%   • Passed: %PASSED_TESTS%%RESET%
echo %RED%   • Failed: %FAILED_TESTS%%RESET%
echo.

if %FAILED_TESTS% equ 0 (
    echo %GREEN%🎉 All tests completed successfully!%RESET%
) else (
    echo %YELLOW%⚠️ Some tests failed - please review the output above%RESET%
)
echo.

REM ===================================================================
REM Next Steps
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Next Steps%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

echo %CYAN%🚀 Recommended next steps:%RESET%
echo %CYAN%   1. Test the application manually: %APP_EXE%%RESET%
echo %CYAN%   2. Verify all features work correctly%RESET%
echo %CYAN%   3. Test with real sample data%RESET%
echo %CYAN%   4. Run performance benchmarks%RESET%
echo %CYAN%   5. Deploy to test environment%RESET%
echo.

echo %CYAN%📋 Features to test:%RESET%
echo %GREEN%   ✅ CASA Analysis (Kalman + Hungarian)%RESET%
echo %GREEN%   ✅ CBC, Urine, Stool Analysis%RESET%
echo %GREEN%   ✅ User Management & Authentication%RESET%
echo %GREEN%   ✅ Advanced Reporting (PDF/Excel)%RESET%
echo %GREEN%   ✅ AuditLog System%RESET%
echo %GREEN%   ✅ Arabic RTL Support%RESET%
echo %GREEN%   ✅ Offline Operation%RESET%
echo.

REM ===================================================================
REM Manual Test Instructions
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Manual Test Instructions%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

echo %CYAN%🔬 CASA Analysis Test:%RESET%
echo %CYAN%   1. Start the application%RESET%
echo %CYAN%   2. Log in as 'lab' user (password: 123)%RESET%
echo %CYAN%   3. Navigate to Calibration screen%RESET%
echo %CYAN%   4. Load a sample video file%RESET%
echo %CYAN%   5. Set calibration parameters%RESET%
echo %CYAN%   6. Run CASA analysis%RESET%
echo %CYAN%   7. Verify VCL, VSL, VAP, ALH, BCF results%RESET%
echo.

echo %CYAN%🩸 CBC Analysis Test:%RESET%
echo %CYAN%   1. Add a new patient%RESET%
echo %CYAN%   2. Create a CBC exam%RESET%
echo %CYAN%   3. Enter CBC values (WBC, RBC, HGB, etc.)%RESET%
echo %CYAN%   4. Save and verify results%RESET%
echo %CYAN%   5. Generate CBC report%RESET%
echo.

echo %CYAN%🧪 Urine Analysis Test:%RESET%
echo %CYAN%   1. Create a urine exam%RESET%
echo %CYAN%   2. Enter physical properties (color, pH, SG)%RESET%
echo %CYAN%   3. Enter chemical tests (protein, glucose, etc.)%RESET%
echo %CYAN%   4. Enter microscopic examination%RESET%
echo %CYAN%   5. Save and verify results%RESET%
echo %CYAN%   6. Generate urine report%RESET%
echo.

echo %CYAN%💩 Stool Analysis Test:%RESET%
echo %CYAN%   1. Create a stool exam%RESET%
echo %CYAN%   2. Enter physical properties%RESET%
echo %CYAN%   3. Enter chemical tests%RESET%
echo %CYAN%   4. Enter microscopic examination%RESET%
echo %CYAN%   5. Enter parasitology results%RESET%
echo %CYAN%   6. Save and verify results%RESET%
echo %CYAN%   7. Generate stool report%RESET%
echo.

echo %CYAN%👥 User Management Test:%RESET%
echo %CYAN%   1. Test login with different user roles%RESET%
echo %CYAN%   2. Verify role-based access control%RESET%
echo %CYAN%   3. Test user creation and management%RESET%
echo %CYAN%   4. Test password changes%RESET%
echo %CYAN%   5. Verify session management%RESET%
echo.

echo %CYAN%📊 Reporting Test:%RESET%
echo %CYAN%   1. Generate PDF reports for all test types%RESET%
echo %CYAN%   2. Export data to Excel%RESET%
echo %CYAN%   3. Verify report formatting and content%RESET%
echo %CYAN%   4. Test report archiving%RESET%
echo %CYAN%   5. Verify Arabic text rendering%RESET%
echo.

REM ===================================================================
REM Cleanup and Exit
REM ===================================================================

echo %MAGENTA%==================================================================%RESET%
echo %MAGENTA%Test Execution Completed%RESET%
echo %MAGENTA%==================================================================%RESET%
echo.

if %FAILED_TESTS% equ 0 (
    echo %GREEN%🎉 All tests passed successfully!%RESET%
    echo %GREEN%The system is ready for deployment.%RESET%
) else (
    echo %YELLOW%⚠️ Some tests failed.%RESET%
    echo %YELLOW%Please review the test output and fix any issues before deployment.%RESET%
)

echo.
echo %CYAN%Press any key to exit...%RESET%
pause >nul

exit /b 0