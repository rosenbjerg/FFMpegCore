using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;

namespace FFMpegCore
{
    public static class FFMpeg
    {
        /// <summary>
        ///     Saves a 'png' thumbnail from the input video.
        /// </summary>
        /// <param name="source">Source video file.</param>
        /// <param name="output">Output video file</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="persistSnapshotOnFileSystem">By default, it deletes the created image on disk. If set to true, it won't delete the image</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static Bitmap Snapshot(MediaAnalysis source, string output, Size? size = null, TimeSpan? captureTime = null,
            bool persistSnapshotOnFileSystem = false)
        {
            if (captureTime == null)
                captureTime = TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3);

            if (Path.GetExtension(output) != FileExtension.Png)
                output = Path.GetFileNameWithoutExtension(output) + FileExtension.Png;

            if (size == null || (size.Value.Height == 0 && size.Value.Width == 0))
                size = new Size(source.PrimaryVideoStream.Width, source.PrimaryVideoStream.Height);

            if (size.Value.Width != size.Value.Height)
            {
                if (size.Value.Width == 0)
                {
                    var ratio = source.PrimaryVideoStream.Width / (double)size.Value.Width;

                    size = new Size((int)(source.PrimaryVideoStream.Width * ratio), (int)(source.PrimaryVideoStream.Height * ratio));
                }

                if (size.Value.Height == 0)
                {
                    var ratio = source.PrimaryVideoStream.Height / (double)size.Value.Height;

                    size = new Size((int)(source.PrimaryVideoStream.Width * ratio), (int)(source.PrimaryVideoStream.Height * ratio));
                }
            }

            var success = FFMpegArguments
                .FromInputFiles(true, source.Path)
                .WithVideoCodec(VideoCodec.Png)
                .WithFrameOutputCount(1)
                .Resize(size)
                .Seek(captureTime)
                .OutputToFile(output)
                .ProcessSynchronously();


            if (!success)
                throw new OperationCanceledException("Could not take snapshot!");

            Bitmap result;
            using (var bmp = (Bitmap)Image.FromFile(output))
            {
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                result = new Bitmap(ms);
            }

            if (File.Exists(output) && !persistSnapshotOnFileSystem)
                File.Delete(output);

