using System.Drawing;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;

namespace FFMpegCore;

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
    public static FFMpegArgumentProcessor Snapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        int inputFileIndex = 0)
    {
        CheckSnapshotOutputExtension(output, FileExtension.Image.All);

        var source = FFProbe.Analyse(input);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, output, source, size, captureTime, streamIndex, inputFileIndex);

        return arguments.OutputToFile(output, true, outputOptions);
    }

    public static FFMpegArgumentProcessor GifSnapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, TimeSpan? duration = null,
        int? streamIndex = null)
    {
        CheckSnapshotOutputExtension(output, [FileExtension.Gif]);

        var source = FFProbe.Analyse(input);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildGifSnapshotArguments(input, source, size, captureTime, duration, streamIndex);

        return arguments.OutputToFile(output, true, outputOptions);
    }

    private static void CheckSnapshotOutputExtension(string output, List<string> extensions)
    {
        if (!extensions.Contains(Path.GetExtension(output).ToLower()))
        {
            throw new ArgumentException(
                $"Invalid snapshot output extension: {output}, needed: {string.Join(",", FileExtension.Image.All)}");
        }
    }

    /// <summary>
    ///     Converts an image sequence to a video.
    /// </summary>
    /// <param name="output">Output video file.</param>
    /// <param name="frameRate">FPS</param>
    /// <param name="images">Image sequence collection</param>
    public static FFMpegArgumentProcessor JoinImageSequence(string output, double frameRate = 30, params string[] images)
    {
        var arguments = FFMpegArguments.FromImageSequenceInput(images, options => options
            .WithFramerate(frameRate));

        var streams = images.Select(image => FFProbe.Analyse(image).PrimaryVideoStream!).ToArray();
        foreach (var stream in streams)
        {
            FFMpegHelper.ConversionSizeExceptionCheck(stream.Width, stream.Height);
        }

        return arguments
            .OutputToFile(output, true, options => options
                .ForcePixelFormat("yuv420p")
                .WithVideoFilters(filters => filters.Scale(streams[0].Width, streams[0].Height))
                .WithFramerate(frameRate))
            .WithKnownDuration(TimeSpan.FromSeconds(images.Length / frameRate));
    }

    /// <summary>
    ///     Adds a poster image to an audio file.
    /// </summary>
    /// <param name="image">Source image file.</param>
    /// <param name="audio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    public static FFMpegArgumentProcessor PosterWithAudio(string image, string audio, string output)
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
                .UsingShortest());
    }

    /// <summary>
    ///     Convert a video do a different format.
    /// </summary>
    /// <param name="input">Input video source.</param>
    /// <param name="output">Output information.</param>
    /// <param name="format">Target conversion video format.</param>
    /// <param name="speed">Conversion target speed/quality (faster speed = lower quality).</param>
    /// <param name="size">Video size.</param>
    /// <param name="audioQuality">Conversion target audio quality.</param>
    /// <param name="multithreaded">Is encoding multithreaded.</param>
    public static FFMpegArgumentProcessor Convert(
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

        var processor = format.Name switch
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
                    .WithAudioBitrate(audioQuality)),
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
                    .WithAudioBitrate(audioQuality)),
            "mpegts" => FFMpegArguments
                .FromFileInput(input)
                .OutputToFile(output, true, options => options
                    .CopyChannel()
                    .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                    .ForceFormat(VideoType.Ts)),
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
                    .WithAudioBitrate(audioQuality)),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        return processor.WithKnownDuration(source.Duration);
    }

    /// <summary>
    ///     Joins a list of video files.
    /// </summary>
    /// <param name="output">Output video file.</param>
    /// <param name="videos">List of vides that need to be joined together.</param>
    public static FFMpegArgumentProcessor Join(string output, params string[] videos)
    {
        var analyses = videos.Select(video => FFProbe.Analyse(video)).ToArray();
        foreach (var analysis in analyses)
        {
            FFMpegHelper.ConversionSizeExceptionCheck(analysis);
        }

        var withAudio = analyses.All(analysis => analysis.PrimaryAudioStream != null);
        var streams = string.Concat(analyses.Select((_, index) => withAudio ? $"[{index}:v:0][{index}:a:0]" : $"[{index}:v:0]"));
        var filter = $"{streams}concat=n={videos.Length}:v=1:a={(withAudio ? 1 : 0)}[v]{(withAudio ? "[a]" : string.Empty)}";
        var mapping = withAudio ? "-map \"[v]\" -map \"[a]\"" : "-map \"[v]\"";

        return FFMpegArguments
            .FromFileInput(videos)
            .OutputToFile(output, true, options => options
                .WithCustomArgument($"-filter_complex \"{filter}\" {mapping}")
                .WithVideoCodec(VideoCodec.LibX264)
                .WithVideoBitrate(2400)
                .WithSpeedPreset(Speed.SuperFast)
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Normal))
            .WithKnownDuration(analyses.Aggregate(TimeSpan.Zero, (total, analysis) => total + analysis.Duration));
    }

    public static FFMpegArgumentProcessor SubVideo(string input, string output, TimeSpan startTime, TimeSpan endTime)
    {
        if (Path.GetExtension(input) != Path.GetExtension(output))
        {
            output = Path.ChangeExtension(output, Path.GetExtension(input));
        }

        return FFMpegArguments
            .FromFileInput(input, true, options => options.Seek(startTime).EndSeek(endTime))
            .OutputToFile(output, true, options => options.CopyChannel())
            .WithKnownDuration(endTime - startTime);
    }

    /// <summary>
    ///     Records M3U8 streams to the specified output.
    /// </summary>
    /// <param name="uri">URI to pointing towards stream.</param>
    /// <param name="output">Output file</param>
    public static FFMpegArgumentProcessor SaveM3U8Stream(Uri uri, string output)
    {
        FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);

        if (uri.Scheme != "http" && uri.Scheme != "https")
        {
            throw new ArgumentException($"Uri: {uri.AbsoluteUri}, does not point to a valid http(s) stream.");
        }

        return FFMpegArguments
            .FromUrlInput(uri)
            .OutputToFile(output, true, options => options.CopyChannel(Channel.All));
    }

    /// <summary>
    ///     Strips a video file of audio.
    /// </summary>
    /// <param name="input">Input video file.</param>
    /// <param name="output">Output video file.</param>
    public static FFMpegArgumentProcessor Mute(string input, string output)
    {
        var source = FFProbe.Analyse(input);
        FFMpegHelper.ConversionSizeExceptionCheck(source);

        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .CopyChannel(Channel.Video)
                .DisableChannel(Channel.Audio))
            .WithKnownDuration(source.Duration);
    }

    /// <summary>
    ///     Saves audio from a specific video file to disk.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="output">Output audio file.</param>
    public static FFMpegArgumentProcessor ExtractAudio(string input, string output)
    {
        FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp3);

        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .DisableChannel(Channel.Video));
    }

    /// <summary>
    ///     Adds audio to a video file.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="inputAudio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
    public static FFMpegArgumentProcessor ReplaceAudio(string input, string inputAudio, string output, bool stopAtShortest = false)
    {
        var source = FFProbe.Analyse(input);
        FFMpegHelper.ConversionSizeExceptionCheck(source);

        return FFMpegArguments
            .FromFileInput(input)
            .AddFileInput(inputAudio)
            .OutputToFile(output, true, options => options
                .CopyChannel()
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good)
                .UsingShortest(stopAtShortest))
            .WithKnownDuration(source.Duration);
    }

    #region PixelFormats

    internal static IReadOnlyList<PixelFormat> GetPixelFormatsInternal()
    {
        var result = ProcessHelper.Run(GlobalFFOptions.GetFFMpegBinaryPath(), "-pix_fmts");
        if (result.ExitCode != 0)
        {
            throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", result.OutputData));
        }

        var list = new List<PixelFormat>();
        foreach (var line in result.OutputData)
        {
            if (PixelFormat.TryParse(line, out var format))
            {
                list.Add(format);
            }
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

        return FFMpegCache.PixelFormats.TryGetValue(name, out format);
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
        var result = ProcessHelper.Run(GlobalFFOptions.GetFFMpegBinaryPath(), arguments);
        if (result.ExitCode != 0)
        {
            throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", result.OutputData));
        }

        foreach (var line in result.OutputData)
        {
            var codec = parser(line);
            if (codec == null)
            {
                continue;
            }

            if (codecs.TryGetValue(codec.Name, out var parentCodec))
            {
                parentCodec.Merge(codec);
            }
            else
            {
                codecs.Add(codec.Name, codec);
            }
        }
    }

    internal static Dictionary<string, Codec> GetCodecsInternal()
    {
        var res = new Dictionary<string, Codec>();
        ParsePartOfCodecs(res, "-codecs", s =>
        {
            if (Codec.TryParseFromCodecs(s, out var codec))
            {
                return codec;
            }

            return null;
        });
        ParsePartOfCodecs(res, "-encoders", s =>
        {
            if (Codec.TryParseFromEncodersDecoders(s, out var codec, true))
            {
                return codec;
            }

            return null;
        });
        ParsePartOfCodecs(res, "-decoders", s =>
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

    public static IReadOnlyList<Codec> GetVideoCodecs()
    {
        return GetCodecs(CodecType.Video);
    }

    public static IReadOnlyList<Codec> GetAudioCodecs()
    {
        return GetCodecs(CodecType.Audio);
    }

    public static IReadOnlyList<Codec> GetSubtitleCodecs()
    {
        return GetCodecs(CodecType.Subtitle);
    }

    public static IReadOnlyList<Codec> GetDataCodecs()
    {
        return GetCodecs(CodecType.Data);
    }

    public static bool TryGetCodec(string name, out Codec codec)
    {
        if (!GlobalFFOptions.Current.UseCache)
        {
            codec = GetCodecsInternal().Values.FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
            return codec != null;
        }

        return FFMpegCache.Codecs.TryGetValue(name, out codec);
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
        var result = ProcessHelper.Run(GlobalFFOptions.GetFFMpegBinaryPath(), "-formats");
        if (result.ExitCode != 0)
        {
            throw new FFMpegException(FFMpegExceptionType.Process, string.Join("\r\n", result.OutputData));
        }

        var list = new List<ContainerFormat>();
        foreach (var line in result.OutputData)
        {
            if (ContainerFormat.TryParse(line, out var fmt))
            {
                list.Add(fmt);
            }
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

        return FFMpegCache.ContainerFormats.TryGetValue(name, out fmt);
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
}
