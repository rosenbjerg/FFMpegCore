using FFMpegCore.Enums;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Enums;
using FFMpegCore.FFMPEG.Exceptions;
using FFMpegCore.FFMPEG.Pipes;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG;

namespace FFMpegCore.Test
{
    [TestClass]
    public class VideoTest : BaseTest
    {
        public bool Convert(VideoType type, bool multithreaded = false, VideoSize size = VideoSize.Original)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var input = FFProbe.Analyse(Input.FullName);
                FFMpeg.Convert(input, output, type, size: size, multithreaded: multithreaded);
                var outputVideo = FFProbe.Analyse(output);

                Assert.IsTrue(File.Exists(output));
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
                return File.Exists(output) &&
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
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        private void ConvertFromStreamPipe(VideoType type, params IArgument[] inputArguments)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var input = FFProbe.Analyse(VideoLibrary.LocalVideoWebm.FullName);
                using (var inputStream = System.IO.File.OpenRead(input.Path))
                {
                    var pipeSource = new StreamPipeDataWriter(inputStream);
                    var arguments = FFMpegArguments.FromPipe(pipeSource);
                    foreach (var arg in inputArguments)
                        arguments.WithArgument(arg);
                    var processor = arguments.OutputToFile(output);

                    var scaling = arguments.Find<ScaleArgument>();

                    var success = processor.ProcessSynchronously();

                    var outputVideo = FFProbe.Analyse(output);
                    
                    Assert.IsTrue(success);
                    Assert.IsTrue(File.Exists(output));
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
            }
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        private void ConvertToStreamPipe(params IArgument[] inputArguments)
        {
            using var ms = new MemoryStream();
            var arguments = FFMpegArguments.FromInputFiles(VideoLibrary.LocalVideo);
            foreach (var arg in inputArguments)
                arguments.WithArgument(arg);

            var streamPipeDataReader = new StreamPipeDataReader(ms);
            var processor = arguments.OutputToPipe(streamPipeDataReader);

            var scaling = arguments.Find<ScaleArgument>();

            processor.ProcessSynchronously();

            ms.Position = 0;
            var outputVideo = FFProbe.Analyse(ms);

            var input = FFProbe.Analyse(VideoLibrary.LocalVideo.FullName);
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

        public void Convert(VideoType type, params IArgument[] inputArguments)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var input = FFProbe.Analyse(Input.FullName);

                var arguments = FFMpegArguments.FromInputFiles(VideoLibrary.LocalVideo.FullName);
                foreach (var arg in inputArguments)
                    arguments.WithArgument(arg);

                var processor = arguments.OutputToFile(output);

                var scaling = arguments.Find<ScaleArgument>();
                processor.ProcessSynchronously();

                var outputVideo = FFProbe.Analyse(output);

                Assert.IsTrue(File.Exists(output));
                Assert.AreEqual(outputVideo.Duration.TotalSeconds, input.Duration.TotalSeconds, 0.1);

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
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        public void ConvertFromPipe(VideoType type, params IArgument[] inputArguments)
        {
            ConvertFromPipe(type, PixelFormat.Format24bppRgb, inputArguments);
            ConvertFromPipe(type, PixelFormat.Format32bppArgb, inputArguments);
            ConvertFromPipe(type, PixelFormat.Format48bppRgb, inputArguments);
        }

        public void ConvertFromPipe(VideoType type, PixelFormat fmt, params IArgument[] inputArguments)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var videoFramesSource = new RawVideoPipeDataWriter(BitmapSource.CreateBitmaps(128, fmt, 256, 256));
                var arguments = FFMpegArguments.FromPipe(videoFramesSource);
                foreach (var arg in inputArguments)
                    arguments.WithArgument(arg);
                var processor = arguments.OutputToFile(output);
                
                var scaling = arguments.Find<ScaleArgument>();
                processor.ProcessSynchronously();

                var outputVideo = FFProbe.Analyse(output);

                Assert.IsTrue(File.Exists(output));

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
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }

        }

        [TestMethod]
        public void Video_ToMP4()
        {
            Convert(VideoType.Mp4);
        }

