using System.Drawing;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.SkiaSharp;
using FFMpegCore.Extensions.System.Drawing.Common;
using FFMpegCore.Pipes;
using SkiaSharp;

var inputPath = "/path/to/input";
var outputPath = "/path/to/output";

{
    var mediaInfo = FFProbe.Analyse(inputPath);
}

{
    var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
}

{
    FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToFile(outputPath, options => options
            .WithVideoCodec(VideoCodec.LibX264)
            .WithConstantRateFactor(21)
            .WithAudioCodec(AudioCodec.Aac)
            .WithVideoFilters(filterOptions => filterOptions
                .Scale(VideoSize.Hd))
            .WithFastStart())
        .ProcessSynchronously();
}

{
    var source = await FFProbe.AnalyseAsync(inputPath);
    using var conversion = new CancellationTokenSource();

    // throwOnError: false reports the failure through the result instead of throwing
    var result = await FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToFile(outputPath, options => options
            .WithVideoCodec(VideoCodec.LibX264)
            .WithSpeedPreset(EncoderPreset.Fast))
        .NotifyOnProgress(time => Console.WriteLine($"at {time}"))
        .NotifyOnPercentageProgress(percent => Console.WriteLine($"{percent:0.#}%"), source.Duration)
        .ProcessAsynchronously(false, cancellationToken: conversion.Token);

    if (!result.Success)
    {
        Console.Error.WriteLine($"ffmpeg exited with {result.ExitCode}");
        Console.Error.WriteLine(string.Join(Environment.NewLine, result.StandardError));
    }
}

{
    // seeking on the input skips ahead before decoding, which is what makes copying a section near-instant
    FFMpegArguments
        .FromFileInput(inputPath, options => options
            .WithStartTime(TimeSpan.FromMinutes(1))
            .WithDuration(TimeSpan.FromSeconds(30)))
        .OutputToFile(outputPath, options => options
            .CopyStreams())
        .ProcessSynchronously();
}

{
    // process the snapshot in-memory and use the Bitmap directly
    var bitmap = SystemDrawingImage.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromMinutes(1));

    // or persists the image on the drive
    FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1)).ProcessSynchronously();
}

{
    // -1 on either axis lets ffmpeg keep the aspect ratio
    FFMpeg.GifSnapshot(inputPath, @"..\preview.gif", new Size(480, -1), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(3))
        .ProcessSynchronously();
}

var inputStream = new MemoryStream();
var outputStream = new MemoryStream();

{
    await FFMpegArguments
        .FromPipeInput(new StreamPipeSource(inputStream))
        .OutputToPipe(new StreamPipeSink(outputStream), options => options
            .WithVideoCodec("vp9")
            .ForceFormat(ContainerFormats.WebM))
        .ProcessAsynchronously();
}

{
    // parts that already share a codec: joined by copying, no re-encode
    FFMpeg.Concat(@"..\joined_video.mp4",
        @"..\part1.mp4",
        @"..\part2.mp4",
        @"..\part3.mp4"
    ).ProcessSynchronously();

    // parts that differ: re-encoded through the concat filter
    FFMpeg.Join(@"..\joined_video.mp4",
        @"..\part1.mp4",
        @"..\part2.mkv"
    ).ProcessSynchronously();
}

{
    FFMpeg.Remux(inputPath, @"..\output.mkv").ProcessSynchronously();
}

{
    FFMpeg.Trim(inputPath, @"..\clip.mkv", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(90)).ProcessSynchronously();
}

{
    FFMpeg.ThumbnailSheet(inputPath, @"..\sheet.png", 5, 5, tileSize: new Size(160, -1)).ProcessSynchronously();
}

{
    FFMpeg.Watermark(inputPath, @"..\logo.png", outputPath, WatermarkPosition.BottomRight, 20).ProcessSynchronously();
}

{
    FFMpeg.AddSubtitles(inputPath, @"..\subs.srt", @"..\subtitled.mkv", "eng").ProcessSynchronously();
    FFMpeg.ExtractSubtitles(@"..\subtitled.mkv", @"..\subs.srt").ProcessSynchronously();
}

{
    // burnt into the picture instead, so they cannot be switched off
    FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToFile(outputPath, options => options
            .WithVideoFilters(filterOptions => filterOptions
                .HardBurnSubtitle(@"..\subs.srt", subtitles => subtitles
                    .WithStyle(style => style
                        .WithParameter("FontName", "DejaVu Serif")
                        .WithParameter("FontSize", "24")))))
        .ProcessSynchronously();
}

{
    var metadata = new FFMetadataBuilder()
        .WithTitle("Interview")
        .WithArtists("Some Artist")
        .WithChapter("Introduction", TimeSpan.FromMinutes(2))
        .WithChapter("Main topic", TimeSpan.FromMinutes(25));

    FFMpegArguments
        .FromFileInput(inputPath)
        .AddMetadata(metadata)
        .OutputToFile(outputPath, options => options.CopyStreams())
        .ProcessSynchronously();
}

