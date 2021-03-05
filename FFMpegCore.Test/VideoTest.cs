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
        public bool Convert(ContainerFormat type, bool multithreaded = false, VideoSize size = VideoSize.Original)
        {
            using var outputFile = new TemporaryFile($"out{type.Extension}");

            var input = FFProbe.Analyse(TestResources.Mp4Video);
            FFMpeg.Convert(input, outputFile, type, size: size, multithreaded: multithreaded);
            var outputVideo = FFProbe.Analyse(outputFile);

            Assert.IsTrue(File.Exists(outputFile));
            Assert.AreEqual(outputVideo.Duration.TotalSeconds, input.Duration.TotalSeconds, 0.1);
            if (size == VideoSize.Original)
            {
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
            else
            {
                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, (int)size);
            }

            return File.Exists(outputFile) && 
                   outputVideo.Duration == input.Duration &&
                   (
                       (
                           size == VideoSize.Original &&
                           outputVideo.PrimaryVideoStream.Width == input.PrimaryVideoStream.Width &&
                           outputVideo.PrimaryVideoStream.Height == input.PrimaryVideoStream.Height
                       ) ||
                       (
                           size != VideoSize.Original &&
                           outputVideo.PrimaryVideoStream.Width != input.PrimaryVideoStream.Width &&
                           outputVideo.PrimaryVideoStream.Height != input.PrimaryVideoStream.Height &&
                           outputVideo.PrimaryVideoStream.Height == (int)size
                       )
                   );
        }

        private void ConvertFromStreamPipe(ContainerFormat type, params IArgument[] arguments)
        {
            using var outputFile = new TemporaryFile($"out{type.Extension}");
            
            var input = FFProbe.Analyse(TestResources.WebmVideo);
            using var inputStream = File.OpenRead(input.Path);
            var processor = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(inputStream))
                .OutputToFile(outputFile, false, opt =>
                {
                    foreach (var arg in arguments)
                        opt.WithArgument(arg);
                });

            var scaling = arguments.OfType<ScaleArgument>().FirstOrDefault();

            var success = processor.ProcessSynchronously();

            var outputVideo = FFProbe.Analyse(outputFile);

            Assert.IsTrue(success);
            Assert.IsTrue(File.Exists(outputFile));
            Assert.IsTrue(Math.Abs((outputVideo.Duration - input.Duration).TotalMilliseconds) < 1000.0 / input.PrimaryVideoStream.FrameRate);

            if (scaling?.Size == null)
            {
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
            else
            {
                if (scaling.Size.Value.Width != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, scaling.Size.Value.Width);
                }

                if (scaling.Size.Value.Height != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, scaling.Size.Value.Height);
                }

                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
        }

        private void ConvertToStreamPipe(params IArgument[] arguments)
        {
            using var ms = new MemoryStream();
            var processor = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToPipe(new StreamPipeSink(ms), opt =>
                {
                    foreach (var arg in arguments)
                        opt.WithArgument(arg);
                });

            var scaling = arguments.OfType<ScaleArgument>().FirstOrDefault();

            processor.ProcessSynchronously();

            ms.Position = 0;
            var outputVideo = FFProbe.Analyse(ms);

            var input = FFProbe.Analyse(TestResources.Mp4Video);
            // Assert.IsTrue(Math.Abs((outputVideo.Duration - input.Duration).TotalMilliseconds) < 1000.0 / input.PrimaryVideoStream.FrameRate);

            if (scaling?.Size == null)
            {
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
            else
            {
                if (scaling.Size.Value.Width != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, scaling.Size.Value.Width);
                }

                if (scaling.Size.Value.Height != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, scaling.Size.Value.Height);
                }

                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
        }

        public void Convert(ContainerFormat type, Action<IMediaAnalysis> validationMethod, params IArgument[] arguments)
        {
            using var outputFile = new TemporaryFile($"out{type.Extension}");

            var input = FFProbe.Analyse(TestResources.Mp4Video);

            var processor = FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToFile(outputFile, false, opt =>
                {
                    foreach (var arg in arguments)
                        opt.WithArgument(arg);
                });

            var scaling = arguments.OfType<ScaleArgument>().FirstOrDefault();
            processor.ProcessSynchronously();

            var outputVideo = FFProbe.Analyse(outputFile);

            Assert.IsTrue(File.Exists(outputFile));
            Assert.AreEqual(outputVideo.Duration.TotalSeconds, input.Duration.TotalSeconds, 0.1);
            validationMethod?.Invoke(outputVideo);
            if (scaling?.Size == null)
            {
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
            else
            {
                if (scaling.Size.Value.Width != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, scaling.Size.Value.Width);
                }

                if (scaling.Size.Value.Height != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, scaling.Size.Value.Height);
                }

                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Width, input.PrimaryVideoStream.Width);
                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Height, input.PrimaryVideoStream.Height);
            }
        }

        public void Convert(ContainerFormat type, params IArgument[] inputArguments)
        {
            Convert(type, null, inputArguments);
        }

        public void ConvertFromPipe(ContainerFormat type, System.Drawing.Imaging.PixelFormat fmt, params IArgument[] arguments)
        {
            using var outputFile = new TemporaryFile($"out{type.Extension}");

            var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(128, fmt, 256, 256));
            var processor = FFMpegArguments.FromPipeInput(videoFramesSource).OutputToFile(outputFile, false, opt =>
            {
                foreach (var arg in arguments)
                    opt.WithArgument(arg);
            });
            var scaling = arguments.OfType<ScaleArgument>().FirstOrDefault();
            processor.ProcessSynchronously();

            var outputVideo = FFProbe.Analyse(outputFile);

            Assert.IsTrue(File.Exists(outputFile));

            if (scaling?.Size == null)
            {
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, videoFramesSource.Width);
                Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, videoFramesSource.Height);
            }
            else
            {
                if (scaling.Size.Value.Width != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Width, scaling.Size.Value.Width);
                }

                if (scaling.Size.Value.Height != -1)
                {
                    Assert.AreEqual(outputVideo.PrimaryVideoStream.Height, scaling.Size.Value.Height);
                }

                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Width, videoFramesSource.Width);
                Assert.AreNotEqual(outputVideo.PrimaryVideoStream.Height, videoFramesSource.Height);
            }
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4()
        {
            Convert(VideoType.Mp4);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_YUV444p()
        {
            Convert(VideoType.Mp4, (a) => Assert.IsTrue(a.VideoStreams.First().PixelFormat == "yuv444p"), 
                new ForcePixelFormat("yuv444p"));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args()
        {
            Convert(VideoType.Mp4, new VideoCodecArgument(VideoCodec.LibX264));
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        public void Video_ToMP4_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            ConvertFromPipe(VideoType.Mp4, pixelFormat, new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_StreamPipe()
        {
            ConvertFromStreamPipe(VideoType.Mp4, new VideoCodecArgument(VideoCodec.LibX264));
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
                    .OutputToPipe(pipeSource, opt => opt.ForceFormat("mkv"))
                    .ProcessAsynchronously();
            });
        }
        [TestMethod, Timeout(10000)]
        public void Video_StreamFile_OutputToMemoryStream()
        {
            var output = new MemoryStream();

            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(File.OpenRead(TestResources.WebmVideo)), options => options.ForceFormat("webm"))
                .OutputToPipe(new StreamPipeSink(output), options => options
                    .ForceFormat("mpegts"))
                .ProcessSynchronously();

            output.Position = 0;
            var result = FFProbe.Analyse(output);
            Console.WriteLine(result.Duration);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_StreamOutputPipe_Failure()
        {
            Assert.ThrowsException<FFMpegException>(() => ConvertToStreamPipe(new ForceFormatArgument("mkv")));
        }


        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_StreamOutputPipe_Async()
        {
            using var ms = new MemoryStream();
            var pipeSource = new StreamPipeSink(ms);
            FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToPipe(pipeSource, opt => opt
                    .WithVideoCodec(VideoCodec.LibX264)
                    .ForceFormat("matroska"))
                .ProcessAsynchronously()
                .WaitForResult();
        }

        [TestMethod, Timeout(10000)]
        public async Task TestDuplicateRun()
        {
            FFMpegArguments.FromFileInput(TestResources.Mp4Video)
                .OutputToFile("temporary.mp4")
                .ProcessSynchronously();
            
            await FFMpegArguments.FromFileInput(TestResources.Mp4Video)
                .OutputToFile("temporary.mp4")
                .ProcessAsynchronously();
            
            File.Delete("temporary.mp4");
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Args_StreamOutputPipe()
        {
            ConvertToStreamPipe(new VideoCodecArgument(VideoCodec.LibX264), new ForceFormatArgument("matroska"));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToTS()
        {
            Convert(VideoType.Ts);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToTS_Args()
        {
            Convert(VideoType.Ts,
                new CopyArgument(),
                new BitStreamFilterArgument(Channel.Video, Filter.H264_Mp4ToAnnexB),
                new ForceFormatArgument(VideoType.MpegTs));
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        public void Video_ToTS_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            ConvertFromPipe(VideoType.Ts, pixelFormat, new ForceFormatArgument(VideoType.Ts));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToOGV_Resize()
        {
            Convert(VideoType.Ogv, true, VideoSize.Ed);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToOGV_Resize_Args()
        {
            Convert(VideoType.Ogv, new ScaleArgument(VideoSize.Ed), new VideoCodecArgument(VideoCodec.LibTheora));
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        // [DataRow(PixelFormat.Format48bppRgb)]
        public void Video_ToOGV_Resize_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            ConvertFromPipe(VideoType.Ogv, pixelFormat, new ScaleArgument(VideoSize.Ed), new VideoCodecArgument(VideoCodec.LibTheora));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Resize()
        {
            Convert(VideoType.Mp4, true, VideoSize.Ed);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_Resize_Args()
        {
            Convert(VideoType.Mp4, new ScaleArgument(VideoSize.Ld), new VideoCodecArgument(VideoCodec.LibX264));
        }

        [DataTestMethod, Timeout(10000)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format24bppRgb)]
        [DataRow(System.Drawing.Imaging.PixelFormat.Format32bppArgb)]
        // [DataRow(PixelFormat.Format48bppRgb)]
        public void Video_ToMP4_Resize_Args_Pipe(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            ConvertFromPipe(VideoType.Mp4, pixelFormat, new ScaleArgument(VideoSize.Ld), new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToOGV()
        {
            Convert(VideoType.Ogv);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToMP4_MultiThread()
        {
            Convert(VideoType.Mp4, true);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToTS_MultiThread()
        {
            Convert(VideoType.Ts, true);
        }

        [TestMethod, Timeout(10000)]
        public void Video_ToOGV_MultiThread()
        {
            Convert(VideoType.Ogv, true);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Snapshot_InMemory()
        {
            var input = FFProbe.Analyse(TestResources.Mp4Video);
            using var bitmap = FFMpeg.Snapshot(input);
            
            Assert.AreEqual(input.PrimaryVideoStream.Width, bitmap.Width);
            Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
            Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
        }

        [TestMethod, Timeout(10000)]
        public void Video_Snapshot_PersistSnapshot()
        {
            var outputPath = new TemporaryFile("out.png");
            var input = FFProbe.Analyse(TestResources.Mp4Video);

            FFMpeg.Snapshot(input, outputPath);

            using var bitmap = Image.FromFile(outputPath);
            Assert.AreEqual(input.PrimaryVideoStream.Width, bitmap.Width);
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
            Assert.AreEqual(input.PrimaryVideoStream.Height, result.PrimaryVideoStream.Height);
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
            Assert.AreEqual(imageSet.First().Width, result.PrimaryVideoStream.Width);
            Assert.AreEqual(imageSet.First().Height, result.PrimaryVideoStream.Height);
        }

        [TestMethod, Timeout(10000)]
        public void Video_With_Only_Audio_Should_Extract_Metadata()
        {
            var video = FFProbe.Analyse(TestResources.Mp4WithoutVideo);
            Assert.AreEqual(null, video.PrimaryVideoStream);
            Assert.AreEqual("aac", video.PrimaryAudioStream.CodecName);
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
            
            FFMpegOptions.Configure(opt => opt.Encoding = Encoding.UTF8);
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
            Assert.AreEqual(vi.PrimaryVideoStream.Width, 128);
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

            var outputInfo = FFProbe.Analyse(outputFile);

            Assert.IsTrue(result);
            Assert.IsNotNull(outputInfo);
            Assert.AreEqual(320, outputInfo.PrimaryVideoStream.Width);
            Assert.AreEqual(240, outputInfo.PrimaryVideoStream.Height);
            Assert.AreEqual("h264", outputInfo.PrimaryVideoStream.CodecName);
            Assert.AreEqual("aac", outputInfo.PrimaryAudioStream.CodecName);
        }
    }
}
