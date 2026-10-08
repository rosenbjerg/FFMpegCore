# Migrating from 5.x to 6.0

FFMpegCore 6.0 renames most option methods after the ffmpeg options they emit, splits input options from output options, and returns a
result object instead of a `bool`. The `FFMpeg.*` helpers no longer run on their own, and several were renamed or removed because their
names or restrictions did not match what they did. This document lists every breaking change and what to replace it with, and ends with
[what 6.0 adds](#new-in-60). See the [README](README.md) for the current API.

## Running ffmpeg

### A result instead of a `bool`

`ProcessSynchronously` and `ProcessAsynchronously` return an `FFMpegResult` instead of a `bool`. Existing `if (…ProcessSynchronously())`
checks become `if (….ProcessSynchronously().Success)`. The `ffMpegOptions` parameter is `ffOptions`, and is meant to be passed by name.

### The helpers return a processor

The `FFMpeg.*` helpers build the arguments and return the `FFMpegArgumentProcessor` without running it, so you choose how to run it — and
can attach progress or cancellation first. Their `…Async` variants are gone:

```csharp
// 5.x — ran immediately, returned bool
FFMpeg.Mute(inputPath, outputPath);
await FFMpeg.SubVideoAsync(inputPath, outputPath, start, end);

// 6.0 — returns a processor, which you then run
FFMpeg.RemoveAudio(inputPath, outputPath).ProcessSynchronously();
await FFMpeg.Trim(inputPath, outputPath, start, end).ProcessAsynchronously();
```

### Cancellation waits for ffmpeg to finalise the output

`CancellableThrough` sent ffmpeg `q` and then killed it after `timeout` milliseconds — and the default was `0`, so it killed straight away
and an mp4 was left without its index. It now waits up to five seconds for ffmpeg to finish before killing it, and takes the grace period
as a `TimeSpan`:

```csharp
.CancellableThrough(token, 10000)                    // 5.x
.CancellableThrough(token, TimeSpan.FromSeconds(10)) // 6.0
.CancellableThrough(token, TimeSpan.Zero)            // 6.0 — the old default
```

`ProcessSynchronously` and `ProcessAsynchronously` also take a `CancellationToken`, as their last parameter, for cancelling a single run
without attaching the token to the processor:

```csharp
await processor.CancellableThrough(token).ProcessAsynchronously(); // still works
await processor.ProcessAsynchronously(cancellationToken: token);   // 6.0
```

## Helpers

### `FFMpeg.Convert` is gone

It only ever supported `mp4`, `ogv`, `mpegts` and `webm`, hard-coded a 2400 kbps video bitrate, and silently ignored `speed`, `size`,
`audioQuality` and `multithreaded` on the `mpegts` path. `VideoSize.Hd` also *upscaled* anything smaller than 720p. Build the conversion with
`FFMpegArguments` instead, which is what the README already recommended for every other container:

```csharp
// 5.x
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
FFMpeg.Mute(inputPath, outputPath);                                // 5.x
FFMpeg.RemoveAudio(inputPath, outputPath).ProcessSynchronously();  // 6.0
```

### `FFMpeg.SubVideo` is `FFMpeg.Trim`, and honours the output path you give it

"Sub video" read as subtitles; it cuts a section out. More importantly, it rewrote the output's extension to match the input's, so asking
for `out.mkv` silently produced `out.mp4`. It now writes exactly the path you passed.

```csharp
FFMpeg.SubVideo(inputPath, "out.mkv", start, end);                       // 5.x — wrote out.mp4
FFMpeg.Trim(inputPath, "out.mkv", start, end).ProcessSynchronously();    // 6.0 — writes out.mkv
```

Because it still copies the streams, a container that cannot mux them now fails the run instead of being quietly swapped out. Pick a
container that can, or re-encode with `FFMpegArguments`. A stream-copied cut can only start on a keyframe, so it may begin a little before
the requested start.

It also keeps every stream now. Without a `-map`, ffmpeg kept one stream of each kind, so a second audio track or a second subtitle
language was dropped.

### `FFMpeg.ExtractAudio` takes any container, and can copy the stream

It required a `.mp3` output, although its arguments are just `-vn` and would have muxed `.m4a`, `.wav`, `.flac` or `.ogg` just as well. The
check is gone, and an optional `audioCodec` chooses the encoder:

```csharp
FFMpeg.ExtractAudio(inputPath, "track.mp3");                                          // 5.x — .mp3 only
FFMpeg.ExtractAudio(inputPath, "track.m4a", AudioCodec.Copy).ProcessSynchronously();  // 6.0 — no re-encode
```

### `FFMpeg.SaveM3U8Stream` is `FFMpeg.SaveStream`

Nothing about it was M3U8-specific — it opens a URL and copies the streams. It rejected any scheme but http(s), ruling out rtmp, rtsp and
srt, and required an `.mp4` output, which is the worst container for a recording that may be interrupted. Both checks are gone.

```csharp
FFMpeg.SaveM3U8Stream(uri, "out.mp4");                                                  // 5.x
FFMpeg.SaveStream(uri, "recording.ts").ProcessSynchronously();                          // 6.0
FFMpeg.SaveStream(new Uri("rtsp://camera/stream"), "cam.mkv").ProcessSynchronously();   // 6.0
```

### `FFMpeg.PosterWithAudio` copies the audio

It re-encoded the track to 128 kbps AAC unconditionally, which is the wrong default for turning a finished master into a video — the
platform it is being uploaded to will transcode it again. It now copies the stream. Optional parameters follow the output: `audioCodec` for
when you do want a re-encode, `addArguments` to choose the video encode in place of libx264 at CRF 21, and `ffOptions`.

```csharp
FFMpeg.PosterWithAudio(image, audio, output);                                        // 5.x — re-encoded to AAC
FFMpeg.PosterWithAudio(image, audio, output).ProcessSynchronously();                 // 6.0 — copies the audio
FFMpeg.PosterWithAudio(image, audio, output, AudioCodec.Aac).ProcessSynchronously(); // 6.0 — the old behaviour
```

It also no longer requires an `.mp4` output — `.mkv`, `.mov` and `.webm` (with a codec that container takes) work as well.

### `FFMpeg.ReplaceAudio` uses the new track, and copies it

It mapped no streams, so ffmpeg picked the "best" audio across both inputs — the one with the most channels, or on a tie the first input's.
A video that already had audio usually kept it, and the replacement was silently dropped. It now takes the video of the first input and the
audio of the second.

It also re-encoded the new track to 192 kbps AAC unconditionally. Like `PosterWithAudio`, it now copies it, and takes a codec when you do
want a re-encode. The new parameter sits before `stopAtShortest`, so a positional `bool` stops compiling — name it:

```csharp
FFMpeg.ReplaceAudio(input, audio, output, true);                          // 5.x
FFMpeg.ReplaceAudio(input, audio, output, stopAtShortest: true);          // 6.0 — copies the audio
FFMpeg.ReplaceAudio(input, audio, output, AudioCodec.Aac);                // 6.0 — the old behaviour, at ffmpeg's default bitrate
```

Its `inputAudio` parameter is `audio`, as in `PosterWithAudio`; only a call naming it changes.

### `FFMpeg.Join` is one encode, at the container's default quality

It converted every part to MPEG-TS in a temporary file — libx264 at a fixed 2400 kbps with the `superfast` preset, the same hard-coded
bitrate `Convert` was removed for — and then joined the parts byte-wise. It now joins them in a single ffmpeg run through the `concat`
filter, which requires the parts to share a resolution, and sets no encoder, so ffmpeg uses the output container's default at its default
quality: libx264 at CRF 23 and AAC for `.mp4`. Pass output options for anything else:

```csharp
FFMpeg.Join(output, parts, options => options
    .WithVideoCodec(VideoCodec.LibX264).WithVideoBitrate(2400).WithSpeedPreset(EncoderPreset.SuperFast)
    .WithAudioCodec(AudioCodec.Aac).WithAudioBitrate(AudioQuality.Normal));
```

To join parts that already share codecs without re-encoding them, use the new `FFMpeg.Concat`.

### The snapshot helpers dropped `inputFileIndex`

`FFMpeg.Snapshot`, `SnapshotArgumentBuilder.BuildSnapshotArguments` and `Snapshot`/`SnapshotAsync` in both image extension packages took an
`inputFileIndex` although the arguments they build have exactly one input, so any value but `0` produced a `-map` against an input that was
never added. Drop the argument; anything passed positionally after it moves up one place.

`SnapshotArgumentBuilder`'s methods also took the input path alongside the `IMediaAnalysis` that already carries it. They take the analysis
first and no path, as the helpers' new analysis overloads do:

```csharp
SnapshotArgumentBuilder.BuildSnapshotArguments(input, output, analysis, size);  // 5.x
SnapshotArgumentBuilder.BuildSnapshotArguments(analysis, output, size);         // 6.0
```

## Input and output options were split

`FFMpegArgumentOptions` is now `FFMpegInputOptions` and `FFMpegOutputOptions`, each carrying only the options ffmpeg accepts on that side.
Most code is unaffected, but an option used on the wrong side will no longer compile — in particular `WithVideoCodec` on an input is now
`WithVideoDecoder`, because `-c:v` before `-i` selects a decoder.

## Renamed option methods

| 5.x | 6.0 |
|---|---|
| `Seek(t)` / `EndSeek(t)` | `WithStartTime(t)` / `WithStopTime(t)` |
| `Loop(n)` | `WithLoop()` to repeat a still image (`-loop 1`; for an image input `-loop` is on or off, never a count), or `WithStreamLoop(n)` to play any input `n` more times (`-stream_loop`, `-1` for ever) |
| `WithFramerate(r)` | `WithFrameRate(r)` |
| `UsingShortest(b)` | `WithShortest()` — it takes no `bool`; leave the call out where you passed `false` |
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
| `WithGifPaletteArgument(…)` | removed — see [A GIF palette is a graph](#a-gif-palette-is-a-graph) |
| `Resize(w, h)` on an output | `WithVideoFilters(f => f.Scale(w, h))` |
| `Resize(w, h)` on an input | `WithFrameSize(w, h)` |
| `Crop(…)` | `WithVideoFilters(f => f.Crop(…))` |
| `Mirror(Mirroring.Horizontal)` | `WithVideoFilters(f => f.HorizontalFlip())` |
| `HardBurnSubtitle(…)` | `BurnSubtitles(…)` — see [below](#drawtext-pad-and-burnsubtitles-take-a-lambda) |
| `WithGlobalOptions(g => g.WithVerbosityLevel(v))` | `WithLogLevel(FFMpegLogLevel.…)` |
| `FromFileInput(paths)` / `AddFileInput(paths)` with several paths | `FromFileInputs(paths)` / `AddFileInputs(paths)` — see [below](#several-file-inputs) |
| `MultiOutput(…)` | `OutputToMany(…)` |
| `AddMetaData(…)` | `AddMetadata(FFMetadataBuilder)` or `AddMetadataFile(path)` — see [Metadata](#metadata) |
| `MapMetaData(i)` on `FFMpegArguments` | `WithMapMetadata(i)` on the output options — see [below](#-map_metadata-is-an-output-option) |
| `NotifyOnError(…)` | `NotifyOnStandardError(…)` — it receives every stderr line, which is all of ffmpeg's logging and progress, not just errors |
| `NotifyOnOutput(…)` | `NotifyOnStandardOutput(…)` |
| `FFMpegArguments.Text` | `….OutputTo…(…).Arguments` — the rendered command line is read from the processor, which is the only place it is complete |
| `WithVideoBitrate(bitrate: n)` / `WithAudioBitrate(bitrate: n)` | `WithVideoBitrate(kilobitsPerSecond: n)` — the unit was always kbps; only a named argument changes |

## Builder behaviour

### `-map_metadata` is an output option

`MapMetaData` sat on `FFMpegArguments` among the inputs, and `AddMetaData` emitted its `-map_metadata` straight after its own `-i`. ffmpeg
reads `-map_metadata` as an option of the *next* file, so any input added after either of them failed the run with "cannot be applied to
input url". The index was also computed by counting input arguments, so several paths given to one `FromFileInput` — one argument,
several `-i` — threw it off, as did `MapMetaData` itself.

`AddMetadata` now only adds the input, and maps it on every output that does not choose its own mapping. Choosing one is an output option:

```csharp
FFMpegArguments.FromFileInput(a).AddFileInput(b).MapMetaData(1).OutputToFile(output);                     // 5.x
FFMpegArguments.FromFileInput(a).AddFileInput(b).OutputToFile(output, o => o.WithMapMetadata(1));         // 6.0
```

`MapMetadataArgument` takes the index it maps, and is no longer an input argument. `MetadataArgument` emits only its `-i`.

### Several file inputs

`FromFileInput` and `AddFileInput` given several paths are `FromFileInputs` and `AddFileInputs`. The plural says what they do — add one `-i`
per path; to join the files, use `FromConcatDemuxerInput` or `FFMpeg.Concat`.

They also applied their options to the first file only: the options were written once, in front of the whole `-i a -i b` run, and ffmpeg
reads an input option as belonging to the next `-i`. They are now repeated before every file, so `opt => opt.WithStartTime(…)` seeks in
each of them.

`FromFileInput(FileInfo)` and `AddFileInput(FileInfo)` now verify that the file exists, like their `string` counterparts always have. Pass
`verifyExists: false` for the old behaviour.

### Relative paths follow `WorkingDirectory`

ffmpeg and ffprobe run in `FFOptions.WorkingDirectory`, so that is where they resolve a relative path. The checks the library makes before
starting them — `verifyExists`, `overwrite: false`, `FFProbe.Analyse`'s missing-file check — and the copies it makes of an image sequence
resolved the same path against the process's current directory instead, so with a `WorkingDirectory` set a relative input either failed a
check ffmpeg would have passed or passed one it would have failed. They now resolve against `WorkingDirectory` too; a path relative to the
current directory needs `Path.GetFullPath` when a `WorkingDirectory` is set.

### Metadata

`MetaDataBuilder`, `MetaData` and `IReadOnlyMetaData` are gone; `FFMetadataBuilder` does their job. It is constructed directly and produces
its document with `Build()`:

```csharp
FFMetadataBuilder.Empty().WithTag("title", "Intro").GetMetadataFileContent();   // 5.x
new FFMetadataBuilder().WithTag("title", "Intro").Build();                      // 6.0
```

`WithChapter(string, long)` meant milliseconds and `WithChapter(string, double)` meant seconds, so `WithChapter("Intro", 90)` bound to the
`long` overload and produced a 90 *millisecond* chapter. Nothing at the call site said which unit applied. Both are gone; pass a `TimeSpan`:

```csharp
builder.WithChapter("Intro", 90);                          // 5.x — 90 ms, probably not what was meant
builder.WithChapter("Intro", TimeSpan.FromSeconds(90));    // 6.0
```

`AddMetaData(string)` took the ffmetadata document's *content*, while every other string on the input builder is a path — so
`AddMetaData("chapters.txt")` wrote the text "chapters.txt" as the metadata. It is gone, so such calls stop compiling instead of changing
meaning. Pass generated metadata as an `FFMetadataBuilder`, and an ffmetadata file you already have to `AddMetadataFile(path)`, which is
mapped onto the outputs the same way.

### `DrawText`, `Pad` and `BurnSubtitles` take a lambda

They took an options object built with a static `Create`, unlike every other part of the builder. They now take their required values
directly and everything else through a lambda, and the options classes only offer `With*` methods. `HardBurnSubtitle` is also
`BurnSubtitles`, plural like `AddSubtitles`, with `SubtitleBurnOptions` and `SubtitleBurnArgument` in place of `SubtitleHardBurnOptions` and
`SubtitleHardBurnArgument`:

```csharp
// 5.x
.DrawText(DrawTextOptions.Create("Hello", "font.ttf", ("fontsize", "24")))
.Pad(PadOptions.Create("iw+20", "ih+20").WithParameter("color", "black"))
.Pad(PadOptions.Create("16/9"))
.HardBurnSubtitle(SubtitleHardBurnOptions.Create("subs.srt").SetOriginalSize(1920, 1080)
    .WithStyle(StyleOptions.Create().WithParameter("FontSize", "24")))

// 6.0
.DrawText("Hello", text => text.WithFontFile("font.ttf").WithParameter("fontsize", "24"))
.Pad("iw+20", "ih+20", pad => pad.WithParameter("color", "black"))
.Pad(configure: pad => pad.WithAspectRatio("16/9"))
.BurnSubtitles("subs.srt", subtitles => subtitles.WithOriginalSize(1920, 1080)
    .WithStyle(style => style.WithParameter("FontSize", "24")))
```

`DrawText` no longer requires a font file: ffmpeg falls back to its default font through fontconfig. `SetOriginalSize`, `SetSubtitleIndex`
and `SetCharacterEncoding` are `WithOriginalSize`, `WithSubtitleIndex` and `WithCharacterEncoding`. `new DrawTextArgument(…)`,
`new PadArgument(…)` and `new SubtitleBurnArgument(…)` take the same arguments as the filter methods, and a `Pad` with no width, height
or aspect ratio throws `ArgumentException` instead of a bare `Exception`.

### Filter parameters with a fixed set of values are typed

`SilenceDetect`'s `noiseType`, `HighPass`/`LowPass`'s `widthType`, `transform` and `precision`, and `AudioGate`'s `mode`, `detection` and
`link` take a small type per parameter — `SilenceDetectNoiseUnit`, `FilterWidthType`, `FilterTransform`, `FilterPrecision`,
`AudioGateMode`, `AudioGateDetection`, `AudioGateLink`, all in `FFMpegCore.Enums` — whose members list the values ffmpeg accepts. A plain
string still converts to each of them, so existing calls compile unchanged:

```csharp
.HighPass(200, widthType: FilterWidthType.Octave)
.HighPass(200, widthType: "o")   // still fine
```

Values the library does not know are passed on to ffmpeg as they are rather than rejected, so a newer ffmpeg's options stay reachable.
In particular `HighPass`/`LowPass` silently dropped a `transform` they did not recognise; it is now emitted. The exception is
`SilenceDetect`'s `noiseType`, which decides how the library formats the threshold and is not an ffmpeg value: anything but `db` and `ar`
still throws.

### Other filter changes

- `SilenceDetect`'s noise threshold defaulted to `60`, which rendered as `silencedetect=n=60.0dB` — a threshold 60 dB *above* full scale.
  Everything is below that, so the whole input came back as one silent stretch. It now defaults to ffmpeg's own -60 dB. Calls that passed
  a threshold are unaffected.
- `Scale(VideoSize.…)` rendered `scale=-1:720`, which keeps the aspect ratio exactly and so can produce an odd width — a 1080×1920
  portrait video becomes 405×720, which libx264 and most other encoders reject for `yuv420p`. It now renders `scale=-2:720`, rounding the
  computed width to an even number.
- `VideoFilterOptions.Arguments` and `AudioFilterOptions.Arguments` are read-only. `f.Arguments.Add(filter)` becomes `f.WithFilter(filter)`,
  and `f.WithCustomFilter(key, value)` adds a filter the library has no method for.
- `WithAudioFilters(f => f.AudioGate(…))` takes `ratio` and `makeup` as `double`, as ffmpeg does; integer calls compile unchanged.
- The audio filters wrote their numbers with a fixed one or two decimals, so `SilenceDetect(AmplitudeRatio, 0.001)` rendered `n=0.00` —
  detecting only digital silence — and defaults such as `HighPass`'s width of 0.707 and `AudioGate`'s range of 0.06125 reached ffmpeg as
  0.71 and 0.06. They now render the value given (`0.001`, `0.707`), dropping trailing zeros. `AudioGate` also accepts a `range` of 0, which
  ffmpeg allows and which turns the gate into an expander.

### A GIF palette is a graph

`WithGifPaletteArgument` wrote its own `-filter_complex`, which clashes with any other filter on the same output, and its `streamIndex`
was really an input index. It is gone. `FFMpeg.GifSnapshot` covers the common case; a pipeline of your own is a complex-filter graph:

```csharp
.OutputToFile("out.gif", options => options
    .WithComplexFilter(graph => graph
        .From(0, StreamType.Video).Video(f => f.Fps(12).Scale(320, -1)).WithCustomFilter("split").As("a", "b")
        .From("a").WithCustomFilter("palettegen").As("palette")
        .From("b").From("palette").WithCustomFilter("paletteuse").As("gif"))
    .WithMap("gif"))
```

### `OutputToTee` targets take tee options

Each target of `OutputToTee` took the full output options, but only a few of them mean anything to a tee target, and the rest rendered as
slave options ffmpeg rejects — `WithVideoCodec` became `[c:v=libx264]`. `WithMap` on a target rendered `select='0:v:0'`, which ffmpeg rejects
as an invalid stream specifier. Targets now take `TeeTargetOptions`: `ForceFormat`, `WithSelect(StreamType, streamIndex)` in place of
`WithMap`, `WithBitstreamFilter`, `WithFastStart`, `WithMovFlags`, `WithOnFail` and `WithMuxerOption(key, value)`. Encoder options go in the
tee's own options lambda, as before.

```csharp
.OutputToTee(t => t.OutputToUrl(url, o => o.ForceFormat("mpegts").SelectStream(0, 0, Channel.Video)))  // 5.x — rejected by ffmpeg
.OutputToTee(t => t.OutputToUrl(url, o => o.ForceFormat("mpegts").WithSelect(StreamType.Video, 0)))    // 6.0
```

A target's `OutputToFile(path, overwrite: false)` was overwritten anyway whenever another target allowed overwriting; it now throws
`IOException` like a plain `OutputToFile`.

## Renamed and removed types

| 5.x | 6.0 |
|---|---|
| `FFMpegArgumentOptions` | `FFMpegInputOptions`, `FFMpegOutputOptions` |
| `Channel` | `StreamType` — `Channel.Both` is gone, use `StreamType.All` |
| `Filter` | `BitstreamFilter` — a struct, adding `Hevc_Mp4ToAnnexB`, `Mpeg4_UnpackBFrames`, `ExtractExtradata` and `DumpExtra`; any other bitstream filter passes through as a string. `WithBitstreamFilter` takes every `StreamType`, not only audio and video: `All` renders a bare `-bsf`, `Subtitle` renders `-bsf:s` |
| `Filter.Aac_AdtstoAsc` | `BitstreamFilter.Aac_AdtsToAsc` |
| `Speed` | `EncoderPreset` — a struct like `EncoderTune`, so `"p4"` (NVENC) or `"8"` (SVT-AV1) pass through as strings. `Placebo` is new |
| `HardwareAccelerationDevice` (enum) | a struct with the same member names except `CUVID` (ffmpeg only accepts it as an alias of `cuda`; use `CUDA`) and `LibMFX` (not a `-hwaccel` value; Intel decoding is `QSV`), plus `VideoToolbox`, `Vulkan`, `D3D12VA`, `OpenCL` and `DRM`. Any other `-hwaccel` value passes through as a string. A `switch` over it no longer compiles — compare `Value` instead. `HardwareAccelerationArgument.HardwareAccelerationDevice` is the field `Device` |
| `Mirroring` | removed — use `HorizontalFlip()` / `VerticalFlip()` |
| `VideoType` | `ContainerFormats` — its members are `ContainerFormat` values, so the old name claimed a video type it never was |
| `VideoType.MpegTs` | `ContainerFormats.Ts` — they were the same value under two names |
| `VideoCodec.MpegTs` | removed — `mpegts` is a container, not a video codec; it emitted `-c:v mpegts`. Use `ForceFormat(ContainerFormats.Ts)` |
| `VideoCodec.LibaomAv1` | `VideoCodec.LibAomAv1`, cased like `LibX264` and the new `LibSvtAv1` |
| `AudioCodec.LibFdk_Aac` | `AudioCodec.LibFdkAac` |
| `Codec.Extension()` (the `FileExtension` extension method) | removed — it mapped eight codecs to a container extension and threw a bare `Exception` for anything else |
| `ContainerFormat.Extension` (property) | `ContainerFormat.GetExtension(FFOptions? = null)` — see [below](#containerformatgetextension) |
| `FileExtension.Image.All` (`List<string>`) | `IReadOnlyList<string>` — it was a writable global the snapshot helpers validate against |
| `MetaDataBuilder`, `MetaData`, `IReadOnlyMetaData` (namespace `FFMpegCore.Builders.MetaData`) | `FFMetadataBuilder` in `FFMpegCore` — see [Metadata](#metadata) |
| `FFMpegGlobalArguments`, `VerbosityLevel` | removed — use `WithLogLevel` or `FFOptions.LogLevel` |
| `FFOptionsException` | removed — `FFMpegException` |
| `FFMpegArgumentException` | removed — see [Errors](#errors) |
| `OverwriteExisting()`, `OverwriteArgument` | removed — `OutputToFile(path, overwrite: true)`, which is the default and already emits `-y` |
| `FromConcatInput` / `AddConcatInput` / `ConcatArgument` | `FromConcatProtocolInput` / `AddConcatProtocolInput` / `ConcatProtocolArgument` — see [below](#concat-inputs) |
| `FromDemuxConcatInput` / `AddDemuxConcatInput` / `DemuxConcatArgument` | `FromConcatDemuxerInput` / `AddConcatDemuxerInput` / `ConcatDemuxerArgument` |
| `new InputArgument(bool, string)` | `new InputArgument(string path, bool verifyExists)` — the two constructors differed only in argument order |
| `new MultiInputArgument(bool, IEnumerable<string>)` | `new MultiInputArgument(IEnumerable<string> paths, bool verifyExists)` — likewise |
| `MetaDataArgument` | `MetadataArgument`, matching `AddMetadata` and `MapMetadataArgument` |
| `FaststartArgument` | `MovFlagsArgument(MovFlags.FastStart)`; `WithFastStart()` is unchanged |
| `VariableBitRateArgument` | `VariableBitrateArgument` |
| `ID3V2VersionArgument` | `Id3v2VersionArgument` |
| `FrameRateArgument.Framerate` (`double`) | `FrameRateArgument.FrameRate` (`string`), so it can hold `30000/1001` |
| `ConstantRateFactorArgument.Crf` (`int`) | `double`, so it can hold x264's `18.5` |
| `IMediaAnalysis` implementations | must add `string? Path` — the input the analysis describes, or null when it came from a stream — and `string Json`, ffprobe's output |
| `IMediaAnalysis.VideoStreams`, `AudioStreams`, `SubtitleStreams`, `Chapters` (`List<T>`) | `IReadOnlyList<T>` — an analysis describes a file, and adding to it changed nothing but the helpers' view of that file |
| `VideoStream.AvgFrameRate` | `AverageFrameRate`, ffprobe's `avg_frame_rate` |
| `VideoStream.AverageFrameRate` | now populated — it was never filled in and always read `0`; it now holds what `AvgFrameRate` held |
| `VideoStream.FrameRate` | `RealFrameRate` — it holds ffprobe's `r_frame_rate`, the lowest rate that represents every timestamp, which for variable-frame-rate video (most phone recordings) can be far above the rate the video plays at. `AverageFrameRate` is usually the one you want |
| `MediaFormat.BitRate` (`double`) | `long`, matching `MediaStream.BitRate` |
| `FFMpegImage` in both image extension packages | `SystemDrawingImage` and `SkiaSharpImage`, so both can be referenced at once |
| `BitmapVideoFrameWrapper` in both image extension packages | `SystemDrawingVideoFrame` and `SkiaSharpVideoFrame` |
| `BitmapExtensions` in both image extension packages | `SystemDrawingImageExtensions` and `SkiaSharpBitmapExtensions` |
| namespace `FFMpegCore.Extend` | removed — `PcmAudioSampleWrapper` moved to `FFMpegCore.Pipes`, `TimeSpanExtensions` is internal, and `UriExtensions` is gone |
| `FFMpegHelper`, `FFProbeHelper` | internal — they were the library's own checks |

### Concat inputs

The two concat input pairs are different ffmpeg mechanisms, and the old names did not say which was which. `concat:` is a *protocol* that
joins the files byte-wise, and works only for formats that survive naive concatenation such as mpegts and mp3; the concat *demuxer* writes
a list file and joins stream-wise, and is the one that works for mp4 and mkv. Both were renamed rather than just the confusing one —
leaving `FromConcatInput` in place with either meaning would let existing code keep compiling while doing something different. For joining
video files, prefer `FFMpeg.Concat`, which uses the demuxer.

### `ContainerFormat.GetExtension`

`ContainerFormat.Extension` read `ExtensionOverrides` off `GlobalFFOptions.Current`, so it ignored the per-run `FFOptions` that 6.0 threads
through everything else. It is now a method taking them, named `GetExtension` to match `MediaStream.GetCodecInfo` and
`VideoStream.GetPixelFormatInfo`. The rename is deliberate: had the property become a method of the same name, `$"out{format.Extension}"`
would have kept compiling and silently interpolated the method group, producing filenames like `out<>f__AnonymousDelegate0\`2[...]`.
`GetExtension` fails to compile instead.

`FileExtension.Mp4`/`.Ts`/`.Ogv`/`.WebM` are now plain constants. They previously read the global options once at static-initialisation time,
so a later `GlobalFFOptions.Configure` never reached them.

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

`RawAudioPipeSource` takes the sample rate and channel count in its constructor. They defaulted to 8000 Hz mono, so a source of 48 kHz
stereo samples that forgot to say so played at the wrong speed and pitch without any error. Both are `int` now, like
`WithAudioSamplingRate` and `WithAudioChannels`, and read-only:

```csharp
new RawAudioPipeSource(samples) { SampleRate = 48000, Channels = 2 }  // 5.x
new RawAudioPipeSource(samples, sampleRate: 48000, channels: 2)       // 6.0
```

`RawVideoPipeSource.StreamFormat` is `PixelFormat`, since it holds the frames' `-pix_fmt`, not a container format. For the same reason
`IVideoFrame.Format` is `IVideoFrame.PixelFormat`, on `SystemDrawingVideoFrame` and `SkiaSharpVideoFrame` too; a custom frame type renames
its property.

## Errors

A failed ffmpeg run throws `FFMpegProcessException`, which derives from `FFMpegException` and carries the run's `FFMpegResult` as `Result`
— so `ExitCode` and `StandardError` are there whether you let the run throw or pass `throwOnError: false`. `catch (FFMpegException)` still
catches it.

The captured stderr had three names across three types. It is now `StandardError`, an `IReadOnlyList<string>`, everywhere — the name
`NotifyOnStandardError` uses, because it holds all of ffmpeg's logging and progress, not just its errors:

| 5.x | 6.0 |
|---|---|
| `FFMpegException.FFMpegErrorOutput` (`string`) | `FFMpegProcessException.Result.StandardError`; removed from `FFMpegException` along with the constructors taking it |
| `FFProbeProcessException.ProcessErrors` (`IReadOnlyCollection<string>`) | `FFProbeProcessException.StandardError` (`IReadOnlyList<string>`), alongside a new `ExitCode`. Its constructor takes `(exitCode, standardError)` |
| `IMediaAnalysis.ErrorData` | `IMediaAnalysis.StandardError` |

Calling a builder method wrongly throws `ArgumentException` or `ArgumentOutOfRangeException`, as .NET APIs do, instead of one of three
types. `FFMpegArgumentException` is gone — it derived from neither `FFMpegException` nor `ArgumentException`, so neither `catch` caught it:

| Mistake | 5.x | 6.0 |
|---|---|---|
| A `Codec` of the wrong type for `WithVideoCodec`/`WithAudioCodec`/`WithSubtitleCodec` | `FFMpegException` | `ArgumentException` |
| A `StreamType` that `-vn`/`-an`/… has no spelling for | `FFMpegException` | `ArgumentOutOfRangeException` |
| `WithVideoFilters`/`WithAudioFilters` that add nothing | `FFMpegArgumentException`, when the command line was rendered | `ArgumentException`, from the `With…Filters` call itself |

`OutputToFile(path, overwrite: false)` onto an existing file throws `IOException` instead of `FFMpegException`, naming the file. A missing
input already threw `FileNotFoundException`; the two now match.

## What an analysis reports

- `PrimaryVideoStream`, `PrimaryAudioStream` and `PrimarySubtitleStream` are the stream of their kind marked `default`, and the lowest
  index only when none is. They used to be the lowest index regardless, so a file whose default audio track is its second now reports that
  one — and the `FFMpeg.*` helpers, which take the primary stream unless given a `streamIndex`, use it too.
- `Duration` is the longest of the container's duration and every stream's, where it took only the primary video and audio streams
  alongside the container's.

## FFProbe's async overloads take the token last

`CancellationToken` sat before `customArguments`; it is now the final parameter, so a positionally-passed token has to be named:

```csharp
await FFProbe.AnalyseAsync(path, ffOptions, cancellationToken);                    // 5.x
await FFProbe.AnalyseAsync(path, ffOptions, cancellationToken: cancellationToken); // 6.0
```

This applies to `AnalyseAsync`, `GetFramesAsync` and `GetPacketsAsync` alike.

## Extension packages

`Snapshot` and `SnapshotAsync` in both image extension packages take the run's `FFOptions`, placed before `cancellationToken` to match
`FFProbe.AnalyseAsync`, so a token passed positionally has to move along:

```csharp
await FFMpegImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, inputFileIndex, cancellationToken);  // 5.x
await SystemDrawingImage.SnapshotAsync(inputPath, size, captureTime, streamIndex, ffOptions, cancellationToken); // 6.0
```

`AddAudio` returns the `FFMpegArgumentProcessor` instead of running, like the `FFMpeg.*` helpers, so progress, cancellation and the choice
of sync or async are the caller's. Like `PosterWithAudio` it copies the audio, and takes optional `audioCodec`, `addArguments` and
`ffOptions`; an overload takes the audio's `IMediaAnalysis`. The bitmap is written to a temporary file when the run starts rather than when
`AddAudio` is called, so keep it alive until the run ends:

```csharp
bitmap.AddAudio(audio, output);                                                          // 5.x — ran immediately
bitmap.AddAudio(audio, output).ProcessSynchronously();                                   // 6.0
await bitmap.AddAudio(audio, output).ProcessAsynchronously(cancellationToken: token);    // 6.0
```

`FFMpegDownloader.DownloadBinaries` is `DownloadBinariesAsync` and takes a `CancellationToken`; its `options` parameter is `ffOptions`, as
everywhere else. Without a `BinaryFolder` it throws before going online, with a message saying how to set one. `FFMpegDownloaderException`
derives from `FFMpegException`, and `EnumExtensions` is no longer part of the package's public surface.

## New in 6.0

### Helpers

- Six helpers for operations that previously had no route through the API:

  | Helper | What it does |
  |---|---|
  | `Remux` | Changes container, copying the streams — no re-encode. |
  | `Concat` | Joins files through the concat demuxer, copying the streams. `Join` re-encodes and stays for inputs that differ. |
  | `ThumbnailSheet` | Samples frames at an interval and tiles them into a contact sheet or scrub-preview strip. |
  | `Watermark` | Overlays an image at a corner, keeping the audio. |
  | `AddSubtitles` | Muxes a subtitle file in as its own switchable stream, rather than burning it into the picture. |
  | `ExtractSubtitles` | Writes a subtitle stream back out to a file. |

- Every helper that probes its input also takes an `IMediaAnalysis` in place of the input path, so the probe can be done by the caller
  with `FFProbe.AnalyseAsync` and the whole operation kept asynchronous. `Concat`, `Join` and `JoinImageSequence` take a sequence of them,
  which also lets the probes run concurrently rather than one per input in a loop.
- Every helper takes an optional `FFOptions` covering both the ffprobe call it makes while building the arguments and the run itself, so
  `BinaryFolder` no longer has to be global for a helper to find ffprobe. `Join` and `JoinImageSequence` end in `params`, so they take
  theirs as a leading argument on a separate overload.
- The helpers that re-encode — `Join`, `Watermark`, `PosterWithAudio`, `JoinImageSequence` (on an overload taking an `IEnumerable`) and the
  image extensions' `AddAudio` — take output options. They replace the helper's own encode choices (libx264 at CRF 21, yuv420p, copying the
  audio) and keep what the helper needs to work.
- `AddSubtitles`, `ExtractAudio`, `PosterWithAudio` and `ReplaceAudio` take an encoder name as a `string` as well as a `Codec`, for the
  encoders that have no constant. The `Codec` overload stays the one to prefer, since it checks the codec is of the right kind at the call.
  A bare `null` for the codec is ambiguous between the two; omit it, or name the parameters after it.
- `PosterWithAudio` has an overload taking the image as an `IInputArgument`, as the image extensions' `AddAudio` uses.

### Running and progress

- `IProgress<TimeSpan>` and `IProgress<double>` overloads alongside the existing callbacks. Percentages run from 0 to 100.
- `NotifyOnPercentageProgress` without a duration after any `FFMpeg.*` helper except `SaveStream`.
- `CancellableThrough(CancellationToken)` registers per run, so a processor can be run more than once.
- Per-run `FFOptions` reach every argument, so `TemporaryFilesFolder` applies to the temp files that concat, metadata and image-sequence
  inputs create.

### Inputs and outputs

- `FromInput(IInputArgument)` and `AddInput(IInputArgument)`, for inputs the library has no method for.
- `FromUrlInput(string)` and `AddUrlInput(string)` alongside the `Uri` overloads, matching the pair `OutputToUrl` already had.
- `FromImageSequenceInput` and `AddImageSequenceInput` for building a video from images through the argument builder.
- `FromFileInput`, `AddFileInput` and `OutputToFile` take the options lambda straight after the path, so
  `OutputToFile(path, true, options => …)` can be written `OutputToFile(path, options => …)`. The overloads with the `bool` stay, for
  `verifyExists: false` and `overwrite: false`.
- `OutputToNull()` for analysis-only runs (`-f null -`), so `SilenceDetect` and `BlackDetect` no longer need a dummy output path.
- `AddMetadataFile(path)` for an ffmetadata file you already have.

### Options

- `WithMap`/`WithNegativeMap` take a `StreamType`, so `-map 0:a` — every audio stream of an input — is expressible without probing first to
  count them.
- An optional `streamIndex` on `WithVideoCodec`, `WithAudioCodec`, `WithSubtitleCodec`, `WithVideoBitrate`, `WithAudioBitrate` and
  `CopyStreams`, for settings that apply to one stream: `CopyStreams(StreamType.Audio, 0).WithAudioCodec(AudioCodec.Aac, 1)` renders
  `-c:a:0 copy -c:a:1 aac`. It counts within the stream type, as ffmpeg's specifier does.
- Encoder tuning options that needed `WithCustomArgument`: `WithAudioChannels` (`-ac`, on inputs too), `WithVideoProfile` (`-profile:v`),
  `WithTune` (`-tune`), `WithGopSize` (`-g`), `WithMaxBitrate` and `WithBufferSize` (`-maxrate`/`-bufsize`), and
  `WithVideoQualityScale`/`WithAudioQualityScale` (`-q:v`/`-q:a`). `VideoProfile` and `EncoderTune` list the common values.
- Fractional values where ffmpeg takes them: `WithConstantRateFactor(double)` (x264's `-crf 18.5`) and `WithFrameRate(string)` for exact
  rates such as `30000/1001`. Existing calls compile unchanged.
- `WithStreamLoop(count)` for `-stream_loop`.
- `WithTag` for `-tag`, the codec tag a stream is written with — `WithTag("hvc1", StreamType.Video)` is what makes HEVC in an mp4 play in
  Apple's players.
- `WithFpsMode` for `-fps_mode` (the replacement for `-vsync`), with `FpsMode.ConstantFrameRate` for the constant-frame-rate output editing
  software expects.
- `WithMovFlags` for any `-movflags`, such as fragmented MP4 (`MovFlags.FragmentKeyframe + MovFlags.EmptyMoov`). Calls combine into one
  option, with `WithFastStart` too, since ffmpeg keeps only the last `-movflags`.
- `WithMetadata`, `WithStreamMetadata` and `WithDisposition` for `-metadata`, `-metadata:s` and `-disposition`. `StreamDisposition` lists
  the dispositions and combines them with `+`.
- `WithMapChapters(inputFileIndex)` and `WithoutChapters()` for `-map_chapters`, alongside `WithMapMetadata` and `WithoutMetadata`.

### Filters

- `WithComplexFilter`, a typed builder for `-filter_complex`, with `WithMap(string label)` to select what a chain produced. A chain's
  `Video(f => …)` and `Audio(f => …)` take the same builders as `WithVideoFilters` and `WithAudioFilters`. Filters needing more than one
  input — `Concat`, `Overlay`, `AudioMix` — are reachable without hand-writing the graph into `WithCustomArgument`; `Concat` and `AudioMix`
  count the chain's inputs themselves unless told otherwise.
- `WithFilter` and `WithCustomFilter(key, value)` on the `-vf` and `-af` builders, so a filter the library has no method for joins the
  chain instead of forcing the whole chain into `WithCustomArgument`.
- Filters that had no method: `Fps`, `Tile`, `Speed` and `Fade` for video, and `LoudnessNormalizer` (`loudnorm`), `VolumeDetect`, `Speed`
  and `Fade` for audio.

### Codecs and formats

- `VideoCodec.Copy`, pairing with the `AudioCodec.Copy` that already existed.
- `VideoCodec.LibVpxVp9` and `AudioCodec.LibOpus` (the WebM pair), `VideoCodec.LibSvtAv1`, and `AudioCodec.Flac`, `Alac` and `PcmS16Le`.
- A `SubtitleCodec` constants class — `MovText`, `Srt`, `Ass`, `WebVtt`, `Copy` — alongside the `VideoCodec` and `AudioCodec` ones, so
  naming a subtitle encoder no longer means `FFMpeg.GetCodec("mov_text")` and the `ffmpeg -codecs` run behind it.
- `ContainerFormats.Matroska`, `Flv`, `Mp3`, `Wav`, `Flac`, `Hls`, `Image2`, `RawVideo` and `Null`. `FFOptions.ExtensionOverrides` maps
  `matroska` to `.mkv` and `hls` to `.m3u8` by default, alongside `mpegts` to `.ts`.
- `VideoCodec.*`, `AudioCodec.*` and `ContainerFormats.*` are built from their known name and type instead of being looked up through
  `ffmpeg -codecs`. They no longer spawn a process, and no longer throw when the build lacks the codec — ffmpeg reports that itself when
  the run starts. `Description`, `EncodingSupported` and the other fields the listing fills in are empty on them; call
  `FFMpeg.GetCodec(name)` for a populated `Codec`.
- The `FFMpeg.GetCodecs` / `GetPixelFormats` / `GetContainerFormats` family and their `TryGet*` counterparts take an optional `FFOptions`,
  and their cache is keyed on the binary that answered. `MediaStream.GetCodecInfo` and `VideoStream.GetPixelFormatInfo` take one too, so
  a stream can be described by the same ffmpeg that is going to process it.

### Probing, errors and binaries

- `FFProbe.Analyse`, `GetFrames` and `GetPackets` each accept a path, a `Uri` and a `Stream`, sync and async. `GetPackets` took only a path,
  and neither `GetFrames` nor `GetPackets` accepted a `Stream`.
- `IMediaAnalysis.Json` holds ffprobe's output, and `FFProbe.FromJson` builds an analysis from it, so a probe result can be stored or sent
  and read back without ffprobe.
- A stream's `Duration` falls back to its `DURATION` tag, which is where Matroska and WebM keep it; such streams reported zero. The
  `TagExtensions` accessors (`GetLanguage`, `GetDuration`, …) match a tag's name in any case, as `MediaStream.Tags` already did.
- `FFProbeException` and `FFProbeProcessException` derive from `FFMpegException`, so one `catch` covers both tools.
- A missing ffmpeg or ffprobe raises `FFMpegException` or `FFProbeException` naming the path that was tried, where `Instances`'
  `InstanceFileNotFoundException` used to escape.
- The image extension packages honour a per-run `FFOptions`, so `BinaryFolder` and `TemporaryFilesFolder` apply to snapshots and to the
  poster `AddAudio` writes.
- `FFMpegDownloader` works on Apple Silicon, where it previously threw `PlatformNotSupportedException`.