            return result;
        }

        /// <summary>
        /// Convert a video do a different format.
        /// </summary>
        /// <param name="source">Input video source.</param>
        /// <param name="output">Output information.</param>
        /// <param name="type">Target conversion video type.</param>
        /// <param name="speed">Conversion target speed/quality (faster speed = lower quality).</param>
        /// <param name="size">Video size.</param>
        /// <param name="audioQuality">Conversion target audio quality.</param>
        /// <param name="multithreaded">Is encoding multithreaded.</param>
        /// <returns>Output video information.</returns>
        public static bool Convert(
            MediaAnalysis source,
            string output,
            ContainerFormat format,
            Speed speed = Speed.SuperFast,
            VideoSize size = VideoSize.Original,
            AudioQuality audioQuality = AudioQuality.Normal,
            bool multithreaded = false)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, format.Extension);
            FFMpegHelper.ConversionSizeExceptionCheck(source);

            var scale = VideoSize.Original == size ? 1 : (double)source.PrimaryVideoStream.Height / (int)size;
            var outputSize = new Size((int)(source.PrimaryVideoStream.Width / scale), (int)(source.PrimaryVideoStream.Height / scale));

            if (outputSize.Width % 2 != 0)
                outputSize.Width += 1;

            return format.Name switch
            {
                "mp4" => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .UsingMultithreading(multithreaded)
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithVideoBitrate(2400)
                    .Scale(outputSize)
                    .WithSpeedPreset(speed)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(audioQuality)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                "ogv" => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .UsingMultithreading(multithreaded)
                    .WithVideoCodec(VideoCodec.LibTheora)
                    .WithVideoBitrate(2400)
                    .Scale(outputSize)
                    .WithSpeedPreset(speed)
                    .WithAudioCodec(AudioCodec.LibVorbis)
                    .WithAudioBitrate(audioQuality)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                "mpegts" => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .CopyChannel()
                    .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                    .ForceFormat(VideoType.Ts)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                "webm" => FFMpegArguments
                    .FromInputFiles(true, source.Path)
                    .UsingMultithreading(multithreaded)
                    .WithVideoCodec(VideoCodec.LibVpx)
                    .WithVideoBitrate(2400)
                    .Scale(outputSize)
                    .WithSpeedPreset(speed)
                    .WithAudioCodec(AudioCodec.LibVorbis)
                    .WithAudioBitrate(audioQuality)
                    .OutputToFile(output)
                    .ProcessSynchronously(),
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
        }

        /// <summary>
        ///     Adds a poster image to an audio file.
        /// </summary>
        /// <param name="image">Source image file.</param>
        /// <param name="audio">Source audio file.</param>
        /// <param name="output">Output video file.</param>
        /// <returns></returns>
        public static bool PosterWithAudio(string image, string audio, string output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);
            FFMpegHelper.ConversionSizeExceptionCheck(Image.FromFile(image));

            return FFMpegArguments
                .FromInputFiles(true, image, audio)
                .Loop(1)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(21)
                .WithAudioBitrate(AudioQuality.Normal)
                .UsingShortest()
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Joins a list of video files.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="videos">List of vides that need to be joined together.</param>
        /// <returns>Output video information.</returns>
        public static bool Join(string output, params MediaAnalysis[] videos)
        {
            var temporaryVideoParts = videos.Select(video =>
            {
                FFMpegHelper.ConversionSizeExceptionCheck(video);
                var destinationPath = Path.Combine(FFMpegOptions.Options.TempDirectory, $"{Path.GetFileNameWithoutExtension(video.Path)}{FileExtension.Ts}");
                Convert(video, destinationPath, VideoType.Ts);
                return destinationPath;
            }).ToArray();

            try
            {
                return FFMpegArguments
                    .FromConcatenation(temporaryVideoParts)
                    .CopyChannel()
                    .WithBitStreamFilter(Channel.Audio, Filter.Aac_AdtstoAsc)
                    .OutputToFile(output)
                    .ProcessSynchronously();
            }
            finally
            {
                Cleanup(temporaryVideoParts);
            }
        }

        /// <summary>
        /// Converts an image sequence to a video.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="frameRate">FPS</param>
        /// <param name="images">Image sequence collection</param>
        /// <returns>Output video information.</returns>
        public static bool JoinImageSequence(string output, double frameRate = 30, params ImageInfo[] images)
        {
            var temporaryImageFiles = images.Select((image, index) =>
            {
                FFMpegHelper.ConversionSizeExceptionCheck(Image.FromFile(image.FullName));
                var destinationPath = Path.Combine(FFMpegOptions.Options.TempDirectory, $"{index.ToString().PadLeft(9, '0')}{image.Extension}");
                File.Copy(image.FullName, destinationPath);
                return destinationPath;
            }).ToArray();

            var firstImage = images.First();
            try
            {
                return FFMpegArguments
                    .FromInputFiles(false, Path.Combine(FFMpegOptions.Options.TempDirectory, "%09d.png"))
                    .WithVideoCodec(VideoCodec.LibX264)
                    .Resize(firstImage.Width, firstImage.Height)
                    .WithFrameOutputCount(images.Length)
                    .WithStartNumber(0)
                    .WithFramerate(frameRate)
                    .OutputToFile(output)
                    .ProcessSynchronously();
            }
            finally
            {
                Cleanup(temporaryImageFiles);
            }
        }

        /// <summary>
        ///     Records M3U8 streams to the specified output.
        /// </summary>
        /// <param name="uri">URI to pointing towards stream.</param>
        /// <param name="output">Output file</param>
        /// <returns>Success state.</returns>
        public static bool SaveM3U8Stream(Uri uri, string output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);

            if (uri.Scheme != "http" && uri.Scheme != "https")
                throw new ArgumentException($"Uri: {uri.AbsoluteUri}, does not point to a valid http(s) stream.");
            
            return FFMpegArguments
                .FromInputFiles(false, uri)
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Strips a video file of audio.
        /// </summary>
        /// <param name="input">Input video file.</param>
        /// <param name="output">Output video file.</param>
        /// <returns></returns>
        public static bool Mute(string input, string output)
        {
            var source = FFProbe.Analyse(input);
            FFMpegHelper.ConversionSizeExceptionCheck(source);
            FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return FFMpegArguments
                .FromInputFiles(true, source.Path)
                .CopyChannel(Channel.Video)
                .DisableChannel(Channel.Audio)
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Saves audio from a specific video file to disk.
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="output">Output audio file.</param>
        /// <returns>Success state.</returns>
        public static bool ExtractAudio(string input, string output)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp3);

            return FFMpegArguments
                .FromInputFiles(true, input)
                .DisableChannel(Channel.Video)
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Adds audio to a video file.
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="inputAudio">Source audio file.</param>
        /// <param name="output">Output video file.</param>
        /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
        /// <returns>Success state</returns>
        public static bool ReplaceAudio(string input, string inputAudio, string output, bool stopAtShortest = false)
        {
            var source = FFProbe.Analyse(input);
            FFMpegHelper.ConversionSizeExceptionCheck(source);
            FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return FFMpegArguments
                .FromInputFiles(true, source.Path, inputAudio)
                .CopyChannel()
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good)
                .UsingShortest(stopAtShortest)
                .OutputToFile(output)
                .ProcessSynchronously();
        }

        #region PixelFormats
        internal static IReadOnlyList<Enums.PixelFormat> GetPixelFormatsInternal()
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);

            var list = new List<Enums.PixelFormat>();
            using var instance = new Instances.Instance(FFMpegOptions.Options.FFmpegBinary(), "-pix_fmts");
            instance.DataReceived += (e, args) =>
            {
                if (Enums.PixelFormat.TryParse(args.Data, out var fmt))
                    list.Add(fmt);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0) throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));

            return list.AsReadOnly();
        }

        public static IReadOnlyList<Enums.PixelFormat> GetPixelFormats()
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetPixelFormatsInternal();
            return FFMpegCache.PixelFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetPixelFormat(string name, out Enums.PixelFormat fmt)
        {
            if (!FFMpegOptions.Options.UseCache)
            {
                fmt = GetPixelFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return fmt != null;
            }
            else
                return FFMpegCache.PixelFormats.TryGetValue(name, out fmt);
        }

        public static Enums.PixelFormat GetPixelFormat(string name)
        {
            if (TryGetPixelFormat(name, out var fmt))
                return fmt;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Pixel format \"{name}\" not supported");
        }
        #endregion

        #region Codecs
        internal static void ParsePartOfCodecs(Dictionary<string, Codec> codecs, string arguments, Func<string, Codec?> parser)
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);

            using var instance = new Instances.Instance(FFMpegOptions.Options.FFmpegBinary(), arguments);
            instance.DataReceived += (e, args) =>
            {
                var codec = parser(args.Data);
                if(codec != null)
                    if (codecs.TryGetValue(codec.Name, out var parentCodec))
                        parentCodec.Merge(codec);
                    else
                        codecs.Add(codec.Name, codec);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0) throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));
        }

        internal static Dictionary<string, Codec> GetCodecsInternal()
        {
            var res = new Dictionary<string, Codec>();
            ParsePartOfCodecs(res, "-codecs", (s) =>
            {
                if (Codec.TryParseFromCodecs(s, out var codec))
                    return codec;
                return null;
            });
            ParsePartOfCodecs(res, "-encoders", (s) =>
            {
                if (Codec.TryParseFromEncodersDecoders(s, out var codec, true))
                    return codec;
                return null;
            });
            ParsePartOfCodecs(res, "-decoders", (s) =>
            {
                if (Codec.TryParseFromEncodersDecoders(s, out var codec, false))
                    return codec;
                return null;
            });

            return res;
        }

        public static IReadOnlyList<Codec> GetCodecs()
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetCodecsInternal().Values.ToList().AsReadOnly();
            return FFMpegCache.Codecs.Values.ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetCodecs(CodecType type)
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetCodecsInternal().Values.Where(x => x.Type == type).ToList().AsReadOnly();
            return FFMpegCache.Codecs.Values.Where(x=>x.Type == type).ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetVideoCodecs() => GetCodecs(CodecType.Video);
        public static IReadOnlyList<Codec> GetAudioCodecs() => GetCodecs(CodecType.Audio);
        public static IReadOnlyList<Codec> GetSubtitleCodecs() => GetCodecs(CodecType.Subtitle);
        public static IReadOnlyList<Codec> GetDataCodecs() => GetCodecs(CodecType.Data);

        public static bool TryGetCodec(string name, out Codec codec)
        {
            if (!FFMpegOptions.Options.UseCache)
            {
                codec = GetCodecsInternal().Values.FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return codec != null;
            }
            else
                return FFMpegCache.Codecs.TryGetValue(name, out codec);
        }

        public static Codec GetCodec(string name)
        {
            if (TryGetCodec(name, out var codec) && codec != null)
                return codec;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{name}\" not supported");
        }
        #endregion

        #region ContainerFormats
        internal static IReadOnlyList<ContainerFormat> GetContainersFormatsInternal()
        {
            FFMpegHelper.RootExceptionCheck(FFMpegOptions.Options.RootDirectory);

            var list = new List<ContainerFormat>();
            using var instance = new Instances.Instance(FFMpegOptions.Options.FFmpegBinary(), "-formats");
            instance.DataReceived += (e, args) =>
            {
                if (ContainerFormat.TryParse(args.Data, out var fmt))
                    list.Add(fmt);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0) throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));

            return list.AsReadOnly();
        }

        public static IReadOnlyList<ContainerFormat> GetContainerFormats()
        {
            if (!FFMpegOptions.Options.UseCache)
                return GetContainersFormatsInternal();
            return FFMpegCache.ContainerFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetContainerFormat(string name, out ContainerFormat fmt)
        {
            if (!FFMpegOptions.Options.UseCache)
            {
                fmt = GetContainersFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return fmt != null;
            }
            else
                return FFMpegCache.ContainerFormats.TryGetValue(name, out fmt);
        }

        public static ContainerFormat GetContinerFormat(string name)
        {
            if (TryGetContainerFormat(name, out var fmt))
                return fmt;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Container format \"{name}\" not supported");
        }
        #endregion

        private static void Cleanup(IEnumerable<string> pathList)
        {
            foreach (var path in pathList)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}