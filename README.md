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

> **Upgrading from 5.x?** Version 6.0 renames most option methods after the ffmpeg options they emit, splits input from output options, and
> renames or removes several `FFMpeg.*` helpers. [MIGRATION.md](MIGRATION.md) lists every breaking change and its replacement, and the new
> helpers and builders that came with it.

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

A missing input throws `FFProbeException`, and a non-zero exit throws `FFProbeProcessException`, which carries `ExitCode` and the captured
stderr lines in `StandardError`. Both derive from `FFMpegException`, so a single `catch (FFMpegException)` covers ffprobe and ffmpeg alike.

## FFMpeg

Use FFMpeg to convert your media files.
Easily build your FFMpeg arguments using the fluent argument builder:

Convert input file to h264/aac scaled to 720p w/ faststart, for web playback

```csharp
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
```

`Scale(VideoSize.Hd)` sets the height and lets ffmpeg pick a width that keeps the aspect ratio, rounded to an even number — `scale=-2:720`.
Most video encoders reject odd dimensions for the usual `yuv420p` pixel format, so when you pass a size yourself, use `-2` rather than `-1`
for the side ffmpeg should compute.

Convert to and/or from streams

```csharp
await FFMpegArguments
    .FromPipeInput(new StreamPipeSource(inputStream))
    .OutputToPipe(new StreamPipeSink(outputStream), options => options
        .WithVideoCodec("vp9")
        .ForceFormat(ContainerFormats.WebM))
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
    .FromFileInput(inputPath, options => options
        .WithStartTime(TimeSpan.FromSeconds(10))
        .WithDuration(TimeSpan.FromSeconds(30)))
    .OutputToFile(outputPath, options => options
        .CopyStreams())
    .ProcessSynchronously();
```

Each option method's summary names the ffmpeg option it emits, so searching your IDE for `-ss` finds `WithStartTime`.

The options lambda goes straight after the path. File inputs check that the file exists before ffmpeg starts, and file outputs overwrite
an existing file; to change either, use the overload with the flag before the lambda, and name it so the call says what it does:

```csharp
FFMpegArguments
    .FromFileInput(inputPath, verifyExists: false, options => options.ForceFormat("mpegts"))
    .OutputToFile(outputPath, overwrite: false, options => options.CopyStreams())
    .ProcessSynchronously();
```

### Selecting streams

`WithMap` mirrors ffmpeg's own `-map input:specifier` — the input index first, then what to take from it. Leaving the stream index out maps
every stream of that kind, which is what you want when the input's stream count is not known up front:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .AddFileInput(audioPath)
    .OutputToFile(outputPath, options => options
        .WithMap(0, StreamType.Video)              // -map 0:v  — every video stream of the first input
        .WithMap(1, StreamType.Audio)              // -map 1:a  — every audio stream of the second
        .WithNegativeMap(0, StreamType.Subtitle)   // -map -0:s — but none of its subtitles
        .CopyStreams())
    .ProcessSynchronously();
```

`WithMap(0)` maps everything from the first input, and `WithMap(0, StreamType.All, 3)` picks a single stream by index — `-map 0:3`.
`WithMap(string)` and `WithNegativeMap(string)` select a label a complex-filter chain produced instead.

### Metadata tags and dispositions

`WithMetadata` sets a tag on the output, and `WithStreamMetadata` sets one on its streams — every stream of a type, or one of them by index.
`WithDisposition` marks streams as the default, forced, and so on:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath, options => options
        .WithMap(0)
        .CopyStreams()
        .WithMetadata("title", "Interview")                                    // -metadata "title=Interview"
        .WithStreamMetadata("language", "dan", StreamType.Audio, 0)            // -metadata:s:a:0 "language=dan"
        .WithDisposition(StreamDisposition.Default + StreamDisposition.Forced,
            StreamType.Subtitle, 0))                                           // -disposition:s:0 default+forced
    .ProcessSynchronously();
```

### Complex filters

