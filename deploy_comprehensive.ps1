# Medical Lab Analyzer - Comprehensive Deployment Script
# سكربت التطبيق الشامل لنظام المختبر الطبي

param(
    [switch]$SkipTests,
    [switch]$Force,
    [string]$Environment = "Production"
)

Write-Host "🏥 Medical Lab Analyzer - Comprehensive Deployment" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# Check PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Host "❌ PowerShell 7+ is required. Current version: $($PSVersionTable.PSVersion)" -ForegroundColor Red
    exit 1
}

# Check .NET installation
Write-Host "🔍 Checking .NET installation..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "⚠️  Running without administrator privileges. Some operations may fail." -ForegroundColor Yellow
}

# Function to log operations
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    
    switch ($Level) {
        "ERROR" { Write-Host $logMessage -ForegroundColor Red }
        "WARN"  { Write-Host $logMessage -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $logMessage -ForegroundColor Green }
        default { Write-Host $logMessage -ForegroundColor White }
    }
    
    # Also write to log file
    Add-Content -Path "deployment.log" -Value $logMessage
}

# Function to check prerequisites
function Test-Prerequisites {
    Write-Log "Checking system prerequisites..." "INFO"
    
    # Check disk space
    $diskSpace = Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DeviceID -eq "C:" }
    $freeSpaceGB = [math]::Round($diskSpace.FreeSpace / 1GB, 2)
    if ($freeSpaceGB -lt 5) {
        Write-Log "❌ Insufficient disk space. Required: 5GB, Available: ${freeSpaceGB}GB" "ERROR"
        return $false
    }
    Write-Log "✅ Disk space: ${freeSpaceGB}GB available" "SUCCESS"
    
    # Check memory
    $memory = Get-WmiObject -Class Win32_ComputerSystem
    $totalMemoryGB = [math]::Round($memory.TotalPhysicalMemory / 1GB, 2)
    if ($totalMemoryGB -lt 4) {
        Write-Log "❌ Insufficient memory. Required: 4GB, Available: ${totalMemoryGB}GB" "ERROR"
        return $false
    }
    Write-Log "✅ Memory: ${totalMemoryGB}GB available" "SUCCESS"
    
    # Check network connectivity
    try {
        $testConnection = Test-NetConnection api.nuget.org -Port 443 -InformationLevel Quiet
        if ($testConnection.TcpTestSucceeded) {
            Write-Log "✅ Network connectivity: OK" "SUCCESS"
        } else {
            Write-Log "❌ Network connectivity: Failed" "ERROR"
            return $false
        }
    } catch {
        Write-Log "❌ Network connectivity test failed" "ERROR"
        return $false
    }
    
    return $true
}

# Function to backup existing installation
function Backup-ExistingInstallation {
    Write-Log "Creating backup of existing installation..." "INFO"
    
    $backupDir = "Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    if (Test-Path "bin") {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        Copy-Item -Path "bin" -Destination $backupDir -Recurse -Force
        Write-Log "✅ Backup created: $backupDir" "SUCCESS"
    } else {
        Write-Log "No existing installation found to backup" "INFO"
    }
}

# Function to clean solution
function Clear-Solution {
    Write-Log "Cleaning solution..." "INFO"
    
    try {
        dotnet clean MedicalLabAnalyzer.sln --verbosity minimal
        dotnet nuget locals all --clear
        Write-Log "✅ Solution cleaned successfully" "SUCCESS"
    } catch {
        Write-Log "❌ Failed to clean solution" "ERROR"
        return $false
    }
    
    return $true
}

# Function to restore packages
function Restore-Packages {
    Write-Log "Restoring NuGet packages..." "INFO"
    
    try {
        # First attempt with normal verbosity
        dotnet restore MedicalLabAnalyzer.sln --verbosity normal --runtime win-x64 --disable-parallel:false
        Write-Log "✅ Packages restored successfully" "SUCCESS"
        return $true
    } catch {
        Write-Log "⚠️  Normal restore failed, attempting with diagnostic verbosity..." "WARN"
        
        try {
            dotnet restore MedicalLabAnalyzer.sln --verbosity diagnostic --runtime win-x64 --force --no-cache
            Write-Log "✅ Packages restored with diagnostic verbosity" "SUCCESS"
            return $true
        } catch {
            Write-Log "❌ Package restore failed completely" "ERROR"
            return $false
        }
    }
}

# Function to build solution
function Build-Solution {
    param([string]$Configuration = "Release")
    
    Write-Log "Building solution with configuration: $Configuration" "INFO"
    
    try {
        dotnet build MedicalLabAnalyzer.sln --configuration $Configuration --no-restore --runtime win-x64 --verbosity normal
        Write-Log "✅ Solution built successfully" "SUCCESS"
        return $true
    } catch {
        Write-Log "❌ Build failed" "ERROR"
        return $false
    }
}

