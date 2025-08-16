# MedicalLabAnalyzer Test Runner Script
# This script runs tests and generates reports

param(
    [string]$Configuration = "Release",
    [string]$Filter = "",
    [switch]$Coverage,
    [switch]$Verbose,
    [switch]$GenerateReport
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

# Function to check prerequisites
function Test-Prerequisites {
    Write-ColorOutput "=== Checking Test Prerequisites ===" "Cyan"
    
    # Check .NET SDK
    if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
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
    
    Write-ColorOutput "✅ All test prerequisites met" "Green"
}

# Function to create test results directory
function Initialize-TestResults {
    Write-ColorOutput "=== Initializing Test Results ===" "Cyan"
    
    $testResultsPath = "TestResults"
    if (Test-Path $testResultsPath) {
        Remove-Item $testResultsPath -Recurse -Force
        Write-ColorOutput "✅ Cleaned existing test results" "Green"
    }
    
    New-Item -ItemType Directory -Path $testResultsPath -Force | Out-Null
    Write-ColorOutput "✅ Test results directory created: $testResultsPath" "Green"
}

# Function to run tests
function Run-Tests {
    Write-ColorOutput "=== Running Tests ===" "Cyan"
    
    $testArgs = @("test", "MedicalLabAnalyzer.sln")
    $testArgs += "--configuration", $Configuration
    $testArgs += "--no-build"
    
    if ($Verbose) {
        $testArgs += "--verbosity", "detailed"
    } else {
        $testArgs += "--verbosity", "normal"
    }
    
    # Add filter if specified
    if ($Filter) {
        $testArgs += "--filter", $Filter
        Write-ColorOutput "Using filter: $Filter" "Yellow"
    }
    
    # Add coverage if requested
    if ($Coverage) {
        $testArgs += "/p:CollectCoverage=true"
        $testArgs += "/p:CoverletOutputFormat=cobertura"
        $testArgs += "/p:CoverletOutput=./TestResults/coverage.cobertura.xml"
        $testArgs += "/p:Threshold=80"
        $testArgs += "/p:ThresholdType=line"
        $testArgs += "/p:ThresholdStat=total"
        Write-ColorOutput "Coverage collection enabled" "Yellow"
    }
    
    # Add test result logging
    $testArgs += "--logger", "trx;LogFileName=test_results.trx"
    $testArgs += "--results-directory", "./TestResults"
    $testArgs += "--logger", "console;verbosity=normal"
    
    Write-ColorOutput "Running: dotnet $($testArgs -join ' ')" "White"
    
    dotnet $testArgs
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-ColorOutput "✅ Tests completed successfully" "Green"
    } else {
        Write-ColorOutput "❌ Tests failed with exit code: $exitCode" "Red"
        exit $exitCode
    }
}

# Function to analyze test results
function Analyze-TestResults {
    Write-ColorOutput "=== Analyzing Test Results ===" "Cyan"
    
    $testResultsPath = "TestResults"
    
    # Check for test result files
    $trxFiles = Get-ChildItem -Path "$testResultsPath/*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        Write-ColorOutput "✅ Test result files found:" "Green"
        $trxFiles | ForEach-Object { Write-ColorOutput "  - $($_.Name)" "White" }
    } else {
        Write-ColorOutput "⚠️ No test result files found" "Yellow"
    }
    
    # Check for coverage files
    if ($Coverage) {
        $coverageFiles = Get-ChildItem -Path "$testResultsPath/*.xml" -ErrorAction SilentlyContinue
        if ($coverageFiles) {
            Write-ColorOutput "✅ Coverage files found:" "Green"
            $coverageFiles | ForEach-Object { Write-ColorOutput "  - $($_.Name)" "White" }
        } else {
            Write-ColorOutput "⚠️ No coverage files found" "Yellow"
        }
    }
}

