using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using System.IO;
using System.Linq;

namespace MedicalLabAnalyzer.Services
{
    public class VideoAnalysisService
    {
        private readonly ILogger<VideoAnalysisService> _logger;

        public VideoAnalysisService(ILogger<VideoAnalysisService> logger = null)
        {
            _logger = logger;
        }

        public async Task<VideoAnalysisResult> AnalyzeVideoAsync(string videoPath, string analysisType)
        {
            try
            {
                _logger?.LogInformation("Starting video analysis: {VideoPath}, Type: {AnalysisType}", videoPath, analysisType);
                
                if (!File.Exists(videoPath))
                {
                    throw new FileNotFoundException($"Video file not found: {videoPath}");
                }

                var result = await Task.Run(() => PerformVideoAnalysis(videoPath, analysisType));
                
                result.VideoPath = videoPath;
                result.AnalysisType = analysisType;
                result.AnalysisDate = DateTime.Now;
                result.IsSuccessful = true;
                
                _logger?.LogInformation("Video analysis completed successfully: {VideoPath}", videoPath);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Video analysis failed: {VideoPath}", videoPath);
                return new VideoAnalysisResult
                {
                    VideoPath = videoPath,
                    AnalysisType = analysisType,
                    AnalysisDate = DateTime.Now,
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private VideoAnalysisResult PerformVideoAnalysis(string videoPath, string analysisType)
        {
            var result = new VideoAnalysisResult();
            
            using var capture = new VideoCapture(videoPath);
            if (!capture.IsOpened)
            {
                throw new InvalidOperationException($"Failed to open video file: {videoPath}");
            }

            // الحصول على معلومات الفيديو
            var frameCount = (int)capture.Get(CapProp.FrameCount);
            var fps = capture.Get(CapProp.Fps);
            var width = (int)capture.Get(CapProp.FrameWidth);
            var height = (int)capture.Get(CapProp.FrameHeight);
            var duration = frameCount / fps;

            result.Duration = duration;
            result.FrameRate = fps;
            result.Resolution = $"{width}x{height}";

            // تحليل نوعي حسب نوع التحليل
            switch (analysisType.ToLower())
            {
                case "casa":
                    var casaResults = AnalyzeCASAVideo(capture, frameCount, fps);
                    result.Results = casaResults;
                    break;
                    
                case "cbc":
                    var cbcResults = AnalyzeCBCVideo(capture, frameCount, fps);
                    result.Results = cbcResults;
                    break;
                    
                case "urine":
                    var urineResults = AnalyzeUrineVideo(capture, frameCount, fps);
                    result.Results = urineResults;
                    break;
                    
                case "stool":
                    var stoolResults = AnalyzeStoolVideo(capture, frameCount, fps);
                    result.Results = stoolResults;
                    break;
                    
                default:
                    var generalResults = AnalyzeGeneralVideo(capture, frameCount, fps);
                    result.Results = generalResults;
                    break;
            }

            return result;
        }

        private Dictionary<string, object> AnalyzeCASAVideo(VideoCapture capture, int frameCount, double fps)
        {
            var results = new Dictionary<string, object>();
            var totalSpermCount = 0;
            var totalMotility = 0.0;
            var processedFrames = 0;
            var tracks = new List<TrackPoint>();

            // تحليل كل 5 إطارات لتحسين الأداء
            var frameInterval = Math.Max(1, frameCount / 100);
            
            for (int i = 0; i < frameCount; i += frameInterval)
            {
                capture.Set(CapProp.PosFrames, i);
                using var frame = new Mat();
                if (capture.Read(frame))
                {
                    var frameAnalysis = AnalyzeCASAFrame(frame);
                    totalSpermCount += frameAnalysis.SpermCount;
                    totalMotility += frameAnalysis.Motility;
                    tracks.AddRange(frameAnalysis.Tracks);
                    processedFrames++;
                }
            }

            results["TotalFrames"] = frameCount;
            results["ProcessedFrames"] = processedFrames;
            results["AverageSpermCount"] = processedFrames > 0 ? totalSpermCount / processedFrames : 0;
            results["AverageMotility"] = processedFrames > 0 ? totalMotility / processedFrames : 0;
            results["TotalTracks"] = tracks.Count;
            results["AverageVelocity"] = CalculateAverageVelocity(tracks, fps);
            results["MotilityPercentage"] = CalculateMotilityPercentage(tracks);

            return results;
        }

        private Dictionary<string, object> AnalyzeCBCVideo(VideoCapture capture, int frameCount, double fps)
        {
            var results = new Dictionary<string, object>();
            var totalCellCount = 0;
            var processedFrames = 0;
            var cellDensities = new List<double>();

            var frameInterval = Math.Max(1, frameCount / 50);
            
            for (int i = 0; i < frameCount; i += frameInterval)
            {
                capture.Set(CapProp.PosFrames, i);
                using var frame = new Mat();
                if (capture.Read(frame))
                {
                    var frameAnalysis = AnalyzeCBCFrame(frame);
                    totalCellCount += frameAnalysis.CellCount;
                    cellDensities.Add(frameAnalysis.CellDensity);
                    processedFrames++;
                }
            }

            results["TotalFrames"] = frameCount;
            results["ProcessedFrames"] = processedFrames;
            results["AverageCellCount"] = processedFrames > 0 ? totalCellCount / processedFrames : 0;
            results["AverageCellDensity"] = cellDensities.Count > 0 ? cellDensities.Average() : 0;
            results["CellCountVariation"] = CalculateVariation(cellDensities);

            return results;
        }

        private Dictionary<string, object> AnalyzeUrineVideo(VideoCapture capture, int frameCount, double fps)
        {
            var results = new Dictionary<string, object>();
            var totalCrystalCount = 0;
            var processedFrames = 0;
            var crystalSizes = new List<double>();

            var frameInterval = Math.Max(1, frameCount / 50);
            
            for (int i = 0; i < frameCount; i += frameInterval)
            {
                capture.Set(CapProp.PosFrames, i);
                using var frame = new Mat();
                if (capture.Read(frame))
                {
                    var frameAnalysis = AnalyzeUrineFrame(frame);
                    totalCrystalCount += frameAnalysis.CrystalCount;
                    crystalSizes.Add(frameAnalysis.AverageCrystalSize);
                    processedFrames++;
                }
            }

            results["TotalFrames"] = frameCount;
            results["ProcessedFrames"] = processedFrames;
            results["AverageCrystalCount"] = processedFrames > 0 ? totalCrystalCount / processedFrames : 0;
            results["AverageCrystalSize"] = crystalSizes.Count > 0 ? crystalSizes.Average() : 0;
            results["CrystalSizeVariation"] = CalculateVariation(crystalSizes);

            return results;
        }

        private Dictionary<string, object> AnalyzeStoolVideo(VideoCapture capture, int frameCount, double fps)
        {
            var results = new Dictionary<string, object>();
            var totalMatterCount = 0;
            var processedFrames = 0;
            var matterSizes = new List<double>();

            var frameInterval = Math.Max(1, frameCount / 50);
            
            for (int i = 0; i < frameCount; i += frameInterval)
            {
                capture.Set(CapProp.PosFrames, i);
                using var frame = new Mat();
                if (capture.Read(frame))
                {
                    var frameAnalysis = AnalyzeStoolFrame(frame);
                    totalMatterCount += frameAnalysis.OrganicMatterCount;
                    matterSizes.Add(frameAnalysis.AverageMatterSize);
                    processedFrames++;
                }
            }

            results["TotalFrames"] = frameCount;
            results["ProcessedFrames"] = processedFrames;
            results["AverageMatterCount"] = processedFrames > 0 ? totalMatterCount / processedFrames : 0;
            results["AverageMatterSize"] = matterSizes.Count > 0 ? matterSizes.Average() : 0;
            results["MatterSizeVariation"] = CalculateVariation(matterSizes);

            return results;
        }

        private Dictionary<string, object> AnalyzeGeneralVideo(VideoCapture capture, int frameCount, double fps)
        {
            var results = new Dictionary<string, object>();
            var processedFrames = 0;
            var motionLevels = new List<double>();

            var frameInterval = Math.Max(1, frameCount / 100);
            
            for (int i = 0; i < frameCount; i += frameInterval)
            {
                capture.Set(CapProp.PosFrames, i);
                using var frame = new Mat();
                if (capture.Read(frame))
                {
                    var motionLevel = AnalyzeMotionLevel(frame);
                    motionLevels.Add(motionLevel);
                    processedFrames++;
                }
            }

            results["TotalFrames"] = frameCount;
            results["ProcessedFrames"] = processedFrames;
            results["AverageMotionLevel"] = motionLevels.Count > 0 ? motionLevels.Average() : 0;
            results["MotionVariation"] = CalculateVariation(motionLevels);

            return results;
        }

        private CASAFrameAnalysis AnalyzeCASAFrame(Mat frame)
        {
            using var hsv = new Mat();
            CvInvoke.CvtColor(frame, hsv, ColorConversion.Bgr2Hsv);
            
            using var mask = new Mat();
            var lower = new ScalarArray(new MCvScalar(0, 0, 200));
            var upper = new ScalarArray(new MCvScalar(60, 30, 255));
            CvInvoke.InRange(hsv, lower, upper, mask);
            
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(mask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var spermCount = 0;
            var tracks = new List<TrackPoint>();
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 50 && area < 500)
                {
                    spermCount++;
                    var moments = CvInvoke.Moments(contours[i]);
                    if (moments.M00 > 0)
                    {
                        var centerX = moments.M10 / moments.M00;
                        var centerY = moments.M01 / moments.M00;
                        tracks.Add(new TrackPoint { X = centerX, Y = centerY, Area = area });
                    }
                }
            }
            
            return new CASAFrameAnalysis
            {
                SpermCount = spermCount,
                Motility = CalculateFrameMotility(tracks),
                Tracks = tracks
            };
        }

        private CBCFrameAnalysis AnalyzeCBCFrame(Mat frame)
        {
            using var gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
            
            using var binary = new Mat();
            CvInvoke.Threshold(gray, binary, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(binary, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var cellCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 20 && area < 200)
                {
                    cellCount++;
                    totalArea += area;
                }
            }
            
            var cellDensity = cellCount / (frame.Width * frame.Height * 1e-6);
            
            return new CBCFrameAnalysis
            {
                CellCount = cellCount,
                CellDensity = cellDensity
            };
        }

        private UrineFrameAnalysis AnalyzeUrineFrame(Mat frame)
        {
            using var hsv = new Mat();
            CvInvoke.CvtColor(frame, hsv, ColorConversion.Bgr2Hsv);
            
            using var mask = new Mat();
            var lower = new ScalarArray(new MCvScalar(15, 50, 100));
            var upper = new ScalarArray(new MCvScalar(45, 255, 255));
            CvInvoke.InRange(hsv, lower, upper, mask);
            
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(mask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var crystalCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 10 && area < 100)
                {
                    crystalCount++;
                    totalArea += area;
                }
            }
            
            var averageCrystalSize = crystalCount > 0 ? totalArea / crystalCount : 0;
            
            return new UrineFrameAnalysis
            {
                CrystalCount = crystalCount,
                AverageCrystalSize = averageCrystalSize
            };
        }

        private StoolFrameAnalysis AnalyzeStoolFrame(Mat frame)
        {
            using var gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
            
            using var binary = new Mat();
            CvInvoke.Threshold(gray, binary, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(binary, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var organicMatterCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 30 && area < 300)
                {
                    organicMatterCount++;
                    totalArea += area;
                }
            }
            
            var averageMatterSize = organicMatterCount > 0 ? totalArea / organicMatterCount : 0;
            
            return new StoolFrameAnalysis
            {
                OrganicMatterCount = organicMatterCount,
                AverageMatterSize = averageMatterSize
            };
        }

        private double AnalyzeMotionLevel(Mat frame)
        {
            // تحليل مستوى الحركة باستخدام مرشح Sobel
            using var gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
            
            using var sobelX = new Mat();
            using var sobelY = new Mat();
            CvInvoke.Sobel(gray, sobelX, DepthType.Cv16S, 1, 0, 3);
            CvInvoke.Sobel(gray, sobelY, DepthType.Cv16S, 0, 1, 3);
            
            using var magnitude = new Mat();
            CvInvoke.Magnitude(sobelX, sobelY, magnitude);
            
            return CvInvoke.Mean(magnitude).V0;
        }

        private double CalculateFrameMotility(List<TrackPoint> tracks)
        {
            if (tracks.Count < 2) return 0;
            
            var totalDistance = 0.0;
            for (int i = 1; i < tracks.Count; i++)
            {
                var dx = tracks[i].X - tracks[i - 1].X;
                var dy = tracks[i].Y - tracks[i - 1].Y;
                totalDistance += Math.Sqrt(dx * dx + dy * dy);
            }
            
            return totalDistance / (tracks.Count - 1);
        }

        private double CalculateAverageVelocity(List<TrackPoint> tracks, double fps)
        {
            if (tracks.Count < 2) return 0;
            
            var totalVelocity = 0.0;
            for (int i = 1; i < tracks.Count; i++)
            {
                var dx = tracks[i].X - tracks[i - 1].X;
                var dy = tracks[i].Y - tracks[i - 1].Y;
                var distance = Math.Sqrt(dx * dx + dy * dy);
                totalVelocity += distance * fps;
            }
            
            return totalVelocity / (tracks.Count - 1);
        }

        private double CalculateMotilityPercentage(List<TrackPoint> tracks)
        {
            if (tracks.Count == 0) return 0;
            
            var movingTracks = tracks.Count(t => t.Area > 0);
            return (double)movingTracks / tracks.Count * 100;
        }

        private double CalculateVariation(List<double> values)
        {
            if (values.Count < 2) return 0;
            
            var mean = values.Average();
            var variance = values.Sum(v => Math.Pow(v - mean, 2)) / values.Count;
            return Math.Sqrt(variance);
        }

        public async Task<bool> ExtractFramesAsync(string videoPath, string outputDirectory, int frameInterval = 1)
        {
            try
            {
                _logger?.LogInformation("Extracting frames from video: {VideoPath}", videoPath);
                
                if (!File.Exists(videoPath))
                {
                    throw new FileNotFoundException($"Video file not found: {videoPath}");
                }

                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                using var capture = new VideoCapture(videoPath);
                if (!capture.IsOpened)
                {
                    throw new InvalidOperationException($"Failed to open video file: {videoPath}");
                }

                var frameCount = (int)capture.Get(CapProp.FrameCount);
                var extractedCount = 0;
                
                for (int i = 0; i < frameCount; i += frameInterval)
                {
                    capture.Set(CapProp.PosFrames, i);
                    using var frame = new Mat();
                    if (capture.Read(frame))
                    {
                        var outputPath = Path.Combine(outputDirectory, $"frame_{i:D6}.jpg");
                        CvInvoke.Imwrite(outputPath, frame);
                        extractedCount++;
                    }
                }
                
                _logger?.LogInformation("Frame extraction completed: {VideoPath}, {ExtractedCount} frames extracted", videoPath, extractedCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Frame extraction failed: {VideoPath}", videoPath);
                return false;
            }
        }

        public async Task<VideoQualityReport> AssessVideoQualityAsync(string videoPath)
        {
            try
            {
                _logger?.LogInformation("Assessing video quality: {VideoPath}", videoPath);
                
                if (!File.Exists(videoPath))
                {
                    throw new FileNotFoundException($"Video file not found: {videoPath}");
                }

                using var capture = new VideoCapture(videoPath);
                if (!capture.IsOpened)
                {
                    throw new InvalidOperationException($"Failed to open video file: {videoPath}");
                }

                var frameCount = (int)capture.Get(CapProp.FrameCount);
                var fps = capture.Get(CapProp.Fps);
                var width = (int)capture.Get(CapProp.FrameWidth);
                var height = (int)capture.Get(CapProp.FrameHeight);
                var duration = frameCount / fps;
                var fileSize = new FileInfo(videoPath).Length;
                var codec = GetVideoCodec(capture);
                
                var qualityMetrics = await Task.Run(() => AnalyzeVideoQuality(capture, frameCount));
                
                var report = new VideoQualityReport
                {
                    VideoPath = videoPath,
                    AssessmentDate = DateTime.Now,
                    OverallQuality = qualityMetrics.OverallQuality,
                    Resolution = $"{width}x{height}",
                    FrameRate = fps,
                    Duration = duration,
                    FileSize = $"{fileSize / (1024.0 * 1024.0):F1} MB",
                    Codec = codec,
                    Sharpness = qualityMetrics.Sharpness,
                    Contrast = qualityMetrics.Contrast,
                    Brightness = qualityMetrics.Brightness,
                    Noise = qualityMetrics.Noise,
                    Recommendations = GenerateVideoQualityRecommendations(qualityMetrics, fps, width, height)
                };
                
                _logger?.LogInformation("Video quality assessment completed: {VideoPath}", videoPath);
                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Video quality assessment failed: {VideoPath}", videoPath);
                throw;
            }
        }

        private VideoQualityMetrics AnalyzeVideoQuality(VideoCapture capture, int frameCount)
        {
            var sharpnessValues = new List<double>();
            var contrastValues = new List<double>();
            var brightnessValues = new List<double>();
            var noiseValues = new List<double>();
            
            var frameInterval = Math.Max(1, frameCount / 20);
            
            for (int i = 0; i < frameCount; i += frameInterval)
            {
                capture.Set(CapProp.PosFrames, i);
                using var frame = new Mat();
                if (capture.Read(frame))
                {
                    var metrics = AnalyzeFrameQuality(frame);
                    sharpnessValues.Add(metrics.Sharpness);
                    contrastValues.Add(metrics.Contrast);
                    brightnessValues.Add(metrics.Brightness);
                    noiseValues.Add(metrics.Noise);
                }
            }
            
            return new VideoQualityMetrics
            {
                OverallQuality = CalculateOverallVideoQuality(sharpnessValues, contrastValues, brightnessValues, noiseValues),
                Sharpness = sharpnessValues.Count > 0 ? sharpnessValues.Average() : 0,
                Contrast = contrastValues.Count > 0 ? contrastValues.Average() : 0,
                Brightness = brightnessValues.Count > 0 ? brightnessValues.Average() : 0,
                Noise = noiseValues.Count > 0 ? noiseValues.Average() : 0
            };
        }

        private FrameQualityMetrics AnalyzeFrameQuality(Mat frame)
        {
            using var gray = new Mat();
            CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);
            
            // تحليل الحدة
            using var laplacian = new Mat();
            CvInvoke.Laplacian(gray, laplacian, DepthType.Cv16S, 3);
            var sharpness = CvInvoke.Mean(laplacian).V0;
            
            // تحليل التباين
            var mean = CvInvoke.Mean(gray);
            var stdDev = new MCvScalar();
            CvInvoke.MeanStdDev(gray, ref mean, ref stdDev);
            var contrast = stdDev.V0;
            
            // تحليل السطوع
            var brightness = mean.V0;
            
            // تحليل الضوضاء
            using var blurred = new Mat();
            CvInvoke.GaussianBlur(gray, blurred, new Size(3, 3), 0);
            using var diff = new Mat();
            CvInvoke.Absdiff(gray, blurred, diff);
            var noise = CvInvoke.Mean(diff).V0;
            
            return new FrameQualityMetrics
            {
                Sharpness = Math.Min(100, Math.Max(0, sharpness / 2)),
                Contrast = Math.Min(100, Math.Max(0, contrast)),
                Brightness = Math.Min(100, Math.Max(0, brightness * 100 / 255)),
                Noise = Math.Min(100, Math.Max(0, noise * 100))
            };
        }

        private string CalculateOverallVideoQuality(List<double> sharpness, List<double> contrast, List<double> brightness, List<double> noise)
        {
            var avgSharpness = sharpness.Count > 0 ? sharpness.Average() : 0;
            var avgContrast = contrast.Count > 0 ? contrast.Average() : 0;
            var avgBrightness = brightness.Count > 0 ? brightness.Average() : 0;
            var avgNoise = noise.Count > 0 ? noise.Average() : 0;
            
            var score = (avgSharpness * 0.3 + avgContrast * 0.3 + avgBrightness * 0.2 + (100 - avgNoise) * 0.2);
            
            return score switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 40 => "Fair",
                >= 20 => "Poor",
                _ => "Very Poor"
            };
        }

