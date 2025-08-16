using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MedicalLabAnalyzer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace MedicalLabAnalyzer.Services
{
    public interface IVideoAnalysisService
    {
        Task<VideoAnalysisResult> AnalyzeVideoAsync(string videoPath, int examId);
        Task<List<VideoFrame>> ExtractFramesAsync(string videoPath, int frameCount = 10);
        Task<bool> SaveFrameAsImageAsync(VideoFrame frame, string outputPath);
        Task<VideoMetadata> GetVideoMetadataAsync(string videoPath);
    }

    public class VideoAnalysisService : IVideoAnalysisService
    {
        private readonly ILogger<VideoAnalysisService> _logger;
        private readonly string _outputDirectory;
        private Mat? _previousFrame;

        public VideoAnalysisService(ILogger<VideoAnalysisService> logger)
        {
            _logger = logger;
            _outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VideoAnalysis");
            Directory.CreateDirectory(_outputDirectory);
        }

        public async Task<VideoAnalysisResult> AnalyzeVideoAsync(string videoPath, int examId)
        {
            try
            {
                _logger.LogInformation("Starting video analysis for exam {ExamId}: {VideoPath}", examId, videoPath);

                if (!File.Exists(videoPath))
                {
                    throw new FileNotFoundException($"Video file not found: {videoPath}");
                }

                var result = new VideoAnalysisResult
                {
                    ExamId = examId,
                    VideoPath = videoPath,
                    AnalysisDate = DateTime.UtcNow,
                    Status = "Processing"
                };

                await Task.Run(() =>
                {
                    using var capture = new VideoCapture(videoPath);
                    if (!capture.IsOpened)
                    {
                        throw new InvalidOperationException("Failed to open video file");
                    }

                    var frameCount = (int)capture.Get(CapProp.FrameCount);
                    var fps = capture.Get(CapProp.Fps);
                    var duration = frameCount / fps;

                    result.TotalFrames = frameCount;
                    result.FPS = fps;
                    result.Duration = duration;

                    // Analyze frames for medical content
                    var analysisFrames = Math.Min(100, frameCount); // Analyze up to 100 frames
                    var step = frameCount / analysisFrames;

                    var motionDetected = false;
                    var objectCount = 0;
                    var averageBrightness = 0.0;
                    var frameAnalysis = new List<FrameAnalysis>();

                    for (int i = 0; i < analysisFrames; i++)
                    {
                        var frameIndex = i * step;
                        capture.Set(CapProp.PosFrames, frameIndex);

                        using var frame = capture.QueryFrame();
                        if (frame != null)
                        {
                            var frameResult = AnalyzeFrame(frame);
                            frameAnalysis.Add(frameResult);

                            motionDetected |= frameResult.MotionDetected;
                            objectCount += frameResult.ObjectCount;
                            averageBrightness += frameResult.Brightness;
                        }
                    }

                    result.MotionDetected = motionDetected;
                    result.AverageObjectCount = objectCount / (double)analysisFrames;
                    result.AverageBrightness = averageBrightness / analysisFrames;
                    result.FrameAnalysis = frameAnalysis;
                    result.Status = "Completed";
                });

                _logger.LogInformation("Video analysis completed for exam {ExamId}", examId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing video for exam {ExamId}", examId);
                return new VideoAnalysisResult
                {
                    ExamId = examId,
                    VideoPath = videoPath,
                    AnalysisDate = DateTime.UtcNow,
                    Status = "Failed",
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<VideoFrame>> ExtractFramesAsync(string videoPath, int frameCount = 10)
        {
            try
            {
                var frames = new List<VideoFrame>();
                
                await Task.Run(() =>
                {
                    using var capture = new VideoCapture(videoPath);
                    if (!capture.IsOpened)
                    {
                        throw new InvalidOperationException("Failed to open video file");
                    }

                    var totalFrames = (int)capture.Get(CapProp.FrameCount);
                    var step = totalFrames / frameCount;

                    for (int i = 0; i < frameCount; i++)
                    {
                        var frameIndex = i * step;
                        capture.Set(CapProp.PosFrames, frameIndex);

                        using var frame = capture.QueryFrame();
                        if (frame != null)
                        {
                            var frameInfo = new VideoFrame
                            {
                                FrameIndex = frameIndex,
                                Timestamp = frameIndex / capture.Get(CapProp.Fps),
                                Image = frame.Clone()
                            };

                            frames.Add(frameInfo);
                        }
                    }
                });

                return frames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting frames from video: {VideoPath}", videoPath);
                throw;
            }
        }

        public async Task<bool> SaveFrameAsImageAsync(VideoFrame frame, string outputPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    frame.Image.Save(outputPath);
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving frame to: {OutputPath}", outputPath);
                return false;
            }
        }

        public async Task<VideoMetadata> GetVideoMetadataAsync(string videoPath)
        {
            try
            {
                var metadata = new VideoMetadata();

                await Task.Run(() =>
                {
                    using var capture = new VideoCapture(videoPath);
                    if (!capture.IsOpened)
                    {
                        throw new InvalidOperationException("Failed to open video file");
                    }

                    metadata.FrameCount = (int)capture.Get(CapProp.FrameCount);
                    metadata.FPS = capture.Get(CapProp.Fps);
                    metadata.Duration = metadata.FrameCount / metadata.FPS;
                    metadata.Width = (int)capture.Get(CapProp.FrameWidth);
                    metadata.Height = (int)capture.Get(CapProp.FrameHeight);
                    metadata.Codec = GetCodecName((int)capture.Get(CapProp.FourCC));
                });

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video metadata: {VideoPath}", videoPath);
                throw;
            }
        }

        private FrameAnalysis AnalyzeFrame(Mat frame)
        {
            var result = new FrameAnalysis();

            try
            {
                // Convert to grayscale for analysis
                using var gray = new Mat();
                CvInvoke.CvtColor(frame, gray, ColorConversion.Bgr2Gray);

                // Calculate brightness
                var mean = new MCvScalar();
                var stdDev = new MCvScalar();
                CvInvoke.MeanStdDev(gray, ref mean, ref stdDev);
                result.Brightness = mean.V0;

                // Motion detection using frame differencing
                if (_previousFrame != null)
                {
                    using var diff = new Mat();
                    CvInvoke.Absdiff(gray, _previousFrame, diff);
                    
                    var motionThreshold = 30.0;
                    var motionPixels = CvInvoke.CountNonZero(diff);
                    var totalPixels = diff.Rows * diff.Cols;
                    var motionPercentage = (double)motionPixels / totalPixels * 100;
                    
                    result.MotionDetected = motionPercentage > motionThreshold;
                    result.MotionPercentage = motionPercentage;
                }

                // Object detection using contours
                using var binary = new Mat();
                CvInvoke.Threshold(gray, binary, 127, 255, ThresholdType.Binary);
                
                using var contours = new VectorOfVectorOfPoint();
                using var hierarchy = new Mat();
                CvInvoke.FindContours(binary, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                
                result.ObjectCount = contours.Size;

                // Update previous frame
                _previousFrame?.Dispose();
                _previousFrame = gray.Clone();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing frame");
            }

            return result;
        }

        private string GetCodecName(int fourCC)
        {
            var bytes = BitConverter.GetBytes(fourCC);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public void Dispose()
        {
            _previousFrame?.Dispose();
        }
    }

    public class VideoAnalysisResult
    {
        public int ExamId { get; set; }
        public string VideoPath { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public int TotalFrames { get; set; }
        public double FPS { get; set; }
        public double Duration { get; set; }
        public bool MotionDetected { get; set; }
        public double AverageObjectCount { get; set; }
        public double AverageBrightness { get; set; }
        public List<FrameAnalysis> FrameAnalysis { get; set; } = new();
    }

    public class FrameAnalysis
    {
        public int FrameIndex { get; set; }
        public double Brightness { get; set; }
        public bool MotionDetected { get; set; }
        public double MotionPercentage { get; set; }
        public int ObjectCount { get; set; }
    }

    public class VideoFrame
    {
        public int FrameIndex { get; set; }
        public double Timestamp { get; set; }
        public Mat Image { get; set; } = null!;
    }

    public class VideoMetadata
    {
        public int FrameCount { get; set; }
        public double FPS { get; set; }
        public double Duration { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Codec { get; set; } = string.Empty;
    }
}