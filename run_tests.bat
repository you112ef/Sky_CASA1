@echo off
echo ========================================
echo Medical Lab Analyzer - Test Runner
echo محلل المختبر الطبي - مشغل الاختبارات
echo ========================================
echo.

echo Building project...
dotnet build src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj

if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please check for errors.
    echo فشل البناء! يرجى التحقق من الأخطاء.
    pause
    exit /b 1
)

echo.
echo Build successful! Running tests...
echo البناء ناجح! تشغيل الاختبارات...
echo.

echo Running CASA Analysis Test...
echo تشغيل اختبار تحليل CASA...
echo.

dotnet run --project src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj -- --test

echo.
echo Tests completed!
echo تم إكمال الاختبارات!
echo.

pause