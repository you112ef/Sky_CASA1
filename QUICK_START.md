# Medical Lab Analyzer - Quick Start Guide

## 🚀 Get Started in 5 Minutes

This guide will help you get the Medical Lab Analyzer system up and running quickly for CASA analysis.

## 📋 Prerequisites

### System Requirements
- ✅ Windows 10/11 (64-bit)
- ✅ .NET 8.0 Runtime
- ✅ 4GB RAM minimum
- ✅ 2GB free disk space

### Download & Install
1. **Download** the latest release from [Releases](https://github.com/your-repo/releases)
2. **Extract** the ZIP file to a folder
3. **Run** `MedicalLabAnalyzer.exe` as Administrator

## 🔧 First-Time Setup

### 1. Initial Configuration
```
1. Launch MedicalLabAnalyzer.exe
2. Accept the license agreement
3. The system will create the database automatically
4. Default admin credentials: admin / admin123
```

### 2. Calibration Setup
```
1. Go to Tools → Calibration
2. Enter your microscope calibration:
   - Microns per Pixel: 0.5 (typical for 40x)
   - Frame Rate: 25 (or your camera's FPS)
   - Objective: 40x
   - Camera Model: Your camera name
3. Click "Save Calibration"
```

### 3. Sample Video
```
1. Place your sperm analysis video in the Samples folder
2. Supported formats: MP4, AVI, MOV
3. Recommended: 640x480 or higher resolution
4. Duration: 10-60 seconds
```

## 🧪 Run Your First Analysis

### Step 1: Load Video
```
1. Click "New Analysis" or File → New Analysis
2. Browse to your video file
3. Select the video and click "Open"
```

### Step 2: Configure Analysis
```
1. Verify calibration settings
2. Adjust analysis parameters if needed:
   - Min Blob Area: 6 (default)
   - Max Match Distance: 60 (default)
   - Max Missed Frames: 8 (default)
3. Click "Start Analysis"
```

### Step 3: Monitor Progress
```
- Progress bar shows analysis status
- Real-time track count display
- Estimated time remaining
- Cancel option available
```

### Step 4: Review Results
```
Analysis complete! Review your results:

📊 Summary Results:
- VCL: 45.23 µm/s
- VSL: 38.67 µm/s  
- VAP: 42.15 µm/s
- ALH: 3.45 µm
- BCF: 12.34 Hz
- Track Count: 15

📈 Individual Tracks:
- Track ID, Duration, VCL, VSL, Quality
- Click any track for detailed view
```

## 📊 Understanding Results

### CASA Parameters
| Parameter | Description | Normal Range |
|-----------|-------------|--------------|
| **VCL** | Curvilinear Velocity | 30-100 µm/s |
| **VSL** | Straight Line Velocity | 20-80 µm/s |
| **VAP** | Average Path Velocity | 25-90 µm/s |
| **ALH** | Amplitude of Lateral Head | 2-8 µm |
| **BCF** | Beat Cross Frequency | 5-20 Hz |
| **LIN** | Linearity (VSL/VCL) | 0.3-0.8 |
| **STR** | Straightness (VSL/VAP) | 0.5-0.9 |
| **WOB** | Wobble (VAP/VCL) | 0.4-0.8 |

### Quality Indicators
- **Track Count**: Number of valid sperm tracks
- **Quality Score**: 0.0-1.0 (higher is better)
- **Duration**: Track duration in seconds
- **Detection Rate**: Percentage of visible sperm detected

## 💾 Export Results

### Export Options
```
1. CSV Export: File → Export → CSV
   - Individual track data
   - Summary statistics
   - Analysis parameters

2. PDF Report: File → Export → PDF
   - Professional report format
   - Charts and graphs
   - Analysis summary

3. Excel Export: File → Export → Excel
   - Multiple worksheets
   - Formatted data
   - Charts and analysis
```

## 🔧 Troubleshooting

### Common Issues

#### No Tracks Detected
```
Possible causes:
- Video quality too low
- Incorrect calibration
- Analysis parameters need adjustment

Solutions:
1. Check video quality and focus
2. Verify calibration values
3. Reduce Min Blob Area parameter
4. Increase Max Match Distance
```

#### Poor Tracking Quality
```
Possible causes:
- Video resolution too low
- Insufficient lighting
- Camera movement

Solutions:
1. Use higher resolution video
2. Improve microscope lighting
3. Stabilize camera setup
4. Adjust analysis parameters
```

#### Slow Processing
```
Possible causes:
- Large video file
- High resolution
- Limited system resources

Solutions:
1. Use shorter video segments
2. Reduce video resolution
3. Close other applications
4. Increase system RAM
```

## 📞 Getting Help

### Documentation
- 📖 [User Manual](docs/UserManual.md)
- 🔧 [Configuration Guide](docs/Configuration.md)
- 🐛 [Troubleshooting Guide](docs/Troubleshooting.md)

### Support
- 📧 Email: support@medicallabsolutions.com
- 🐛 Issues: [GitHub Issues](https://github.com/your-repo/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/your-repo/discussions)

### Community
- 👥 User Forum: [Community Forum](https://forum.medicallabsolutions.com)
- 📹 Video Tutorials: [YouTube Channel](https://youtube.com/medicallabsolutions)
- 📚 Knowledge Base: [Knowledge Base](https://kb.medicallabsolutions.com)

## 🎯 Next Steps

### Advanced Features
1. **Batch Processing**: Analyze multiple videos
2. **Custom Reports**: Create custom report templates
3. **Data Analysis**: Advanced statistical analysis
4. **Quality Control**: Implement quality control procedures

### Best Practices
1. **Regular Calibration**: Calibrate before each session
2. **Quality Control**: Use control samples regularly
3. **Data Backup**: Regular database backups
4. **Training**: Train staff on proper procedures

### Validation
1. **Method Validation**: Validate against reference methods
2. **Quality Assurance**: Implement QA procedures
3. **Documentation**: Maintain proper documentation
4. **Compliance**: Ensure regulatory compliance

---

## ⚡ Quick Commands

### Run Test Analysis
```powershell
# From command line
MedicalLabAnalyzer.exe --test --video "Samples/sperm_sample.mp4"
```

### Run Calibration Test
```powershell
# Test calibration system
MedicalLabAnalyzer.exe --calibration-test
```

### Export Results
```powershell
# Export to CSV
MedicalLabAnalyzer.exe --export-csv "results.csv"
```

---

**🎉 Congratulations!** You're now ready to perform professional CASA analysis with the Medical Lab Analyzer system.

For detailed information, refer to the complete [User Manual](docs/UserManual.md) and [Technical Documentation](docs/Technical.md).

---

**Medical Lab Analyzer** - Professional CASA Analysis System  
*Built with ❤️ for medical research and laboratory excellence*