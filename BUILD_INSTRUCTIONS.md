# Medical Lab Analyzer - Build Instructions

## 🛠️ Building from Source

This guide provides detailed instructions for building the Medical Lab Analyzer system from source code.

## 📋 Prerequisites

### Required Software
- **Visual Studio 2022** (Community, Professional, or Enterprise)
- **.NET 8.0 SDK** (latest version)
- **Git** for version control
- **PowerShell** (Windows 10/11)

### Optional Software
- **NSIS** for installer creation
- **Inno Setup** for alternative installer
- **Docker** for containerized builds

### System Requirements
- **OS**: Windows 10/11 (64-bit)
- **RAM**: 8GB minimum, 16GB recommended
- **Storage**: 10GB free space
- **CPU**: Multi-core processor (Intel i5 or AMD equivalent)

## 🚀 Quick Build

### Option 1: Automated Build Script
```powershell
# Clone the repository
git clone https://github.com/your-repo/medical-lab-analyzer.git
cd medical-lab-analyzer

# Run the automated build script
.\build_offline.ps1

# The application will be built to the 'install' directory
```

### Option 2: Manual Build
```powershell
# Restore packages
dotnet restore

# Build in Release mode
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Publish application
dotnet publish src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj --configuration Release --output ./publish
```

## 🔧 Detailed Build Process

### Step 1: Environment Setup
```powershell
# Check .NET version
dotnet --version  # Should be 8.0.x

# Check Git
git --version

# Verify PowerShell execution policy
Get-ExecutionPolicy
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Step 2: Clone Repository
```powershell
# Clone with submodules (if any)
git clone --recursive https://github.com/your-repo/medical-lab-analyzer.git
cd medical-lab-analyzer

# Or if already cloned, update submodules
git submodule update --init --recursive
```

### Step 3: Package Management
```powershell
# Restore NuGet packages
dotnet restore --configfile nuget.config

# Verify packages are restored
dotnet list package
```

### Step 4: Build Configuration
```powershell
# Clean previous builds
dotnet clean

# Build in Debug mode (development)
dotnet build --configuration Debug

# Build in Release mode (production)
dotnet build --configuration Release

# Build with specific platform
dotnet build --configuration Release --runtime win-x64
```

### Step 5: Testing
```powershell
# Run all tests
dotnet test --configuration Release --verbosity normal

# Run specific test project
dotnet test src/MedicalLabAnalyzer.Tests/MedicalLabAnalyzer.Tests.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Step 6: Publishing
```powershell
# Publish self-contained (includes .NET runtime)
dotnet publish src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output ./publish/win-x64

# Publish framework-dependent (requires .NET runtime)
dotnet publish src/MedicalLabAnalyzer/MedicalLabAnalyzer.csproj \
    --configuration Release \
    --output ./publish/framework-dependent
```

## 📦 Advanced Build Options

### Offline Build
```powershell
# Build without internet connection
.\build_offline.ps1 -SkipTests -CreateInstaller

# Use local NuGet feed
dotnet restore --configfile nuget.config --source ./local-nuget
```

### Custom Configuration
```powershell
# Build with custom configuration
dotnet build --configuration Custom --property:DefineConstants=CUSTOM

# Build with specific version
dotnet build --property:Version=1.0.0.0 --property:FileVersion=1.0.0.0
```

### Multi-Platform Build
```powershell
# Build for multiple platforms
dotnet publish --configuration Release --runtime win-x64 --output ./publish/win-x64
dotnet publish --configuration Release --runtime win-x86 --output ./publish/win-x86
dotnet publish --configuration Release --runtime linux-x64 --output ./publish/linux-x64
```

## 🔨 Build Script Options

### PowerShell Build Script Parameters
```powershell
.\build_offline.ps1 -Configuration Release -Platform AnyCPU -OutputPath ".\install" -Clean -SkipTests -CreateInstaller
```

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-Configuration` | Build configuration (Debug/Release) | Release |
| `-Platform` | Target platform | AnyCPU |
| `-OutputPath` | Output directory | .\install |
| `-Clean` | Clean before building | false |
| `-SkipTests` | Skip running tests | false |
| `-CreateInstaller` | Create installer package | false |

### Examples
```powershell
# Quick development build
.\build_offline.ps1 -Configuration Debug -SkipTests

# Production build with installer
.\build_offline.ps1 -Configuration Release -Clean -CreateInstaller

# Custom output location
.\build_offline.ps1 -OutputPath "C:\MyApp" -CreateInstaller
```

## 🧪 Testing

### Unit Tests
```powershell
# Run all unit tests
dotnet test --configuration Release

# Run specific test class
dotnet test --filter "FullyQualifiedName~CasaAnalysisRealTest"

# Run tests with detailed output
dotnet test --configuration Release --verbosity detailed --logger "console;verbosity=detailed"
```

### Integration Tests
```powershell
# Run integration tests
dotnet test --filter "Category=Integration"