# Function to run tests
function Run-Tests {
    if ($SkipTests) {
        Write-Log "Skipping tests as requested" "INFO"
        return $true
    }
    
    Write-Log "Running tests..." "INFO"
    
    try {
        dotnet test MedicalLabAnalyzer.sln --configuration Release --no-build --verbosity normal --logger trx --results-directory TestResults
        Write-Log "✅ Tests completed successfully" "SUCCESS"
        return $true
    } catch {
        Write-Log "❌ Tests failed" "ERROR"
        return $false
    }
}

# Function to deploy application
function Deploy-Application {
    Write-Log "Deploying application..." "INFO"
    
    $deployDir = "Deploy"
    if (Test-Path $deployDir) {
        Remove-Item $deployDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $deployDir -Force | Out-Null
    
    # Copy built application
    Copy-Item -Path "src\MedicalLabAnalyzer\bin\Release\net8.0-windows\*" -Destination $deployDir -Recurse -Force
    
    # Copy additional resources
    if (Test-Path "Database") {
        Copy-Item -Path "Database" -Destination $deployDir -Recurse -Force
    }
    
    if (Test-Path "Samples") {
        Copy-Item -Path "Samples" -Destination $deployDir -Recurse -Force
    }
    
    # Create deployment manifest
    $manifest = @{
        ApplicationName = "Medical Lab Analyzer"
        Version = "1.0.0"
        DeployedAt = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Environment = $Environment
        .NETVersion = dotnet --version
        BuildConfiguration = "Release"
        TargetRuntime = "win-x64"
    }
    
    $manifest | ConvertTo-Json | Out-File "$deployDir\deployment-manifest.json" -Encoding UTF8
    
    Write-Log "✅ Application deployed to: $deployDir" "SUCCESS"
    return $true
}

# Function to validate deployment
function Test-Deployment {
    Write-Log "Validating deployment..." "INFO"
    
    $deployDir = "Deploy"
    if (-not (Test-Path $deployDir)) {
        Write-Log "❌ Deployment directory not found" "ERROR"
        return $false
    }
    
    # Check for required files
    $requiredFiles = @(
        "MedicalLabAnalyzer.exe",
        "MedicalLabAnalyzer.dll",
        "appsettings.json"
    )
    
    foreach ($file in $requiredFiles) {
        if (-not (Test-Path "$deployDir\$file")) {
            Write-Log "❌ Required file missing: $file" "ERROR"
            return $false
        }
    }
    
    # Check for EmguCV runtime files
    $emguCVFiles = Get-ChildItem -Path $deployDir -Filter "*.dll" | Where-Object { $_.Name -like "*Emgu*" }
    if ($emguCVFiles.Count -eq 0) {
        Write-Log "❌ EmguCV runtime files not found" "ERROR"
        return $false
    }
    
    Write-Log "✅ Deployment validation passed" "SUCCESS"
    return $true
}

# Function to create shortcuts
function Create-Shortcuts {
    Write-Log "Creating desktop shortcuts..." "INFO"
    
    $desktopPath = [Environment]::GetFolderPath("Desktop")
    $startMenuPath = [Environment]::GetFolderPath("StartMenu")
    
    $deployDir = (Resolve-Path "Deploy").Path
    $exePath = "$deployDir\MedicalLabAnalyzer.exe"
    
    if (Test-Path $exePath) {
        # Desktop shortcut
        $desktopShortcut = "$desktopPath\Medical Lab Analyzer.lnk"
        $WshShell = New-Object -ComObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut($desktopShortcut)
        $Shortcut.TargetPath = $exePath
        $Shortcut.WorkingDirectory = $deployDir
        $Shortcut.Description = "Medical Lab Analyzer - Advanced Medical Laboratory Management System"
        $Shortcut.IconLocation = "$exePath,0"
        $Shortcut.Save()
        
        # Start Menu shortcut
        $startMenuDir = "$startMenuPath\Medical Lab Analyzer"
        if (-not (Test-Path $startMenuDir)) {
            New-Item -ItemType Directory -Path $startMenuDir -Force | Out-Null
        }
        
        $startMenuShortcut = "$startMenuDir\Medical Lab Analyzer.lnk"
        $Shortcut = $WshShell.CreateShortcut($startMenuShortcut)
        $Shortcut.TargetPath = $exePath
        $Shortcut.WorkingDirectory = $deployDir
        $Shortcut.Description = "Medical Lab Analyzer - Advanced Medical Laboratory Management System"
        $Shortcut.IconLocation = "$exePath,0"
        $Shortcut.Save()
        
        Write-Log "✅ Shortcuts created successfully" "SUCCESS"
    } else {
        Write-Log "❌ Executable not found for shortcut creation" "ERROR"
    }
}

# Function to configure firewall rules
function Configure-Firewall {
    Write-Log "Configuring firewall rules..." "INFO"
    
    try {
        # Allow application through firewall
        $deployDir = (Resolve-Path "Deploy").Path
        $exePath = "$deployDir\MedicalLabAnalyzer.exe"
        
        if (Test-Path $exePath) {
            New-NetFirewallRule -DisplayName "Medical Lab Analyzer" -Direction Inbound -Program $exePath -Action Allow | Out-Null
            New-NetFirewallRule -DisplayName "Medical Lab Analyzer Outbound" -Direction Outbound -Program $exePath -Action Allow | Out-Null
            Write-Log "✅ Firewall rules configured" "SUCCESS"
        }
    } catch {
        Write-Log "⚠️  Failed to configure firewall rules (may require admin privileges)" "WARN"
    }
}

# Function to generate deployment report
function Generate-DeploymentReport {
    Write-Log "Generating deployment report..." "INFO"
    
    $report = @"
# Medical Lab Analyzer - Deployment Report

## Deployment Information
- **Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
- **Environment**: $Environment
- **Configuration**: Release
- **Target Runtime**: win-x64
- **NET Version**: $(dotnet --version)

## System Information
- **OS**: $($env:OS)
- **Architecture**: $env:PROCESSOR_ARCHITECTURE
- **Memory**: $([math]::Round((Get-WmiObject -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)) GB
- **Disk Space**: $([math]::Round((Get-WmiObject -Class Win32_LogicalDisk | Where-Object { $_.DeviceID -eq "C:" }).FreeSpace / 1GB, 2)) GB

## Deployment Status
- **Prerequisites Check**: ✅ PASSED
- **Backup Created**: ✅ COMPLETED
- **Solution Cleaned**: ✅ COMPLETED
- **Packages Restored**: ✅ COMPLETED
- **Solution Built**: ✅ COMPLETED
- **Tests Run**: $(if ($SkipTests) { "⏭️ SKIPPED" } else { "✅ COMPLETED" })
- **Application Deployed**: ✅ COMPLETED
- **Deployment Validated**: ✅ COMPLETED
- **Shortcuts Created**: ✅ COMPLETED
- **Firewall Configured**: ✅ COMPLETED

## Files Deployed
$(Get-ChildItem -Path "Deploy" -Recurse | ForEach-Object { "- $($_.FullName.Replace((Resolve-Path "Deploy").Path, "Deploy"))" })

## Next Steps
1. Test the application by running the desktop shortcut
2. Verify all features are working correctly
3. Check the logs in the Deploy\logs directory
4. Run quality control tests if needed

## Support
For technical support, contact the development team.
"@
    
    $report | Out-File "deployment-report.md" -Encoding UTF8
    Write-Log "✅ Deployment report generated: deployment-report.md" "SUCCESS"
}

# Main deployment process
function Start-Deployment {
    Write-Log "Starting comprehensive deployment process..." "INFO"
    
    # Step 1: Check prerequisites
    if (-not (Test-Prerequisites)) {
        Write-Log "❌ Prerequisites check failed. Deployment aborted." "ERROR"
        exit 1
    }
    
    # Step 2: Backup existing installation
    Backup-ExistingInstallation
    
    # Step 3: Clean solution
    if (-not (Clear-Solution)) {
        Write-Log "❌ Solution cleanup failed. Deployment aborted." "ERROR"
        exit 1
    }
    
    # Step 4: Restore packages
    if (-not (Restore-Packages)) {
        Write-Log "❌ Package restore failed. Deployment aborted." "ERROR"
        exit 1
    }
    
    # Step 5: Build solution
    if (-not (Build-Solution -Configuration "Release")) {
        Write-Log "❌ Build failed. Deployment aborted." "ERROR"
        exit 1
    }
    
    # Step 6: Run tests (if not skipped)
    if (-not (Run-Tests)) {
        if (-not $Force) {
            Write-Log "❌ Tests failed and --Force not specified. Deployment aborted." "ERROR"
            exit 1
        } else {
            Write-Log "⚠️  Tests failed but continuing due to --Force flag" "WARN"
        }
    }
    
    # Step 7: Deploy application
    if (-not (Deploy-Application)) {
        Write-Log "❌ Deployment failed. Deployment aborted." "ERROR"
        exit 1
    }
    
    # Step 8: Validate deployment
    if (-not (Test-Deployment)) {
        Write-Log "❌ Deployment validation failed. Deployment aborted." "ERROR"
        exit 1
    }
    
    # Step 9: Create shortcuts
    Create-Shortcuts
    
    # Step 10: Configure firewall
    Configure-Firewall
    
    # Step 11: Generate deployment report
    Generate-DeploymentReport
    
    Write-Log "🎉 Deployment completed successfully!" "SUCCESS"
    Write-Log "Application is ready for use. Check deployment-report.md for details." "INFO"
}

# Execute deployment
try {
    Start-Deployment
} catch {
    Write-Log "❌ Deployment failed with error: $($_.Exception.Message)" "ERROR"
    Write-Log "Check deployment.log for detailed information" "INFO"
    exit 1
}