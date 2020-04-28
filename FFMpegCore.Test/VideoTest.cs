﻿using FFMpegCore.Enums;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Enums;
using FFMpegCore.FFMPEG.Pipes;
using FFMpegCore.Test.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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
                var input = VideoInfo.FromFileInfo(Input);

                Encoder.Convert(input, output, type, size: size, multithreaded: multithreaded);

                var outputVideo = new VideoInfo(output.FullName);

                Assert.IsTrue(File.Exists(output.FullName));
                Assert.AreEqual(outputVideo.Duration, input.Duration);
                if (size == VideoSize.Original)
                {
                    Assert.AreEqual(outputVideo.Width, input.Width);
                    Assert.AreEqual(outputVideo.Height, input.Height);
                }
                else
                {
                    Assert.AreNotEqual(outputVideo.Width, input.Width);
                    Assert.AreNotEqual(outputVideo.Height, input.Height);
                    Assert.AreEqual(outputVideo.Height, (int)size);
                }
                return File.Exists(output.FullName) &&
                       outputVideo.Duration == input.Duration &&
                       (
                           (
                           size == VideoSize.Original &&
                           outputVideo.Width == input.Width &&
                           outputVideo.Height == input.Height
                           ) ||
                           (
                           size != VideoSize.Original &&
                           outputVideo.Width != input.Width &&
                           outputVideo.Height != input.Height &&
                           outputVideo.Height == (int)size
                           )
                       );
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);
            }
        }
        
        private void ConvertFromStreamPipe(VideoType type, ArgumentContainer container)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var input = VideoInfo.FromFileInfo(VideoLibrary.LocalVideoWebm);
                using (var inputStream = System.IO.File.OpenRead(input.FullName))
                {
                    var pipeSource = new StreamPipeDataWriter(inputStream);
                    var arguments = new ArgumentContainer { new InputPipeArgument(pipeSource) };
                    foreach (var arg in container)
                    {
                        arguments.Add(arg.Value);
                    }
                    arguments.Add(new OutputArgument(output));

                    var scaling = container.Find<ScaleArgument>();

                    Encoder.Convert(arguments);

                    var outputVideo = new VideoInfo(output.FullName);

                    Assert.IsTrue(File.Exists(output.FullName));
                    Assert.IsTrue(Math.Abs((outputVideo.Duration - input.Duration).TotalMilliseconds) < 1000.0 / input.FrameRate);

                    if (scaling == null)
                    {
                        Assert.AreEqual(outputVideo.Width, input.Width);
                        Assert.AreEqual(outputVideo.Height, input.Height);
                    }
                    else
                    {
                        if (scaling.Value.Width != -1)
                        {
                            Assert.AreEqual(outputVideo.Width, scaling.Value.Width);
                        }

                        if (scaling.Value.Height != -1)
                        {
                            Assert.AreEqual(outputVideo.Height, scaling.Value.Height);
                        }

                        Assert.AreNotEqual(outputVideo.Width, input.Width);
                        Assert.AreNotEqual(outputVideo.Height, input.Height);
                    }
                }
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);
            }
        }

        public void Convert(VideoType type, ArgumentContainer container)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var input = VideoInfo.FromFileInfo(Input);

                var arguments = new ArgumentContainer { new InputArgument(input) };
                foreach (var arg in container)
                {
                    arguments.Add(arg.Value);
                }
                arguments.Add(new OutputArgument(output));

                var scaling = container.Find<ScaleArgument>();

                Encoder.Convert(arguments);

                var outputVideo = new VideoInfo(output.FullName);

                Assert.IsTrue(File.Exists(output.FullName));
                Assert.AreEqual(outputVideo.Duration, input.Duration);

                if (scaling == null)
                {
                    Assert.AreEqual(outputVideo.Width, input.Width);
                    Assert.AreEqual(outputVideo.Height, input.Height);
                }
                else
                {
                    if (scaling.Value.Width != -1)
                    {
                        Assert.AreEqual(outputVideo.Width, scaling.Value.Width);
                    }

                    if (scaling.Value.Height != -1)
                    {
                        Assert.AreEqual(outputVideo.Height, scaling.Value.Height);
                    }

                    Assert.AreNotEqual(outputVideo.Width, input.Width);
                    Assert.AreNotEqual(outputVideo.Height, input.Height);
                }
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);
            }
        }

        public void ConvertFromPipe(VideoType type, ArgumentContainer container)
        {
            ConvertFromPipe(type, container, PixelFormat.Format24bppRgb);
            ConvertFromPipe(type, container, PixelFormat.Format32bppArgb);
            ConvertFromPipe(type, container, PixelFormat.Format48bppRgb);
        }

        public void ConvertFromPipe(VideoType type, ArgumentContainer container, PixelFormat fmt)
        {
            var output = Input.OutputLocation(type);

            try
            {
                var videoFramesSource = new RawVideoPipeDataWriter(BitmapSource.CreateBitmaps(128, fmt, 256, 256));
                var arguments = new ArgumentContainer { new InputPipeArgument(videoFramesSource) };
                foreach (var arg in container)
                {
                    arguments.Add(arg.Value);
                }
                arguments.Add(new OutputArgument(output));

                var scaling = container.Find<ScaleArgument>();

                Encoder.Convert(arguments);

                var outputVideo = new VideoInfo(output.FullName);

                Assert.IsTrue(File.Exists(output.FullName));

                if (scaling == null)
                {
                    Assert.AreEqual(outputVideo.Width, videoFramesSource.Width);
                    Assert.AreEqual(outputVideo.Height, videoFramesSource.Height);
                }
                else
                {
                    if (scaling.Value.Width != -1)
                    {
                        Assert.AreEqual(outputVideo.Width, scaling.Value.Width);
                    }

                    if (scaling.Value.Height != -1)
                    {
                        Assert.AreEqual(outputVideo.Height, scaling.Value.Height);
                    }

                    Assert.AreNotEqual(outputVideo.Width, videoFramesSource.Width);
                    Assert.AreNotEqual(outputVideo.Height, videoFramesSource.Height);
                }
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);
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
            var container = new ArgumentContainer { new VideoCodecArgument(VideoCodec.LibX264) };
            Convert(VideoType.Mp4, container);
        }

        [TestMethod]
        public void Video_ToMP4_Args_Pipe()
        {
            var container = new ArgumentContainer { new VideoCodecArgument(VideoCodec.LibX264) };
            ConvertFromPipe(VideoType.Mp4, container);
        }

        [TestMethod]
        public void Video_ToMP4_Args_StreamPipe()
        {
            var container = new ArgumentContainer { new VideoCodecArgument(VideoCodec.LibX264) };
            ConvertFromStreamPipe(VideoType.Mp4, container);
        }

        [TestMethod]
        public void Video_ToTS()
        {
            Convert(VideoType.Ts);
        }

        [TestMethod]
        public void Video_ToTS_Args()
        {
            var container = new ArgumentContainer
            {
                new CopyArgument(),
                new BitStreamFilterArgument(Channel.Video, Filter.H264_Mp4ToAnnexB),
                new ForceFormatArgument(VideoCodec.MpegTs)
            };
            Convert(VideoType.Ts, container);
        }

        [TestMethod]
        public void Video_ToTS_Args_Pipe()
        {
            var container = new ArgumentContainer
            {
                new CopyArgument(),
                new ForceFormatArgument(VideoCodec.MpegTs)
            };
            ConvertFromPipe(VideoType.Ts, container);
        }

        [TestMethod]
        public void Video_ToOGV_Resize()
        {
            Convert(VideoType.Ogv, true, VideoSize.Ed);
        }

        [TestMethod]
        public void Video_ToOGV_Resize_Args()
        {
            var container = new ArgumentContainer
            {
                new ScaleArgument(VideoSize.Ed),
                new VideoCodecArgument(VideoCodec.LibTheora)
            };
            Convert(VideoType.Ogv, container);
        }

        [TestMethod]
        public void Video_ToOGV_Resize_Args_Pipe()
        {
            var container = new ArgumentContainer
            {
                new ScaleArgument(VideoSize.Ed),
                new VideoCodecArgument(VideoCodec.LibTheora)
            };
            ConvertFromPipe(VideoType.Ogv, container);
        }

        [TestMethod]
        public void Video_ToMP4_Resize()
        {
            Convert(VideoType.Mp4, true, VideoSize.Ed);
        }

        [TestMethod]
        public void Video_ToMP4_Resize_Args()
        {
            var container = new ArgumentContainer
            {
                new ScaleArgument(VideoSize.Ld),
                new VideoCodecArgument(VideoCodec.LibX264)
            };
            Convert(VideoType.Mp4, container);
        }

        [TestMethod]
        public void Video_ToMP4_Resize_Args_Pipe()
        {
            var container = new ArgumentContainer
            {
                new ScaleArgument(VideoSize.Ld),
                new VideoCodecArgument(VideoCodec.LibX264)
            };
            ConvertFromPipe(VideoType.Mp4, container);
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
                var input = VideoInfo.FromFileInfo(Input);

                using var bitmap = Encoder.Snapshot(input, output);
                Assert.AreEqual(input.Width, bitmap.Width);
                Assert.AreEqual(input.Height, bitmap.Height);
                Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);
            }
        }

        [TestMethod]
        public void Video_Snapshot_PersistSnapshot()
        {
            var output = Input.OutputLocation(ImageType.Png);
            try
            {
                var input = VideoInfo.FromFileInfo(Input);

                using var bitmap = Encoder.Snapshot(input, output, persistSnapshotOnFileSystem: true);
                Assert.AreEqual(input.Width, bitmap.Width);
                Assert.AreEqual(input.Height, bitmap.Height);
                Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
                Assert.IsTrue(File.Exists(output.FullName));
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);
            }
        }

        [TestMethod]
        public void Video_Join()
        {
            var output = Input.OutputLocation(VideoType.Mp4);
            var newInput = Input.OutputLocation(VideoType.Mp4, "duplicate");
            try
            {
                var input = VideoInfo.FromFileInfo(Input);
                File.Copy(input.FullName, newInput.FullName);
                var input2 = VideoInfo.FromFileInfo(newInput);

                var result = Encoder.Join(output, input, input2);

                Assert.IsTrue(File.Exists(output.FullName));
                var expectedDuration = input.Duration * 2;
                Assert.AreEqual(expectedDuration.Days, result.Duration.Days);
                Assert.AreEqual(expectedDuration.Hours, result.Duration.Hours);
                Assert.AreEqual(expectedDuration.Minutes, result.Duration.Minutes);
                Assert.AreEqual(expectedDuration.Seconds, result.Duration.Seconds);
                Assert.AreEqual(input.Height, result.Height);
                Assert.AreEqual(input.Width, result.Width);
            }
            finally
            {
                if (File.Exists(output.FullName))
                    File.Delete(output.FullName);

                if (File.Exists(newInput.FullName))
                    File.Delete(newInput.FullName);
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

                var result = Encoder.JoinImageSequence(VideoLibrary.ImageJoinOutput, images: imageSet.ToArray());

                VideoLibrary.ImageJoinOutput.Refresh();

                Assert.IsTrue(VideoLibrary.ImageJoinOutput.Exists);
                Assert.AreEqual(3, result.Duration.Seconds);
                Assert.AreEqual(imageSet.First().Width, result.Width);
                Assert.AreEqual(imageSet.First().Height, result.Height);
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
            var video = VideoInfo.FromFileInfo(VideoLibrary.LocalVideoAudioOnly);
            Assert.AreEqual("none", video.VideoFormat);
            Assert.AreEqual("aac", video.AudioFormat);
            Assert.AreEqual(79.5, video.Duration.TotalSeconds, 0.5);
            Assert.AreEqual(1.25, video.Size);
        }

        [TestMethod]
        public void Video_Duration()
        {
            var video = VideoInfo.FromFileInfo(VideoLibrary.LocalVideo);
            var output = Input.OutputLocation(VideoType.Mp4);

            var arguments = new ArgumentContainer
            {
                new InputArgument(VideoLibrary.LocalVideo),
                new DurationArgument(TimeSpan.FromSeconds(video.Duration.TotalSeconds - 5)),
                new OutputArgument(output)
            };

            try
            {
                Encoder.Convert(arguments);

                Assert.IsTrue(File.Exists(output.FullName));
                var outputVideo = new VideoInfo(output.FullName);

                Assert.AreEqual(video.Duration.Days, outputVideo.Duration.Days);
                Assert.AreEqual(video.Duration.Hours, outputVideo.Duration.Hours);
                Assert.AreEqual(video.Duration.Minutes, outputVideo.Duration.Minutes);
                Assert.AreEqual(video.Duration.Seconds - 5, outputVideo.Duration.Seconds);
            }
            finally
            {
                if (File.Exists(output.FullName))
                    output.Delete();
            }
        }

        [TestMethod]
        public void Video_UpdatesProgress()
        {
            var output = Input.OutputLocation(VideoType.Mp4);

            var percentageDone = 0.0;
            void OnProgess(double percentage) => percentageDone = percentage;
            Encoder.OnProgress += OnProgess;

            var arguments = new ArgumentContainer
            {
                new InputArgument(VideoLibrary.LocalVideo),
                new DurationArgument(TimeSpan.FromSeconds(8)),
                new OutputArgument(output)
            };

            try
            {
                Encoder.Convert(arguments);
                Encoder.OnProgress -= OnProgess;

                Assert.IsTrue(File.Exists(output.FullName));
                Assert.AreNotEqual(0.0, percentageDone);
            }
            finally
            {
                if (File.Exists(output.FullName))
                    output.Delete();
            }
        }
    }
}