# Run with test data
dotnet test --environment "ASPNETCORE_ENVIRONMENT=Test"
```

### Performance Tests
```powershell
# Run performance benchmarks
dotnet test --filter "Category=Performance"

# Run with specific test data
dotnet test --filter "FullyQualifiedName~PerformanceTest" --logger "console;verbosity=detailed"
```

## 📦 Package Creation

### Create Installer
```powershell
# Using NSIS (if installed)
makensis installer.nsi

# Using Inno Setup (if installed)
iscc installer.iss
```

### Create Offline Package
```powershell
# Create complete offline package
.\build_offline.ps1 -CreateInstaller

# Package includes:
# - Application files
# - NuGet packages
# - Installation scripts
# - Documentation
```

### Create ZIP Archive
```powershell
# Create deployment ZIP
Compress-Archive -Path ".\publish\*" -DestinationPath "MedicalLabAnalyzer-v1.0.0.zip"

# Create source ZIP
git archive --format=zip --output=MedicalLabAnalyzer-Source-v1.0.0.zip HEAD
```

## 🔍 Troubleshooting

### Common Build Issues

#### Package Restore Fails
```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed

# Check network connectivity
Test-NetConnection api.nuget.org -Port 443
```

#### Build Errors
```powershell
# Clean solution
dotnet clean --configuration Release

# Delete bin and obj folders
Remove-Item -Recurse -Force src\MedicalLabAnalyzer\bin, src\MedicalLabAnalyzer\obj

# Rebuild
dotnet build --configuration Release --verbosity detailed
```

#### Test Failures
```powershell
# Run tests individually
dotnet test --filter "FullyQualifiedName~SpecificTest"

# Check test data
ls Samples/

# Verify database connection
Test-Path Database/medical_lab.db
```

#### EmguCV Issues
```powershell
# Verify EmguCV packages
dotnet list package | findstr Emgu

# Check native DLLs
ls src/MedicalLabAnalyzer/bin/Release/net8.0-windows/ | findstr opencv

# Install Visual C++ Redistributable
# Download from Microsoft website
```

### Performance Issues
```powershell
# Build with optimization
dotnet build --configuration Release --property:Optimize=true

# Use parallel builds
dotnet build --configuration Release --maxcpucount

# Monitor memory usage
Get-Process dotnet | Select-Object ProcessName, WorkingSet, CPU
```

## 📊 Build Metrics

### Performance Benchmarks
```powershell
# Measure build time
Measure-Command { dotnet build --configuration Release }

# Measure test execution time
Measure-Command { dotnet test --configuration Release }

# Measure publish time
Measure-Command { dotnet publish --configuration Release }
```

### Quality Metrics
```powershell
# Code coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Code analysis
dotnet build --configuration Release --verbosity normal | Select-String "warning|error"

# Package size
Get-ChildItem -Path ".\publish" -Recurse | Measure-Object -Property Length -Sum
```

## 🚀 Deployment

### Local Deployment
```powershell
# Copy to local directory
Copy-Item -Path ".\publish\*" -Destination "C:\MedicalLabAnalyzer" -Recurse

# Create desktop shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$Home\Desktop\Medical Lab Analyzer.lnk")
$Shortcut.TargetPath = "C:\MedicalLabAnalyzer\MedicalLabAnalyzer.exe"
$Shortcut.Save()
```

### Network Deployment
```powershell
# Copy to network share
Copy-Item -Path ".\publish\*" -Destination "\\server\share\MedicalLabAnalyzer" -Recurse

# Set permissions
icacls "\\server\share\MedicalLabAnalyzer" /grant "Users:(OI)(CI)RX"
```

### Silent Installation
```powershell
# Silent installer
.\MedicalLabAnalyzer-Setup.exe /S /D=C:\MedicalLabAnalyzer

# Unattended installation
.\MedicalLabAnalyzer-Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
```

## 📚 Additional Resources

### Documentation
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [EmguCV Documentation](https://www.emgu.com/wiki/index.php/Main_Page)

### Tools
- [Visual Studio](https://visualstudio.microsoft.com/)
- [NSIS](https://nsis.sourceforge.io/)
- [Inno Setup](https://jrsoftware.org/isinfo.php)

### Community
- [Stack Overflow](https://stackoverflow.com/questions/tagged/wpf)
- [GitHub Discussions](https://github.com/your-repo/discussions)
- [Microsoft Q&A](https://docs.microsoft.com/en-us/answers/)

---

## 📝 Build Checklist

### Pre-Build
- [ ] Install .NET 8.0 SDK
- [ ] Install Visual Studio 2022
- [ ] Clone repository
- [ ] Check prerequisites

### Build
- [ ] Restore packages
- [ ] Build solution
- [ ] Run tests
- [ ] Fix any errors

### Post-Build
- [ ] Test application
- [ ] Create installer
- [ ] Package for distribution
- [ ] Document changes

---

**Medical Lab Analyzer** - Professional CASA Analysis System  
*Built with ❤️ for medical research and laboratory excellence*