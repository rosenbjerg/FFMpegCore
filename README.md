# [FFMpegCore](https://www.nuget.org/packages/FFMpegCore/)

[![NuGet Version](https://img.shields.io/nuget/v/FFMpegCore)](https://www.nuget.org/packages/FFMpegCore/)
[![GitHub issues](https://img.shields.io/github/issues/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/issues)
[![GitHub stars](https://img.shields.io/github/stars/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/stargazers)
[![GitHub](https://img.shields.io/github/license/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/blob/main/LICENSE)
[![codecov](https://codecov.io/gh/rosenbjerg/FFMpegCore/branch/main/graph/badge.svg)](https://codecov.io/gh/rosenbjerg/FFMpegCore)
[![CI](https://github.com/rosenbjerg/FFMpegCore/workflows/CI/badge.svg)](https://github.com/rosenbjerg/FFMpegCore/actions/workflows/ci.yml)
[![GitHub code contributors](https://img.shields.io/github/contributors/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/graphs/contributors)

A .NET Standard FFMpeg/FFProbe wrapper for easily integrating media analysis and conversion into your .NET applications. Supports both
synchronous and asynchronous calls

> **Upgrading from 5.x?** Version 6.0 renames most option methods after the ffmpeg options they emit and splits input from output options.
> [MIGRATION.md](MIGRATION.md) lists every breaking change and its replacement.

# API

## FFProbe

Use FFProbe to analyze media files:

```csharp
var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
```

or

```csharp
var mediaInfo = FFProbe.Analyse(inputPath);
```

A missing input throws `FFProbeException`, and a non-zero exit throws `FFProbeProcessException`, which carries the captured stderr lines in
`ErrorOutput`. Both derive from `FFMpegException`, so a single `catch (FFMpegException)` covers ffprobe and ffmpeg alike.

## FFMpeg

Use FFMpeg to convert your media files.
Easily build your FFMpeg arguments using the fluent argument builder:

Convert input file to h264/aac scaled to 720p w/ faststart, for web playback

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath, false, options => options
        .WithVideoCodec(VideoCodec.LibX264)
        .WithConstantRateFactor(21)
        .WithAudioCodec(AudioCodec.Aac)
        .WithVariableBitrate(4)
        .WithVideoFilters(filterOptions => filterOptions
            .Scale(VideoSize.Hd))
        .WithFastStart())
    .ProcessSynchronously();
```

Convert to and/or from streams

```csharp
await FFMpegArguments
    .FromPipeInput(new StreamPipeSource(inputStream))
    .OutputToPipe(new StreamPipeSink(outputStream), options => options
        .WithVideoCodec("vp9")
        .ForceFormat("webm"))
    .ProcessAsynchronously();
```

### Input options and output options

Options given to an input land before that input's `-i`, and options given to an output land before the output path — which is what ffmpeg
requires, and what decides their meaning. The two sides therefore have their own types, `FFMpegInputOptions` and `FFMpegOutputOptions`, each
offering only what ffmpeg accepts there: `WithVideoDecoder` selects a decoder on an input, while `WithVideoCodec` selects an encoder on an
output, and `-y` or `-map` are output-only. Options that are valid on both sides, such as `-ss` and `-t`, appear on both.

Seeking on the input is fast, because ffmpeg skips ahead before decoding:

```csharp
FFMpegArguments
    .FromFileInput(inputPath, true, options => options
        .WithStartTime(TimeSpan.FromSeconds(10))
        .WithDuration(TimeSpan.FromSeconds(30)))
    .OutputToFile(outputPath, true, options => options
        .CopyStreams())
    .ProcessSynchronously();
```

Each option method's summary names the ffmpeg option it emits, so searching your IDE for `-ss` finds `WithStartTime`.

### Selecting streams

`WithMap` takes either a stream index or a `StreamType`. Passing the type maps every stream of that kind, which is what you want when the
input's stream count is not known up front:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .AddFileInput(audioPath)
    .OutputToFile(outputPath, true, options => options
        .WithMap(StreamType.Video)              // -map 0:v  — every video stream of the first input
        .WithMap(StreamType.Audio, 1)           // -map 1:a  — every audio stream of the second
        .WithNegativeMap(StreamType.Subtitle)   // -map -0:s — but none of its subtitles
        .CopyStreams())
    .ProcessSynchronously();
```

`WithMap(0)` still selects one stream by index, and `WithMap(StreamType.All)` maps everything from an input.

### Complex filters

`WithVideoFilters` builds the single `-vf` chain that one input feeds. When a filter needs more than one input, or you want to route what a
filter produces into another, use `WithComplexFilter`, which builds `-filter_complex`:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .AddFileInput(logoPath)
    .OutputToFile(outputPath, true, options => options
        .WithComplexFilter(graph => graph
            .From(0, StreamType.Video)
            .From(1, StreamType.Video)
            .Overlay(x: "W-w-10", y: "H-h-10")
            .As("v"))
        .WithMap("v"))
    .ProcessSynchronously();
```

A chain reads its inputs with `From` — either a stream of an input file, or a label an earlier chain produced — applies filters in order, and
closes with `As`, naming what it produced. `As` returns the graph, so the next `From` starts another chain; chains are joined with `;`.
`WithMap(label)` then selects a labelled output:

```csharp
.WithComplexFilter(graph => graph
    .From(0, StreamType.Video).Scale(640, 360).As("small")
    .From("small").HorizontalFlip().As("flipped"))
.WithMap("flipped")
```

Filters ffmpeg only accepts in a complex graph live here rather than on `WithVideoFilters`: `Concat`, `Overlay` and `AudioMix`. For anything
the builder does not cover, `WithCustomFilter(key, value)` and the `WithFilter(IVideoFilterArgument)` overloads take an arbitrary filter.

### Reading the result

`ProcessSynchronously()` and `ProcessAsynchronously()` return an `FFMpegResult` describing the run:

```csharp
var result = FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath)
    .ProcessSynchronously(throwOnError: false);

if (!result.Success)
{
    Console.Error.WriteLine($"ffmpeg exited with {result.ExitCode}");
    Console.Error.WriteLine(string.Join("\n", result.ErrorOutput));
}
```

By default (`throwOnError: true`) a non-zero exit throws `FFMpegException` and a cancellation throws `OperationCanceledException`. Pass
`false` and the result reports what happened instead, through `ExitCode`, `ErrorOutput`, `Cancelled` and `Success`.

### Progress and cancellation

`NotifyOnProgress` reports the timestamp ffmpeg has reached. `NotifyOnPercentageProgress` reports a percentage, which needs the output
duration — pass it explicitly, or omit it after an `FFMpeg.*` helper that already probed the input. Both take an `Action<T>` or an
`IProgress<T>`:

```csharp
await FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath)
    .NotifyOnProgress(time => Console.WriteLine($"at {time}"))
    .NotifyOnPercentageProgress(percent => Console.WriteLine($"{percent}%"), mediaInfo.Duration)
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously();
```

`CancellableThrough` sends `q` to ffmpeg so it finalises the output, then kills the process after the optional timeout. It also accepts an
`out Action` if you would rather cancel by calling it. Tokens are registered per run, so the same processor can be run more than once.

### Multiple outputs

`OutputToMany` writes several outputs from one input using ffmpeg's own repeated outputs, which encodes once per output:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToMany(outputs => outputs
        .OutputToFile("sd.mp4", true, options => options.WithVideoFilters(f => f.Scale(1280, 720)))
        .OutputToFile("hd.mp4", true, options => options.WithVideoFilters(f => f.Scale(1920, 1080))))
    .ProcessSynchronously();
```

`OutputToTee` instead encodes once and fans the result out through ffmpeg's `tee` muxer, which is cheaper but requires every target to accept
the same encoded streams:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToTee(outputs => outputs
        .OutputToFile("recording.mp4")
        .OutputToUrl("rtmp://example.com/live/key", options => options.ForceFormat("flv")),
        options => options.CopyStreams())
    .ProcessSynchronously();
```

### Metadata

`FFMetadataBuilder` builds the ffmetadata document that `AddMetadata` passes to ffmpeg:

```csharp
var metadata = new FFMetadataBuilder()
    .WithTitle("Interview")
    .WithArtists("Some Artist")
    .WithChapter("Introduction", TimeSpan.FromMinutes(2))
    .WithChapter("Main topic", TimeSpan.FromMinutes(25));

FFMpegArguments
    .FromFileInput(inputPath)
    .AddMetadata(metadata)
    .OutputToFile(outputPath, true, options => options.CopyStreams())
    .ProcessSynchronously();
```

A chapter given a single `TimeSpan` is a duration, and starts where the previous one ended; the overload taking two starts and ends it
explicitly. Chapters are `ChapterData`, the same type `IMediaAnalysis.Chapters` returns, so chapters read from one file can be fed straight
into another. Call `Build()` if you want the document text itself.

## Helper methods

The provided helper methods make it simple to perform common operations. Each one builds the ffmpeg arguments and returns an
`FFMpegArgumentProcessor`, so you choose how to run it — `ProcessSynchronously()` or `await ProcessAsynchronously()` — and can
attach progress callbacks or cancellation exactly as with `FFMpegArguments`.

### Easily capture snapshots from a video file:

```csharp
// persist the image on the drive
FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1))
    .ProcessSynchronously();

// or asynchronously, with cancellation
await FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1))
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously();

// or process the snapshot in-memory using one of the image extension packages
var bitmap = SystemDrawingImage.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromMinutes(1)); // FFMpegCore.Extensions.System.Drawing.Common
var skBitmap = SkiaSharpImage.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromMinutes(1));   // FFMpegCore.Extensions.SkiaSharp
```

### You can also capture GIF snapshots from a video file:

```csharp
FFMpeg.GifSnapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromSeconds(10))
    .ProcessSynchronously();

// you can also supply -1 to either one of Width/Height Size properties if you'd like FFMPEG to resize while maintaining the aspect ratio
await FFMpeg.GifSnapshot(inputPath, outputPath, new Size(480, -1), TimeSpan.FromSeconds(10))
    .ProcessAsynchronously();
```

### Join video parts into one single file:

When the parts already share a codec — segments of one recording, say — `Concat` joins them through ffmpeg's concat demuxer without
re-encoding, which is both lossless and far faster:

```csharp
FFMpeg.Concat(@"..\joined_video.mp4",
    @"..\part1.mp4",
    @"..\part2.mp4",
    @"..\part3.mp4"
).ProcessSynchronously();
```

`Join` re-encodes through the `concat` filter instead, which is what you need when the parts differ. They must still share a resolution:

```csharp
FFMpeg.Join(@"..\joined_video.mp4",
    @"..\part1.mp4",
    @"..\part2.mkv"
).ProcessSynchronously();

// the default is h264/aac at 2400 kbps; pass output options to choose your own
FFMpeg.Join(@"..\joined_video.mp4", parts, options => options
    .WithVideoCodec(VideoCodec.LibX265)
    .WithConstantRateFactor(23)
).ProcessSynchronously();
```

### Change container without re-encoding:

```csharp
FFMpeg.Remux(@"..\input.mp4", @"..\output.mkv").ProcessSynchronously();
```

Every stream is copied across, so this is lossless and near-instant. The target container has to be able to mux the streams as they are —
h264/aac into `.mkv` or `.mov` is fine, into `.webm` is not, and ffmpeg fails the run rather than silently re-encoding.

### Cut a section out of a video

```csharp
FFMpeg.Trim(inputPath,
    outputPath,
    TimeSpan.FromSeconds(0),
    TimeSpan.FromSeconds(30)
).ProcessSynchronously();
```

`Trim` copies the streams rather than re-encoding, so the output container has to be able to mux them as they are — cutting an h264/aac mp4
into a `.mkv` is fine, into a `.webm` is not, and ffmpeg says so.

### Join images into a video:

```csharp
FFMpeg.JoinImageSequence(@"..\joined_video.mp4", frameRate: 1,
    @"..\1.png",
    @"..\2.png",
    @"..\3.png"
).ProcessSynchronously();
```

### Remove the audio track of a video file:

```csharp
FFMpeg.RemoveAudio(inputPath, outputPath).ProcessSynchronously();
```

### Extract the audio track from a video file:

```csharp
// the output extension picks the container, and ffmpeg picks the encoder for it
FFMpeg.ExtractAudio(inputPath, "track.mp3").ProcessSynchronously();
FFMpeg.ExtractAudio(inputPath, "track.flac").ProcessSynchronously();

// or lift the stream out untouched, with no re-encode
FFMpeg.ExtractAudio(inputPath, "track.m4a", AudioCodec.Copy).ProcessSynchronously();
```

### Record a remote stream to a file:

```csharp
await FFMpeg.SaveStream(new Uri("https://example.com/live/stream.m3u8"), "recording.ts")
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously();
```

Any protocol ffmpeg can open works — http(s), rtmp, rtsp, srt. The streams are copied, not re-encoded. Prefer `.ts` or `.mkv` over `.mp4` for
anything long-running: an mp4 is only finalised when the run ends, so a crash loses the recording, while cancelling through
`CancellableThrough` finalises it properly.

### Add or replace the audio track of a video file:

```csharp
FFMpeg.ReplaceAudio(inputPath, inputAudioPath, outputPath).ProcessSynchronously();
```

### Combine an image with audio file, for youtube or similar platforms

```csharp
FFMpeg.PosterWithAudio(inputImagePath, inputAudioPath, outputPath).ProcessSynchronously();

// or using one of the image extension packages
var image = Image.FromFile(inputImagePath);
image.AddAudio(inputAudioPath, outputPath);
await image.AddAudioAsync(inputAudioPath, outputPath, cancellationToken: cancellationToken);
```

The audio is copied, not re-encoded, so the track is not degraded a second time on its way to a platform that will transcode it anyway. Pass a
codec if you do need to re-encode:

```csharp
FFMpeg.PosterWithAudio(inputImagePath, inputAudioPath, outputPath, AudioCodec.Aac).ProcessSynchronously();
```

Other available arguments could be found in `FFMpegCore.Arguments` namespace.

## Input piping

With input piping it is possible to write video frames directly from program memory without saving them to jpeg or png and then passing path
to input of ffmpeg. This feature also allows for converting video on-the-fly while frames are being generated or received.

An object implementing the `IPipeSource` interface is used as the source of data. Currently, the `IPipeSource` interface has three
implementations; `StreamPipeSource` for streams, `RawVideoPipeSource` for raw video frames, and `RawAudioPipeSource` for raw audio samples.

### Working with raw video frames

Method for generating bitmap frames:

```csharp
IEnumerable<IVideoFrame> CreateFrames(int count)
{
    for(int i = 0; i < count; i++)
    {
        yield return GetNextFrame(); //method that generates of receives the next frame
    }
}
```

Then create a `RawVideoPipeSource` that utilises your video frame source

```csharp
var videoFramesSource = new RawVideoPipeSource(CreateFrames(64))
{
    FrameRate = 30 //set source frame rate
};
await FFMpegArguments
    .FromPipeInput(videoFramesSource)
    .OutputToFile(outputPath, false, options => options
        .WithVideoCodec(VideoCodec.LibVpx))
    .ProcessAsynchronously();
```

The image extension packages provide `SystemDrawingVideoFrame` and `SkiaSharpVideoFrame`, which adapt a `System.Drawing.Bitmap` or an
`SKBitmap` to `IVideoFrame`.

# Binaries

## Runtime Auto Installation

The `FFMpegCore.Extensions.Downloader` package can install ffmpeg and ffprobe at runtime into the configured `BinaryFolder`:

```csharp
GlobalFFOptions.Configure(options => options.BinaryFolder = "./bin");
await FFMpegDownloader.DownloadBinariesAsync();
```

This feature uses the api from [ffbinaries](https://ffbinaries.com/api).

## Manual Installation

If you prefer to manually download them, visit [ffbinaries](https://ffbinaries.com/downloads)
or the [official ffmpeg download page](https://ffmpeg.org/download.html).

### Windows (using choco)

command: `choco install ffmpeg -y`

location: `C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin`

### Mac OSX

command: `brew install ffmpeg`

location: `/opt/homebrew/bin` (Apple Silicon) or `/usr/local/bin` (Intel)

### Ubuntu

command: `sudo apt-get install -y ffmpeg`

location: `/usr/bin`

## Path Configuration

### Option 1

The default value of an empty string (expecting ffmpeg to be found through PATH) can be overwritten via the `FFOptions` class:

```csharp
// setting global options
GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "/tmp" });

// or
GlobalFFOptions.Configure(options => options.BinaryFolder = "./bin");

// on some systems the absolute path may be required, in which case 
GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Server.MapPath("./bin"), TemporaryFilesFolder = Server.MapPath("/tmp") });