`WithVideoFilters` builds the single `-vf` chain that one input feeds. When a filter needs more than one input, or you want to route what a
filter produces into another, use `WithComplexFilter`, which builds `-filter_complex`:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .AddFileInput(logoPath)
    .OutputToFile(outputPath, options => options
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
    .From(0, StreamType.Video).Video(f => f.Scale(640, -2)).As("small")
    .From("small").Video(f => f.HorizontalFlip()).As("flipped"))
.WithMap("flipped")
```

The chain has methods only for the filters ffmpeg accepts nowhere else — `Concat`, `Overlay` and `AudioMix`, all of which need more than one
input and are rejected in `-vf`. Every other filter goes in through `Video` and `Audio`, which take the same builders as `WithVideoFilters`
and `WithAudioFilters`, so `Scale`, `Fade` and the rest are spelled the same in both places. `WithFilter` takes a filter argument object
directly, and `WithCustomFilter(key, value)` covers anything the library has no method for. Both are on the `-vf` and `-af` builders as
well, so a filter without a method joins the built-in ones rather than replacing the whole chain:

```csharp
.WithVideoFilters(f => f.WithCustomFilter("yadif").Scale(VideoSize.Hd))
```

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
    Console.Error.WriteLine(string.Join("\n", result.StandardError));
}
```

By default (`throwOnError: true`) a non-zero exit throws `FFMpegProcessException`, whose `Result` is the same `FFMpegResult`, and a
cancellation throws `OperationCanceledException`. Pass `false` and the result reports what happened instead, through `ExitCode`,
`StandardError`, `Cancelled` and `Success`.

```csharp
try
{
    FFMpeg.Remux(inputPath, outputPath).ProcessSynchronously();
}
catch (FFMpegProcessException exception)
{
    logger.LogError("ffmpeg exited with {ExitCode}: {Output}", exception.Result.ExitCode, exception.Result.StandardError);
}
```

Problems found before ffmpeg starts throw the usual .NET exceptions: `FileNotFoundException` for a missing input, `IOException` for an
existing output when `overwrite: false`.

### Progress and cancellation

`NotifyOnProgress` reports the timestamp ffmpeg has reached. `NotifyOnPercentageProgress` reports a percentage, which needs the output
duration — pass it explicitly, or omit it after any `FFMpeg.*` helper except `SaveStream`, which reads a live stream with no known length.
Both take an `Action<T>` or an `IProgress<T>`:

```csharp
await FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath)
    .NotifyOnProgress(time => Console.WriteLine($"at {time}"))
    .NotifyOnPercentageProgress(percent => Console.WriteLine($"{percent}%"), mediaInfo.Duration)
    .ProcessAsynchronously(cancellationToken: cancellationToken);
```

Cancelling sends `q` to ffmpeg, so it finalises the output — an mp4 gets its index written and stays playable — and kills the process if it
has not exited within five seconds. `ProcessSynchronously` and `ProcessAsynchronously` take the token for a single run. `CancellableThrough`
attaches one to the processor instead, for every run it makes, and is where to choose a different grace period, or take an `out Action` to
cancel by calling it:

```csharp
var processor = FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath)
    .CancellableThrough(shutdownToken, TimeSpan.FromSeconds(30))
    .CancellableThrough(out var cancel);
```

Pass `TimeSpan.Zero` to kill ffmpeg straight away, at the cost of whatever it had not yet written.

### Multiple outputs

`OutputToMany` writes several outputs from one input using ffmpeg's own repeated outputs, which encodes once per output:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToMany(outputs => outputs
        .OutputToFile("sd.mp4", options => options.WithVideoFilters(f => f.Scale(1280, 720)))
        .OutputToFile("hd.mp4", options => options.WithVideoFilters(f => f.Scale(1920, 1080))))
    .ProcessSynchronously();
```

`OutputToTee` instead encodes once and fans the result out through ffmpeg's `tee` muxer, which is cheaper but requires every target to accept
the same encoded streams:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToTee(outputs => outputs
        .OutputToFile("recording.mp4")
        .OutputToUrl("rtmp://example.com/live/key", options => options.ForceFormat(ContainerFormats.Flv)),
        options => options.CopyStreams())
    .ProcessSynchronously();
```

