using System.Drawing;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Extensions.System.Drawing.Common;
using FFMpegCore.Pipes;

var inputPath = "/path/to/input";
var outputPath = "/path/to/output";

{
    var mediaInfo = FFProbe.Analyse(inputPath);
}

{
    var mediaInfo = await FFProbe.AnalyseAsync(inputPath);
}

{
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
}

{
    // process the snapshot in-memory and use the Bitmap directly
    var bitmap = FFMpegImage.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromMinutes(1));

    // or persists the image on the drive
    FFMpeg.Snapshot(inputPath, outputPath, new Size(200, 400), TimeSpan.FromMinutes(1));
}

var inputStream = new MemoryStream();
var outputStream = new MemoryStream();

{
    await FFMpegArguments
        .FromPipeInput(new StreamPipeSource(inputStream))
        .OutputToPipe(new StreamPipeSink(outputStream), options => options
            .WithVideoCodec("vp9")
            .ForceFormat("webm"))
        .ProcessAsynchronously();
}

{
    FFMpeg.Join(@"..\joined_video.mp4",
        @"..\part1.mp4",
        @"..\part2.mp4",
        @"..\part3.mp4"
    );
}

{
    FFMpeg.JoinImageSequence(@"..\joined_video.mp4", frameRate: 1, @"..\1.png", @"..\2.png", @"..\3.png");
}

{
    FFMpeg.Mute(inputPath, outputPath);
}

{
    FFMpeg.ExtractAudio(inputPath, outputPath);
}

var inputAudioPath = "/path/to/input/audio";
{
    FFMpeg.ReplaceAudio(inputPath, inputAudioPath, outputPath);
}

var inputImagePath = "/path/to/input/image";
{
    FFMpeg.PosterWithAudio(inputPath, inputAudioPath, outputPath);
    // or 
#pragma warning disable CA1416
    using var image = Image.FromFile(inputImagePath);
    image.AddAudio(inputAudioPath, outputPath);
#pragma warning restore CA1416
}

IVideoFrame GetNextFrame() => throw new NotImplementedException();
{
    IEnumerable<IVideoFrame> CreateFrames(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return GetNextFrame(); //method of generating new frames
        }
    }

    var videoFramesSource = new RawVideoPipeSource(CreateFrames(64)) //pass IEnumerable<IVideoFrame> or IEnumerator<IVideoFrame> to constructor of RawVideoPipeSource
    {
        FrameRate = 30 //set source frame rate
    };
    await FFMpegArguments
        .FromPipeInput(videoFramesSource)
        .OutputToFile(outputPath, false, options => options
            .WithVideoCodec(VideoCodec.LibVpx))
        .ProcessAsynchronously();
}

{
    // setting global options
    GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./bin", TemporaryFilesFolder = "/tmp" });
    // or
    GlobalFFOptions.Configure(options => options.BinaryFolder = "./bin");

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
}
