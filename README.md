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

```csharp
FFMpeg.Join(@"..\joined_video.mp4",
    @"..\part1.mp4",
    @"..\part2.mp4",
    @"..\part3.mp4"
).ProcessSynchronously();
```

### Create a sub video

``` csharp
FFMpeg.SubVideo(inputPath,
    outputPath,
    TimeSpan.FromSeconds(0),
    TimeSpan.FromSeconds(30)
).ProcessSynchronously();
```

### Join images into a video:

```csharp
FFMpeg.JoinImageSequence(@"..\joined_video.mp4", frameRate: 1,
    @"..\1.png",
    @"..\2.png",
    @"..\3.png"
).ProcessSynchronously();
```

### Convert a video to another format:

```csharp
FFMpeg.Convert(inputPath, @"..\output.webm", VideoType.WebM).ProcessSynchronously();

// scale down, and encode across every processor rather than on a single thread
FFMpeg.Convert(inputPath, @"..\output.mp4", VideoType.Mp4,
    speed: Speed.Medium,
    size: VideoSize.Hd,
    audioQuality: AudioQuality.Good,
    multithreaded: true
).ProcessSynchronously();
```

`Convert` supports the `mp4`, `ogv`, `mpegts` and `webm` container formats, and picks a codec pairing for each. For anything else, build the
arguments yourself with `FFMpegArguments`.

### Mute the audio of a video file:

```csharp
FFMpeg.Mute(inputPath, outputPath).ProcessSynchronously();
```

### Extract the audio track from a video file:

```csharp
FFMpeg.ExtractAudio(inputPath, outputPath).ProcessSynchronously();
```

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
await FFMpegDownloader.DownloadBinaries();
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