### Analysing without writing an output

Detection filters such as `SilenceDetect` and `BlackDetect` report what they find on stderr rather than in an output file. `OutputToNull`
decodes the input through them and throws the result away (`-f null -`); the findings are in the result's `StandardError`, or arrive line by
line through `NotifyOnStandardError`:

```csharp
var result = FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToNull(options => options
        .DisableVideo()
        .WithAudioFilters(filters => filters.SilenceDetect(noise: -50, duration: 1)))
    .ProcessSynchronously();

var silences = result.StandardError.Where(line => line.Contains("silence_start") || line.Contains("silence_end"));
```

The filters log at ffmpeg's `info` level, so a quieter `LogLevel` drops their findings.

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
    .OutputToFile(outputPath, options => options.CopyStreams())
    .ProcessSynchronously();
```

A chapter given a single `TimeSpan` is a duration, and starts where the previous one ended; the overload taking two starts and ends it
explicitly. Chapters are `ChapterData`, the same type `IMediaAnalysis.Chapters` returns, so chapters read from one file can be fed straight
into another. Call `Build()` if you want the document text itself.

`AddMetadata` adds the document as an input and maps its metadata onto every output. An ffmetadata file you already have goes in
through `AddMetadataFile(path)`, which is mapped the same way. To take the metadata from another input instead, or
drop it, say so on the output — `WithMapMetadata(inputFileIndex)` and `WithoutMetadata()` replace the automatic mapping:

```csharp
FFMpegArguments
    .FromFileInput(videoPath)
    .AddFileInput(audiobookPath)
    .OutputToFile(outputPath, options => options
        .WithMapMetadata(1)   // -map_metadata 1 — keep the audiobook's tags
        .CopyStreams())
    .ProcessSynchronously();
```

## Helper methods

The provided helper methods make it simple to perform common operations. Each one builds the ffmpeg arguments and returns an
`FFMpegArgumentProcessor`, so you choose how to run it — `ProcessSynchronously()` or `await ProcessAsynchronously()` — and can
attach progress callbacks or cancellation exactly as with `FFMpegArguments`.

### Helpers and ffprobe

Most helpers need to know something about their input before they can build the arguments — its duration, its resolution, whether it has an
audio stream — so they run ffprobe while building. That probe is synchronous, which means `await FFMpeg.Remux(path, out).ProcessAsynchronously()`
blocks the calling thread for the probe before it reaches the `await`.

Every one of those helpers also has an overload taking an `IMediaAnalysis` instead of an input path, so you can do the probing yourself and
keep the whole thing asynchronous. The analysis carries the path it was made from, so it replaces the path rather than accompanying it:

```csharp
var source = await FFProbe.AnalyseAsync(inputPath, cancellationToken: cancellationToken);

await FFMpeg.Remux(source, "output.mkv")
    .ProcessAsynchronously(cancellationToken: cancellationToken);
```

For the helpers taking several inputs this also lets the probes run concurrently, where the path overloads probe one after another:

```csharp
var sources = await Task.WhenAll(parts.Select(part => FFProbe.AnalyseAsync(part, cancellationToken: cancellationToken)));

await FFMpeg.Concat("joined.mp4", sources)
    .ProcessAsynchronously(cancellationToken: cancellationToken);
```

It is also worth using whenever you have already probed the input to decide what to do — passing the analysis in saves a second probe of the
same file. An analysis made from a `Stream` has no path, so these overloads reject it; use the path overloads there.

### Easily capture snapshots from a video file:

```csharp
// persist the image on the drive
FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1))
    .ProcessSynchronously();

// or asynchronously, with cancellation
await FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1))
    .ProcessAsynchronously(cancellationToken: cancellationToken);

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

