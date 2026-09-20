# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A .NET Standard 2.0 wrapper around the `ffmpeg` and `ffprobe` CLIs, published to NuGet as `FFMpegCore` plus three optional extension packages. It shells out to real binaries — nothing is linked natively — so every integration test needs `ffmpeg`/`ffprobe` on `PATH`.

## Commands

```bash
dotnet test FFMpegCore.sln                                              # all tests (needs ffmpeg on PATH)
dotnet test FFMpegCore.sln --filter "FullyQualifiedName=FFMpegCore.Test.VideoTest.Video_ToMP4"  # one test (= is exact; ~ is substring)
dotnet test FFMpegCore.sln --filter "FullyQualifiedName~ArgumentBuilderTest"     # one class
dotnet format FFMpegCore.sln --severity warn --verify-no-changes        # lint, as CI runs it (drop --verify-no-changes to fix)
dotnet pack FFMpegCore.sln -c Release                                   # packages land in nupkg/
```

`TreatWarningsAsErrors` is on solution-wide, which turns NuGet's vulnerability-audit warning (NU1900) into a restore failure when the audit can't reach nuget.org. If restore fails with NU1900, append `-p:NuGetAudit=false`.

`.github/workflows/ci.yml` is the only workflow and has three jobs. `ci` runs on PRs to `main` and on pushes to `main`: the test matrix across Windows/Ubuntu/macOS with ffmpeg 8.1 (the version string is set per OS because Linux/Windows pull from BtbN's builds and macOS from osxexperts.net); lint runs on Ubuntu only. On pushes to `main`, `version` evaluates every csproj with `IsPackable=true` and asks nuget.org's flat-container index whether its `PackageVersion` already exists; `release` then fans out over the ones that don't and, per package, packs, pushes and creates a GitHub release. Bumping `PackageVersion` on `main` is therefore the release trigger — there is no release branch, tag push or button, and a push where every version is already on nuget.org is a no-op. `release` is gated on the full test matrix and is idempotent (`--skip-duplicate`; re-run it on failure). Tags are `vX.Y.Z` for `FFMpegCore` and `<PackageId>/vX.Y.Z` for the extensions; only `FFMpegCore` releases are marked "latest". Release notes are GitHub's auto-generated notes (merged PRs) between the package's previous tag and the release commit; the same text is written to the nupkg via `PackageReleaseNotesFile` (see `Directory.Build.props`), so don't set `PackageReleaseNotes` in a csproj — it would override the generated notes. Publishing uses trusted publishing: `NuGet/login` exchanges the job's GitHub OIDC token for a short-lived API key, so there is no NuGet secret in the repo. The matching policy lives on nuget.org under the `rosenbjergsoftworks` account (owner `rosenbjerg`, repo `FFMpegCore`, workflow `ci.yml`, no environment) — renaming the workflow file or the repo requires updating that policy.

## Solution layout

| Project | Purpose |
|---|---|
| `FFMpegCore` | Core library (netstandard2.0). Depends only on `Instances` (process wrapper) and `System.Text.Json`. |
| `FFMpegCore.Extensions.SkiaSharp` / `.System.Drawing.Common` | In-memory `Snapshot` → bitmap, and `BitmapVideoFrameWrapper` for piping frames in. Separate packages because the core must not depend on an image library. |
| `FFMpegCore.Extensions.Downloader` | Downloads ffmpeg binaries from the ffbinaries API at runtime. |
| `FFMpegCore.Test` | MSTest, net8.0. |
| `FFMpegCore.Examples` | Console sample; not packed. |

`Directory.Build.props` sets the shared defaults (netstandard2.0, nullable, implicit usings, warnings-as-errors). Test and Examples override to net8.0; the test project disables nullable.

Each packable csproj carries its own `PackageVersion`; there is no central version file. Bumping it and merging to `main` is what releases that package (see CI above). Don't add `GeneratePackageOnBuild` — it makes `dotnet pack` skip the build and fail with NU5026 on a clean checkout.

## Core architecture

### The fluent argument pipeline

```
FFMpegArguments.From*Input(...)      // adds input(s); each is an IInputArgument
    .AddFileInput / .AddPipeInput    // more inputs
    .OutputToFile / .OutputToPipe    // adds the IOutputArgument, returns FFMpegArgumentProcessor
    .NotifyOnProgress / .CancellableThrough / .Configure
    .ProcessSynchronously() / .ProcessAsynchronously()
```

- `FFMpegArgumentsBase` holds a flat `List<IArgument>`. `FFMpegArguments.Text` joins every argument's `Text` in insertion order. Options passed via the `addArguments` lambda are appended **before** the input/output they belong to, so `-ss` lands before `-i` and `-c:v` lands before the output path. Order is significant to ffmpeg; don't reorder the list.
- Every ffmpeg flag is its own class in `FFMpegCore/FFMpeg/Arguments/` implementing `IArgument` (`Text` property), exposed through a `With*` method on `FFMpegArgumentOptions`. Adding a flag means: new `XxxArgument`, new `WithXxx` on `FFMpegArgumentOptions`, and an exact-string assertion in `ArgumentBuilderTest`.
- Filters are a second tier: `Arguments/Filters/` holds `IVideoFilterArgument`/`IAudioFilterArgument` implementations (`ScaleArgument`, `PadArgument`, `AudioGateArgument`, …) that only ever appear inside the single `-vf`/`-af` string built by `VideoFiltersArgument`/`AudioFiltersArgument`. Adding a filter means: new class in `Filters/`, new method on `VideoFilterOptions`/`AudioFilterOptions`.
- `IDynamicArgument.GetText(context)` receives all preceding arguments; used when an argument needs to compute its own input index (e.g. `MetaDataArgument` → `-map_metadata N`).
- `IInputOutputArgument` adds a lifecycle: `Pre(options)` runs with the run's resolved `FFOptions` before the command line is built, `During(token)` runs concurrently with the process, `Post()` runs once both the process and `During` are finished. `FFMpegArgumentProcessor.Process` drives this. `PipeArgument` (named-pipe server for `IPipeSource`/`IPipeSink`) and the temp-file arguments (`MetaDataArgument`, `DemuxConcatArgument`, `ImageSequenceInputArgument`) rely on it; the latter place their files under the run's `TemporaryFilesFolder` and fall back to `GlobalFFOptions.Current` only when `Text` is inspected without a run.
- `FFMpegGlobalArguments` (via `WithGlobalOptions`) are prepended before all inputs. `-v <loglevel>` is appended by the processor from `FFOptions.LogLevel` / `WithLogLevel`.
- `ProcessSynchronously`/`ProcessAsynchronously` return an `FFMpegResult` (`ExitCode`, `ErrorOutput` = stderr lines, `Cancelled`, `Success`). With the default `throwOnError: true` a non-zero exit throws `FFMpegException` and a cancel throws `OperationCanceledException`; with `false` the result carries what happened instead.
- Cancellation: `CancellableThrough` sends `q` to ffmpeg's stdin, then kills after `timeout` ms.
- `FFMpegMultiOutputOptions` / `OutputToTee` support multiple outputs from one input.

### FFProbe

`FFProbe.Analyse*` runs `ffprobe -print_format json -sexagesimal -show_format -show_streams -show_chapters`, deserialises to `FFProbeAnalysis` (raw JSON shape), then wraps it in `MediaAnalysis : IMediaAnalysis` which parses durations, rotation, frame rates, tags, etc. `GetFrames`/`GetPackets` follow the same pattern. Stream input is analysed by piping through the same `InputPipeArgument` machinery.

ffprobe failures throw `FFProbeException` (missing input) or `FFProbeProcessException` (non-zero exit, with `ProcessErrors` = stderr lines); both derive from `FFMpegException`, so a `catch (FFMpegException)` still covers them.

### Options and binary lookup

- `FFOptions` is per-run; `GlobalFFOptions.Current` is the process-wide default, lazily loaded from `ffmpeg.config.json` in the working directory if present. `FFMpegArgumentProcessor.GetConfiguredOptions` clones the global, then applies `.Configure(...)` lambdas.
- `BinaryFolder` empty ⇒ rely on `PATH`. Otherwise `{BinaryFolder}/{x64|x86}/ffmpeg[.exe]` is tried first, then `{BinaryFolder}/ffmpeg[.exe]`.
- `FFMpegHelper.VerifyFFMpegExists` runs `ffmpeg -version` once per process and caches the result.
- `FFMpegCache` lazily caches codec / pixel-format / container lists from `ffmpeg -codecs` etc. (`FFOptions.UseCache`).
- Those short informational runs (`-version`, `-formats`, `-codecs`, `-pix_fmts`) go through `Helpers/ProcessHelper`, not Instances — it reads the output on the calling thread so a caller blocked on it (under `FFMpegCache`'s lock) never depends on free thread-pool threads. Add any new listing query there too; see the comment in the helper and issue #580.

### High-level helpers

`FFMpeg` (static) — `Snapshot`, `GifSnapshot`, `Join`, `SubVideo`, `Mute`, `ExtractAudio`, `ReplaceAudio`, `JoinImageSequence`, `PosterWithAudio`, `SaveM3U8Stream`, `Convert` — are thin compositions of the builder that return the `FFMpegArgumentProcessor` without running it; the caller picks `ProcessSynchronously`/`ProcessAsynchronously`. Anything a helper needs before the run (ffprobe for sizes and durations, extension checks) happens synchronously inside the helper; anything that must wrap the run lives in an argument's `Pre`/`Post` (`ImageSequenceInputArgument` copies the images into a temp folder and deletes it afterwards). `Join` is a single ffmpeg invocation through the `concat` filter, so its inputs must share a resolution. `SnapshotArgumentBuilder` is public so the image extensions can reuse the same argument construction and just swap the output for a `StreamPipeSink` + `ForceFormat("rawvideo")`.

`FFMetadataBuilder` (project root) generates the ffmetadata text used by `AddMetaData`; its chapters are `ChapterData`, the same type `IMediaAnalysis.Chapters` returns, so probed chapters can be fed straight back in.

## Tests

- MSTest with `[Parallelize(Scope = ExecutionScope.MethodLevel)]` — tests run concurrently. Always write outputs to `new TemporaryFile("out.mp4")` (GUID-prefixed in the temp dir, deleted on dispose) and read inputs from `TestResources.*` (`Resources/` is copied to the output dir).
- Long-running tests use `[Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]` and chain `.CancellableThrough(TestContext.CancellationToken)` so a timeout actually stops ffmpeg.
- `[OsSpecificTestMethod(OsPlatforms.Windows | OsPlatforms.Linux)]` marks tests inconclusive on other platforms — `System.Drawing.Common` tests are Windows-only; the downloader tests skip macOS.
- `ArgumentBuilderTest` asserts exact argument strings via `.Arguments` without launching ffmpeg — prefer this for argument changes; it's fast and platform-independent.
- `DownloaderTests` hit the network (ffbinaries).
- `FFMpegCore` exposes internals to the test project (`InternalsVisibleTo`).

## Style

`.editorconfig` rules are enforced by `dotnet format` in CI: 4-space indent, UTF-8 BOM on `.cs`, `_camelCase` private/static fields, PascalCase constants, `var` everywhere (error severity). By convention the code also uses file-scoped namespaces, and argument classes expose their values as `public readonly` fields, not properties.