{
    FFMpeg.JoinImageSequence(@"..\joined_video.mp4", 1, @"..\1.png", @"..\2.png", @"..\3.png").ProcessSynchronously();
}

{
    FFMpeg.RemoveAudio(inputPath, outputPath).ProcessSynchronously();
}

{
    FFMpeg.ExtractAudio(inputPath, outputPath).ProcessSynchronously();
}

{
    // cancelling sends q, so ffmpeg finalises the recording rather than leaving it truncated
    using var recording = new CancellationTokenSource(TimeSpan.FromMinutes(30));
    await FFMpeg.SaveStream(new Uri("https://example.com/live/stream.m3u8"), @"..\recording.ts")
        .ProcessAsynchronously(false, cancellationToken: recording.Token);
}

var inputAudioPath = "/path/to/input/audio";
{
    FFMpeg.ReplaceAudio(inputPath, inputAudioPath, outputPath).ProcessSynchronously();
}

{
    // leaving the stream index out maps every stream of that kind, however many the input turns out to have
    FFMpegArguments
        .FromFileInput(inputPath)
        .AddFileInput(inputAudioPath)
        .OutputToFile(outputPath, options => options
            .WithMap(0, StreamType.Video)
            .WithMap(1, StreamType.Audio)
            .WithNegativeMap(0, StreamType.Subtitle)
            .CopyStreams())
        .ProcessSynchronously();
}

{
    // repeated outputs: ffmpeg encodes once per output
    FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToMany(outputs => outputs
            .OutputToFile(@"..\sd.mp4", options => options.WithVideoFilters(f => f.Scale(VideoSize.Ed)))
            .OutputToFile(@"..\hd.mp4", options => options.WithVideoFilters(f => f.Scale(VideoSize.Hd))))
        .ProcessSynchronously();

    // the tee muxer: one encode fanned out, so every target gets the same streams
    FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToTee(outputs => outputs
                .OutputToFile(@"..\recording.mp4")
                .OutputToUrl("rtmp://example.com/live/key", options => options.ForceFormat(ContainerFormats.Flv)),
            options => options.CopyStreams())
        .ProcessSynchronously();
}

var inputImagePath = "/path/to/input/image";
{
    FFMpeg.PosterWithAudio(inputImagePath, inputAudioPath, outputPath).ProcessSynchronously();
    // or using FFMpegCore.Extensions.System.Drawing.Common
#pragma warning disable CA1416
    using var image = Image.FromFile(inputImagePath);
    image.AddAudio(inputAudioPath, outputPath).ProcessSynchronously();
#pragma warning restore CA1416
    // or using FFMpegCore.Extensions.SkiaSharp
    using var skiaSharpImage = SKBitmap.Decode(inputImagePath);
    skiaSharpImage.AddAudio(inputAudioPath, outputPath).ProcessSynchronously();
}

IVideoFrame GetNextFrame()
{
    throw new NotImplementedException();
}

{
    IEnumerable<IVideoFrame> CreateFrames(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return GetNextFrame(); //method of generating new frames
        }
    }

    var videoFramesSource =
        new RawVideoPipeSource(CreateFrames(64)) //pass IEnumerable<IVideoFrame> or IEnumerator<IVideoFrame> to constructor of RawVideoPipeSource
        {
            FrameRate = 30 //set source frame rate
        };
    await FFMpegArguments
        .FromPipeInput(videoFramesSource)
        .OutputToFile(outputPath, options => options
            .WithVideoCodec(VideoCodec.LibVpx))
        .ProcessAsynchronously();
}

{
    // asking the binary what it can do, rather than assuming
    if (FFMpeg.TryGetCodec("libsvtav1", out var av1) && av1.EncodingSupported)
    {
        Console.WriteLine($"{av1.Name}: {av1.Description}");
    }

    foreach (var codec in FFMpeg.GetVideoCodecs().Where(codec => codec.IsLossless))
    {
        Console.WriteLine(codec.Name);
    }

    var containers = FFMpeg.GetContainerFormats();
    var pixelFormats = FFMpeg.GetPixelFormats();
}

{
    // setting global options
    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "/tmp" });
    // or
    GlobalFFOptions.Configure(options => options.BinaryFolder = "./bin");

    // or individual, per-run options
    await FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToFile(outputPath)
        .ProcessAsynchronously(true, new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "/tmp" });

    // or combined, setting global defaults and adapting per-run options
    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "./globalTmp", WorkingDirectory = "./" });

    await FFMpegArguments
        .FromFileInput(inputPath)
        .OutputToFile(outputPath)
        .Configure(options => options.WorkingDirectory = "./CurrentRunWorkingDir")
        .Configure(options => options.TemporaryFilesFolder = "./CurrentRunTmpFolder")
        .ProcessAsynchronously();
}

{
    // fetching the binaries into BinaryFolder at runtime, using FFMpegCore.Extensions.Downloader
    GlobalFFOptions.Configure(options => options.BinaryFolder = "./bin");
    var downloaded = await FFMpegDownloader.DownloadBinariesAsync();
}
