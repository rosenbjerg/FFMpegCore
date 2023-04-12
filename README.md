# [FFMpegCore](https://www.nuget.org/packages/FFMpegCore/) 
[![NuGet Badge](https://buildstats.info/nuget/FFMpegCore)](https://www.nuget.org/packages/FFMpegCore/)
[![GitHub issues](https://img.shields.io/github/issues/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/issues)
[![GitHub stars](https://img.shields.io/github/stars/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/stargazers)
[![GitHub](https://img.shields.io/github/license/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/blob/master/LICENSE)
[![CI](https://github.com/rosenbjerg/FFMpegCore/workflows/CI/badge.svg)](https://github.com/rosenbjerg/FFMpegCore/actions/workflows/ci.yml)
[![GitHub code contributors](https://img.shields.io/github/contributors/rosenbjerg/FFMpegCore)](https://github.com/rosenbjerg/FFMpegCore/graphs/contributors)

A .NET Standard FFMpeg/FFProbe wrapper for easily integrating media analysis and conversion into your .NET applications. Supports both synchronous and asynchronous calls

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

## Helper methods
The provided helper methods makes it simple to perform common operations.

### Easily capture snapshots from a video file:
```csharp
// process the snapshot in-memory and use the Bitmap directly
var bitmap = FFMpeg.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromMinutes(1));

// or persists the image on the drive
FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1));
```

### You can also capture GIF snapshots from a video file:
```csharp
FFMpeg.GifSnapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromSeconds(10));

// or async
await FFMpeg.GifSnapshotAsync(inputPath, outputPath, new Size(200, 400), TimeSpan.FromSeconds(10));

// you can also supply -1 to either one of Width/Height Size properties if you'd like FFMPEG to resize while maintaining the aspect ratio
await FFMpeg.GifSnapshotAsync(inputPath, outputPath, new Size(480, -1), TimeSpan.FromSeconds(10));
```

### Join video parts into one single file:
```csharp
FFMpeg.Join(@"..\joined_video.mp4",
    @"..\part1.mp4",
    @"..\part2.mp4",
    @"..\part3.mp4"
);
```

### Create a sub video
``` csharp
FFMpeg.SubVideo(inputPath, 
    outputPath,
    TimeSpan.FromSeconds(0)
    TimeSpan.FromSeconds(30)
);
```

### Join images into a video:
```csharp
FFMpeg.JoinImageSequence(@"..\joined_video.mp4", frameRate: 1,
    ImageInfo.FromPath(@"..\1.png"),
    ImageInfo.FromPath(@"..\2.png"),
    ImageInfo.FromPath(@"..\3.png")
);
```

### Mute the audio of a video file:
```csharp
FFMpeg.Mute(inputPath, outputPath);
```

### Extract the audio track from a video file:
```csharp
FFMpeg.ExtractAudio(inputPath, outputPath);
```

### Add or replace the audio track of a video file:
```csharp
FFMpeg.ReplaceAudio(inputPath, inputAudioPath, outputPath);
```

### Combine an image with audio file, for youtube or similar platforms
```csharp
FFMpeg.PosterWithAudio(inputPath, inputAudioPath, outputPath);
// or
var image = Image.FromFile(inputImagePath);
image.AddAudio(inputAudioPath, outputPath);
```

Other available arguments could be found in `FFMpegCore.Arguments` namespace.

## Input piping
With input piping it is possible to write video frames directly from program memory without saving them to jpeg or png and then passing path to input of ffmpeg. This feature also allows for converting video on-the-fly while frames are being generated or received.

An object implementing the `IPipeSource` interface is used as the source of data. Currently, the `IPipeSource` interface has two implementations; `StreamPipeSource` for streams, and `RawVideoPipeSource` for raw video frames.

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

If you want to use `System.Drawing.Bitmap`s as `IVideoFrame`s, a `BitmapVideoFrameWrapper` wrapper class is provided.


# Binaries

## Installation
If you prefer to manually download them, visit [ffbinaries](https://ffbinaries.com/downloads) or [zeranoe Windows builds](https://ffmpeg.zeranoe.com/builds/).

### Windows (using choco)
command: `choco install ffmpeg -y`

location: `C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin`

### Mac OSX
command: `brew install ffmpeg mono-libgdiplus`

location: `/usr/local/bin`

### Ubuntu
command: `sudo apt-get install -y ffmpeg libgdiplus`

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

The root and temp directory for the ffmpeg binaries can be configured via the `ffmpeg.config.json` file, which will be read on first use only.

```json
{
  "BinaryFolder": "./bin",
  "TemporaryFilesFolder": "/tmp"
}
```

### Supporting both 32 and 64 bit processes
If you wish to support multiple client processor architectures, you can do so by creating two folders, `x64` and `x86`, in the `BinaryFolder` directory.
Both folders should contain the binaries (`ffmpeg.exe` and `ffprobe.exe`) built for the respective architectures. 

By doing so, the library will attempt to use either `/{BinaryFolder}/{ARCH}/(ffmpeg|ffprobe).exe`.

If these folders are not defined, it will try to find the binaries in `/{BinaryFolder}/(ffmpeg|ffprobe.exe)`.

(`.exe` is only appended on Windows)


# Compatibility
Older versions of ffmpeg might not support all ffmpeg arguments available through this library. The library has been tested with version `3.3` to `4.2`


## Code contributors
<a href="https://github.com/rosenbjerg/ffmpegcore/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=rosenbjerg/ffmpegcore" />
</a>

## Other contributors
<a href="https://github.com/tiesont"><img src="https://avatars3.githubusercontent.com/u/420293?v=4" title="tiesont" width="80" height="80"></a>


### License

Copyright © 2023

Released under [MIT license](https://github.com/rosenbjerg/FFMpegCore/blob/master/LICENSE)
