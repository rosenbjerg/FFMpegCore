using System.Drawing;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;

namespace FFMpegCore;

public static class FFMpeg
{
    /// <summary>
    ///     Saves a single frame of the input to an image file.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="output">Output image file. Its extension decides the format: .png, .jpg, .bmp or .webp.</param>
    /// <param name="size">Thumbnail size. If width or height is 0 or -1, it is computed from the other.</param>
    /// <param name="captureTime">Seek position the frame is taken from. Defaults to a third of the way in.</param>
    /// <param name="streamIndex">Selected video stream index.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Snapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, int? streamIndex = null,
        FFOptions? ffOptions = null)
    {
        return Snapshot(FFProbe.Analyse(input, ffOptions), output, size, captureTime, streamIndex, ffOptions);
    }

    /// <summary>
    ///     Saves a single frame of an already analysed input to an image file.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="output">Output image file. Its extension decides the format: .png, .jpg, .bmp or .webp.</param>
    /// <param name="size">Thumbnail size. If width or height is 0 or -1, it is computed from the other.</param>
    /// <param name="captureTime">Seek position the frame is taken from. Defaults to a third of the way in.</param>
    /// <param name="streamIndex">Selected video stream index.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Snapshot(IMediaAnalysis source, string output, Size? size = null, TimeSpan? captureTime = null,
        int? streamIndex = null, FFOptions? ffOptions = null)
    {
        CheckSnapshotOutputExtension(output, FileExtension.Image.All);

        var input = InputPathOf(source, nameof(source));
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildSnapshotArguments(input, output, source, size, captureTime, streamIndex);

        return arguments.OutputToFile(output, true, outputOptions).WithOptions(ffOptions);
    }

    /// <summary>
    ///     Saves a section of the input as an animated gif.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="output">Output .gif file.</param>
    /// <param name="size">Output size. If width or height is 0 or -1, it is computed from the other. Defaults to 480 wide.</param>
    /// <param name="captureTime">Seek position the section starts at. Defaults to a third of the way in.</param>
    /// <param name="duration">How much of the input to capture.</param>
    /// <param name="streamIndex">Selected video stream index.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor GifSnapshot(string input, string output, Size? size = null, TimeSpan? captureTime = null, TimeSpan? duration = null,
        int? streamIndex = null, FFOptions? ffOptions = null)
    {
        return GifSnapshot(FFProbe.Analyse(input, ffOptions), output, size, captureTime, duration, streamIndex, ffOptions);
    }

    /// <summary>
    ///     Saves a section of an already analysed input as an animated gif.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="output">Output .gif file.</param>
    /// <param name="size">Output size. If width or height is 0 or -1, it is computed from the other. Defaults to 480 wide.</param>
    /// <param name="captureTime">Seek position the section starts at. Defaults to a third of the way in.</param>
    /// <param name="duration">How much of the input to capture.</param>
    /// <param name="streamIndex">Selected video stream index.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor GifSnapshot(IMediaAnalysis source, string output, Size? size = null, TimeSpan? captureTime = null,
        TimeSpan? duration = null, int? streamIndex = null, FFOptions? ffOptions = null)
    {
        CheckSnapshotOutputExtension(output, [FileExtension.Gif]);

        var input = InputPathOf(source, nameof(source));
        var (arguments, outputOptions) = SnapshotArgumentBuilder.BuildGifSnapshotArguments(input, source, size, captureTime, duration, streamIndex);

        return arguments.OutputToFile(output, true, outputOptions).WithOptions(ffOptions);
    }

    private static string InputPathOf(IMediaAnalysis source, string parameterName)
    {
        return source.Path ?? throw new ArgumentException(
            "This analysis came from a stream, so it names no input ffmpeg could open. Use the overload that takes an input path.", parameterName);
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
        return JoinImageSequence(ffOptions, output, frameRate, images.Select(image => FFProbe.Analyse(image, ffOptions)).ToArray());
    }

    /// <inheritdoc cref="JoinImageSequence(FFOptions,string,double,IMediaAnalysis[])" />
    public static FFMpegArgumentProcessor JoinImageSequence(string output, double frameRate = 30, params IMediaAnalysis[] images)
    {
        return JoinImageSequence(null, output, frameRate, images);
    }

    /// <summary>
    ///     Converts a sequence of already analysed images to a video.
    /// </summary>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="frameRate">FPS</param>
    /// <param name="images">Analyses of the images, in order. The first one's dimensions decide the output size.</param>
    public static FFMpegArgumentProcessor JoinImageSequence(FFOptions? ffOptions, string output, double frameRate = 30,
        params IMediaAnalysis[] images)
    {
        var paths = images.Select(image => InputPathOf(image, nameof(images))).ToArray();
        var arguments = FFMpegArguments.FromImageSequenceInput(paths, options => options
            .WithFrameRate(frameRate));

        var streams = images.Select(image => image.PrimaryVideoStream!).ToArray();
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

    /// <inheritdoc cref="PosterWithAudio(string,string,string,Codec,FFOptions)" />
    /// <param name="audioCodec">Name of the encoder for the audio, such as "aac".</param>
    public static FFMpegArgumentProcessor PosterWithAudio(string image, string audio, string output, string audioCodec,
        FFOptions? ffOptions = null)
    {
        return PosterWithAudio(image, audio, output, new Codec(audioCodec, CodecType.Audio), ffOptions);
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
        return PosterWithAudio(FFProbe.Analyse(image, ffOptions), audio, output, audioCodec, ffOptions);
    }

    /// <inheritdoc cref="PosterWithAudio(IMediaAnalysis,string,string,Codec,FFOptions)" />
    /// <param name="audioCodec">Name of the encoder for the audio, such as "aac".</param>
    public static FFMpegArgumentProcessor PosterWithAudio(IMediaAnalysis imageSource, string audio, string output, string audioCodec,
        FFOptions? ffOptions = null)
    {
        return PosterWithAudio(imageSource, audio, output, new Codec(audioCodec, CodecType.Audio), ffOptions);
    }

    /// <summary>
    ///     Adds an already analysed poster image to an audio file.
    /// </summary>
    /// <param name="imageSource">Analysis of the poster image, which supplies its path and its dimensions.</param>
    /// <param name="audio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="audioCodec">Encoder for the audio. Defaults to copying it, so the track is not degraded a second time.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor PosterWithAudio(IMediaAnalysis imageSource, string audio, string output, Codec? audioCodec = null,
        FFOptions? ffOptions = null)
    {
        FFMpegHelper.ExtensionExceptionCheck(output, FileExtension.Mp4);
        var image = InputPathOf(imageSource, nameof(imageSource));
        FFMpegHelper.ConversionSizeExceptionCheck(imageSource.PrimaryVideoStream!.Width, imageSource.PrimaryVideoStream!.Height);

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
    ///     Overlays an image onto a video. The audio is copied; the video is re-encoded, because the picture changes.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="watermark">Image to overlay. A PNG with an alpha channel keeps its transparency.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="position">Corner to place it in.</param>
    /// <param name="margin">Distance in pixels from the edges, ignored when centred.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Watermark(string input, string watermark, string output,
        WatermarkPosition position = WatermarkPosition.BottomRight, int margin = 10, FFOptions? ffOptions = null)
    {
        return Watermark(FFProbe.Analyse(input, ffOptions), watermark, output, position, margin, ffOptions);
    }

    /// <summary>
    ///     Overlays an image onto an already analysed video. The audio is copied; the video is re-encoded, because the picture
    ///     changes. Only the video is analysed — the watermark image is passed straight to ffmpeg.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="watermark">Image to overlay. A PNG with an alpha channel keeps its transparency.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="position">Corner to place it in.</param>
    /// <param name="margin">Distance in pixels from the edges, ignored when centred.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Watermark(IMediaAnalysis source, string watermark, string output,
        WatermarkPosition position = WatermarkPosition.BottomRight, int margin = 10, FFOptions? ffOptions = null)
    {
        var input = InputPathOf(source, nameof(source));
        var (x, y) = OverlayPosition(position, margin);

        return FFMpegArguments
            .FromFileInput(input)
            .AddFileInput(watermark)
            .OutputToFile(output, true, options =>
            {
                options
                    .WithComplexFilter(graph => graph
                        .From(0, StreamType.Video)
                        .From(1, StreamType.Video)
                        .Overlay(x, y)
                        .As("v"))
                    .WithMap("v");
                if (source.PrimaryAudioStream != null)
                {
                    options.WithMap(0, StreamType.Audio).CopyStreams(StreamType.Audio);
                }
            })
            .WithKnownDuration(source.Duration)
            .WithOptions(ffOptions);
    }

    private static (string X, string Y) OverlayPosition(WatermarkPosition position, int margin)
    {
        return position switch
        {
            WatermarkPosition.TopLeft => ($"{margin}", $"{margin}"),
            WatermarkPosition.TopRight => ($"W-w-{margin}", $"{margin}"),
            WatermarkPosition.BottomLeft => ($"{margin}", $"H-h-{margin}"),
            WatermarkPosition.BottomRight => ($"W-w-{margin}", $"H-h-{margin}"),
            WatermarkPosition.Center => ("(W-w)/2", "(H-h)/2"),
            _ => throw new ArgumentOutOfRangeException(nameof(position))
        };
    }

    /// <inheritdoc cref="AddSubtitles(string,string,string,string,Codec,FFOptions)" />
    /// <param name="subtitleCodec">Name of the encoder for the subtitles, such as "mov_text".</param>
    public static FFMpegArgumentProcessor AddSubtitles(string input, string subtitle, string output, string? language,
        string subtitleCodec, FFOptions? ffOptions = null)
    {
        return AddSubtitles(input, subtitle, output, language, new Codec(subtitleCodec, CodecType.Subtitle), ffOptions);
    }

    /// <summary>
    ///     Muxes a subtitle file in as its own stream, leaving the picture untouched. The player can then turn the subtitles
    ///     on and off; to burn them into the picture instead, use <c>WithVideoFilters(f => f.HardBurnSubtitle(…))</c>.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="subtitle">Subtitle file to add.</param>
    /// <param name="output">Output video file. Its container has to support subtitle streams; .mkv takes any, .mp4 needs mov_text.</param>
    /// <param name="language">ISO 639 language tag for the new stream, such as "eng".</param>
    /// <param name="subtitleCodec">Encoder for the subtitles. Defaults to the muxer's choice.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor AddSubtitles(string input, string subtitle, string output, string? language = null,
        Codec? subtitleCodec = null, FFOptions? ffOptions = null)
    {
        return AddSubtitles(FFProbe.Analyse(input, ffOptions), subtitle, output, language, subtitleCodec, ffOptions);
    }

    /// <inheritdoc cref="AddSubtitles(IMediaAnalysis,string,string,string,Codec,FFOptions)" />
    /// <param name="subtitleCodec">Name of the encoder for the subtitles, such as "mov_text".</param>
    public static FFMpegArgumentProcessor AddSubtitles(IMediaAnalysis source, string subtitle, string output, string? language,
        string subtitleCodec, FFOptions? ffOptions = null)
    {
        return AddSubtitles(source, subtitle, output, language, new Codec(subtitleCodec, CodecType.Subtitle), ffOptions);
    }

    /// <summary>
    ///     Muxes a subtitle file into an already analysed video as its own stream, leaving the picture untouched.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="subtitle">Subtitle file to add.</param>
    /// <param name="output">Output video file. Its container has to support subtitle streams; .mkv takes any, .mp4 needs mov_text.</param>
    /// <param name="language">ISO 639 language tag for the new stream, such as "eng".</param>
    /// <param name="subtitleCodec">Encoder for the subtitles. Defaults to the muxer's choice.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor AddSubtitles(IMediaAnalysis source, string subtitle, string output, string? language = null,
        Codec? subtitleCodec = null, FFOptions? ffOptions = null)
    {
        var input = InputPathOf(source, nameof(source));

        return FFMpegArguments
            .FromFileInput(input)
            .AddFileInput(subtitle)
            .OutputToFile(output, true, options =>
            {
                options
                    .WithMap(0)
                    .WithMap(1)
                    .CopyStreams();
                if (subtitleCodec != null)
                {
                    options.WithSubtitleCodec(subtitleCodec);
                }

                if (language != null)
                {
                    options.WithCustomArgument($"-metadata:s:s:{source.SubtitleStreams.Count} language={language}");
                }
            })
            .WithKnownDuration(source.Duration)
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Writes one of the input's subtitle streams out to its own file.
    /// </summary>
    /// <param name="input">Source media file.</param>
    /// <param name="output">Output subtitle file. Its extension decides the format.</param>
    /// <param name="streamIndex">Which subtitle stream to take, counting from zero among the subtitle streams.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ExtractSubtitles(string input, string output, int streamIndex = 0, FFOptions? ffOptions = null)
    {
        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .WithMap(0, StreamType.Subtitle, streamIndex))
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Samples frames at a fixed interval and tiles them into a single image — a contact sheet, or the strip a player
    ///     shows when scrubbing.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="output">Output image file.</param>
    /// <param name="columns">Tiles across.</param>
    /// <param name="rows">Tiles down.</param>
    /// <param name="interval">
    ///     How much video each tile advances by. Defaults to spreading <paramref name="columns" /> × <paramref name="rows" />
    ///     tiles evenly across the whole input.
    /// </param>
    /// <param name="tileSize">Size of one tile. If width or height is -1, it is computed from the other.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ThumbnailSheet(string input, string output, int columns = 5, int rows = 5, TimeSpan? interval = null,
        Size? tileSize = null, FFOptions? ffOptions = null)
    {
        return ThumbnailSheet(FFProbe.Analyse(input, ffOptions), output, columns, rows, interval, tileSize, ffOptions);
    }

    /// <summary>
    ///     Samples frames of an already analysed input at a fixed interval and tiles them into a single image.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="output">Output image file.</param>
    /// <param name="columns">Tiles across.</param>
    /// <param name="rows">Tiles down.</param>
    /// <param name="interval">How much video each tile advances by. Defaults to spreading the tiles evenly across the whole input.</param>
    /// <param name="tileSize">Size of one tile. If width or height is -1, it is computed from the other.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ThumbnailSheet(IMediaAnalysis source, string output, int columns = 5, int rows = 5,
        TimeSpan? interval = null, Size? tileSize = null, FFOptions? ffOptions = null)
    {
        CheckSnapshotOutputExtension(output, FileExtension.Image.All);

        var input = InputPathOf(source, nameof(source));
        var step = interval ?? TimeSpan.FromTicks(Math.Max(source.Duration.Ticks / (columns * rows), TimeSpan.TicksPerMillisecond));
        var size = tileSize ?? new Size(-2, 120);

        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .WithVideoFilters(filters => filters
                    .Fps(1 / step.TotalSeconds)
                    .Scale(size)
                    .Tile(columns, rows))
                .WithFrameCount(1))
            .WithOptions(ffOptions);
    }

    /// <summary>
    ///     Rewraps a file into a different container, copying the streams rather than re-encoding. The target container has to
    ///     be able to mux the streams as they are; ffmpeg fails the run if it cannot.
    /// </summary>
    /// <param name="input">Input media file.</param>
    /// <param name="output">Output media file. Its extension decides the container.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Remux(string input, string output, FFOptions? ffOptions = null)
    {
        return Remux(FFProbe.Analyse(input, ffOptions), output, ffOptions);
    }

    /// <summary>
    ///     Rewraps an already analysed file into a different container, copying the streams rather than re-encoding.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="output">Output media file. Its extension decides the container.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Remux(IMediaAnalysis source, string output, FFOptions? ffOptions = null)
    {
        var input = InputPathOf(source, nameof(source));

        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .WithMap(0)
                .CopyStreams())
            .WithKnownDuration(source.Duration)
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
        return Concat(ffOptions, output, inputs.Select(input => FFProbe.Analyse(input, ffOptions)).ToArray());
    }

    /// <inheritdoc cref="Concat(FFOptions,string,IMediaAnalysis[])" />
    public static FFMpegArgumentProcessor Concat(string output, params IMediaAnalysis[] sources)
    {
        return Concat(null, output, sources);
    }

    /// <summary>
    ///     Joins already analysed media files through the concat demuxer, copying the streams rather than re-encoding.
    /// </summary>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    /// <param name="output">Output file.</param>
    /// <param name="sources">Analyses of the files to join, in order.</param>
    public static FFMpegArgumentProcessor Concat(FFOptions? ffOptions, string output, params IMediaAnalysis[] sources)
    {
        var inputs = sources.Select(source => InputPathOf(source, nameof(sources))).ToArray();
        var duration = sources.Aggregate(TimeSpan.Zero, (total, source) => total + source.Duration);

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
        return Join(output, videos.Select(video => FFProbe.Analyse(video, ffOptions)).ToArray(), addArguments, ffOptions);
    }

    /// <inheritdoc cref="Join(string,IEnumerable{IMediaAnalysis},Action{FFMpegOutputOptions},FFOptions)" />
    public static FFMpegArgumentProcessor Join(string output, params IMediaAnalysis[] sources)
    {
        return Join(output, sources, null);
    }

    /// <inheritdoc cref="Join(string,IEnumerable{IMediaAnalysis},Action{FFMpegOutputOptions},FFOptions)" />
    public static FFMpegArgumentProcessor Join(FFOptions? ffOptions, string output, params IMediaAnalysis[] sources)
    {
        return Join(output, sources, null, ffOptions);
    }

    /// <summary>
    ///     Joins already analysed videos by re-encoding them through the concat filter, which requires them to share a
    ///     resolution.
    /// </summary>
    /// <param name="output">Output video file.</param>
    /// <param name="sources">Analyses of the videos to join, in order.</param>
    /// <param name="addArguments">Output options, replacing the default h264/aac encode.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor Join(string output, IEnumerable<IMediaAnalysis> sources, Action<FFMpegOutputOptions>? addArguments = null,
        FFOptions? ffOptions = null)
    {
        var analyses = sources.ToArray();
        var paths = analyses.Select(analysis => InputPathOf(analysis, nameof(sources))).ToArray();
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
        return RemoveAudio(FFProbe.Analyse(input, ffOptions), output, ffOptions);
    }

    /// <summary>
    ///     Strips an already analysed video file of its audio, copying every other stream.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor RemoveAudio(IMediaAnalysis source, string output, FFOptions? ffOptions = null)
    {
        var input = InputPathOf(source, nameof(source));
        FFMpegHelper.ConversionSizeExceptionCheck(source);

        return FFMpegArguments
            .FromFileInput(input)
            .OutputToFile(output, true, options => options
                .WithMap(0)
                .CopyStreams()
                .DisableAudio())
            .WithKnownDuration(source.Duration)
            .WithOptions(ffOptions);
    }

    /// <inheritdoc cref="ExtractAudio(string,string,Codec,FFOptions)" />
    /// <param name="audioCodec">Name of the encoder for the audio, such as "copy" or "libopus".</param>
    public static FFMpegArgumentProcessor ExtractAudio(string input, string output, string audioCodec, FFOptions? ffOptions = null)
    {
        return ExtractAudio(input, output, new Codec(audioCodec, CodecType.Audio), ffOptions);
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

    /// <inheritdoc cref="ReplaceAudio(string,string,string,Codec,bool,FFOptions)" />
    /// <param name="audioCodec">Name of the encoder for the audio, such as "aac".</param>
    public static FFMpegArgumentProcessor ReplaceAudio(string input, string inputAudio, string output, string audioCodec,
        bool stopAtShortest = false, FFOptions? ffOptions = null)
    {
        return ReplaceAudio(input, inputAudio, output, new Codec(audioCodec, CodecType.Audio), stopAtShortest, ffOptions);
    }

    /// <summary>
    ///     Replaces the audio of a video file with the audio of another file, or adds it if the video has none.
    /// </summary>
    /// <param name="input">Source video file.</param>
    /// <param name="inputAudio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="audioCodec">Encoder for the audio. Defaults to copying it, so the track is not degraded a second time.</param>
    /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ReplaceAudio(string input, string inputAudio, string output, Codec? audioCodec = null,
        bool stopAtShortest = false, FFOptions? ffOptions = null)
    {
        return ReplaceAudio(FFProbe.Analyse(input, ffOptions), inputAudio, output, audioCodec, stopAtShortest, ffOptions);
    }

    /// <inheritdoc cref="ReplaceAudio(IMediaAnalysis,string,string,Codec,bool,FFOptions)" />
    /// <param name="audioCodec">Name of the encoder for the audio, such as "aac".</param>
    public static FFMpegArgumentProcessor ReplaceAudio(IMediaAnalysis source, string inputAudio, string output, string audioCodec,
        bool stopAtShortest = false, FFOptions? ffOptions = null)
    {
        return ReplaceAudio(source, inputAudio, output, new Codec(audioCodec, CodecType.Audio), stopAtShortest, ffOptions);
    }

    /// <summary>
    ///     Replaces the audio of an already analysed video file with the audio of another file, or adds it if the video has
    ///     none. Only the video is analysed — the audio file is passed straight to ffmpeg.
    /// </summary>
    /// <param name="source">Analysis of the input, which supplies the input path as well as what the helper needs to know about it.</param>
    /// <param name="inputAudio">Source audio file.</param>
    /// <param name="output">Output video file.</param>
    /// <param name="audioCodec">Encoder for the audio. Defaults to copying it, so the track is not degraded a second time.</param>
    /// <param name="stopAtShortest">Indicates if the encoding should stop at the shortest input file.</param>
    /// <param name="ffOptions">Options for this run, defaulting to the global options.</param>
    public static FFMpegArgumentProcessor ReplaceAudio(IMediaAnalysis source, string inputAudio, string output, Codec? audioCodec = null,
        bool stopAtShortest = false, FFOptions? ffOptions = null)
    {
        var input = InputPathOf(source, nameof(source));
        FFMpegHelper.ConversionSizeExceptionCheck(source);

        return FFMpegArguments
            .FromFileInput(input)
            .AddFileInput(inputAudio)
            .OutputToFile(output, true, options => options
                .WithMap(0, StreamType.Video)
                .WithMap(1, StreamType.Audio)
                .CopyStreams()
                .WithAudioCodec(audioCodec ?? AudioCodec.Copy)
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