        [TestMethod]
        public void Video_ToMP4_Args()
        {
            Convert(VideoType.Mp4, new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod]
        public void Video_ToMP4_Args_Pipe()
        {
            ConvertFromPipe(VideoType.Mp4, new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod]
        public void Video_ToMP4_Args_StreamPipe()
        {
            ConvertFromStreamPipe(VideoType.Mp4, new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod]
        public void Video_ToMP4_Args_StreamOutputPipe_Async_Failure()
        {
            Assert.ThrowsException<FFMpegException>(() =>
            {
                using var ms = new MemoryStream();
                var pipeSource = new StreamPipeDataReader(ms);
                FFMpegArguments
                    .FromInputFiles(VideoLibrary.LocalVideo)
                    .ForceFormat("mkv")
                    .OutputToPipe(pipeSource)
                    .ProcessAsynchronously()
                    .WaitForResult();
                FFProbe.Analyse(ms);
            });
        }

        [TestMethod]
        public void Video_ToMP4_Args_StreamOutputPipe_Failure()
        {
            Assert.ThrowsException<FFMpegException>(() =>
            {
                ConvertToStreamPipe(new ForceFormatArgument("mkv"));
            });
        }


        [TestMethod]
        public void Video_ToMP4_Args_StreamOutputPipe_Async()
        {
            using (var ms = new MemoryStream())
            {
                var pipeSource = new StreamPipeDataReader(ms);
                FFMpegArguments
                    .FromInputFiles(VideoLibrary.LocalVideo)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .ForceFormat("matroska")
                    .OutputToPipe(pipeSource)
                    .ProcessAsynchronously()
                    .WaitForResult();
            }
        }

        [TestMethod]
        public void Video_ToMP4_Args_StreamOutputPipe()
        {
            ConvertToStreamPipe(new VideoCodecArgument(VideoCodec.LibX264), new ForceFormatArgument("matroska"));
        }

        [TestMethod]
        public void Video_ToTS()
        {
            Convert(VideoType.Ts);
        }

        [TestMethod]
        public void Video_ToTS_Args()
        {
            Convert(VideoType.Ts, 
                new CopyArgument(),
                new BitStreamFilterArgument(Channel.Video, Filter.H264_Mp4ToAnnexB),
                new ForceFormatArgument(VideoCodec.MpegTs));
        }

        [TestMethod]
        public void Video_ToTS_Args_Pipe()
        {
            ConvertFromPipe(VideoType.Ts, new ForceFormatArgument(VideoCodec.MpegTs));
        }

        [TestMethod]
        public void Video_ToOGV_Resize()
        {
            Convert(VideoType.Ogv, true, VideoSize.Ed);
        }

        [TestMethod]
        public void Video_ToOGV_Resize_Args()
        {
            Convert(VideoType.Ogv, new ScaleArgument(VideoSize.Ed), new VideoCodecArgument(VideoCodec.LibTheora));
        }

        [TestMethod]
        public void Video_ToOGV_Resize_Args_Pipe()
        {
            ConvertFromPipe(VideoType.Ogv, new ScaleArgument(VideoSize.Ed), new VideoCodecArgument(VideoCodec.LibTheora));
        }

        [TestMethod]
        public void Video_ToMP4_Resize()
        {
            Convert(VideoType.Mp4, true, VideoSize.Ed);
        }

        [TestMethod]
        public void Video_ToMP4_Resize_Args()
        {
            Convert(VideoType.Mp4, new ScaleArgument(VideoSize.Ld), new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod]
        public void Video_ToMP4_Resize_Args_Pipe()
        {
            ConvertFromPipe(VideoType.Mp4, new ScaleArgument(VideoSize.Ld), new VideoCodecArgument(VideoCodec.LibX264));
        }

        [TestMethod]
        public void Video_ToOGV()
        {
            Convert(VideoType.Ogv);
        }

        [TestMethod]
        public void Video_ToMP4_MultiThread()
        {
            Convert(VideoType.Mp4, true);
        }

        [TestMethod]
        public void Video_ToTS_MultiThread()
        {
            Convert(VideoType.Ts, true);
        }

        [TestMethod]
        public void Video_ToOGV_MultiThread()
        {
            Convert(VideoType.Ogv, true);
        }

        [TestMethod]
        public void Video_Snapshot()
        {
            var output = Input.OutputLocation(ImageType.Png);

            try
            {
                var input = FFProbe.Analyse(Input.FullName);

                using var bitmap = FFMpeg.Snapshot(input, output);
                Assert.AreEqual(input.PrimaryVideoStream.Width, bitmap.Width);
                Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
                Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
            }
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        [TestMethod]
        public void Video_Snapshot_PersistSnapshot()
        {
            var output = Input.OutputLocation(ImageType.Png);
            try
            {
                var input = FFProbe.Analyse(Input.FullName);

                using var bitmap = FFMpeg.Snapshot(input, output, persistSnapshotOnFileSystem: true);
                Assert.AreEqual(input.PrimaryVideoStream.Width, bitmap.Width);
                Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
                Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
                Assert.IsTrue(File.Exists(output));
            }
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        [TestMethod]
        public void Video_Join()
        {
            var output = Input.OutputLocation(VideoType.Mp4);
            var newInput = Input.OutputLocation(VideoType.Mp4, "duplicate");
            try
            {
                var input = FFProbe.Analyse(Input.FullName);
                File.Copy(input.Path, newInput);
                var input2 = FFProbe.Analyse(newInput);

                var success = FFMpeg.Join(output, input, input2);
                Assert.IsTrue(success);
                
                Assert.IsTrue(File.Exists(output));
                var expectedDuration = input.Duration * 2;
                var result = FFProbe.Analyse(output);
                Assert.AreEqual(expectedDuration.Days, result.Duration.Days);
                Assert.AreEqual(expectedDuration.Hours, result.Duration.Hours);
                Assert.AreEqual(expectedDuration.Minutes, result.Duration.Minutes);
                Assert.AreEqual(expectedDuration.Seconds, result.Duration.Seconds);
                Assert.AreEqual(input.PrimaryVideoStream.Height, result.PrimaryVideoStream.Height);
                Assert.AreEqual(input.PrimaryVideoStream.Width, result.PrimaryVideoStream.Width);
            }
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);

                if (File.Exists(newInput))
                    File.Delete(newInput);
            }
        }

        [TestMethod]
        public void Video_Join_Image_Sequence()
        {
            try
            {
                var imageSet = new List<ImageInfo>();
                Directory.EnumerateFiles(VideoLibrary.ImageDirectory.FullName)
                    .Where(file => file.ToLower().EndsWith(".png"))
                    .ToList()
                    .ForEach(file =>
                    {
                        for (var i = 0; i < 15; i++)
                        {
                            imageSet.Add(new ImageInfo(file));
                        }
                    });

                var success = FFMpeg.JoinImageSequence(VideoLibrary.ImageJoinOutput.FullName, images: imageSet.ToArray());
                var result = FFProbe.Analyse(VideoLibrary.ImageJoinOutput.FullName);

                VideoLibrary.ImageJoinOutput.Refresh();

                Assert.IsTrue(VideoLibrary.ImageJoinOutput.Exists);
                Assert.AreEqual(3, result.Duration.Seconds);
                Assert.AreEqual(imageSet.First().Width, result.PrimaryVideoStream.Width);
                Assert.AreEqual(imageSet.First().Height, result.PrimaryVideoStream.Height);
            }
            finally
            {
                VideoLibrary.ImageJoinOutput.Refresh();
                if (VideoLibrary.ImageJoinOutput.Exists)
                {
                    VideoLibrary.ImageJoinOutput.Delete();
                }
            }
        }

        [TestMethod]
        public void Video_With_Only_Audio_Should_Extract_Metadata()
        {
            var video = FFProbe.Analyse(VideoLibrary.LocalVideoAudioOnly.FullName);
            Assert.AreEqual(null, video.PrimaryVideoStream);
            Assert.AreEqual("aac", video.PrimaryAudioStream.CodecName);
            Assert.AreEqual(79.5, video.Duration.TotalSeconds, 0.5);
            // Assert.AreEqual(1.25, video.Size);
        }

        [TestMethod]
        public void Video_Duration()
        {
            var video = FFProbe.Analyse(VideoLibrary.LocalVideo.FullName);
            var output = Input.OutputLocation(VideoType.Mp4);
            
            try
            {
                FFMpegArguments
                    .FromInputFiles(VideoLibrary.LocalVideo)
                    .WithDuration(TimeSpan.FromSeconds(video.Duration.TotalSeconds - 5))
                    .OutputToFile(output)
                    .ProcessSynchronously();

                Assert.IsTrue(File.Exists(output));
                var outputVideo = FFProbe.Analyse(output);

                Assert.AreEqual(video.Duration.Days, outputVideo.Duration.Days);
                Assert.AreEqual(video.Duration.Hours, outputVideo.Duration.Hours);
                Assert.AreEqual(video.Duration.Minutes, outputVideo.Duration.Minutes);
                Assert.AreEqual(video.Duration.Seconds - 5, outputVideo.Duration.Seconds);
            }
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        [TestMethod]
        public void Video_UpdatesProgress()
        {
            var output = Input.OutputLocation(VideoType.Mp4);

            var percentageDone = 0.0;
            var timeDone = TimeSpan.Zero;
            void OnPercentageProgess(double percentage) => percentageDone = percentage;
            void OnTimeProgess(TimeSpan time) => timeDone = time;

            var analysis = FFProbe.Analyse(VideoLibrary.LocalVideo.FullName);
            

            try
            {
                var success = FFMpegArguments
                    .FromInputFiles(VideoLibrary.LocalVideo)
                    .WithDuration(TimeSpan.FromSeconds(8))
                    .OutputToFile(output)
                    .NotifyOnProgress(OnPercentageProgess, analysis.Duration)
                    .NotifyOnProgress(OnTimeProgess)
                    .ProcessSynchronously();

                Assert.IsTrue(success);
                Assert.IsTrue(File.Exists(output));
                Assert.AreNotEqual(0.0, percentageDone);
                Assert.AreNotEqual(TimeSpan.Zero, timeDone);
            }
            finally
            {
                if (File.Exists(output))
                    File.Delete(output);
            }
        }

        [TestMethod]
        public void Video_TranscodeInMemory()
        {
            using var resStream = new MemoryStream();
            var reader = new StreamPipeDataReader(resStream);
            var writer = new RawVideoPipeDataWriter(BitmapSource.CreateBitmaps(128, PixelFormat.Format24bppRgb, 128, 128));

            FFMpegArguments
                .FromPipe(writer)
                .WithVideoCodec("vp9")
                .ForceFormat("webm")
                .OutputToPipe(reader)
                .ProcessSynchronously();

            resStream.Position = 0;
            var vi = FFProbe.Analyse(resStream);
            Assert.AreEqual(vi.PrimaryVideoStream.Width, 128);
            Assert.AreEqual(vi.PrimaryVideoStream.Height, 128);
        }
    }
}
