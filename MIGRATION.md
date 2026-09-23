# Migrating from 5.x to 6.0

FFMpegCore 6.0 renames most option methods after the ffmpeg options they emit, splits input options from output options, and returns a
result object instead of a `bool`. This document lists every breaking change and what to replace it with. See the
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
FFMpeg.Mute(inputPath, outputPath).ProcessSynchronously();
await FFMpeg.SubVideo(inputPath, outputPath, start, end).ProcessAsynchronously();
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
| `SelectStream(…)` / `SelectStreams(…)` | `WithMap(…)` |
| `DeselectStream(…)` / `DeselectStreams(…)` | `WithNegativeMap(…)` |
| `WithCopyCodec()` / `CopyChannel(Channel.Both)` | `CopyStreams()` |
| `CopyChannel(Channel.Audio)` | `CopyStreams(StreamType.Audio)` |
| `DisableChannel(Channel.Audio)` | `DisableAudio()`, `DisableVideo()`, `DisableSubtitles()`, `DisableData()` |
| `WithBitStreamFilter(Channel, Filter)` | `WithBitstreamFilter(StreamType, BitstreamFilter)` |
| `ForcePixelFormat(f)` | `WithPixelFormat(f)` |
| `WithTagVersion(n)` | `WithId3v2Version(n)` |
| `WithGifPaletteArgument(…)` | `WithGifPalette(…)` |
| `Resize(w, h)` on an output | `WithVideoFilters(f => f.Scale(w, h))` |
| `Resize(w, h)` on an input | `WithFrameSize(w, h)` |
| `Crop(…)` | `WithVideoFilters(f => f.Crop(…))` |
| `Mirror(Mirroring.Horizontal)` | `WithVideoFilters(f => f.HorizontalFlip())` |
| `WithGlobalOptions(g => g.WithVerbosityLevel(v))` | `WithLogLevel(FFMpegLogLevel.…)` |
| `MultiOutput(…)` | `OutputToMany(…)` |
| `AddMetaData(…)` / `MapMetaData(…)` | `AddMetadata(…)` / `MapMetadata(…)` |

## Renamed and removed types

| 5.x | 6.0 |
|---|---|
| `FFMpegArgumentOptions` | `FFMpegInputOptions`, `FFMpegOutputOptions` |
| `Channel` | `StreamType` — `Channel.Both` is gone, use `StreamType.All` |
| `Filter` | `BitstreamFilter` |
| `Mirroring` | removed — use `HorizontalFlip()` / `VerticalFlip()` |
| `MetaDataBuilder`, `MetaData`, `IReadOnlyMetaData` (namespace `FFMpegCore.Builders.MetaData`) | `FFMetadataBuilder` in `FFMpegCore` |
| `FFMpegImage` in both image extension packages | `SystemDrawingImage` and `SkiaSharpImage`, so both can be referenced at once |
| `BitmapVideoFrameWrapper` in both image extension packages | `SystemDrawingVideoFrame` and `SkiaSharpVideoFrame` |
| `BitmapExtensions` in both image extension packages | `SystemDrawingImageExtensions` and `SkiaSharpBitmapExtensions` |
| `FFMpegGlobalArguments`, `VerbosityLevel` | removed — use `WithLogLevel` or `FFOptions.LogLevel` |
| `FFOptionsException` | removed — `FFMpegException` |
| namespace `FFMpegCore.Extend` | removed — its types moved to `FFMpegCore`, `FFMpegCore.Helpers` and `FFMpegCore.Pipes` |

`FFMetadataBuilder` is constructed directly (`new FFMetadataBuilder()`) and produces its document with `Build()`.

## New in 6.0

- `IProgress<TimeSpan>` and `IProgress<double>` overloads alongside the existing callbacks.
- `NotifyOnPercentageProgress` without a duration after an `FFMpeg.*` helper that already probed the input.
- `CancellableThrough(CancellationToken)` registers per run, so a processor can be run more than once.
- `FromImageSequenceInput` and `AddImageSequenceInput` for building a video from images through the argument builder.
- Per-run `FFOptions` now reach every argument, so `TemporaryFilesFolder` applies to the temp files that concat, metadata and image-sequence
  inputs create.
- `FFProbeException` and `FFProbeProcessException` derive from `FFMpegException`, so one `catch` covers both tools.
