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
    .OutputToFile("output.mp4", true, options => options
        .WithVideoCodec(VideoCodec.LibX264)
        .WithVideoBitrate(2400)
        .WithVideoFilters(filters => filters.Scale(VideoSize.Hd))
        .WithSpeedPreset(Speed.Medium)
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

`AddAudio`/`AddAudioAsync` in the image extension packages follow the same default.

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
| `SelectStream(streamIndex, inputFileIndex, …)` | `WithMap(inputFileIndex, streamType, streamIndex)` — **note the order** |
| `SelectStreams(streamIndices, inputFileIndex, …)` | `WithMap(inputFileIndex, streamIndices, streamType)` |
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
| `AddMetaData(…)` / `MapMetaData(…)` | `AddMetadata(…)` / `MapMetadata(…)` |
| `ProcessSynchronously(…, ffMpegOptions: o)` | `ProcessSynchronously(…, ffOptions: o)` — named only |

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
| `OverwriteExisting()`, `OverwriteArgument` | removed — `OutputToFile(path, overwrite: true)`, which is the default and already emits `-y` |
| `VideoStream.AverageFrameRate` | removed — it was never populated and always read `0`; use `AvgFrameRate` (ffprobe's `avg_frame_rate`) or `FrameRate` (`r_frame_rate`) |
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
| namespace `FFMpegCore.Extend` | removed — its types moved to `FFMpegCore`, `FFMpegCore.Helpers` and `FFMpegCore.Pipes` |

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

## Extension packages

`Snapshot` and `SnapshotAsync` in both image extension packages take the run's `FFOptions`, placed before `cancellationToken` to match
`FFProbe.AnalyseAsync`, so a token passed positionally has to move along:

```csharp
// 5.x
await FFMpegImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, inputFileIndex, cancellationToken);

// 6.0
await SystemDrawingImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, ffOptions, cancellationToken);
```

`AddAudio` takes the same `FFOptions`, and `AddAudioAsync` adds cancellation.

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

- `IProgress<TimeSpan>` and `IProgress<double>` overloads alongside the existing callbacks.
- `NotifyOnPercentageProgress` without a duration after an `FFMpeg.*` helper that already probed the input.
- `CancellableThrough(CancellationToken)` registers per run, so a processor can be run more than once.
- `FromImageSequenceInput` and `AddImageSequenceInput` for building a video from images through the argument builder.
- `WithMap`/`WithNegativeMap` take a `StreamType` in place of a stream index, so `-map 0:a` — every audio stream of an input — is
  expressible without probing first to count them.
- `FromUrlInput(string)` and `AddUrlInput(string)` alongside the `Uri` overloads, matching the pair `OutputToUrl` already had.
- `VideoCodec.Copy`, pairing with the `AudioCodec.Copy` that already existed.
- `FFProbe.Analyse`, `GetFrames` and `GetPackets` each accept a path, a `Uri` and a `Stream`, sync and async. Previously `GetPackets` took
  only a path and neither `GetFrames` nor `GetPackets` accepted a `Stream`.
- Five filters that had no method on the filter builders: `Fps`, `Tile`, `Speed` and `Fade` on `VideoFilterOptions`, and `Loudnorm`,
  `Speed` and `Fade` on `AudioFilterOptions`.
- `WithComplexFilter`, a typed builder for `-filter_complex`, with `WithMap(string label)` to select what a chain produced. Filters needing
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
