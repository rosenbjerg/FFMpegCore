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
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Snapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        int inputFileIndex = 0, FFOptions? ffOptions = null)
    {
        CheckSnapshotOutputExtension(output, FileExtension.Image.All);

        var source = FFProbe.Analyse(input, ffOptions);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, output, source, size, captureTime, streamIndex, inputFileIndex);

        return arguments.OutputToFile(output, true, outputOptions).WithOptions(ffOptions);
    }

    public static FFMpegArgumentProcessor GifSnapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, TimeSpan? duration = null,
        int? streamIndex = null, FFOptions? ffOptions = null)
    {
        CheckSnapshotOutputExtension(output, [FileExtension.Gif]);

        var source = FFProbe.Analyse(input, ffOptions);
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildGifSnapshotArguments(input, source, size, captureTime, duration, streamIndex);

        return arguments.OutputToFile(output, true, outputOptions).WithOptions(ffOptions);
    }

    private static void CheckSnapshotOutputExtension(string output, List<string> extensions)
    {
        if (!extensions.Contains(Path.GetExtension(output).ToLower()))
        {
            throw new ArgumentException(
                $"Invalid snapshot output extension: {output}, needed: {string.Join(",", extensions)}");
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
        return JoinImageSequence(null, output, frameRate, images);
    }

    /// <summary>
    ///     Converts an image sequence to a video.
    /// </summary>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="frameRate">FPS</param>
    /// <param name="images">Image sequence collection</param>
    public static FFMpegArgumentProcessor JoinImageSequence(FFOptions? ffOptions, string output, double frameRate = 30, params string[] images)
    {
        var arguments = FFMpegArguments.FromImageSequenceInput(images, options => options
            .WithFrameRate(frameRate));

        var streams = images.Select(image => FFProbe.Analyse(image, ffOptions).PrimaryVideoStream!).ToArray();
        foreach (var stream in streams)
        {
            FFMpegHelper.ConversionSizeExceptionCheck(stream.Width, stream.Height);
        }

        return arguments
            .OutputToFile(output, true, options => options
                .WithPixelFormat("yuv420p")
                .WithVideoFilters(filters => filters.Scale(streams[0].Width, streams[0].Height))
                .WithFrameRate(frameRate))
            .WithKnownDuration(TimeSpan.FromSeconds(images.Length / frameRate))
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Adds a poster image to an audio file.
    /// </summary>
    /// <param name="image">Source image file.</param>
    /// <param name="audio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="audioCodec">Encoder for the audio. Defaults to copying it, so the track is not degraded a second time.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor PosterWithAudio(string image, string audio, string output, Codec? audioCodec = null,
        FFOptions? ffOptions = null)
    {
        FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);
        var analysis = FFProbe.Analyse(image, ffOptions);
        FFMpegHelper.ConversionSizeExceptionCheck(analysis.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Height);

        return FFMpegArguments
            .FromFileInput(image, false, options => options
                .WithLoop(1)
                .ForceFormat("image2"))
            .AddFileInput(audio)
            .OutputToFile(output, true, options => options
                .WithPixelFormat("yuv420p")
                .WithVideoCodec(VideoCodec.LibX264)
                .WithConstantRateFactor(21)
                .WithAudioCodec(audioCodec ?? AudioCodec.Copy)
                .WithShortest())
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Joins media files through the concat demuxer, copying the streams rather than re-encoding. The inputs must share
    ///     codecs and parameters; where they do not, use <see cref="Join(string, string[])" />, which re-encodes.
    /// </summary>
    /// <param name="output">Output file.</param>
    /// <param name="inputs">Files to join, in order.</param>
    public static FFMpegArgumentProcessor Concat(string output, params string[] inputs)
    {
        return Concat(null, output, inputs);
    }

    /// <inheritdoc cref="Concat(string, string[])" />
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="output">Output file.</param>
    /// <param name="inputs">Files to join, in order.</param>
    public static FFMpegArgumentProcessor Concat(FFOptions? ffOptions, string output, params string[] inputs)
    {
        var duration = inputs.Aggregate(TimeSpan.Zero, (total, input) => total + FFProbe.Analyse(input, ffOptions).Duration);

        return FFMpegArguments
            .FromConcatDemuxerInput(inputs)
            .OutputToFile(output, true, options => options.CopyStreams())
            .WithKnownDuration(duration)
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Joins videos by re-encoding them through the concat filter, which requires them to share a resolution.
    ///     To join files that already share a codec, use <see cref="Concat(string, string[])" /> instead — it does not re-encode.
    /// </summary>
    /// <param name="output">Output video file.</param>
    /// <param name="videos">Videos to join, in order.</param>
    public static FFMpegArgumentProcessor Join(string output, params string[] videos)
    {
        return Join(null, output, videos);
    }

    /// <inheritdoc cref="Join(string, string[])" />
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="videos">Videos to join, in order.</param>
    public static FFMpegArgumentProcessor Join(FFOptions? ffOptions, string output, params string[] videos)
    {
        return Join(output, videos, null, ffOptions);
    }

    /// <inheritdoc cref="Join(string, string[])" />
    /// <param name="output">Output video file.</param>
    /// <param name="videos">Videos to join, in order.</param>
    /// <param name="addArguments">Output options, replacing the default h264/aac encode.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Join(string output, IEnumerable<string> videos, Action<FFMpegOutputOptions>? addArguments = null,
        FFOptions? ffOptions = null)
    {
        var paths = videos.ToArray();
        var analyses = paths.Select(video => FFProbe.Analyse(video, ffOptions)).ToArray();
        foreach (var analysis in analyses)
        {
            FFMpegHelper.ConversionSizeExceptionCheck(analysis);
        }

        var withAudio = analyses.All(analysis => analysis.PrimaryAudioStream != null);

        return FFMpegArguments
            .FromFileInput(paths)
            .OutputToFile(output, true, options =>
            {
                options.WithComplexFilter(graph =>
                {
                    var chain = graph.From(0, StreamType.Video, 0);
                    for (var index = 0; index < paths.Length; index++)
                    {
                        if (index > 0)
                        {
                            chain.From(index, StreamType.Video, 0);
                        }

                        if (withAudio)
                        {
                            chain.From(index, StreamType.Audio, 0);
                        }
                    }

                    chain.Concat(paths.Length, 1, withAudio ? 1 : 0)
                        .As(withAudio ? ["v", "a"] : ["v"]);
                });
                options.WithMap("v");
                if (withAudio)
                {
                    options.WithMap("a");
                }

                if (addArguments != null)
                {
                    addArguments(options);
                    return;
                }

                options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithVideoBitrate(2400)
                    .WithSpeedPreset(Speed.SuperFast)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(AudioQuality.Normal);
            })
            .WithKnownDuration(analyses.Aggregate(TimeSpan.Zero, (total, analysis) => total + analysis.Duration))
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Cuts the section between two timestamps out of a media file, copying the streams rather than re-encoding.
    /// </summary>
    /// <param name="input">Input media file.</param>
    /// <param name="output">Output media file. Its container must be able to mux the input's streams as they are.</param>
    /// <param name="startTime">Where the section starts.</param>
    /// <param name="endTime">Where the section ends.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Trim(string input, string output, TimeSpan startTime, TimeSpan endTime, FFOptions? ffOptions = null)
    {
        return FFMpegArguments
            .FromFileInput(input, true, options => options.WithStartTime(startTime).WithStopTime(endTime))
            .OutputToFile(output, true, options => options.CopyStreams())
            .WithKnownDuration(endTime - startTime)
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Records a remote stream to a file, copying the streams rather than re-encoding.
    /// </summary>
    /// <param name="uri">The stream to record — any protocol ffmpeg can open, such as http(s), rtmp, rtsp or srt.</param>
    /// <param name="output">
    ///     Output file. Prefer a container that stays playable when the recording is interrupted, such as .ts or .mkv;
    ///     an .mp4 is only finalised when the run ends cleanly.
    /// </param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor SaveStream(Uri uri, string output, FFOptions? ffOptions = null)
    {
        return FFMpegArguments
            .FromUrlInput(uri)
            .OutputToFile(output, true, options => options.CopyStreams())
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Strips a video file of its audio, copying every other stream.
    /// </summary>
    /// <param name="input">Input video file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor RemoveAudio(string input, string output, FFOptions? ffOptions = null)
    {
        var source = FFProbe.Analyse(input, ffOptions);
        FFMpegHelper.ConversionSizeExceptionCheck(source);

        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .WithMap(StreamType.All)
                .CopyStreams()
                .DisableAudio())
            .WithKnownDuration(source.Duration)
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Saves audio from a specific video file to disk.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="output">Output audio file. Its extension decides the container.</param>
    /// <param name="audioCodec">Encoder for the audio, or <see cref="AudioCodec.Copy" /> to extract it as it is. Defaults to the muxer's choice.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ExtractAudio(string input, string output, Codec? audioCodec = null, FFOptions? ffOptions = null)
    {
        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options =>
            {
                options.DisableVideo();
                if (audioCodec != null)
                {
                    options.WithAudioCodec(audioCodec);
                }
            })
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Adds audio to a video file.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="inputAudio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ReplaceAudio(string input, string inputAudio, string output, bool stopAtShortest = false,
        FFOptions? ffOptions = null)
    {
        var source = FFProbe.Analyse(input, ffOptions);
        FFMpegHelper.ConversionSizeExceptionCheck(source);

        return FFMpegArguments
            .FromFileInput(input)
            .AddFileInput(inputAudio)
            .OutputToFile(output, true, options => options
                .CopyStreams()
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioBitrate(AudioQuality.Good)
                .WithShortest(stopAtShortest))
            .WithKnownDuration(source.Duration)
            .WithOptions(ffOptions);
    }

    #region PixelFormats

    internal static IReadOnlyList<PixelFormat> GetPixelFormatsInternal(FFOptions ffOptions)
    {
        FFMpegHelper.VerifyFFMpegExists(ffOptions);

        var result = ProcessHelper.Run(GlobalFFOptions.GetFFMpegBinaryPath(ffOptions), "-pix_fmts");
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

    public static IReadOnlyList<PixelFormat> GetPixelFormats(FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            return GetPixelFormatsInternal(options);
        }

        return FFMpegCache.PixelFormats(options).Values.ToList().AsReadOnly();
    }

    public static bool TryGetPixelFormat(string name, out PixelFormat format, FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            format = GetPixelFormatsInternal(options).FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
            return format != null;
        }

        return FFMpegCache.PixelFormats(options).TryGetValue(name, out format);
    }

    public static PixelFormat GetPixelFormat(string name, FFOptions? ffOptions = null)
    {
        if (TryGetPixelFormat(name, out var fmt, ffOptions))
        {
            return fmt;
        }

        throw new FFMpegException(FFMpegExceptionType.Operation, $"Pixel format \"{name}\" not supported");
    }

    #endregion

    #region Codecs

    private static void ParsePartOfCodecs(Dictionary<string, Codec> codecs, FFOptions ffOptions, string arguments, Func<string, Codec?> parser)
    {
        var result = ProcessHelper.Run(GlobalFFOptions.GetFFMpegBinaryPath(ffOptions), arguments);
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

    internal static Dictionary<string, Codec> GetCodecsInternal(FFOptions ffOptions)
    {
        FFMpegHelper.VerifyFFMpegExists(ffOptions);

        var res = new Dictionary<string, Codec>();
        ParsePartOfCodecs(res, ffOptions, "-codecs", s =>
        {
            if (Codec.TryParseFromCodecs(s, out var codec))
            {
                return codec;
            }

            return null;
        });
        ParsePartOfCodecs(res, ffOptions, "-encoders", s =>
        {
            if (Codec.TryParseFromEncodersDecoders(s, out var codec, true))
            {
                return codec;
            }

            return null;
        });
        ParsePartOfCodecs(res, ffOptions, "-decoders", s =>
        {
            if (Codec.TryParseFromEncodersDecoders(s, out var codec, false))
            {
                return codec;
            }

            return null;
        });

        return res;
    }

    public static IReadOnlyList<Codec> GetCodecs(FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            return GetCodecsInternal(options).Values.ToList().AsReadOnly();
        }

        return FFMpegCache.Codecs(options).Values.ToList().AsReadOnly();
    }

    public static IReadOnlyList<Codec> GetCodecs(CodecType type, FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            return GetCodecsInternal(options).Values.Where(x => x.Type == type).ToList().AsReadOnly();
        }

        return FFMpegCache.Codecs(options).Values.Where(x => x.Type == type).ToList().AsReadOnly();
    }

    public static IReadOnlyList<Codec> GetVideoCodecs(FFOptions? ffOptions = null)
    {
        return GetCodecs(CodecType.Video, ffOptions);
    }

    public static IReadOnlyList<Codec> GetAudioCodecs(FFOptions? ffOptions = null)
    {
        return GetCodecs(CodecType.Audio, ffOptions);
    }

    public static IReadOnlyList<Codec> GetSubtitleCodecs(FFOptions? ffOptions = null)
    {
        return GetCodecs(CodecType.Subtitle, ffOptions);
    }

    public static IReadOnlyList<Codec> GetDataCodecs(FFOptions? ffOptions = null)
    {
        return GetCodecs(CodecType.Data, ffOptions);
    }

    public static bool TryGetCodec(string name, out Codec codec, FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            codec = GetCodecsInternal(options).Values.FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
            return codec != null;
        }

        return FFMpegCache.Codecs(options).TryGetValue(name, out codec);
    }

    public static Codec GetCodec(string name, FFOptions? ffOptions = null)
    {
        if (TryGetCodec(name, out var codec, ffOptions) && codec != null)
        {
            return codec;
        }

        throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{name}\" not supported");
    }

    #endregion

    #region ContainerFormats

    internal static IReadOnlyList<ContainerFormat> GetContainersFormatsInternal(FFOptions ffOptions)
    {
        FFMpegHelper.VerifyFFMpegExists(ffOptions);

        var result = ProcessHelper.Run(GlobalFFOptions.GetFFMpegBinaryPath(ffOptions), "-formats");
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

    public static IReadOnlyList<ContainerFormat> GetContainerFormats(FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            return GetContainersFormatsInternal(options);
        }

        return FFMpegCache.ContainerFormats(options).Values.ToList().AsReadOnly();
    }

    public static bool TryGetContainerFormat(string name, out ContainerFormat fmt, FFOptions? ffOptions = null)
    {
        var options = ffOptions ?? GlobalFFOptions.Current;
        if (!options.UseCache)
        {
            fmt = GetContainersFormatsInternal(options).FirstOrDefault(x => x.Name == name.ToLowerInvariant().Trim());
            return fmt != null;
        }

        return FFMpegCache.ContainerFormats(options).TryGetValue(name, out fmt);
    }

    public static ContainerFormat GetContainerFormat(string name, FFOptions? ffOptions = null)
    {
        if (TryGetContainerFormat(name, out var fmt, ffOptions))
        {
            return fmt;
        }

        throw new FFMpegException(FFMpegExceptionType.Operation, $"Container format \"{name}\" not supported");
    }

    #endregion
}
