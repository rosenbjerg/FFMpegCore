# Migrating from 5.x to 6.0

FFMpegCore 6.0 renames most option methods after the ffmpeg options they emit, splits input options from output options, and returns a
result object instead of a `bool`. It also renames or removes several `FFMpeg.*` helpers whose names or restrictions did not match what they
did. This document lists every breaking change and what to replace it with, and ends with [what 6.0 adds](#new-in-60). See the
[README](README.md) for the current API.

## Return values

`ProcessSynchronously` and `ProcessAsynchronously` return an `FFMpegResult` instead of a `bool`. Existing `if (…ProcessSynchronously())`
checks become `if (….ProcessSynchronously().Success)`.

The `FFMpeg.*` helpers now build the arguments and return the `FFMpegArgumentProcessor` without running it, so you choose how to run it — and
can attach progress or cancellation first. Their `…Async` variants are gone:

```csharp
// 5.x — ran immediately, returned bool
FFMpeg.Mute(inputPath, outputPath);
await FFMpeg.SubVideoAsync(inputPath, outputPath, start, end);

// 6.0 — returns a processor, which you then run
FFMpeg.RemoveAudio(inputPath, outputPath).ProcessSynchronously();
await FFMpeg.Trim(inputPath, outputPath, start, end).ProcessAsynchronously();
```

## Removed and renamed helpers

### `FFMpeg.Convert` is gone

It only ever supported `mp4`, `ogv`, `mpegts` and `webm`, hard-coded a 2400 kbps video bitrate, and silently ignored `speed`, `size`,
`audioQuality` and `multithreaded` on the `mpegts` path. `VideoSize.Hd` also *upscaled* anything smaller than 720p. Build the conversion with
`FFMpegArguments` instead, which is what the README already recommended for every other container:

```csharp
// 5.x / early 6.0
FFMpeg.Convert(inputPath, "output.mp4", VideoType.Mp4, Speed.Medium, VideoSize.Hd, AudioQuality.Good, multithreaded: true);

// 6.0
FFMpegArguments
    .FromFileInput(inputPath)
    .OutputToFile("output.mp4", options => options
        .WithVideoCodec(VideoCodec.LibX264)
        .WithVideoBitrate(2400)
        .WithVideoFilters(filters => filters.Scale(VideoSize.Hd))
        .WithSpeedPreset(EncoderPreset.Medium)
        .WithAudioCodec(AudioCodec.Aac)
        .WithAudioBitrate(AudioQuality.Good)
        .WithThreads(Environment.ProcessorCount))
    .ProcessSynchronously();
```

To change container without re-encoding, use `FFMpeg.Remux`.

### `FFMpeg.Mute` is `FFMpeg.RemoveAudio`

It never muted anything — it drops the audio stream. It also copied only the video stream, so subtitles and data were silently re-encoded or
dropped; it now maps and copies every stream and disables audio alone.

```csharp
FFMpeg.Mute(inputPath, outputPath);        // 5.x / early 6.0
FFMpeg.RemoveAudio(inputPath, outputPath); // 6.0
```

### `FFMpeg.SubVideo` is `FFMpeg.Trim`, and honours the output path you give it

"Sub video" read as subtitles; it cuts a section out. More importantly, it used to rewrite the output's extension to match the input's, so
asking for `out.mkv` silently produced `out.mp4` — and since the helper returns a processor rather than a path, there was no way to learn
where the file had gone. It now writes exactly the path you passed.

```csharp
FFMpeg.SubVideo(inputPath, "out.mkv", start, end); // 5.x / early 6.0 — wrote out.mp4
FFMpeg.Trim(inputPath, "out.mkv", start, end);     // 6.0 — writes out.mkv
```

Because it still copies the streams, a container that cannot mux them now fails the run instead of being quietly swapped out. Pick a
container that can, or re-encode with `FFMpegArguments`.

It also keeps every stream now. Without a `-map`, ffmpeg kept one stream of each kind, so a second audio track or a second subtitle
language was dropped. `Concat` does the same.

### `FFMpeg.ExtractAudio` takes any container, and can copy the stream

It required a `.mp3` output, although its arguments are just `-vn` and would have muxed `.m4a`, `.wav`, `.flac` or `.ogg` just as well. The
check is gone, and a new `audioCodec` parameter sits before `ffOptions`, so a positional `FFOptions` has to move along:

```csharp
FFMpeg.ExtractAudio(inputPath, outputPath, ffOptions);                    // 5.x / early 6.0
FFMpeg.ExtractAudio(inputPath, outputPath, ffOptions: ffOptions);         // 6.0
FFMpeg.ExtractAudio(inputPath, "track.m4a", AudioCodec.Copy);             // 6.0 — no re-encode
```

### `FFMpeg.SaveM3U8Stream` is `FFMpeg.SaveStream`

Nothing about it was M3U8-specific — it opens a URL and copies the streams. It rejected any scheme but http(s), ruling out rtmp, rtsp and
srt, and required an `.mp4` output, which is the worst container for a recording that may be interrupted. Both checks are gone.

```csharp
FFMpeg.SaveM3U8Stream(uri, "out.mp4");                          // 5.x / early 6.0
FFMpeg.SaveStream(uri, "recording.ts");                         // 6.0
FFMpeg.SaveStream(new Uri("rtsp://camera/stream"), "cam.mkv");  // 6.0
```

### `FFMpeg.PosterWithAudio` copies the audio

It used to re-encode the track to 128 kbps AAC unconditionally, which is the wrong default for turning a finished master into a video — the
platform it is being uploaded to will transcode it again. It now copies the stream, and takes a codec when you do want a re-encode. The new
parameter sits before `ffOptions`:

```csharp
FFMpeg.PosterWithAudio(image, audio, output, ffOptions);                  // 5.x / early 6.0
FFMpeg.PosterWithAudio(image, audio, output, ffOptions: ffOptions);       // 6.0 — copies the audio
FFMpeg.PosterWithAudio(image, audio, output, AudioCodec.Aac);             // 6.0 — old behaviour
```

It also no longer requires an `.mp4` output — `.mkv`, `.mov` and `.webm` (with a codec that container takes) work as well.

`AddAudio` in the image extension packages follows the same default.

### `FFMpeg.ReplaceAudio` uses the new track, and copies it

It mapped no streams, so ffmpeg picked the "best" audio across both inputs — the one with the most channels, or on a tie the first input's.
A video that already had audio usually kept it, and the replacement was silently dropped. It now takes the video of the first input and the
audio of the second.

It also re-encoded the new track to 192 kbps AAC unconditionally. Like `PosterWithAudio`, it now copies it, and takes a codec when you do
want a re-encode. The new parameter sits before `stopAtShortest`, so a positional `bool` stops compiling — name it:

```csharp
FFMpeg.ReplaceAudio(input, audio, output, true);                          // 5.x / early 6.0
FFMpeg.ReplaceAudio(input, audio, output, stopAtShortest: true);          // 6.0 — copies the audio
FFMpeg.ReplaceAudio(input, audio, output, AudioCodec.Aac);                // 6.0 — old behaviour, at ffmpeg's default bitrate
```

### `FFMpeg.PosterWithAudio`'s analysis overload takes the audio's analysis too

It probes the audio to learn the output's duration, so percentage progress works without passing one. The overload taking the image's
`IMediaAnalysis` therefore takes the audio's in place of its path:

```csharp
FFMpeg.PosterWithAudio(imageAnalysis, audioPath, output);                                // early 6.0
FFMpeg.PosterWithAudio(imageAnalysis, await FFProbe.AnalyseAsync(audioPath), output);    // 6.0
```

## Input and output options were split

`FFMpegArgumentOptions` is now `FFMpegInputOptions` and `FFMpegOutputOptions`, each carrying only the options ffmpeg accepts on that side.
Most code is unaffected, but an option used on the wrong side will no longer compile — in particular `WithVideoCodec` on an input is now
`WithVideoDecoder`, because `-c:v` before `-i` selects a decoder.

## Renamed option methods

| 5.x | 6.0 |
|---|---|
| `Seek(t)` / `EndSeek(t)` | `WithStartTime(t)` / `WithStopTime(t)` |
| `Loop(n)` | `WithLoop(n)` |
| `WithFramerate(r)` | `WithFrameRate(r)` |
| `UsingShortest(b)` | `WithShortest(b)` |
| `UsingThreads(n)` | `WithThreads(n)` |
| `UsingMultithreading(true)` | `WithThreads(Environment.ProcessorCount)` |
| `SelectStream(streamIndex, inputFileIndex, …)` | `WithMap(inputFileIndex, streamType, streamIndex)` — **note the order**: input, then stream type, then index, in every overload |
| `SelectStreams(streamIndices, inputFileIndex, …)` | `WithMap(inputFileIndex, streamType, streamIndices)` — the stream type is required here, `StreamType.All` for absolute indices |
| `DeselectStream(…)` / `DeselectStreams(…)` | `WithNegativeMap(…)`, reordered the same way |
| `WithCopyCodec()` / `CopyChannel(Channel.Both)` | `CopyStreams()` |
| `CopyChannel(Channel.Audio)` | `CopyStreams(StreamType.Audio)` |
| `DisableChannel(Channel.Audio)` | `DisableAudio()`, `DisableVideo()`, `DisableSubtitles()`, `DisableData()` |
| `WithBitStreamFilter(Channel, Filter)` | `WithBitstreamFilter(StreamType, BitstreamFilter)` |
| `ForcePixelFormat(f)` | `WithPixelFormat(f)` |
| `WithTagVersion(n)` | `WithId3v2Version(n)` |
| `WithFrameOutputCount(n)` | `WithFrameCount(n)` |
| `WithGifPaletteArgument(…)` | `WithGifPalette(…)` |
| `Resize(w, h)` on an output | `WithVideoFilters(f => f.Scale(w, h))` |
| `Resize(w, h)` on an input | `WithFrameSize(w, h)` |
| `Crop(…)` | `WithVideoFilters(f => f.Crop(…))` |
| `Mirror(Mirroring.Horizontal)` | `WithVideoFilters(f => f.HorizontalFlip())` |
| `WithGlobalOptions(g => g.WithVerbosityLevel(v))` | `WithLogLevel(FFMpegLogLevel.…)` |
| `MultiOutput(…)` | `OutputToMany(…)` |
| `AddMetaData(…)` | `AddMetadata(…)` |
| `MapMetaData(i)` / `MapMetadata(i)` on `FFMpegArguments` | `WithMapMetadata(i)` on the output options — see [below](#-map_metadata-is-an-output-option) |
| `ProcessSynchronously(…, ffMpegOptions: o)` | `ProcessSynchronously(…, ffOptions: o)` — named only |
| `NotifyOnError(…)` | `NotifyOnStandardError(…)` — it receives every stderr line, which is all of ffmpeg's logging and progress, not just errors |
| `NotifyOnOutput(…)` | `NotifyOnStandardOutput(…)` |
| `FFMpegArguments.Text` | `….OutputTo…(…).Arguments` — the rendered command line is read from the processor, which is the only place it is complete |
| `WithVideoBitrate(bitrate: n)` / `WithAudioBitrate(bitrate: n)` | `WithVideoBitrate(kilobitsPerSecond: n)` — the unit was always kbps; only a named argument changes |

## Renamed and removed types

| 5.x | 6.0 |
|---|---|
| `FFMpegArgumentOptions` | `FFMpegInputOptions`, `FFMpegOutputOptions` |
| `Channel` | `StreamType` — `Channel.Both` is gone, use `StreamType.All` |
| `FFMpegCore.Arguments.FadeDirection` | `FFMpegCore.Enums.FadeDirection`, with the other values passed to builder methods |
| `Filter` | `BitstreamFilter` — a struct, adding `Hevc_Mp4ToAnnexB`, `Mpeg4_UnpackBFrames`, `ExtractExtradata` and `DumpExtra`; any other bitstream filter passes through as a string |
| `Speed` | `EncoderPreset` — a struct like `EncoderTune`, so `"p4"` (NVENC) or `"8"` (SVT-AV1) pass through as strings; `Speed.UltraFast` is `EncoderPreset.UltraFast`, and `Placebo` is new |
| `HardwareAccelerationDevice` enum | a struct with the same member names plus `VideoToolbox`, `Vulkan`, `D3D12VA`, `OpenCL` and `DRM`; any other `-hwaccel` value passes through as a string. A `switch` over it no longer compiles — compare `Value` instead. `HardwareAccelerationArgument.HardwareAccelerationDevice` is the field `Device` |
| `Mirroring` | removed — use `HorizontalFlip()` / `VerticalFlip()` |
| `MetaDataBuilder`, `MetaData`, `IReadOnlyMetaData` (namespace `FFMpegCore.Builders.MetaData`) | `FFMetadataBuilder` in `FFMpegCore` |
| `FFMpegImage` in both image extension packages | `SystemDrawingImage` and `SkiaSharpImage`, so both can be referenced at once |
| `BitmapVideoFrameWrapper` in both image extension packages | `SystemDrawingVideoFrame` and `SkiaSharpVideoFrame` |
| `BitmapExtensions` in both image extension packages | `SystemDrawingImageExtensions` and `SkiaSharpBitmapExtensions` |
| `FFMpegGlobalArguments`, `VerbosityLevel` | removed — use `WithLogLevel` or `FFOptions.LogLevel` |
| `FFOptionsException` | removed — `FFMpegException` |
| `OverwriteExisting()`, `OverwriteArgument` | removed — `OutputToFile(path, overwrite: true)`, which is the default and already emits `-y` |
| `IMediaAnalysis` implementations | must add `string? Path` — the input the analysis describes, or null when it came from a stream |
| `IMediaAnalysis.VideoStreams`, `AudioStreams`, `SubtitleStreams`, `Chapters` (`List<T>`) | `IReadOnlyList<T>` — an analysis describes a file, and adding to it changed nothing but the helpers' view of that file |
| `FileExtension.Image.All` (`List<string>`) | `IReadOnlyList<string>` — it was a writable global the snapshot helpers validate against |
| `VideoStream.AverageFrameRate` | removed — it was never populated and always read `0`; use `AvgFrameRate` (ffprobe's `avg_frame_rate`) or `RealFrameRate` (`r_frame_rate`) |
| `VideoStream.FrameRate` | `RealFrameRate` — it holds ffprobe's `r_frame_rate`, the lowest rate that represents every timestamp, which for variable-frame-rate video (most phone recordings) can be far above the rate the video plays at. `AvgFrameRate` is usually the one you want |
| `new InputArgument(bool, string)` | `new InputArgument(string path, bool verifyExists)` — the two constructors differed only in argument order |
| `new MultiInputArgument(bool, IEnumerable<string>)` | `new MultiInputArgument(IEnumerable<string> paths, bool verifyExists)` — likewise |
| `VideoCodec.MpegTs` | removed — `mpegts` is a container, not a video codec; it emitted `-c:v mpegts`. Use `ForceFormat(ContainerFormats.Ts)` |
| `VideoType` | `ContainerFormats` — its members are `ContainerFormat` values, so the old name claimed a video type it never was |
| `VideoType.MpegTs` | `ContainerFormats.Ts` — they were the same value under two names |
| `AudioCodec.LibFdk_Aac` | `AudioCodec.LibFdkAac` |
| `Filter.Aac_AdtstoAsc` | `BitstreamFilter.Aac_AdtsToAsc` |
| `MetaDataArgument` | `MetadataArgument`, matching `AddMetadata` and `MapMetadataArgument` |
| `FaststartArgument` | `FastStartArgument` |
| `VariableBitRateArgument` | `VariableBitrateArgument` |
| `ID3V2VersionArgument` | `Id3v2VersionArgument` |
| `Codec.Extension()` (the `FileExtension` extension method) | removed — it mapped eight codecs to a container extension and threw a bare `Exception` for anything else |
| `ContainerFormat.Extension` (property) | `ContainerFormat.GetExtension(FFOptions? = null)` |
| `MediaFormat.BitRate` is `double` | `long`, matching `MediaStream.BitRate` |
| `FromConcatInput` / `AddConcatInput` / `ConcatArgument` | `FromConcatProtocolInput` / `AddConcatProtocolInput` / `ConcatProtocolArgument` |
| `FromDemuxConcatInput` / `AddDemuxConcatInput` / `DemuxConcatArgument` | `FromConcatDemuxerInput` / `AddConcatDemuxerInput` / `ConcatDemuxerArgument` |
| namespace `FFMpegCore.Extend` | removed — its pipe types moved to `FFMpegCore.Pipes`; `TimeSpanExtensions` and `ProcessArgumentsExtensions` are internal |
| `FFMpegHelper`, `FFProbeHelper` | internal — they were the library's own checks; `ExtensionExceptionCheck`, which nothing called, is gone |

The two are different ffmpeg mechanisms and the old names did not say which was which. `concat:` is a *protocol* that joins the files
byte-wise, and works only for formats that survive naive concatenation such as mpegts and mp3; the concat *demuxer* writes a list file and
joins stream-wise, and is the one that works for mp4 and mkv. Both were renamed rather than just the confusing one — leaving
`FromConcatInput` in place with either meaning would let existing code keep compiling while doing something different.

For joining video files, prefer `FFMpeg.Concat`, which uses the demuxer.

`ContainerFormat.Extension` read `ExtensionOverrides` off `GlobalFFOptions.Current`, so it ignored the per-run `FFOptions` that 6.0 threads
through everything else. It is now a method taking them, named `GetExtension` to match `MediaStream.GetCodecInfo` and
`VideoStream.GetPixelFormatInfo`.

The rename is deliberate rather than just adding a parameter: had the property become a method of the same name, `$"out{format.Extension}"`
would have kept compiling and silently interpolated the method group, producing filenames like
`out<>f__AnonymousDelegate0\`2[...]`. `GetExtension` fails to compile instead.

`FileExtension.Mp4`/`.Ts`/`.Ogv`/`.WebM` are now plain constants. They previously read the global options once at static-initialisation time,
so a later `GlobalFFOptions.Configure` never reached them.

`FromFileInput(FileInfo)` and `AddFileInput(FileInfo)` now verify that the file exists, like their `string` counterparts always have. Pass
`verifyExists: false` for the old behaviour.

`FFMetadataBuilder` is constructed directly (`new FFMetadataBuilder()`) and produces its document with `Build()`.

### `WithChapter` takes a `TimeSpan`, not a number

`WithChapter(string, long)` meant milliseconds and `WithChapter(string, double)` meant seconds, so `WithChapter("Intro", 90)` bound to the
`long` overload and produced a 90 *millisecond* chapter. Nothing at the call site said which unit applied. Both are gone; pass a `TimeSpan`:

```csharp
builder.WithChapter("Intro", 90);                          // 5.x / early 6.0 — 90 ms, probably not what was meant
builder.WithChapter("Intro", TimeSpan.FromSeconds(90));    // 6.0
```

### The snapshot helpers dropped `inputFileIndex`

`FFMpeg.Snapshot`, `SnapshotArgumentBuilder.BuildSnapshotArguments` and `Snapshot`/`SnapshotAsync` in both image extension packages took an
`inputFileIndex` although the arguments they build have exactly one input, so any value but `0` produced a `-map` against an input that was
never added. Drop the argument; a positional `FFOptions` or `CancellationToken` after it moves up one place.

`SnapshotArgumentBuilder`'s methods also took the input path alongside the `IMediaAnalysis` that already carries it. They take the analysis
first and no path, as the `FFMpeg.*` helpers' analysis overloads do:

```csharp
SnapshotArgumentBuilder.BuildSnapshotArguments(input, output, analysis, size);  // 5.x / early 6.0
SnapshotArgumentBuilder.BuildSnapshotArguments(analysis, output, size);         // 6.0
```

### `WithGifPalette`'s `streamIndex` is a stream index

It rendered `[N:v]` — the video of *input* `N` — so `GifSnapshot`, which passes the video stream's index, failed with "Invalid file
index" on any file whose video is not stream 0. It now renders `[0:N]`, stream `N` of the first input, as its name and `Snapshot`'s
`streamIndex` always said. A call passing `0` for a file whose first stream is audio now selects that audio stream; pass the video
stream's `Index` instead.

### `SilenceDetect`'s noise threshold defaults to -60dB

The default was `60`, which rendered as `silencedetect=n=60.0dB` — a threshold 60 dB *above* full scale. Everything is below that, so the
whole input came back as one silent stretch. ffmpeg's own default is -60 dB, and that is what `SilenceDetect()` and
`new SilenceDetectArgument()` now emit. Calls that passed a threshold are unaffected.

### `Scale(VideoSize.…)` keeps the width even

It rendered `scale=-1:720`, which keeps the aspect ratio exactly and so can produce an odd width — a 1080×1920 portrait video becomes
405×720, which libx264 and most other encoders reject for `yuv420p`. It now renders `scale=-2:720`, rounding the computed width to an even
number. `ThumbnailSheet`'s default tile size does the same.

## `-map_metadata` is an output option

`MapMetadata` sat on `FFMpegArguments` among the inputs, and `AddMetadata` emitted its `-map_metadata` straight after its own `-i`. ffmpeg
reads `-map_metadata` as an option of the *next* file, so any input added after either of them failed the run with "cannot be applied to
input url". The index was also computed by counting input arguments, so `FromFileInput(IEnumerable<string>)` — one argument, several `-i`
— threw it off, as did `MapMetadata` itself.

`AddMetadata` now only adds the input, and maps it on every output that does not choose its own mapping. Choosing one is an output option:

```csharp
FFMpegArguments.FromFileInput(a).AddFileInput(b).MapMetadata(1).OutputToFile(output);                     // 5.x / early 6.0
FFMpegArguments.FromFileInput(a).AddFileInput(b).OutputToFile(output, o => o.WithMapMetadata(1));         // 6.0
```

`MapMetadataArgument` takes the index it maps, and is no longer an input argument. `MetadataArgument` emits only its `-i`.

## `DrawText`, `Pad` and `HardBurnSubtitle` take a lambda

They took an options object built with a static `Create`, unlike every other part of the builder. They now take their required values
directly and everything else through a lambda, and the options classes only offer `With*` methods:

```csharp
// 5.x / early 6.0
.DrawText(DrawTextOptions.Create("Hello", "font.ttf", ("fontsize", "24")))
.Pad(PadOptions.Create("iw+20", "ih+20").WithParameter("color", "black"))
.Pad(PadOptions.Create("16/9"))
.HardBurnSubtitle(SubtitleHardBurnOptions.Create("subs.srt").SetOriginalSize(1920, 1080)
    .WithStyle(StyleOptions.Create().WithParameter("FontSize", "24")))

// 6.0
.DrawText("Hello", text => text.WithFontFile("font.ttf").WithParameter("fontsize", "24"))
.Pad("iw+20", "ih+20", pad => pad.WithParameter("color", "black"))
.Pad(configure: pad => pad.WithAspectRatio("16/9"))
.HardBurnSubtitle("subs.srt", subtitles => subtitles.WithOriginalSize(1920, 1080)
    .WithStyle(style => style.WithParameter("FontSize", "24")))
```

`DrawText` no longer requires a font file: ffmpeg falls back to its default font through fontconfig. `SetOriginalSize`, `SetSubtitleIndex`
and `SetCharacterEncoding` are `WithOriginalSize`, `WithSubtitleIndex` and `WithCharacterEncoding`. `new DrawTextArgument(…)`,
`new PadArgument(…)` and `new SubtitleHardBurnArgument(…)` take the same arguments as the filter methods, and a `Pad` with no width, height
or aspect ratio throws `ArgumentException` instead of a bare `Exception`.

## Filter parameters with a fixed set of values are typed

`Overlay`'s `eofAction`, `AudioMix`'s `duration`, `Fps`'s `round`, `SilenceDetect`'s `noiseType`, `HighPass`/`LowPass`'s `widthType`,
`transform` and `precision`, and `AudioGate`'s `mode`, `detection` and `link` take a small type per parameter — `OverlayEofAction`,
`AudioMixDuration`, `FpsRounding`, `SilenceDetectNoiseUnit`, `FilterWidthType`, `FilterTransform`, `FilterPrecision`, `AudioGateMode`,
`AudioGateDetection`, `AudioGateLink`, all in `FFMpegCore.Enums` — whose members list the values ffmpeg accepts. A plain string still converts to each of them, so
existing calls compile unchanged:

```csharp
.Overlay("W-w-10", "H-h-10", OverlayEofAction.Pass)
.Overlay("W-w-10", "H-h-10", "pass")   // still fine
```

Values the library does not know are passed on to ffmpeg as they are rather than rejected, so a newer ffmpeg's options stay reachable.
In particular `HighPass`/`LowPass` silently dropped a `transform` they did not recognise; it is now emitted. The exception is
`SilenceDetect`'s `noiseType`, which decides how the library formats the threshold and is not an ffmpeg value: anything but `db` and `ar`
still throws.

## Pipes

`IPipeSink.GetFormat()` is `GetStreamArguments()`, matching `IPipeSource`: both return the arguments ffmpeg needs on their side of the pipe.
It was never called, so `StreamPipeSink.Format` did nothing — the output format had to be forced with `ForceFormat` regardless. Setting it
now emits `-f`. `StreamPipeSource` gains the same settable `Format` (its `StreamFormat` was get-only and always empty), and `BlockSize`
becomes settable as on the sink.

```csharp
.OutputToPipe(new StreamPipeSink(stream), o => o.ForceFormat("matroska"))  // still works
.OutputToPipe(new StreamPipeSink(stream) { Format = "matroska" })           // 6.0 — now does what it says
.FromPipeInput(new StreamPipeSource(stream) { Format = "mpegts" })          // 6.0
```

`RawVideoPipeSource.StreamFormat` is `PixelFormat`, since it holds the frames' `-pix_fmt`, not a container format.

## Errors

A failed ffmpeg run throws `FFMpegProcessException`, which derives from `FFMpegException` and carries the run's `FFMpegResult` as `Result`
— so `ExitCode` and `ErrorOutput` are there whether you let the run throw or pass `throwOnError: false`. `catch (FFMpegException)` still
catches it.

The captured stderr had four names across three types. It is now `ErrorOutput`, an `IReadOnlyList<string>`, everywhere:

| 5.x / early 6.0 | 6.0 |
|---|---|
| `FFMpegException.FFMpegErrorOutput` (`string`) | `FFMpegProcessException.Result.ErrorOutput`; removed from `FFMpegException` along with the constructors taking it |
| `FFProbeProcessException.ErrorOutput` (`IReadOnlyCollection<string>`) | `IReadOnlyList<string>`, alongside a new `ExitCode`. Its constructor takes `(exitCode, errorOutput)` |
| `IMediaAnalysis.ErrorData` | `IMediaAnalysis.ErrorOutput` |
| `FFProbeException(…, ffProbeErrorOutput)` | the parameter is gone |

Calling a builder method wrongly throws `ArgumentException` or `ArgumentOutOfRangeException`, as .NET APIs do, instead of one of three
types. `FFMpegArgumentException` is gone — it derived from neither `FFMpegException` nor `ArgumentException`, so neither `catch` caught it:

| Mistake | 5.x / early 6.0 | 6.0 |
|---|---|---|
| A `Codec` of the wrong type for `WithVideoCodec`/`WithAudioCodec`/`WithSubtitleCodec` | `FFMpegException` | `ArgumentException` |
| A `StreamType` that `-bsf` or `-vn`/`-an`/… has no spelling for | `FFMpegException` | `ArgumentOutOfRangeException` |
| `WithVideoFilters`/`WithAudioFilters`/`WithComplexFilter` that add nothing | `FFMpegArgumentException`, when the command line was rendered | `ArgumentException`, from the `With…Filter…` call itself |

`OutputToFile(path, overwrite: false)` onto an existing file throws `IOException` instead of `FFMpegException`, naming the file. A missing
input already threw `FileNotFoundException`; the two now match. This now applies to the targets of `OutputToTee` too, which were
overwritten regardless whenever another target allowed it.

## Cancellation waits for ffmpeg to finalise the output

`CancellableThrough` sent ffmpeg `q` and then killed it after `timeout` milliseconds — and the default was `0`, so it killed straight away
and an mp4 was left without its index. It now waits up to five seconds for ffmpeg to finish before killing it, and takes the grace period
as a `TimeSpan`:

```csharp
.CancellableThrough(token, 10000)                    // 5.x / early 6.0
.CancellableThrough(token, TimeSpan.FromSeconds(10)) // 6.0
.CancellableThrough(token, TimeSpan.Zero)            // 6.0 — the old default
```

`ProcessSynchronously` and `ProcessAsynchronously` also take a `CancellationToken` now, as their last parameter, for cancelling a single
run without attaching the token to the processor:

```csharp
await processor.CancellableThrough(token).ProcessAsynchronously(); // still works
await processor.ProcessAsynchronously(cancellationToken: token);   // 6.0
```

## Extension packages

`Snapshot` and `SnapshotAsync` in both image extension packages take the run's `FFOptions`, placed before `cancellationToken` to match
`FFProbe.AnalyseAsync`, so a token passed positionally has to move along:

```csharp
// 5.x
await FFMpegImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, inputFileIndex, cancellationToken);

// 6.0
await SystemDrawingImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, ffOptions, cancellationToken);
```

`AddAudio` returns the `FFMpegArgumentProcessor` instead of running, like the `FFMpeg.*` helpers, so progress, cancellation and the choice
of sync or async are the caller's. `AddAudioAsync` is gone. It takes an optional `Codec` for the audio, and an overload takes the audio's
`IMediaAnalysis`. The bitmap is written to a temporary file when the run starts rather than when `AddAudio` is called, so keep it alive until
the run ends:

```csharp
bitmap.AddAudio(audio, output);                                                          // 5.x — ran immediately
await bitmap.AddAudioAsync(audio, output, cancellationToken: token);                     // 5.x
bitmap.AddAudio(audio, output).ProcessSynchronously();                                   // 6.0
await bitmap.AddAudio(audio, output).ProcessAsynchronously(cancellationToken: token);    // 6.0
```

## FFProbe's async overloads take the token last

`CancellationToken` sat before `customArguments`; it is now the final parameter, so a positionally-passed token has to be named:

```csharp
await FFProbe.AnalyseAsync(path, ffOptions, cancellationToken);                  // 5.x / early 6.0
await FFProbe.AnalyseAsync(path, ffOptions, cancellationToken: cancellationToken); // 6.0
```

This applies to `AnalyseAsync`, `GetFramesAsync` and `GetPacketsAsync` alike.

`FFMpegDownloader.DownloadBinaries` is `DownloadBinariesAsync` and takes a `CancellationToken`. `FFMpegDownloaderException` derives from
`FFMpegException`, and `EnumExtensions` is no longer part of the package's public surface.

## New in 6.0

- Six `FFMpeg.*` helpers for operations that previously had no route through the API:

  | Helper | What it does |
  |---|---|
  | `Remux` | Changes container, copying the streams — no re-encode. |
  | `Concat` | Joins files through the concat demuxer, copying the streams. `Join` re-encodes and stays for inputs that differ. |
  | `ThumbnailSheet` | Samples frames at an interval and tiles them into a contact sheet or scrub-preview strip. |
  | `Watermark` | Overlays an image at a corner, keeping the audio. |
  | `AddSubtitles` | Muxes a subtitle file in as its own switchable stream, rather than burning it into the picture. |
  | `ExtractSubtitles` | Writes a subtitle stream back out to a file. |

- Every `FFMpeg.*` helper that probes its input also takes an `IMediaAnalysis` in place of the input path, so the probe can be done by the
  caller with `FFProbe.AnalyseAsync` and the whole operation kept asynchronous. `Concat`, `Join` and `JoinImageSequence` take a sequence of
  them, which also lets the probes run concurrently rather than one per input in a loop.
- `IProgress<TimeSpan>` and `IProgress<double>` overloads alongside the existing callbacks.
- `NotifyOnPercentageProgress` without a duration after any `FFMpeg.*` helper except `SaveStream`. To know the duration, `ExtractAudio`
  and `ExtractSubtitles` now probe their input, and take an `IMediaAnalysis` in its place like the other helpers.
- `CancellableThrough(CancellationToken)` registers per run, so a processor can be run more than once.
- `ProcessSynchronously` and `ProcessAsynchronously` take a `CancellationToken`.
- `FromImageSequenceInput` and `AddImageSequenceInput` for building a video from images through the argument builder.
- `WithMap`/`WithNegativeMap` take a `StreamType` in place of a stream index, so `-map 0:a` — every audio stream of an input — is
  expressible without probing first to count them.
- `FromFileInput`, `AddFileInput` and `OutputToFile` take the options lambda straight after the path, so
  `OutputToFile(path, true, options => …)` can be written `OutputToFile(path, options => …)`. The overloads with the `bool` stay, for
  `verifyExists: false` and `overwrite: false`.
- Encoder tuning options that needed `WithCustomArgument`: `WithAudioChannels` (`-ac`, on inputs too), `WithVideoProfile` (`-profile:v`),
  `WithTune` (`-tune`), `WithGopSize` (`-g`), `WithMaxBitrate` and `WithBufferSize` (`-maxrate`/`-bufsize`), and
  `WithVideoQualityScale`/`WithAudioQualityScale` (`-q:v`/`-q:a`). `VideoProfile` and `EncoderTune` list the common values.
- `OutputToNull()` for analysis-only runs (`-f null -`), so `SilenceDetect` and `BlackDetect` no longer need a dummy output path.
- `ContainerFormats.Matroska`, `Flv`, `Mp3`, `Wav`, `Flac`, `Hls`, `Image2`, `RawVideo` and `Null`. `FFOptions.ExtensionOverrides` maps
  `matroska` to `.mkv` and `hls` to `.m3u8` by default, alongside `mpegts` to `.ts`.
- `WithMetadata`, `WithStreamMetadata` and `WithDisposition` for `-metadata`, `-metadata:s` and `-disposition`, which needed
  `WithCustomArgument` before. `StreamDisposition` lists the dispositions and combines them with `+`.
- `FromInput(IInputArgument)` and `AddInput(IInputArgument)`, for inputs the library has no method for, and a `PosterWithAudio` overload
  taking the image as such an argument.
- `FromUrlInput(string)` and `AddUrlInput(string)` alongside the `Uri` overloads, matching the pair `OutputToUrl` already had.
- `VideoCodec.Copy`, pairing with the `AudioCodec.Copy` that already existed.
- A `SubtitleCodec` constants class — `MovText`, `Srt`, `Ass`, `WebVtt`, `Copy` — alongside the `VideoCodec` and `AudioCodec` ones, so
  naming a subtitle encoder no longer means `FFMpeg.GetCodec("mov_text")` and the `ffmpeg -codecs` run behind it.
- `AddSubtitles`, `ExtractAudio`, `PosterWithAudio` and `ReplaceAudio` take an encoder name as a `string` as well as a `Codec`, for the
  encoders that have no constant. The `Codec` overload stays the one to prefer, since it checks the codec is of the right kind at the call
  rather than leaving ffmpeg to reject it. Passing a bare `null` for the codec is now ambiguous between the two; omit it, or name `ffOptions:`.
- `FFProbe.Analyse`, `GetFrames` and `GetPackets` each accept a path, a `Uri` and a `Stream`, sync and async. Previously `GetPackets` took
  only a path and neither `GetFrames` nor `GetPackets` accepted a `Stream`.
- `WithFilter` and `WithCustomFilter(key, value)` on `VideoFilterOptions` and `AudioFilterOptions`, so a filter the library has no method
  for joins the `-vf`/`-af` chain instead of forcing the whole chain into `WithCustomArgument`. Their `Arguments` list is now read-only;
  `f.Arguments.Add(filter)` becomes `f.WithFilter(filter)`.
- Five filters that had no method on the filter builders: `Fps`, `Tile`, `Speed` and `Fade` on `VideoFilterOptions`, and `Loudnorm`,
  `Speed` and `Fade` on `AudioFilterOptions`.
- `WithComplexFilter`, a typed builder for `-filter_complex`, with `WithMap(string label)` to select what a chain produced. A chain's
  `Video(f => …)` and `Audio(f => …)` take the same builders as `WithVideoFilters` and `WithAudioFilters`. Filters needing
  more than one input — `Concat`, `Overlay`, `AudioMix` — are reachable for the first time without hand-writing the graph into
  `WithCustomArgument`.
- Per-run `FFOptions` now reach every argument, so `TemporaryFilesFolder` applies to the temp files that concat, metadata and image-sequence
  inputs create.
- `FFProbeException` and `FFProbeProcessException` derive from `FFMpegException`, so one `catch` covers both tools.
- The image extension packages honour a per-run `FFOptions`, so `BinaryFolder` and `TemporaryFilesFolder` apply to snapshots and to the
  poster `AddAudio` writes.
- `FFMpegDownloader` works on Apple Silicon, where it previously threw `PlatformNotSupportedException`.
- A missing ffmpeg or ffprobe raises `FFMpegException` or `FFProbeException` naming the path that was tried, where `Instances`'
  `InstanceFileNotFoundException` used to escape.
- `VideoCodec.*`, `AudioCodec.*` and `ContainerFormats.*` are built from their known name and type instead of being looked up through
  `ffmpeg -codecs`. They no longer spawn a process, and no longer throw when the build lacks the codec — ffmpeg reports that itself when
  the run starts. `Description`, `EncodingSupported` and the other fields the listing fills in are empty on them; call
  `FFMpeg.GetCodec(name)` for a populated `Codec`.
- The `FFMpeg.GetCodecs` / `GetPixelFormats` / `GetContainerFormats` family and their `TryGet*` counterparts take an optional `FFOptions`,
  and their cache is keyed on the binary that answered.
- `MediaStream.GetCodecInfo` and `VideoStream.GetPixelFormatInfo` take one too, so a stream can be described by the same ffmpeg that is
  going to process it.
- Every `FFMpeg.*` helper takes an optional `FFOptions` covering both the ffprobe call it makes while building the arguments and the run
  itself, so `BinaryFolder` no longer has to be global for a helper to find ffprobe. `Join` and `JoinImageSequence` end in `params`, so
  they take theirs as a leading argument on a separate overload.
