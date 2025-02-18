using System.Drawing;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using Instances;

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
            {
                output = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + FileExtension.Png);
            }

            var source = FFProbe.Analyse(input);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);

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
            {
                output = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + FileExtension.Png);
            }

            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, source, size, captureTime, streamIndex, inputFileIndex);

            return await arguments
                .OutputToFile(output, true, outputOptions)
                .ProcessAsynchronously();
        }

        public static bool GifSnapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, TimeSpan? duration = null, int? streamIndex = null)
        {
            if (Path.GetExtension(output)?.ToLower() != FileExtension.Gif)
            {
                output = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + FileExtension.Gif);
            }

            var source = FFProbe.Analyse(input);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildGifSnapshotArguments(input, source, size, captureTime, duration, streamIndex);

            return arguments
                .OutputToFile(output, true, outputOptions)
                .ProcessSynchronously();
        }

        public static async Task<bool> GifSnapshotAsync(string input, string output, Size? size = null, TimeSpan? captureTime = null, TimeSpan? duration = null, int? streamIndex = null)
        {
            if (Path.GetExtension(output)?.ToLower() != FileExtension.Gif)
            {
                output = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + FileExtension.Gif);
            }

            var source = await FFProbe.AnalyseAsync(input).ConfigureAwait(false);
            var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildGifSnapshotArguments(input, source, size, captureTime, duration, streamIndex);

            return await arguments
                .OutputToFile(output, true, outputOptions)
                .ProcessAsynchronously();
        }

        /// <summary>
        /// Converts an image sequence to a video.
        /// </summary>
        /// <param name="output">Output video file.</param>
        /// <param name="frameRate">FPS</param>
        /// <param name="images">Image sequence collection</param>
        /// <returns>Output video information.</returns>
        public static bool JoinImageSequence(string output, double frameRate = 30, params string[] images)
        {
            var fileExtensions = images.Select(Path.GetExtension).Distinct().ToArray();
            if (fileExtensions.Length != 1)
            {
                throw new ArgumentException("All images must have the same extension", nameof(images));
            }

            var fileExtension = fileExtensions[0].ToLowerInvariant();
            int? width = null, height = null;

            var tempFolderName = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolderName);

            try
            {
                var index = 0;
                foreach (var imagePath in images)
                {
                    var analysis = FFProbe.Analyse(imagePath);
                    FFMpegHelper.ConversionSizeExceptionCheck(analysis.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Height);
                    width ??= analysis.PrimaryVideoStream.Width;
                    height ??= analysis.PrimaryVideoStream.Height;

                    var destinationPath = Path.Combine(tempFolderName, $"{index++.ToString().PadLeft(9, '0')}{fileExtension}");
                    File.Copy(imagePath, destinationPath);
                }

                return FFMpegArguments
                    .FromFileInput(Path.Combine(tempFolderName, $"%09d{fileExtension}"), false)
                    .OutputToFile(output, true, options => options
                        .ForcePixelFormat("yuv420p")
                        .Resize(width!.Value, height!.Value)
                        .WithFramerate(frameRate))
                    .ProcessSynchronously();
            }
            finally
            {
                Directory.Delete(tempFolderName, true);
            }
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
            var analysis = FFProbe.Analyse(image);
            FFMpegHelper.ConversionSizeExceptionCheck(analysis.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Height);

            return FFMpegArguments
                .FromFileInput(image, false, options => options
                    .Loop(1)
                    .ForceFormat("image2"))
                .AddFileInput(audio)
                .OutputToFile(output, true, options => options
                    .ForcePixelFormat("yuv420p")
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(21)
                    .WithAudioBitrate(AudioQuality.Normal)
                    .UsingShortest())
                .ProcessSynchronously();
        }

        /// <summary>
        /// Convert a video do a different format.
        /// </summary>
        /// <param name="input">Input video source.</param>
        /// <param name="output">Output information.</param>
        /// <param name="format">Target conversion video format.</param>
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
            {
                outputSize.Width += 1;
            }

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

        private static FFMpegArgumentProcessor BaseSubVideo(string input, string output, TimeSpan startTime, TimeSpan endTime)
        {
            if (Path.GetExtension(input) != Path.GetExtension(output))
            {
                output = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output), Path.GetExtension(input));
            }

            return FFMpegArguments
                .FromFileInput(input, true, options => options.Seek(startTime).EndSeek(endTime))
                .OutputToFile(output, true, options => options.CopyChannel());
        }

        /// <summary>
        ///     Creates a new video starting and ending at the specified times
        /// </summary>
        /// <param name="input">Input video file.</param>
        /// <param name="output">Output video file.</param>
        /// <param name="startTime">The start time of when the sub video needs to start</param>
        /// <param name="endTime">The end time of where the sub video needs to  end</param>
        /// <returns>Output video information.</returns>
        public static bool SubVideo(string input, string output, TimeSpan startTime, TimeSpan endTime)
        {
            return BaseSubVideo(input, output, startTime, endTime)
                .ProcessSynchronously();
        }

        /// <summary>
        ///     Creates a new video starting and ending at the specified times
        /// </summary>
        /// <param name="input">Input video file.</param>
        /// <param name="output">Output video file.</param>
        /// <param name="startTime">The start time of when the sub video needs to start</param>
        /// <param name="endTime">The end time of where the sub video needs to  end</param>
        /// <returns>Output video information.</returns>
        public static async Task<bool> SubVideoAsync(string input, string output, TimeSpan startTime, TimeSpan endTime)
        {
            return await BaseSubVideo(input, output, startTime, endTime)
                .ProcessAsynchronously();
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
            {
                throw new ArgumentException($"Uri: {uri.AbsoluteUri}, does not point to a valid http(s) stream.");
            }

            return FFMpegArguments
                .FromUrlInput(uri, options =>
                {
                    options.WithCopyCodec();
                })
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
            var processArguments = new ProcessArguments(GlobalFFOptions.GetFFMpegBinaryPath(), "-pix_fmts");
            processArguments.OutputDataReceived += (e, data) =>
            {
                if (PixelFormat.TryParse(data, out var format))
                {
                    list.Add(format);
                }
            };

            var result = processArguments.StartAndWaitForExit();
            if (result.ExitCode != 0)
            {
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", result.OutputData));
            }

            return list.AsReadOnly();
        }

        public static IReadOnlyList<PixelFormat> GetPixelFormats()
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                return GetPixelFormatsInternal();
            }

            return FFMpegCache.PixelFormats.Values.ToList().AsReadOnly();
        }

        public static bool TryGetPixelFormat(string name, out PixelFormat format)
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                format = GetPixelFormatsInternal().FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
                return format != null;
            }
            else
            {
                return FFMpegCache.PixelFormats.TryGetValue(name, out format);
            }
        }

        public static PixelFormat GetPixelFormat(string name)
        {
            if (TryGetPixelFormat(name, out var fmt))
            {
                return fmt;
            }

            throw new FFMpegException(FFMpegExceptionType.Operation, $"Pixel format \"{name}\" not supported");
        }
        #endregion

        #region Codecs

        private static void ParsePartOfCodecs(Dictionary<string, Codec> codecs, string arguments, Func<string, Codec?> parser)
        {
            FFMpegHelper.RootExceptionCheck();

            var processArguments = new ProcessArguments(GlobalFFOptions.GetFFMpegBinaryPath(), arguments);
            processArguments.OutputDataReceived += (e, data) =>
            {
                var codec = parser(data);
                if (codec != null)
                {
                    if (codecs.TryGetValue(codec.Name, out var parentCodec))
                    {
                        parentCodec.Merge(codec);
                    }
                    else
                    {
                        codecs.Add(codec.Name, codec);
                    }
                }
            };

            var result = processArguments.StartAndWaitForExit();
            if (result.ExitCode != 0)
            {
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", result.OutputData));
            }
        }

        internal static Dictionary<string, Codec> GetCodecsInternal()
        {
            var res = new Dictionary<string, Codec>();
            ParsePartOfCodecs(res, "-codecs", (s) =>
            {
                if (Codec.TryParseFromCodecs(s, out var codec))
                {
                    return codec;
                }

                return null;
            });
            ParsePartOfCodecs(res, "-encoders", (s) =>
            {
                if (Codec.TryParseFromEncodersDecoders(s, out var codec, true))
                {
                    return codec;
                }

                return null;
            });
            ParsePartOfCodecs(res, "-decoders", (s) =>
            {
                if (Codec.TryParseFromEncodersDecoders(s, out var codec, false))
                {
                    return codec;
                }

                return null;
            });

            return res;
        }

        public static IReadOnlyList<Codec> GetCodecs()
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                return GetCodecsInternal().Values.ToList().AsReadOnly();
            }

            return FFMpegCache.Codecs.Values.ToList().AsReadOnly();
        }

        public static IReadOnlyList<Codec> GetCodecs(CodecType type)
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                return GetCodecsInternal().Values.Where(x => x.Type == type).ToList().AsReadOnly();
            }

            return FFMpegCache.Codecs.Values.Where(x => x.Type == type).ToList().AsReadOnly();
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
            {
                return FFMpegCache.Codecs.TryGetValue(name, out codec);
            }
        }

        public static Codec GetCodec(string name)
        {
            if (TryGetCodec(name, out var codec) && codec != null)
            {
                return codec;
            }

            throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{name}\" not supported");
        }
        #endregion

        #region ContainerFormats
        internal static IReadOnlyList<ContainerFormat> GetContainersFormatsInternal()
        {
            FFMpegHelper.RootExceptionCheck();

            var list = new List<ContainerFormat>();
            var instance = new ProcessArguments(GlobalFFOptions.GetFFMpegBinaryPath(), "-formats");
            instance.OutputDataReceived += (e, data) =>
            {
                if (ContainerFormat.TryParse(data, out var fmt))
                {
                    list.Add(fmt);
                }
            };

            var result = instance.StartAndWaitForExit();
            if (result.ExitCode != 0)
            {
                throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", result.OutputData));
            }

            return list.AsReadOnly();
        }

        public static IReadOnlyList<ContainerFormat> GetContainerFormats()
        {
            if (!GlobalFFOptions.Current.UseCache)
            {
                return GetContainersFormatsInternal();
            }

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
            {
                return FFMpegCache.ContainerFormats.TryGetValue(name, out fmt);
            }
        }

        public static ContainerFormat GetContainerFormat(string name)
        {
            if (TryGetContainerFormat(name, out var fmt))
            {
                return fmt;
            }

            throw new FFMpegException(FFMpegExceptionType.Operation, $"Container format \"{name}\" not supported");
        }
        #endregion

        private static void Cleanup(IEnumerable<string> pathList)
        {
            foreach (var path in pathList)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
