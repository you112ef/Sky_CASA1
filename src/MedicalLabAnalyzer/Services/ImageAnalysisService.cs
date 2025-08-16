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
    public class ImageAnalysisService
    {
        private readonly ILogger<ImageAnalysisService> _logger;

        public ImageAnalysisService(ILogger<ImageAnalysisService> logger = null)
        {
            _logger = logger;
        }

        public async Task<ImageAnalysisResult> AnalyzeImageAsync(string imagePath, string analysisType)
        {
            try
            {
                _logger?.LogInformation("Starting image analysis: {ImagePath}, Type: {AnalysisType}", imagePath, analysisType);
                
                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException($"Image file not found: {imagePath}");
                }

                using var image = new Image<Bgr, byte>(imagePath);
                var result = await Task.Run(() => PerformImageAnalysis(image, analysisType));
                
                result.ImagePath = imagePath;
                result.AnalysisType = analysisType;
                result.AnalysisDate = DateTime.Now;
                result.IsSuccessful = true;
                
                _logger?.LogInformation("Image analysis completed successfully: {ImagePath}", imagePath);
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Image analysis failed: {ImagePath}", imagePath);
                return new ImageAnalysisResult
                {
                    ImagePath = imagePath,
                    AnalysisType = analysisType,
                    AnalysisDate = DateTime.Now,
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private ImageAnalysisResult PerformImageAnalysis(Image<Bgr, byte> image, string analysisType)
        {
            var result = new ImageAnalysisResult();
            
            // تحليل جودة الصورة
            var qualityMetrics = AnalyzeImageQuality(image);
            result.Results = new Dictionary<string, object>
            {
                { "Quality", qualityMetrics.OverallQuality },
                { "Resolution", $"{image.Width}x{image.Height}" },
                { "Format", Path.GetExtension(imagePath)?.ToUpperInvariant() ?? "Unknown" },
                { "Size", $"{new FileInfo(imagePath).Length / 1024.0:F1} KB" },
                { "Sharpness", qualityMetrics.Sharpness },
                { "Contrast", qualityMetrics.Contrast },
                { "Brightness", qualityMetrics.Brightness },
                { "Noise", qualityMetrics.Noise }
            };

            // تحليل نوعي حسب نوع التحليل
            switch (analysisType.ToLower())
            {
                case "casa":
                    var casaResults = AnalyzeCASAImage(image);
                    foreach (var item in casaResults)
                    {
                        result.Results[item.Key] = item.Value;
                    }
                    break;
                    
                case "cbc":
                    var cbcResults = AnalyzeCBCImage(image);
                    foreach (var item in cbcResults)
                    {
                        result.Results[item.Key] = item.Value;
                    }
                    break;
                    
                case "urine":
                    var urineResults = AnalyzeUrineImage(image);
                    foreach (var item in urineResults)
                    {
                        result.Results[item.Key] = item.Value;
                    }
                    break;
                    
                case "stool":
                    var stoolResults = AnalyzeStoolImage(image);
                    foreach (var item in stoolResults)
                    {
                        result.Results[item.Key] = item.Value;
                    }
                    break;
            }

            return result;
        }

        private ImageQualityMetrics AnalyzeImageQuality(Image<Bgr, byte> image)
        {
            // تحليل حدة الصورة باستخدام Laplacian
            using var gray = image.Convert<Gray, byte>();
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
            var noise = EstimateNoise(gray);
            
            var overallQuality = CalculateOverallQuality(sharpness, contrast, brightness, noise);
            
            return new ImageQualityMetrics
            {
                OverallQuality = overallQuality,
                Sharpness = Math.Min(100, Math.Max(0, sharpness / 2)),
                Contrast = Math.Min(100, Math.Max(0, contrast)),
                Brightness = Math.Min(100, Math.Max(0, brightness * 100 / 255)),
                Noise = Math.Min(100, Math.Max(0, noise * 100))
            };
        }

        private double EstimateNoise(Image<Gray, byte> image)
        {
            // تقدير الضوضاء باستخدام مرشح Gaussian
            using var blurred = new Image<Gray, byte>(image.Size);
            CvInvoke.GaussianBlur(image, blurred, new Size(3, 3), 0);
            
            using var diff = new Image<Gray, byte>(image.Size);
            CvInvoke.Absdiff(image, blurred, diff);
            
            return CvInvoke.Mean(diff).V0;
        }

        private string CalculateOverallQuality(double sharpness, double contrast, double brightness, double noise)
        {
            var score = (sharpness * 0.3 + contrast * 0.3 + brightness * 0.2 + (100 - noise) * 0.2);
            
            return score switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 40 => "Fair",
                >= 20 => "Poor",
                _ => "Very Poor"
            };
        }

        private Dictionary<string, object> AnalyzeCASAImage(Image<Bgr, byte> image)
        {
            var results = new Dictionary<string, object>();
            
            // تحليل CASA - البحث عن الحيوانات المنوية
            using var hsv = image.Convert<Hsv, byte>();
            using var mask = new Image<Gray, byte>(image.Size);
            
            // إنشاء قناع للحيوانات المنوية (لون أبيض-أصفر)
            var lower = new Hsv(0, 0, 200);
            var upper = new Hsv(60, 30, 255);
            CvInvoke.InRange(hsv, lower, upper, mask);
            
            // العثور على الكائنات
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(mask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var spermCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 50 && area < 500) // نطاق حجم الحيوان المنوي
                {
                    spermCount++;
                    totalArea += area;
                }
            }
            
            results["SpermCount"] = spermCount;
            results["AverageSpermSize"] = spermCount > 0 ? totalArea / spermCount : 0;
            results["Motility"] = EstimateMotility(image);
            
            return results;
        }

        private Dictionary<string, object> AnalyzeCBCImage(Image<Bgr, byte> image)
        {
            var results = new Dictionary<string, object>();
            
            // تحليل CBC - البحث عن خلايا الدم
            using var gray = image.Convert<Gray, byte>();
            using var binary = new Image<Gray, byte>(image.Size);
            
            // عتبة ثنائية للعثور على الخلايا
            CvInvoke.Threshold(gray, binary, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            
            // العثور على الكائنات
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(binary, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var cellCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 20 && area < 200) // نطاق حجم خلية الدم
                {
                    cellCount++;
                    totalArea += area;
                }
            }
            
            results["CellCount"] = cellCount;
            results["AverageCellSize"] = cellCount > 0 ? totalArea / cellCount : 0;
            results["CellDensity"] = cellCount / (image.Width * image.Height * 1e-6); // خلايا لكل مم²
            
            return results;
        }

        private Dictionary<string, object> AnalyzeUrineImage(Image<Bgr, byte> image)
        {
            var results = new Dictionary<string, object>();
            
            // تحليل البول - البحث عن البلورات والخلايا
            using var hsv = image.Convert<Hsv, byte>();
            using var mask = new Image<Gray, byte>(image.Size);
            
            // إنشاء قناع للبلورات (لون أصفر-بني)
            var lower = new Hsv(15, 50, 100);
            var upper = new Hsv(45, 255, 255);
            CvInvoke.InRange(hsv, lower, upper, mask);
            
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(mask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var crystalCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 10 && area < 100) // نطاق حجم البلورة
                {
                    crystalCount++;
                    totalArea += area;
                }
            }
            
            results["CrystalCount"] = crystalCount;
            results["AverageCrystalSize"] = crystalCount > 0 ? totalArea / crystalCount : 0;
            results["CrystalDensity"] = crystalCount / (image.Width * image.Height * 1e-6);
            
            return results;
        }

        private Dictionary<string, object> AnalyzeStoolImage(Image<Bgr, byte> image)
        {
            var results = new Dictionary<string, object>();
            
            // تحليل البراز - البحث عن الطفيليات والمواد العضوية
            using var gray = image.Convert<Gray, byte>();
            using var binary = new Image<Gray, byte>(image.Size);
            
            // عتبة ثنائية للعثور على المواد العضوية
            CvInvoke.Threshold(gray, binary, 0, 255, ThresholdType.Binary | ThresholdType.Otsu);
            
            using var contours = new VectorOfVectorOfPoint();
            using var hierarchy = new Mat();
            CvInvoke.FindContours(binary, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            
            var organicMatterCount = 0;
            var totalArea = 0.0;
            
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 30 && area < 300) // نطاق حجم المواد العضوية
                {
                    organicMatterCount++;
                    totalArea += area;
                }
            }
            
            results["OrganicMatterCount"] = organicMatterCount;
            results["AverageMatterSize"] = organicMatterCount > 0 ? totalArea / organicMatterCount : 0;
            results["MatterDensity"] = organicMatterCount / (image.Width * image.Height * 1e-6);
            
            return results;
        }

        private double EstimateMotility(Image<Bgr, byte> image)
        {
            // تقدير الحركة باستخدام تحليل الفرق بين الإطارات
            // هذا مثال مبسط - في التطبيق الحقيقي نحتاج إلى فيديو
            var random = new Random();
            return random.NextDouble() * 100; // قيمة عشوائية للتوضيح
        }

        public async Task<bool> ProcessBatchImagesAsync(List<string> imagePaths, string analysisType)
        {
            try
            {
                _logger?.LogInformation("Starting batch image processing: {Count} images, Type: {AnalysisType}", imagePaths.Count, analysisType);
                
                var tasks = imagePaths.Select(path => AnalyzeImageAsync(path, analysisType));
                var results = await Task.WhenAll(tasks);
                
                var successCount = results.Count(r => r.IsSuccessful);
                _logger?.LogInformation("Batch image processing completed: {SuccessCount}/{TotalCount} successful", successCount, imagePaths.Count);
                
                return successCount == imagePaths.Count;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Batch image processing failed");
                return false;
            }
        }

        public async Task<ImageQualityReport> AssessImageQualityAsync(string imagePath)
        {
            try
            {
                _logger?.LogInformation("Assessing image quality: {ImagePath}", imagePath);
                
                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException($"Image file not found: {imagePath}");
                }

                using var image = new Image<Bgr, byte>(imagePath);
                var qualityMetrics = await Task.Run(() => AnalyzeImageQuality(image));
                
                var report = new ImageQualityReport
                {
                    ImagePath = imagePath,
                    AssessmentDate = DateTime.Now,
                    OverallQuality = qualityMetrics.OverallQuality,
                    Sharpness = qualityMetrics.Sharpness,
                    Contrast = qualityMetrics.Contrast,
                    Brightness = qualityMetrics.Brightness,
                    Noise = qualityMetrics.Noise,
                    Recommendations = GenerateQualityRecommendations(qualityMetrics)
                };
                
                _logger?.LogInformation("Image quality assessment completed: {ImagePath}", imagePath);
                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Image quality assessment failed: {ImagePath}", imagePath);
                throw;
            }
        }

        private List<string> GenerateQualityRecommendations(ImageQualityMetrics metrics)
        {
            var recommendations = new List<string>();
            
            if (metrics.Sharpness < 60)
                recommendations.Add("Consider using a tripod or image stabilization");
            
            if (metrics.Contrast < 50)
                recommendations.Add("Adjust lighting conditions for better contrast");
            
            if (metrics.Brightness < 30 || metrics.Brightness > 80)
                recommendations.Add("Adjust exposure settings for optimal brightness");
            
            if (metrics.Noise > 30)
                recommendations.Add("Use lower ISO settings or noise reduction filters");
            
            if (recommendations.Count == 0)
                recommendations.Add("Image quality is excellent for analysis");
            
            return recommendations;
        }
    }

    public class ImageQualityMetrics
    {
        public string OverallQuality { get; set; }
        public double Sharpness { get; set; }
        public double Contrast { get; set; }
        public double Brightness { get; set; }
        public double Noise { get; set; }
    }
}