# Function to generate test report
function Generate-TestReport {
    if (-not $GenerateReport) {
        return
    }
    
    Write-ColorOutput "=== Generating Test Report ===" "Cyan"
    
    $reportPath = "TestResults/test-report.md"
    $testResultsPath = "TestResults"
    
    $report = @"
# MedicalLabAnalyzer Test Report

## Test Execution Details
- **Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
- **Configuration**: $Configuration
- **Filter**: $(if ($Filter) { $Filter } else { "All tests" })
- **Coverage**: $(if ($Coverage) { "Enabled" } else { "Disabled" })
- **Verbose**: $(if ($Verbose) { "Yes" } else { "No" })

## Test Results Summary
- **Test Results Directory**: $testResultsPath
- **TRX Files**: $(@(Get-ChildItem -Path "$testResultsPath/*.trx" -ErrorAction SilentlyContinue).Count)
- **Coverage Files**: $(@(Get-ChildItem -Path "$testResultsPath/*.xml" -ErrorAction SilentlyContinue).Count)

## Test Files
"@
    
    # Add test result files
    $trxFiles = Get-ChildItem -Path "$testResultsPath/*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        $report += "`n### Test Result Files`n"
        foreach ($file in $trxFiles) {
            $report += "- $($file.Name)`n"
        }
    }
    
    # Add coverage files
    if ($Coverage) {
        $coverageFiles = Get-ChildItem -Path "$testResultsPath/*.xml" -ErrorAction SilentlyContinue
        if ($coverageFiles) {
            $report += "`n### Coverage Files`n"
            foreach ($file in $coverageFiles) {
                $report += "- $($file.Name)`n"
            }
        }
    }
    
    $report += @"

## Recommendations
1. Review test results for any failures
2. Check coverage reports if enabled
3. Address any failing tests
4. Consider adding more test coverage if needed

---
*Generated by MedicalLabAnalyzer Test Runner*
"@
    
    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-ColorOutput "✅ Test report generated: $reportPath" "Green"
}

# Function to display test summary
function Show-TestSummary {
    Write-ColorOutput "=== Test Summary ===" "Cyan"
    Write-ColorOutput "Configuration: $Configuration" "White"
    Write-ColorOutput "Filter: $(if ($Filter) { $Filter } else { 'All tests' })" "White"
    Write-ColorOutput "Coverage: $(if ($Coverage) { 'Enabled' } else { 'Disabled' })" "White"
    Write-ColorOutput "Verbose: $(if ($Verbose) { 'Yes' } else { 'No' })" "White"
    Write-ColorOutput "Report: $(if ($GenerateReport) { 'Generated' } else { 'Skipped' })" "White"
    
    $testResultsPath = "TestResults"
    $trxCount = @(Get-ChildItem -Path "$testResultsPath/*.trx" -ErrorAction SilentlyContinue).Count
    $xmlCount = @(Get-ChildItem -Path "$testResultsPath/*.xml" -ErrorAction SilentlyContinue).Count
    
    Write-ColorOutput "Test Result Files: $trxCount" "White"
    Write-ColorOutput "Coverage Files: $xmlCount" "White"
    
    Write-ColorOutput "✅ Test execution completed successfully!" "Green"
}

# Main execution
try {
    Write-ColorOutput "🧪 MedicalLabAnalyzer Test Runner" "Magenta"
    Write-ColorOutput "Configuration: $Configuration" "Magenta"
    Write-ColorOutput "Filter: $(if ($Filter) { $Filter } else { 'All tests' })" "Magenta"
    Write-ColorOutput "Coverage: $(if ($Coverage) { 'Enabled' } else { 'Disabled' })" "Magenta"
    Write-ColorOutput ""
    
    Test-Prerequisites
    Initialize-TestResults
    Run-Tests
    Analyze-TestResults
    Generate-TestReport
    Show-TestSummary
}
catch {
    Write-ColorOutput "❌ Test execution failed with error: $($_.Exception.Message)" "Red"
    exit 1
}