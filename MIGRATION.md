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
| `VideoStream.AverageFrameRate` | removed — it was never populated and always read `0`; use `AvgFrameRate` (ffprobe's `avg_frame_rate`) or `FrameRate` (`r_frame_rate`) |
| `new InputArgument(bool, string)` | `new InputArgument(string path, bool verifyExists)` — the two constructors differed only in argument order |
| `new MultiInputArgument(bool, IEnumerable<string>)` | `new MultiInputArgument(IEnumerable<string> paths, bool verifyExists)` — likewise |
| `VideoCodec.MpegTs` | removed — `mpegts` is a container, not a video codec; it emitted `-c:v mpegts`. Use `ForceFormat(VideoType.Ts)` |
| `VideoType.MpegTs` | `VideoType.Ts` — they were the same value under two names |
| `Codec.Extension()` (the `FileExtension` extension method) | removed — it mapped eight codecs to a container extension and threw a bare `Exception` for anything else |

`FromFileInput(FileInfo)` and `AddFileInput(FileInfo)` now verify that the file exists, like their `string` counterparts always have. Pass
`verifyExists: false` for the old behaviour.
| namespace `FFMpegCore.Extend` | removed — its types moved to `FFMpegCore`, `FFMpegCore.Helpers` and `FFMpegCore.Pipes` |

`FFMetadataBuilder` is constructed directly (`new FFMetadataBuilder()`) and produces its document with `Build()`.

## Extension packages

`Snapshot` and `SnapshotAsync` in both image extension packages take the run's `FFOptions`, placed before `cancellationToken` to match
`FFProbe.AnalyseAsync`, so a token passed positionally has to move along:

```csharp
// 5.x
await FFMpegImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, inputFileIndex, cancellationToken);

// 6.0
await SystemDrawingImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, inputFileIndex, ffOptions, cancellationToken);
```

`AddAudio` takes the same `FFOptions`, and `AddAudioAsync` adds cancellation.

`FFMpegDownloader.DownloadBinaries` is `DownloadBinariesAsync` and takes a `CancellationToken`. `FFMpegDownloaderException` derives from
`FFMpegException`, and `EnumExtensions` is no longer part of the package's public surface.

## New in 6.0

- `IProgress<TimeSpan>` and `IProgress<double>` overloads alongside the existing callbacks.
- `NotifyOnPercentageProgress` without a duration after an `FFMpeg.*` helper that already probed the input.
- `CancellableThrough(CancellationToken)` registers per run, so a processor can be run more than once.
- `FromImageSequenceInput` and `AddImageSequenceInput` for building a video from images through the argument builder.
- `WithMap`/`WithNegativeMap` take a `StreamType` in place of a stream index, so `-map 0:a` — every audio stream of an input — is
  expressible without probing first to count them.
- `FromUrlInput(string)` and `AddUrlInput(string)` alongside the `Uri` overloads, matching the pair `OutputToUrl` already had.
- `VideoCodec.Copy`, pairing with the `AudioCodec.Copy` that already existed.
- Per-run `FFOptions` now reach every argument, so `TemporaryFilesFolder` applies to the temp files that concat, metadata and image-sequence
  inputs create.
- `FFProbeException` and `FFProbeProcessException` derive from `FFMpegException`, so one `catch` covers both tools.
- The image extension packages honour a per-run `FFOptions`, so `BinaryFolder` and `TemporaryFilesFolder` apply to snapshots and to the
  poster `AddAudio` writes.
- `FFMpegDownloader` works on Apple Silicon, where it previously threw `PlatformNotSupportedException`.
- A missing ffmpeg or ffprobe raises `FFMpegException` or `FFProbeException` naming the path that was tried, where `Instances`'
  `InstanceFileNotFoundException` used to escape.
- `VideoCodec.*`, `AudioCodec.*` and `VideoType.*` are built from their known name and type instead of being looked up through
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