// by default ffmpeg picks the container's encoder at its default quality; pass output options to choose your own
FFMpeg.Join(@"..\joined_video.mp4", parts, options => options
    .WithVideoCodec(VideoCodec.LibX265)
    .WithConstantRateFactor(23)
).ProcessSynchronously();
```

### Overlay a watermark:

```csharp
FFMpeg.Watermark(inputPath, "logo.png", outputPath,
    WatermarkPosition.BottomRight, margin: 20
).ProcessSynchronously();
```

A PNG with an alpha channel keeps its transparency. The audio is copied across untouched; the video is re-encoded, since the picture is what
changes. For anything more elaborate — scaling the logo first, fading it in — build the graph yourself with
[`WithComplexFilter`](#complex-filters).

### Add or extract subtitles:

```csharp
// mux the subtitles in as their own stream, so the viewer can switch them off
FFMpeg.AddSubtitles(inputPath, "subs.srt", "output.mkv", language: "eng").ProcessSynchronously();

// mp4 only carries subtitles as mov_text
FFMpeg.AddSubtitles(inputPath, "subs.srt", "output.mp4", "eng", SubtitleCodec.MovText).ProcessSynchronously();

// and back out again
FFMpeg.ExtractSubtitles("output.mkv", "subs.srt").ProcessSynchronously();
```

To burn the subtitles into the picture instead, so they cannot be switched off, use the `subtitles` filter:

```csharp
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile(outputPath, options => options
        .WithVideoFilters(filters => filters
            .BurnSubtitles("subs.srt")))
    .ProcessSynchronously();
```

### Build a contact sheet of thumbnails:

```csharp
// 5x5 frames spread evenly across the whole video
FFMpeg.ThumbnailSheet(inputPath, "sheet.png").ProcessSynchronously();

// or one frame every 10 seconds, 160px wide, height following the aspect ratio
FFMpeg.ThumbnailSheet(inputPath, "sheet.jpg",
    columns: 10, rows: 8,
    interval: TimeSpan.FromSeconds(10),
    tileSize: new Size(160, -1)
).ProcessSynchronously();
```

This is the sprite sheet a player loads to show previews while scrubbing. Leave `interval` out and the frames are spread across the input's
duration instead.

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

`Trim` copies every stream rather than re-encoding, so the output container has to be able to mux them as they are — cutting an h264/aac mp4
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

// an encoder with no constant can be named directly
FFMpeg.ExtractAudio(inputPath, "track.opus", "libopus").ProcessSynchronously();
```

### Record a remote stream to a file:

```csharp
await FFMpeg.SaveStream(new Uri("https://example.com/live/stream.m3u8"), "recording.ts")
    .ProcessAsynchronously(cancellationToken: cancellationToken);
```

Any protocol ffmpeg can open works — http(s), rtmp, rtsp, srt. The streams are copied, not re-encoded. Prefer `.ts` or `.mkv` over `.mp4` for
anything long-running: an mp4 is only finalised when the run ends, so a crash loses the recording, while cancelling finalises it properly.

### Add or replace the audio track of a video file:

```csharp
FFMpeg.ReplaceAudio(inputPath, inputAudioPath, outputPath).ProcessSynchronously();

// re-encode the new track, for audio the output container cannot carry as it is
FFMpeg.ReplaceAudio(inputPath, "voiceover.wav", outputPath, AudioCodec.Aac).ProcessSynchronously();
```

The output takes the video of the first file and the audio of the second, whatever audio the video already had. Both are copied unless
you pass a codec.

### Combine an image with audio file, for youtube or similar platforms

```csharp
FFMpeg.PosterWithAudio(inputImagePath, inputAudioPath, outputPath).ProcessSynchronously();

// or from a bitmap in memory, using one of the image extension packages
using var image = SKBitmap.Decode(inputImagePath);
await image.AddAudio(inputAudioPath, outputPath).ProcessAsynchronously(cancellationToken: cancellationToken);
```

`AddAudio` returns the processor like the `FFMpeg.*` helpers. The bitmap is written to a temporary file when the run starts and deleted when
it ends, so keep it alive until then.

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
    .OutputToFile(outputPath, options => options
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
