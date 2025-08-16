# Sample Videos for CASA Analysis

This directory contains sample video files for testing and demonstrating the CASA analysis capabilities of the Medical Lab Analyzer system.

## 📹 Video Requirements

### Supported Formats
- **MP4** (H.264 codec) - Recommended
- **AVI** (uncompressed or compressed)
- **MOV** (QuickTime format)
- **WMV** (Windows Media Video)

### Recommended Specifications
- **Resolution**: 640x480 minimum, 1920x1080 preferred
- **Frame Rate**: 25-30 FPS (standard), 60 FPS (high-speed)
- **Duration**: 10-60 seconds for analysis
- **Quality**: Good lighting, minimal background noise
- **Focus**: Sharp focus on sperm cells

### File Naming Convention
```
sperm_sample_[date]_[magnification]x_[duration]s.mp4
Example: sperm_sample_20241219_40x_30s.mp4
```

## 🔬 Sample Videos

### Included Samples
- `sperm_sample.mp4` - Default sample video for testing
- `sperm_sample_40x_30s.mp4` - 40x magnification, 30 seconds
- `sperm_sample_100x_60s.mp4` - 100x magnification, 60 seconds

### Video Characteristics
Each sample video should include:
- Clear visualization of sperm cells
- Consistent lighting throughout
- Minimal background artifacts
- Appropriate magnification for analysis
- Sufficient duration for meaningful analysis

## 📊 Expected Results

### Typical CASA Parameters
Based on standard sperm analysis:

| Parameter | Normal Range | Units |
|-----------|-------------|-------|
| VCL | 30-100 | µm/s |
| VSL | 20-80 | µm/s |
| VAP | 25-90 | µm/s |
| ALH | 2-8 | µm |
| BCF | 5-20 | Hz |
| LIN | 0.3-0.8 | - |
| STR | 0.5-0.9 | - |
| WOB | 0.4-0.8 | - |

### Quality Metrics
- **Track Count**: 10-50 tracks per video
- **Average Duration**: 2-10 seconds per track
- **Quality Score**: 0.5-1.0 (higher is better)
- **Detection Rate**: >80% of visible sperm

## 🧪 Testing Procedures

### 1. Basic Functionality Test
```csharp
// Test basic video loading and analysis
MedicalLabAnalyzer.Tests.CasaAnalysisRealTest.RunReal(
    videoPath: "Samples/sperm_sample.mp4"
);
```

### 2. Calibration Test
```csharp
// Test calibration with known values
MedicalLabAnalyzer.Tests.CasaAnalysisRealTest.RunCalibrationTest();
```

### 3. Performance Test
```csharp
// Test with different video sizes and durations
// Monitor processing time and memory usage
```

## 🔧 Calibration Setup

### For Sample Videos
1. **Measure Calibration Slide**: Use a calibration slide with known distances
2. **Calculate µm/pixel**: Measure pixel distance between calibration marks
3. **Set Calibration**: Enter values in the calibration interface
4. **Validate**: Run test analysis to verify calibration

### Example Calibration
```
Known distance: 100 µm
Measured pixels: 200 px
Microns per pixel: 100/200 = 0.5 µm/px
```

## 📝 Notes

### Video Quality Tips
- Ensure good microscope focus
- Use consistent lighting
- Minimize camera movement
- Avoid compression artifacts
- Use appropriate magnification

### Analysis Tips
- Start with shorter videos for testing
- Verify calibration before analysis
- Check track quality scores
- Review individual track results
- Export results for further analysis

### Troubleshooting
- **No tracks detected**: Check video quality and calibration
- **Poor tracking**: Adjust analysis parameters
- **Slow processing**: Reduce video resolution or duration
- **Memory issues**: Process shorter video segments

## 📞 Support

For questions about sample videos or analysis:
- Check the main README.md file
- Review the user manual
- Contact support: support@medicallabsolutions.com

---

**Note**: These sample videos are for testing and demonstration purposes. For clinical use, ensure proper validation and regulatory compliance.