        private string GetVideoCodec(VideoCapture capture)
        {
            try
            {
                var fourcc = (int)capture.Get(CapProp.Fourcc);
                var codec = "";
                for (int i = 0; i < 4; i++)
                {
                    codec += (char)((fourcc >> (i * 8)) & 0xFF);
                }
                return codec.Trim('\0');
            }
            catch
            {
                return "Unknown";
            }
        }

        private List<string> GenerateVideoQualityRecommendations(VideoQualityMetrics metrics, double fps, int width, int height)
        {
            var recommendations = new List<string>();
            
            if (metrics.Sharpness < 60)
                recommendations.Add("Consider using higher resolution camera or better optics");
            
            if (metrics.Contrast < 50)
                recommendations.Add("Improve lighting conditions for better contrast");
            
            if (metrics.Brightness < 30 || metrics.Brightness > 80)
                recommendations.Add("Adjust camera exposure settings");
            
            if (metrics.Noise > 30)
                recommendations.Add("Use lower ISO settings or noise reduction");
            
            if (fps < 25)
                recommendations.Add("Consider higher frame rate for better motion analysis");
            
            if (width < 1280 || height < 720)
                recommendations.Add("Higher resolution would improve analysis accuracy");
            
            if (recommendations.Count == 0)
                recommendations.Add("Video quality is excellent for analysis");
            
            return recommendations;
        }
    }

    public class CASAFrameAnalysis
    {
        public int SpermCount { get; set; }
        public double Motility { get; set; }
        public List<TrackPoint> Tracks { get; set; } = new List<TrackPoint>();
    }

    public class CBCFrameAnalysis
    {
        public int CellCount { get; set; }
        public double CellDensity { get; set; }
    }

    public class UrineFrameAnalysis
    {
        public int CrystalCount { get; set; }
        public double AverageCrystalSize { get; set; }
    }

    public class StoolFrameAnalysis
    {
        public int OrganicMatterCount { get; set; }
        public double AverageMatterSize { get; set; }
    }

    public class FrameQualityMetrics
    {
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
    }

    public class VideoQualityMetrics
    {
        public string OverallQuality { get; set; }
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
    }

    public class TrackPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Area { get; set; }
    }
}