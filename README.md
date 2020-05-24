# FFMpegCore 
[![NuGet Badge](https://buildstats.info/nuget/FFMpegCore)](https://www.nuget.org/packages/FFMpegCore/)
[![CI](https://github.com/rosenbjerg/FFMpegCore/workflows/CI/badge.svg)](https://github.com/rosenbjerg/FFMpegCore/actions?query=workflow%3ACI)

# Setup

#### NuGet:

```
Install-Package FFMpegCore
```

A great way to use FFMpeg encoding when writing video applications, client-side and server-side. It has wrapper methods that allow conversion to popular web formats, such as Mp4, WebM, Ogv, TS, and methods for capturing screenshots from videos, among other.

# API

## FFProbe

FFProbe is used to gather media information:

```csharp
var mediaInfo = FFProbe.Analyse(inputFile);
```
or 
```csharp
var mediaInfo = await FFProbe.AnalyseAsync(inputFile);
```


## FFMpeg
FFMpeg is used for converting your media files to web ready formats.
Easily build your FFMpeg arguments using the fluent argument builder:

Convert input file to h264/aac scaled to 720p w/ faststart, for web playback
```csharp
FFMpegArguments
    .FromInputFiles(inputFilePath)
    .WithVideoCodec(VideoCodec.LibX264)
    .WithConstantRateFactor(21)
    .WithAudioCodec(AudioCodec.Aac)
    .WithVariableBitrate(4)
    .WithFastStart()
    .Scale(VideoSize.Hd)
    .OutputToFile(output)
    .ProcessSynchronously(),
```

Easily capture screens from your videos:
```csharp
var mediaFileAnalysis = FFProbe.Analyse(inputFilePath);

// process the snapshot in-memory and use the Bitmap directly
var bitmap = FFMpeg.Snapshot(mediaFileAnalysis, new Size(200, 400), TimeSpan.FromMinutes(1));

// or persists the image on the drive
FFMpeg.Snapshot(mediaFileAnalysis, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1))
```

Convert to and/or from streams
```csharp
await FFMpegArguments
    .FromPipe(new StreamPipeDataWriter(inputStream))
    .WithVideoCodec("vp9")
    .ForceFormat("webm")
    .OutputToPipe(new StreamPipeDataReader(outputStream))
    .ProcessAsynchronously();
```

Join video parts into one single file:
```csharp
FFMpeg.Join(@"..\joined_video.mp4",
    @"..\part1.mp4",
    @"..\part2.mp4",
    @"..\part3.mp4"
);
```

Join images into a video:
```csharp
FFMpeg.JoinImageSequence(@"..\joined_video.mp4", frameRate: 1,
    ImageInfo.FromPath(@"..\1.png"),
    ImageInfo.FromPath(@"..\2.png"),
    ImageInfo.FromPath(@"..\3.png")
);
```

Mute videos:
```csharp
FFMpeg.Mute(inputFilePath, outputFilePath);
```

Save audio track from video:
```csharp
FFMpeg.ExtractAudio(inputVideoFilePath, outputAudioFilePath);
```

Add or replace audio track on video:
```csharp
FFMpeg.ReplaceAudio(inputVideoFilePath, inputAudioFilePath, outputVideoFilePath);
```

Add poster image to audio file (good for youtube videos):
```csharp
FFMpeg.PosterWithAudio(inputImageFilePath, inputAudioFilePath, outputVideoFilePath);
// or
var image = Image.FromFile(inputImageFile);
image.AddAudio(inputAudioFilePath, outputVideoFilePath);
```

Other available arguments could be found in `FFMpegCore.Arguments` namespace.

### Input piping
With input piping it is possible to write video frames directly from program memory without saving them to jpeg or png and then passing path to input of ffmpeg. This feature also allows us to convert video on-the-fly while frames are being generated or received.

The `IPipeSource` interface is used as the source of data. It could be represented as encoded video stream or raw frames stream. Currently, the `IPipeSource` interface has single implementation, `RawVideoPipeSource` that is used for raw stream encoding.

For example:

Method that is generating bitmap frames:
```csharp
IEnumerable<IVideoFrame> CreateFrames(int count)
{
    for(int i = 0; i < count; i++)
    {
        yield return GetNextFrame(); //method of generating new frames
    }
}
```
Then create `ArgumentsContainer` with `InputPipeArgument`
```csharp
var videoFramesSource = new RawVideoPipeSource(CreateFrames(64)) //pass IEnumerable<IVideoFrame> or IEnumerator<IVideoFrame> to constructor of RawVideoPipeSource
{
    FrameRate = 30 //set source frame rate
};
FFMpegArguments
    .FromPipe(videoFramesSource)
    // ... other encoding arguments
    .OutputToFile("temporary.mp4")
    .ProcessSynchronously();
```

if you want to use `System.Drawing.Bitmap` as `IVideoFrame`, there is a `BitmapVideoFrameWrapper` wrapper class.


## Binaries

If you prefer to manually download them, visit [ffbinaries](https://ffbinaries.com/downloads) or [zeranoe Windows builds](https://ffmpeg.zeranoe.com/builds/).

#### Windows

command: `choco install ffmpeg -Y`

location: `C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin`

#### Mac OSX

command: `brew install ffmpeg mono-libgdiplus`

location: `/usr/local/bin`

#### Ubuntu

command: `sudo apt-get install -y ffmpeg libgdiplus`

location: `/usr/bin`

## Path Configuration

#### Behavior

If you wish to support multiple client processor architectures, you can do so by creating a folder `x64` and `x86` in the `root` directory.
Both folders should contain the binaries (`ffmpeg.exe` and `ffprobe.exe`) for build for the respective architectures. 

By doing so, the library will attempt to use either `/root/{ARCH}/(ffmpeg|ffprobe).exe`.

If these folders are not defined, it will try to find the binaries in `/root/(ffmpeg|ffprobe.exe)`

#### Option 1

The default value (`\\FFMPEG\\bin`) can be overwritten via the `FFMpegOptions` class:

```c#
public Startup() 
{
    FFMpegOptions.Configure(new FFMpegOptions { RootDirectory = "./bin", TempDirectory = "/tmp" });
}
```

#### Option 2

The root and temp directory for the ffmpeg binaries can be configured via the `ffmpeg.config.json` file.

```json
{
  "RootDirectory": "./bin",
  "TempDirectory": "/tmp"
}
```

# Compatibility
 Some versions of FFMPEG might not have the same argument schema. The lib has been tested with version `3.3` to `4.2`


## Contributors

<a href="https://github.com/vladjerca"><img src="https://avatars.githubusercontent.com/u/6339681?v=4" title="vladjerca" width="80" height="80"></a>
<a href="https://github.com/max619"><img src="https://avatars.githubusercontent.com/u/26447324?v=4" title="max619" width="80" height="80"></a>
<a href="https://github.com/kyriakosio"><img src="https://avatars3.githubusercontent.com/u/6959989?v=4" title="kyriakosio" width="80" height="80"></a>
<a href="https://github.com/winchesterag"><img src="https://avatars3.githubusercontent.com/u/47878681?v=4" title="winchesterag" width="80" height="80"></a>
<a href="https://github.com/devlev"><img src="https://avatars3.githubusercontent.com/u/2109995?v=4" title="devlev" width="80" height="80"></a>
<a href="https://github.com/tugrulelmas"><img src="https://avatars3.githubusercontent.com/u/3829187?v=4" title="tugrulelmas" width="80" height="80"></a>
<a href="https://github.com/rosenbjerg"><img src="https://avatars3.githubusercontent.com/u/11181960?v=4" title="rosenbjerg" width="80" height="80"></a>

### License

Copyright © 2020 

Released under [MIT license](https://github.com/rosenbjerg/FFMpegCore/blob/master/LICENSE)
