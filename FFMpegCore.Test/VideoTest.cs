using FFMpegCore.Enums;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Arguments;
using FFMpegCore.Exceptions;
using FFMpegCore.Pipes;
using System.Threading;

namespace FFMpegCore.Test
{
    [TestClass]
    public class VideoTest
    {
        [TestMethod, Timeout(10000)]
        public void Video_ToOGV()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Ogv.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.WebmVideo)
                .OutputToFile(outputFile, false)
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.WebmVideo)
                .OutputToFile(outputFile, false)
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_YUV444p()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.WebmVideo)
                .OutputToFile(outputFile, false, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264)
                    .ForcePixelFormat("yuv444p"))
                .ProcessSynchronously();
            Assert.IsTrue(success);
            var analysis = FFProbe.Analyse(outputFile);
            Assert.IsTrue(analysis.VideoStreams.First().PixelFormat == "yuv444p");
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.WebmVideo)
                .OutputToFile(outputFile, false, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToH265_MKV_Args()
        {
            using var outputFile = new TemporaryFile($"out.mkv");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.WebmVideo)
                .OutputToFile(outputFile, false, opt => opt
                    .WithVideoCodec(VideoCodec.LibX265))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        public void Video_ToMP4_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(128, pixelFormat, 256, 256));
            var success = FFMpegArguments
                .FromPipeInput(videoFramesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_Pipe_DifferentImageSizes()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var frames = new List<IVideoFrame> 
            {
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 255, 255, 1, 0),
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 256, 256, 1, 0)
            };

            var videoFramesSource = new RawVideoPipeSource(frames);
            var ex = Assert.ThrowsException<FFMpegException>(() => FFMpegArguments
              .FromPipeInput(videoFramesSource)
              .OutputToFile(outputFile, false, opt => opt
                  .WithVideoCodec(VideoCodec.LibX264))
              .ProcessSynchronously());

            Assert.IsInstanceOfType(ex.GetBaseException(), typeof(FFMpegStreamFormatException));
        }


        [TestMethod, Timeout(10000)]
        public async Task Video_ToMP4_Args_Pipe_DifferentImageSizes_Async()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var frames = new List<IVideoFrame>
            {
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 255, 255, 1, 0),
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 256, 256, 1, 0)
            };

            var videoFramesSource = new RawVideoPipeSource(frames);
            var ex = await Assert.ThrowsExceptionAsync<FFMpegException>(() => FFMpegArguments
              .FromPipeInput(videoFramesSource)
              .OutputToFile(outputFile, false, opt => opt
                  .WithVideoCodec(VideoCodec.LibX264))
              .ProcessAsynchronously());

            Assert.IsInstanceOfType(ex.GetBaseException(), typeof(FFMpegStreamFormatException));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_Pipe_DifferentPixelFormats()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var frames = new List<IVideoFrame>
            {
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 255, 255, 1, 0),
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format32bppRgb, 255, 255, 1, 0)
            };

            var videoFramesSource = new RawVideoPipeSource(frames);
            var ex = Assert.ThrowsException<FFMpegException>(() => FFMpegArguments
              .FromPipeInput(videoFramesSource)
              .OutputToFile(outputFile, false, opt => opt
                  .WithVideoCodec(VideoCodec.LibX264))
              .ProcessSynchronously());

            Assert.IsInstanceOfType(ex.GetBaseException(), typeof(FFMpegStreamFormatException));
        }


        [TestMethod, Timeout(10000)]
        public async Task Video_ToMP4_Args_Pipe_DifferentPixelFormats_Async()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

            var frames = new List<IVideoFrame>
            {
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 255, 255, 1, 0),
                BitmapSource.CreateVideoFrame(0, System.Drawing.Imaging.PixelFormat.Format32bppRgb, 255, 255, 1, 0)
            };

            var videoFramesSource = new RawVideoPipeSource(frames);
            var ex = await Assert.ThrowsExceptionAsync<FFMpegException>(() => FFMpegArguments
              .FromPipeInput(videoFramesSource)
              .OutputToFile(outputFile, false, opt => opt
                  .WithVideoCodec(VideoCodec.LibX264))
              .ProcessAsynchronously());

            Assert.IsInstanceOfType(ex.GetBaseException(), typeof(FFMpegStreamFormatException));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_StreamPipe()
        {
            using var input = File.OpenRead(TestResources.WebmVideo);
            using var output = new TemporaryFile($"out{VideoType.Mp4.Extension}");
            
            var success = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(input))
                .OutputToFile(output, false, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public async Task Video_ToMP4_Args_StreamOutputPipe_Async_Failure()
        {
            await Assert.ThrowsExceptionAsync<FFMpegException>(async () =>
            {
                await using var ms = new MemoryStream();
                var pipeSource = new StreamPipeSink(ms);
                await FFMpegArguments
                    .FromFileInput(TestResources.Mp4Video)
                    .OutputToPipe(pipeSource, opt => opt.ForceFormat("mp4"))
                    .ProcessAsynchronously();
            });
        }


        [TestMethod, Timeout(10000)]
        public void Video_StreamFile_OutputToMemoryStream()
        {
            var output = new MemoryStream();

            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(File.OpenRead(TestResources.WebmVideo)), opt => opt
                    .ForceFormat("webm"))
                .OutputToPipe(new StreamPipeSink(output), opt => opt
                    .ForceFormat("mpegts"))
                .ProcessSynchronously();

            output.Position = 0;
            var result = FFProbe.Analyse(output);
            Console.WriteLine(result.Duration);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_StreamOutputPipe_Failure()
        {
            Assert.ThrowsException<FFMpegException>(() =>
            {
                using var ms = new MemoryStream();
                FFMpegArguments
                    .FromFileInput(TestResources.Mp4Video)
                    .OutputToPipe(new StreamPipeSink(ms), opt => opt
                        .ForceFormat("mkv"))
                    .ProcessSynchronously();
            });
        }


        [TestMethod, Timeout(10000)]
        public async Task Video_ToMP4_Args_StreamOutputPipe_Async()
        {
            await using var ms = new MemoryStream();
            var pipeSource = new StreamPipeSink(ms);
            await FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToPipe(pipeSource, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264)
                    .ForceFormat("matroska"))
                .ProcessAsynchronously();
        }

        [TestMethod, Timeout(10000)]
        public async Task TestDuplicateRun()
        {
            FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile("temporary.mp4")
                .ProcessSynchronously();
            
            await FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile("temporary.mp4")
                .ProcessAsynchronously();
            
            File.Delete("temporary.mp4");
        }

        [TestMethod, Timeout(10000)]
        public void TranscodeToMemoryStream_Success()
        {
            using var output = new MemoryStream();
            var success = FFMpegArguments
                .FromFileInput(TestResources.WebmVideo)
                .OutputToPipe(new StreamPipeSink(output), opt => opt
                    .WithVideoCodec(VideoCodec.LibVpx)
                    .ForceFormat("matroska"))
                .ProcessSynchronously();
            Assert.IsTrue(success);

            output.Position = 0;
            var inputAnalysis = FFProbe.Analyse(TestResources.WebmVideo);
            var outputAnalysis = FFProbe.Analyse(output);
            Assert.AreEqual(inputAnalysis.Duration.TotalSeconds, outputAnalysis.Duration.TotalSeconds, 0.3);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToTS()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.MpegTs.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false)
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToTS_Args()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.MpegTs.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false, opt => opt
                    .CopyChannel()
                    .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                    .ForceFormat(VideoType.MpegTs))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        public async Task Video_ToTS_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            using var output = new TemporaryFile($"out{VideoType.Ts.Extension}");
            var input = new RawVideoPipeSource(BitmapSource.CreateBitmaps(128, pixelFormat, 256, 256));
            
            var success = await FFMpegArguments
                .FromPipeInput(input)
                .OutputToFile(output, false, opt => opt
                    .ForceFormat(VideoType.Ts))
                .ProcessAsynchronously();
            Assert.IsTrue(success);

            var analysis = await FFProbe.AnalyseAsync(output);
            Assert.AreEqual(VideoType.Ts.Name, analysis.Format.FormatName);
        }

        [TestMethod, Timeout(10000)]
        public async Task Video_ToOGV_Resize()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Ogv.Extension}");
            var success = await FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false, opt => opt
                    .Resize(200, 200)
                    .WithVideoCodec(VideoCodec.LibTheora))
                .ProcessAsynchronously();
            Assert.IsTrue(success);
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        // [DataRow(PixelFormat.Format48bppRgb)]
        public void RawVideoPipeSource_Ogv_Scale(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Ogv.Extension}");
            var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(128, pixelFormat, 256, 256));
            
            FFMpegArguments
                .FromPipeInput(videoFramesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithVideoFilters(filterOptions => filterOptions
                        .Scale(VideoSize.Ed))
                    .WithVideoCodec(VideoCodec.LibTheora))
                .ProcessSynchronously();

            var analysis = FFProbe.Analyse(outputFile);
            Assert.AreEqual((int)VideoSize.Ed, analysis.PrimaryVideoStream!.Width);
        }

        [TestMethod, Timeout(10000)]
        public void Scale_Mp4_Multithreaded()
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");
            
            var success = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false, opt => opt
                    .UsingMultithreading(true)
                    .WithVideoCodec(VideoCodec.LibX264))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        // [DataRow(PixelFormat.Format48bppRgb)]
        public void Video_ToMP4_Resize_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");
            var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(128, pixelFormat, 256, 256));
            
            var success = FFMpegArguments
                .FromPipeInput(videoFramesSource)
                .OutputToFile(outputFile, false, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264))
                .ProcessSynchronously();
            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Snapshot_InMemory()
        {
            var input = FFProbe.Analyse(TestResources.Mp4Video);
            using var bitmap = FFMpeg.Snapshot(TestResources.Mp4Video);
            
            Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
            Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
            Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Snapshot_PersistSnapshot()
        {
            var outputPath = new TemporaryFile("out.png");
            var input = FFProbe.Analyse(TestResources.Mp4Video);

            FFMpeg.Snapshot(TestResources.Mp4Video, outputPath);

            using var bitmap = Image.FromFile(outputPath);
            Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
            Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
            Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Join()
        {
            var inputCopy = new TemporaryFile("copy-input.mp4");
            File.Copy(TestResources.Mp4Video, inputCopy);
            
            var outputPath = new TemporaryFile("out.mp4");
            var input = FFProbe.Analyse(TestResources.Mp4Video);
            var success = FFMpeg.Join(outputPath, TestResources.Mp4Video, inputCopy);
            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(outputPath));
            
            var expectedDuration = input.Duration * 2;
            var result = FFProbe.Analyse(outputPath);
            Assert.AreEqual(expectedDuration.Days, result.Duration.Days);
            Assert.AreEqual(expectedDuration.Hours, result.Duration.Hours);
            Assert.AreEqual(expectedDuration.Minutes, result.Duration.Minutes);
            Assert.AreEqual(expectedDuration.Seconds, result.Duration.Seconds);
            Assert.AreEqual(input.PrimaryVideoStream!.Height, result.PrimaryVideoStream!.Height);
            Assert.AreEqual(input.PrimaryVideoStream.Width, result.PrimaryVideoStream.Width);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Join_Image_Sequence()
        {
            var imageSet = new List<ImageInfo>();
            Directory.EnumerateFiles(TestResources.ImageCollection)
                .Where(file => file.ToLower().EndsWith(".png"))
                .ToList()
                .ForEach(file =>
                {
                    for (var i = 0; i < 15; i++)
                    {
                        imageSet.Add(new ImageInfo(file));
                    }
                });

            var outputFile = new TemporaryFile("out.mp4");
            var success = FFMpeg.JoinImageSequence(outputFile, images: imageSet.ToArray());
            Assert.IsTrue(success);
            var result = FFProbe.Analyse(outputFile);
            Assert.AreEqual(3, result.Duration.Seconds);
            Assert.AreEqual(imageSet.First().Width, result.PrimaryVideoStream!.Width);
            Assert.AreEqual(imageSet.First().Height, result.PrimaryVideoStream.Height);
        }

        [TestMethod, Timeout(10000)]
        public void Video_With_Only_Audio_Should_Extract_Metadata()
        {
            var video = FFProbe.Analyse(TestResources.Mp4WithoutVideo);
            Assert.AreEqual(null, video.PrimaryVideoStream);
            Assert.AreEqual("aac", video.PrimaryAudioStream!.CodecName);
            Assert.AreEqual(10, video.Duration.TotalSeconds, 0.5);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Duration()
        {
            var video = FFProbe.Analyse(TestResources.Mp4Video);
            var outputFile = new TemporaryFile("out.mp4");

            FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false, opt => opt.WithDuration(TimeSpan.FromSeconds(video.Duration.TotalSeconds - 2)))
                .ProcessSynchronously();

            Assert.IsTrue(File.Exists(outputFile));
            var outputVideo = FFProbe.Analyse(outputFile);

            Assert.AreEqual(video.Duration.Days, outputVideo.Duration.Days);
            Assert.AreEqual(video.Duration.Hours, outputVideo.Duration.Hours);
            Assert.AreEqual(video.Duration.Minutes, outputVideo.Duration.Minutes);
            Assert.AreEqual(video.Duration.Seconds - 2, outputVideo.Duration.Seconds);
        }

        [TestMethod, Timeout(10000)]
        public void Video_UpdatesProgress()
        {
            var outputFile = new TemporaryFile("out.mp4");

            var percentageDone = 0.0;
            var timeDone = TimeSpan.Zero;
            void OnPercentageProgess(double percentage) => percentageDone = percentage;
            void OnTimeProgess(TimeSpan time) => timeDone = time;

            var analysis = FFProbe.Analyse(TestResources.Mp4Video);
            var success = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false, opt => opt
                    .WithDuration(TimeSpan.FromSeconds(2)))
                .NotifyOnProgress(OnPercentageProgess, analysis.Duration)
                .NotifyOnProgress(OnTimeProgess)
                .ProcessSynchronously();

            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(outputFile));
            Assert.AreNotEqual(0.0, percentageDone);
            Assert.AreNotEqual(TimeSpan.Zero, timeDone);
        }

        [TestMethod, Timeout(10000)]
        public void Video_OutputsData()
        {
            var outputFile = new TemporaryFile("out.mp4");
            var dataReceived = false;
            
            GlobalFFOptions.Configure(opt => opt.Encoding = Encoding.UTF8);
            var success = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .WithGlobalOptions(options => options
                    .WithVerbosityLevel(VerbosityLevel.Info))
                .OutputToFile(outputFile, false, opt => opt
                    .WithDuration(TimeSpan.FromSeconds(2)))
                .NotifyOnOutput((_, _) => dataReceived = true)
                .ProcessSynchronously();

            Assert.IsTrue(dataReceived);
            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod, Timeout(10000)]
        public void Video_TranscodeInMemory()
        {
            using var resStream = new MemoryStream();
            var reader = new StreamPipeSink(resStream);
            var writer = new RawVideoPipeSource(BitmapSource.CreateBitmaps(128, System.Drawing.Imaging.PixelFormat.Format24bppRgb, 128, 128));

            FFMpegArguments
                .FromPipeInput(writer)
                .OutputToPipe(reader, opt => opt
                    .WithVideoCodec("vp9")
                    .ForceFormat("webm"))
                .ProcessSynchronously();

            resStream.Position = 0;
            var vi = FFProbe.Analyse(resStream);
            Assert.AreEqual(vi.PrimaryVideoStream!.Width, 128);
            Assert.AreEqual(vi.PrimaryVideoStream.Height, 128);
        }

        [TestMethod, Timeout(10000)]
        public async Task Video_Cancel_Async()
        {
            var outputFile = new TemporaryFile("out.mp4");

            var task = FFMpegArguments
                .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                    .WithCustomArgument("-re")
                    .ForceFormat("lavfi"))
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithSpeedPreset(Speed.VeryFast))
                .CancellableThrough(out var cancel)
                .ProcessAsynchronously(false);

            await Task.Delay(300);
            cancel();

            var result = await task;

            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(10000)]
        public async Task Video_Cancel_Async_With_Timeout()
        {
            var outputFile = new TemporaryFile("out.mp4");

            var task = FFMpegArguments
                .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                    .WithCustomArgument("-re")
                    .ForceFormat("lavfi"))
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithSpeedPreset(Speed.VeryFast))
                .CancellableThrough(out var cancel, 10000)
                .ProcessAsynchronously(false);

            await Task.Delay(300);
            cancel();

            var result = await task;

            var outputInfo = await FFProbe.AnalyseAsync(outputFile);

            Assert.IsTrue(result);
            Assert.IsNotNull(outputInfo);
            Assert.AreEqual(320, outputInfo.PrimaryVideoStream!.Width);
            Assert.AreEqual(240, outputInfo.PrimaryVideoStream.Height);
            Assert.AreEqual("h264", outputInfo.PrimaryVideoStream.CodecName);
            Assert.AreEqual("aac", outputInfo.PrimaryAudioStream!.CodecName);
        }

        [TestMethod, Timeout(10000)]
        public async Task Video_Cancel_CancellationToken_Async()
        {
            var outputFile = new TemporaryFile("out.mp4");

            var cts = new CancellationTokenSource();

            var task = FFMpegArguments
                .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                    .WithCustomArgument("-re")
                    .ForceFormat("lavfi"))
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithSpeedPreset(Speed.VeryFast))
                .CancellableThrough(cts.Token)
                .ProcessAsynchronously(false);

            await Task.Delay(300);
            cts.Cancel();

            var result = await task;

            Assert.IsFalse(result);
        }

        [TestMethod, Timeout(10000)]
        public async Task Video_Cancel_CancellationToken_Async_With_Timeout()
        {
            var outputFile = new TemporaryFile("out.mp4");

            var cts = new CancellationTokenSource();

            var task = FFMpegArguments
                .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                    .WithCustomArgument("-re")
                    .ForceFormat("lavfi"))
                .OutputToFile(outputFile, false, opt => opt
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithSpeedPreset(Speed.VeryFast))
                .CancellableThrough(cts.Token, 8000)
                .ProcessAsynchronously(false);

            await Task.Delay(300);
            cts.Cancel();

            var result = await task;

            var outputInfo = await FFProbe.AnalyseAsync(outputFile);

            Assert.IsTrue(result);
            Assert.IsNotNull(outputInfo);
            Assert.AreEqual(320, outputInfo.PrimaryVideoStream!.Width);
            Assert.AreEqual(240, outputInfo.PrimaryVideoStream.Height);
            Assert.AreEqual("h264", outputInfo.PrimaryVideoStream.CodecName);
            Assert.AreEqual("aac", outputInfo.PrimaryAudioStream!.CodecName);
        }
    }
}
