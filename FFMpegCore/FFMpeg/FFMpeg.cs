using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using FFMpegCore.Pipes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FFMpegCore
{
    public static class FFMpeg
    {
        /// <summary>
        ///     Saves a 'png' thumbnail from the input video to drive
        /// </summary>
        /// <param name="input">Source video analysis</param>
        /// <param name="output">Output video file path</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static bool Snapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            if (Path.GetExtension(output) != FileExtension.Png)
                output = Path.GetFileNameWithoutExtension(output) + FileExtension.Png;

            var source = FFProbe.Analyse(input);
            var (arguments, outputOptions) = BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);

            return arguments
                .OutputToFile(output, true, outputOptions)
                .ProcessSynchronously();
        }
        /// <summary>
        ///     Saves a 'png' thumbnail from the input video to drive
        /// </summary>
        /// <param name="input">Source video analysis</param>
        /// <param name="output">Output video file path</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static async Task<bool> SnapshotAsync(string input, string output, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            if (Path.GetExtension(output) != FileExtension.Png)
                output = Path.GetFileNameWithoutExtension(output) + FileExtension.Png;

            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);

            return await arguments
                .OutputToFile(output, true, outputOptions)
                .ProcessAsynchronously();
        }

        /// <summary>
        ///     Saves a 'png' thumbnail to an in-memory bitmap
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static Bitmap Snapshot(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = FFProbe.Analyse(input);
            var (arguments, outputOptions) = BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessSynchronously();

            ms.Position = 0;
            using var bitmap = new Bitmap(ms);
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        }
        /// <summary>
        ///     Saves a 'png' thumbnail to an in-memory bitmap
        /// </summary>
        /// <param name="input">Source video file.</param>
        /// <param name="captureTime">Seek position where the thumbnail should be taken.</param>
        /// <param name="size">Thumbnail size. If width or height equal 0, the other will be computed automatically.</param>
        /// <param name="streamIndex">Selected video stream index.</param>
        /// <param name="inputFileIndex">Input file index</param>
        /// <returns>Bitmap with the requested snapshot.</returns>
        public static async Task<Bitmap> SnapshotAsync(string input, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null, int inputFileIndex = 0)
        {
            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);
            using var ms = new MemoryStream();

            await arguments
                .OutputToPipe(new StreamPipeSink(ms), options => outputOptions(options
                    .ForceFormat("rawvideo")))
                .ProcessAsynchronously();

            ms.Position = 0;
            return new Bitmap(ms);
        }

        private static (FFMpegArguments, Action<FFMpegArgumentOptions> outputOptions) BuildSnapshotArguments(
            string input,
            IMediaAnalysis source,
            Size? size = null,
            TimeSpan? captureTime = null,
            int? streamIndex = null,
            int inputFileIndex = 0)
        {
            captureTime ??= TimeSpan.FromSeconds(source.Duration.TotalSeconds / 3);
            size = PrepareSnapshotSize(source, size);
            streamIndex ??= source.PrimaryVideoStream?.Index
                            ?? source.VideoStreams.FirstOrDefault()?.Index
                            ?? 0;

            return (FFMpegArguments
                .FromFileInput(input, false, options => options
                    .Seek(captureTime)),
                options => options
                    .SelectStream((int)streamIndex, inputFileIndex)
                    .WithVideoCodec(VideoCodec.Png)
                    .WithFrameOutputCount(1)
                    .Resize(size));
        }

        private static Size? PrepareSnapshotSize(IMediaAnalysis source, Size? wantedSize)
        {
            if (wantedSize == null || (wantedSize.Value.Height <= 0 && wantedSize.Value.Width <= 0) || source.PrimaryVideoStream == null)
                return null;

            var currentSize = new Size(source.PrimaryVideoStream.Width, source.PrimaryVideoStream.Height);
            if (source.PrimaryVideoStream.Rotation == 90 || source.PrimaryVideoStream.Rotation == 180)
                currentSize = new Size(source.PrimaryVideoStream.Height, source.PrimaryVideoStream.Width);

            if (wantedSize.Value.Width != currentSize.Width || wantedSize.Value.Height != currentSize.Height)
            {
                if (wantedSize.Value.Width <= 0 && wantedSize.Value.Height > 0)
                {
                    var ratio = (double)wantedSize.Value.Height / currentSize.Height;
                    return new Size((int)(currentSize.Width * ratio), (int)(currentSize.Height * ratio));
                }
                if (wantedSize.Value.Height <= 0 && wantedSize.Value.Width > 0)
                {
                    var ratio = (double)wantedSize.Value.Width / currentSize.Width;
                    return new Size((int)(currentSize.Width * ratio), (int)(currentSize.Height * ratio));
                }
                return wantedSize;
            }

            return null;
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
            string input,
            string output,
            ContainerFormat format,
            Speed speed = Speed.SuperFast,
            VideoSize size = VideoSize.Original,
            AudioQuality audioQuality = AudioQuality.Normal,
            bool multithreaded = false)
        {
            FFMpegHelper.ExtensionExceptionCheck(output, format.Extension);
            var source = FFProbe.Analyse(input);
            FFMpegHelper.ConversionSizeExceptionCheck(source);

            var scale = VideoSize.Original == size ? 1 : (double)source.PrimaryVideoStream!.Height / (int)size;
            var outputSize = new Size((int)(source.PrimaryVideoStream!.Width / scale), (int)(source.PrimaryVideoStream.Height / scale));

            if (outputSize.Width % 2 != 0)
                outputSize.Width += 1;

            return format.Name switch
            {
                "mp4" => FFMpegArguments
                    .FromFileInput(input)
                    .OutputToFile(output, true, options => options
                        .UsingMultithreading(multithreaded)
                        .WithVideoCodec(VideoCodec.LibX264)
                        .WithVideoBitrate(2400)
                        .WithVideoFilters(filterOptions => filterOptions 
                            .Scale(outputSize))
                        .WithSpeedPreset(speed)
                        .WithAudioCodec(AudioCodec.Aac)
                        .WithAudioBitrate(audioQuality))
                    .ProcessSynchronously(),
                "ogv" => FFMpegArguments
                    .FromFileInput(input)
                    .OutputToFile(output, true, options => options
                        .UsingMultithreading(multithreaded)
                        .WithVideoCodec(VideoCodec.LibTheora)
                        .WithVideoBitrate(2400)
                        .WithVideoFilters(filterOptions => filterOptions 
                            .Scale(outputSize))
                        .WithSpeedPreset(speed)
                        .WithAudioCodec(AudioCodec.LibVorbis)
                        .WithAudioBitrate(audioQuality))
                    .ProcessSynchronously(),
                "mpegts" => FFMpegArguments
                    .FromFileInput(input)
                    .OutputToFile(output, true, options => options
                        .CopyChannel()
                        .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                        .ForceFormat(VideoType.Ts))
                    .ProcessSynchronously(),
                "webm" => FFMpegArguments
                    .FromFileInput(input)
                    .OutputToFile(output, true, options => options
                        .UsingMultithreading(multithreaded)
                        .WithVideoCodec(VideoCodec.LibVpx)
                        .WithVideoBitrate(2400)
                        .WithVideoFilters(filterOptions => filterOptions 
                            .Scale(outputSize))
                        .WithSpeedPreset(speed)
                        .WithAudioCodec(AudioCodec.LibVorbis)
                        .WithAudioBitrate(audioQuality))
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
                .FromFileInput(image, false, options => options
                    .Loop(1))
                .AddFileInput(audio)
                .OutputToFile(output, true, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(21)
                    .WithAudioBitrate(AudioQuality.Normal)
                    .UsingShortest())
                .ProcessSynchronously();
        }
        
        /// <summary>
        ///     Joins a list of video files.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="videos">List of vides that need to be joined together.</param>
        /// <returns>Output video information.</returns>
        public static bool Join(string output, params string[] videos)
        {
            var temporaryVideoParts = videos.Select(videoPath =>
            {
                var video = FFProbe.Analyse(videoPath);
                FFMpegHelper.ConversionSizeExceptionCheck(video);
                var destinationPath = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, $"{Path.GetFileNameWithoutExtension(videoPath)}{FileExtension.Ts}");
                Directory.CreateDirectory(GlobalFFOptions.Current.TemporaryFilesFolder);
                Convert(videoPath, destinationPath, VideoType.Ts);
                return destinationPath;
            }).ToArray();

            try
            {
                return FFMpegArguments
                    .FromConcatInput(temporaryVideoParts)
                    .OutputToFile(output, true, options => options
                        .CopyChannel()
                        .WithBitStreamFilter(Channel.Audio, Filter.Aac_AdtstoAsc))
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
            var tempFolderName = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, Guid.NewGuid().ToString());
            var temporaryImageFiles = images.Select((imageInfo, index) =>
            {
                using var image = Image.FromFile(imageInfo.FullName);
                FFMpegHelper.ConversionSizeExceptionCheck(image);
                var destinationPath = Path.Combine(tempFolderName, $"{index.ToString().PadLeft(9, '0')}{imageInfo.Extension}");
                Directory.CreateDirectory(tempFolderName);
                File.Copy(imageInfo.FullName, destinationPath);
                return destinationPath;
            }).ToArray();

            var firstImage = images.First();
            try
            {
                return FFMpegArguments
                    .FromFileInput(Path.Combine(tempFolderName, "%09d.png"), false)
                    .OutputToFile(output, true, options => options
                        .Resize(firstImage.Width, firstImage.Height)
                        .WithFramerate(frameRate))
                    .ProcessSynchronously();
            }
            finally
            {
                Cleanup(temporaryImageFiles);
                Directory.Delete(tempFolderName);
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
                .FromUrlInput(uri)
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
            // FFMpegHelper.ExtensionExceptionCheck(output, source.Extension);

            return FFMpegArguments
                .FromFileInput(input)
                .OutputToFile(output, true, options => options
                    .CopyChannel(Channel.Video)
                    .DisableChannel(Channel.Audio))
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
                .FromFileInput(input)
                .OutputToFile(output, true, options => options
                    .DisableChannel(Channel.Video))
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
            // FFMpegHelper.ExtensionExceptionCheck(output, source.Format.);

            return FFMpegArguments
                .FromFileInput(input)
                .AddFileInput(inputAudio)
                .OutputToFile(output, true, options => options
                    .CopyChannel()
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(AudioQuality.Good)
                    .UsingShortest(stopAtShortest))
                .ProcessSynchronously();
        }

        #region PixelFormats
        internal static IReadOnlyList<PixelFormat> GetPixelFormatsInternal()
        {
            FFMpegHelper.RootExceptionCheck();

            var list = new List<PixelFormat>();
            using var instance = new Instances.Instance(GlobalFFOptions.GetFFMpegBinaryPath(), "-pix_fmts");
            instance.DataReceived += (e, args) =>
            {
                if (PixelFormat.TryParse(args.Data, out var format))
                    list.Add(format);
            };

            var exitCode = instance.BlockUntilFinished();
            if (exitCode != 0) throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", instance.OutputData));

            return list.AsReadOnly();
        }

        public static IReadOnlyList<PixelFormat> GetPixelFormats()
        {
            if (!GlobalFFOptions.Current.UseCache)
                return GetPixelFormatsInternal();
            return FFMpegCache.PixelFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetPixelFormat(string name, out PixelFormat fmt)
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                fmt = GetPixelFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return fmt != null;
            }
            else
                return FFMpegCache.PixelFormats.TryGetValue(name, out fmt);
        }

        public static PixelFormat GetPixelFormat(string name)
        {
            if (TryGetPixelFormat(name, out var fmt))
                return fmt;
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Pixel format \"{name}\" not supported");
        }
        #endregion

        #region Codecs

        private static void ParsePartOfCodecs(Dictionary<string, Codec> codecs, string arguments, Func<string, Codec?> parser)
        {
            FFMpegHelper.RootExceptionCheck();

            using var instance = new Instances.Instance(GlobalFFOptions.GetFFMpegBinaryPath(), arguments);
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
            if (!GlobalFFOptions.Current.UseCache)
                return GetCodecsInternal().Values.ToList().AsReadOnly();
            return FFMpegCache.Codecs.Values.ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetCodecs(CodecType type)
        {
            if (!GlobalFFOptions.Current.UseCache)
                return GetCodecsInternal().Values.Where(x => x.Type == type).ToList().AsReadOnly();
            return FFMpegCache.Codecs.Values.Where(x=>x.Type == type).ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetVideoCodecs() => GetCodecs(CodecType.Video);
        public static IReadOnlyList<Codec> GetAudioCodecs() => GetCodecs(CodecType.Audio);
        public static IReadOnlyList<Codec> GetSubtitleCodecs() => GetCodecs(CodecType.Subtitle);
        public static IReadOnlyList<Codec> GetDataCodecs() => GetCodecs(CodecType.Data);

        public static bool TryGetCodec(string name, out Codec codec)
        {
            if (!GlobalFFOptions.Current.UseCache)
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
            FFMpegHelper.RootExceptionCheck();

            var list = new List<ContainerFormat>();
            using var instance = new Instances.Instance(GlobalFFOptions.GetFFMpegBinaryPath(), "-formats");
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
            if (!GlobalFFOptions.Current.UseCache)
                return GetContainersFormatsInternal();
            return FFMpegCache.ContainerFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetContainerFormat(string name, out ContainerFormat fmt)
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                fmt = GetContainersFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return fmt != null;
            }
            else
                return FFMpegCache.ContainerFormats.TryGetValue(name, out fmt);
        }

        public static ContainerFormat GetContainerFormat(string name)
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