// or individual, per-run options
await FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath)
    .ProcessAsynchronously(true, new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "/tmp" });

// the FFMpeg.* helpers take the same options, covering the ffprobe call they make before the run
await FFMpeg.RemoveAudio(inputPath, outputPath, new FFOptions { BinaryFolder = "./bin" })
    .ProcessAsynchronously();

// or combined, setting global defaults and adapting per-run options
GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "./globalTmp", WorkingDirectory = "./" });

await FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath)
    .Configure(options => options.WorkingDirectory = "./CurrentRunWorkingDir")
    .Configure(options => options.TemporaryFilesFolder = "./CurrentRunTmpFolder")
    .ProcessAsynchronously();
```

### Option 2

The root and temp directory for the ffmpeg binaries can be configured via the `ffmpeg.config.json` file, which will be read on first use
only.

```json
{
  "BinaryFolder": "./bin",
  "TemporaryFilesFolder": "/tmp"
}
```

### Supporting both 32 and 64 bit processes

If you wish to support multiple client processor architectures, you can do so by creating two folders, `x64` and `x86`, in the
`BinaryFolder` directory.
Both folders should contain the binaries (`ffmpeg.exe` and `ffprobe.exe`) built for the respective architectures.

By doing so, the library will attempt to use either `/{BinaryFolder}/{ARCH}/(ffmpeg|ffprobe).exe`.

If these folders are not defined, it will try to find the binaries in `/{BinaryFolder}/(ffmpeg|ffprobe.exe)`.

(`.exe` is only appended on Windows)

# Compatibility

Older versions of ffmpeg might not support all ffmpeg arguments available through this library. CI runs the test suite against
ffmpeg `8.1`.

## Code contributors

<a href="https://github.com/rosenbjerg/ffmpegcore/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=rosenbjerg/ffmpegcore" />
</a>

### License

Released under the [MIT license](https://github.com/rosenbjerg/FFMpegCore/blob/main/LICENSE), which carries the copyright notice.
