using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Extensions.SkiaSharp;
using FFMpegCore.Extensions.System.Drawing.Common;
using FFMpegCore.Pipes;
using FFMpegCore.Test.Resources;
using FFMpegCore.Test.Utilities;
using SkiaSharp;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace FFMpegCore.Test;

[TestClass]
public class VideoTest
{
    private const int BaseTimeoutMilliseconds = 60_000;

    public TestContext TestContext { get; set; }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToOGV()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Ogv.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_YUV444p()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264)
                .WithPixelFormat("yuv444p"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
        var analysis = FFProbe.Analyse(outputFile);
        Assert.AreEqual("yuv444p", analysis.VideoStreams.First().PixelFormat);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_MetadataBuilder()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        await FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .AddMetadata(new FFMetadataBuilder()
                .WithTag("title", "noname")
                .WithTag("artist", "unknown")
                .WithChapter("Chapter 1", TimeSpan.FromSeconds(1.1))
                .WithChapter("Chapter 2", TimeSpan.FromSeconds(1.23)))
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();

        var analysis = await FFProbe.AnalyseAsync(outputFile, cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(analysis.Format.Tags!.TryGetValue("title", out var title));
        Assert.IsTrue(analysis.Format.Tags!.TryGetValue("artist", out var artist));
        Assert.AreEqual("noname", title);
        Assert.AreEqual("unknown", artist);

        Assert.HasCount(2, analysis.Chapters);
        Assert.AreEqual("Chapter 1", analysis.Chapters.First().Title);
        Assert.AreEqual(1.1, analysis.Chapters.First().Duration.TotalSeconds);
        Assert.AreEqual(1.1, analysis.Chapters.First().End.TotalSeconds);

        Assert.AreEqual("Chapter 2", analysis.Chapters.Last().Title);
        Assert.AreEqual(1.23, analysis.Chapters.Last().Duration.TotalSeconds);
        Assert.AreEqual(1.1 + 1.23, analysis.Chapters.Last().End.TotalSeconds);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToH265_MKV_Args()
    {
        using var outputFile = new TemporaryFile("out.mkv");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX265))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(PixelFormat.Format24bppRgb)]
    [DataRow(PixelFormat.Format32bppArgb)]
    public void Video_ToMP4_Args_Pipe_WindowsOnly(PixelFormat pixelFormat)
    {
        Video_ToMP4_Args_Pipe_Internal(pixelFormat, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(SKColorType.Rgb565)]
    [DataRow(SKColorType.Bgra8888)]
    public void Video_ToMP4_Args_Pipe(SKColorType pixelFormat)
    {
        Video_ToMP4_Args_Pipe_Internal(pixelFormat, TestContext.CancellationToken);
    }

    private static void Video_ToMP4_Args_Pipe_Internal(dynamic pixelFormat, CancellationToken cancellationToken)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));
        var success = FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args_Pipe_DifferentImageSizes_WindowsOnly()
    {
        Video_ToMP4_Args_Pipe_DifferentImageSizes_Internal(PixelFormat.Format24bppRgb, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args_Pipe_DifferentImageSizes()
    {
        Video_ToMP4_Args_Pipe_DifferentImageSizes_Internal(SKColorType.Rgb565, TestContext.CancellationToken);
    }

    private static void Video_ToMP4_Args_Pipe_DifferentImageSizes_Internal(dynamic pixelFormat, CancellationToken cancellationToken)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var frames = new List<IVideoFrame>
        {
            BitmapSource.CreateVideoFrame(0, pixelFormat, 255, 255, 1, 0), BitmapSource.CreateVideoFrame(0, pixelFormat, 256, 256, 1, 0)
        };

        var videoFramesSource = new RawVideoPipeSource(frames);
        Assert.ThrowsExactly<FFMpegStreamFormatException>(() => FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously());
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_Pipe_DifferentImageSizes_WindowsOnly_Async()
    {
        await Video_ToMP4_Args_Pipe_DifferentImageSizes_Internal_Async(PixelFormat.Format24bppRgb, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_Pipe_DifferentImageSizes_Async()
    {
        await Video_ToMP4_Args_Pipe_DifferentImageSizes_Internal_Async(SKColorType.Rgb565, TestContext.CancellationToken);
    }

    private static async Task Video_ToMP4_Args_Pipe_DifferentImageSizes_Internal_Async(dynamic pixelFormat, CancellationToken cancellationToken)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var frames = new List<IVideoFrame>
        {
            BitmapSource.CreateVideoFrame(0, pixelFormat, 255, 255, 1, 0), BitmapSource.CreateVideoFrame(0, pixelFormat, 256, 256, 1, 0)
        };

        var videoFramesSource = new RawVideoPipeSource(frames);
        await Assert.ThrowsExactlyAsync<FFMpegStreamFormatException>(() => FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously());
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args_Pipe_DifferentPixelFormats_WindowsOnly()
    {
        Video_ToMP4_Args_Pipe_DifferentPixelFormats_Internal(PixelFormat.Format24bppRgb,
            PixelFormat.Format32bppRgb, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args_Pipe_DifferentPixelFormats()
    {
        Video_ToMP4_Args_Pipe_DifferentPixelFormats_Internal(SKColorType.Rgb565, SKColorType.Bgra8888, TestContext.CancellationToken);
    }

    private static void Video_ToMP4_Args_Pipe_DifferentPixelFormats_Internal(dynamic pixelFormatFrame1, dynamic pixelFormatFrame2,
        CancellationToken cancellationToken)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var frames = new List<IVideoFrame>
        {
            BitmapSource.CreateVideoFrame(0, pixelFormatFrame1, 255, 255, 1, 0), BitmapSource.CreateVideoFrame(0, pixelFormatFrame2, 255, 255, 1, 0)
        };

        var videoFramesSource = new RawVideoPipeSource(frames);
        Assert.ThrowsExactly<FFMpegStreamFormatException>(() => FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously());
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_Pipe_DifferentPixelFormats_WindowsOnly_Async()
    {
        await Video_ToMP4_Args_Pipe_DifferentPixelFormats_Internal_Async(PixelFormat.Format24bppRgb,
            PixelFormat.Format32bppRgb, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_Pipe_DifferentPixelFormats_Async()
    {
        await Video_ToMP4_Args_Pipe_DifferentPixelFormats_Internal_Async(SKColorType.Rgb565, SKColorType.Bgra8888, TestContext.CancellationToken);
    }

    private static async Task Video_ToMP4_Args_Pipe_DifferentPixelFormats_Internal_Async(dynamic pixelFormatFrame1, dynamic pixelFormatFrame2,
        CancellationToken cancellationToken)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var frames = new List<IVideoFrame>
        {
            BitmapSource.CreateVideoFrame(0, pixelFormatFrame1, 255, 255, 1, 0), BitmapSource.CreateVideoFrame(0, pixelFormatFrame2, 255, 255, 1, 0)
        };

        var videoFramesSource = new RawVideoPipeSource(frames);
        await Assert.ThrowsExactlyAsync<FFMpegStreamFormatException>(() => FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args_StreamPipe()
    {
        using var input = File.OpenRead(TestResources.WebmVideo);
        using var output = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments
            .FromPipeInput(new StreamPipeSource(input))
            .OutputToFile(output, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_StreamOutputPipe_Async_Failure()
    {
        await Assert.ThrowsExactlyAsync<FFMpegProcessException>(async () =>
        {
            await using var ms = new MemoryStream();
            var pipeSource = new StreamPipeSink(ms);
            await FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToPipe(pipeSource, opt => opt.ForceFormat("mp4"))
                .CancellableThrough(TestContext.CancellationToken)
                .ProcessAsynchronously();
        });
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_StreamFile_OutputToMemoryStream()
    {
        var output = new MemoryStream();

        FFMpegArguments
            .FromPipeInput(new StreamPipeSource(File.OpenRead(TestResources.WebmVideo)), opt => opt
                .ForceFormat("webm"))
            .OutputToPipe(new StreamPipeSink(output), opt => opt
                .ForceFormat("mpegts"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        output.Position = 0;
        var result = FFProbe.Analyse(output);
        Console.WriteLine(result.Duration);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args_StreamOutputPipe_Failure()
    {
        Assert.ThrowsExactly<FFMpegProcessException>(() =>
        {
            using var ms = new MemoryStream();
            FFMpegArguments
                .FromFileInput(TestResources.Mp4Video)
                .OutputToPipe(new StreamPipeSink(ms), opt => opt
                    .ForceFormat("mkv"))
                .CancellableThrough(TestContext.CancellationToken)
                .ProcessSynchronously();
        });
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_StreamOutputPipe_Async()
    {
        await using var ms = new MemoryStream();
        var pipeSource = new StreamPipeSink(ms);
        await FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToPipe(pipeSource, opt => opt
                .WithVideoCodec(VideoCodec.LibX264)
                .ForceFormat("matroska"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task TestDuplicateRun()
    {
        FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile("temporary.mp4")
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        await FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile("temporary.mp4")
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();

        File.Delete("temporary.mp4");
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void TranscodeToMemoryStream_Success()
    {
        using var output = new MemoryStream();
        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToPipe(new StreamPipeSink(output), opt => opt
                .WithVideoCodec(VideoCodec.LibVpx)
                .ForceFormat("matroska"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);

        output.Position = 0;
        var inputAnalysis = FFProbe.Analyse(TestResources.WebmVideo);
        var outputAnalysis = FFProbe.Analyse(output);
        Assert.AreEqual(inputAnalysis.Duration.TotalSeconds, outputAnalysis.Duration.TotalSeconds, 0.3);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToTS()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Ts.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToTS_Args()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Ts.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .CopyStreams()
                .WithBitstreamFilter(StreamType.Video, BitstreamFilter.H264_Mp4ToAnnexB)
                .ForceFormat(ContainerFormats.Ts))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(PixelFormat.Format24bppRgb)]
    [DataRow(PixelFormat.Format32bppArgb)]
    public async Task Video_ToTS_Args_Pipe_WindowsOnly(PixelFormat pixelFormat)
    {
        await Video_ToTS_Args_Pipe_Internal(pixelFormat, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(SKColorType.Rgb565)]
    [DataRow(SKColorType.Bgra8888)]
    public async Task Video_ToTS_Args_Pipe(SKColorType pixelFormat)
    {
        await Video_ToTS_Args_Pipe_Internal(pixelFormat, TestContext.CancellationToken);
    }

    private static async Task Video_ToTS_Args_Pipe_Internal(dynamic pixelFormat, CancellationToken cancellationToken)
    {
        using var output = new TemporaryFile($"out{ContainerFormats.Ts.GetExtension()}");
        var input = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));

        var success = await FFMpegArguments
            .FromPipeInput(input)
            .OutputToFile(output, false, opt => opt
                .ForceFormat(ContainerFormats.Ts))
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();
        Assert.IsTrue(success.Success);

        var analysis = await FFProbe.AnalyseAsync(output);
        Assert.AreEqual(ContainerFormats.Ts.Name, analysis.Format.FormatName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToOGV_Resize()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Ogv.GetExtension()}");
        var success = await FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoFilters(filters => filters.Scale(200, 200))
                .WithVideoCodec(VideoCodec.LibTheora))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();
        Assert.IsTrue(success.Success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(SKColorType.Rgb565)]
    [DataRow(SKColorType.Bgra8888)]
    public void RawVideoPipeSource_Ogv_Scale(SKColorType pixelFormat)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Ogv.GetExtension()}");
        var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));

        FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Scale(VideoSize.Ed))
                .WithVideoCodec(VideoCodec.LibTheora))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputFile);
        Assert.AreEqual((int)VideoSize.Ed, analysis.PrimaryVideoStream!.Width);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Scale_Mp4_Multithreaded()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .WithThreads(Environment.ProcessorCount)
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(PixelFormat.Format24bppRgb)]
    [DataRow(PixelFormat.Format32bppArgb)]
    // [DataRow(PixelFormat.Format48bppRgb)]
    public void Video_ToMP4_Resize_Args_Pipe(PixelFormat pixelFormat)
    {
        Video_ToMP4_Resize_Args_Pipe_Internal(pixelFormat, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(SKColorType.Rgb565)]
    [DataRow(SKColorType.Bgra8888)]
    public void Video_ToMP4_Resize_Args_Pipe(SKColorType pixelFormat)
    {
        Video_ToMP4_Resize_Args_Pipe_Internal(pixelFormat, TestContext.CancellationToken);
    }

    private static void Video_ToMP4_Resize_Args_Pipe_Internal(dynamic pixelFormat, CancellationToken cancellationToken)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");
        var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));

        var success = FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_InMemory_SystemDrawingCommon()
    {
        using var bitmap = SystemDrawingImage.Snapshot(TestResources.Mp4Video);

        var input = FFProbe.Analyse(TestResources.Mp4Video);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
        Assert.AreEqual(255, bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2).A);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_SnapshotAsync_InMemory_SystemDrawingCommon()
    {
        using var bitmap = await SystemDrawingImage.SnapshotAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);

        var input = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
        Assert.AreEqual(255, bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2).A);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(SKColorType.Rgb888x)]
    [DataRow(SKColorType.Rgba8888)]
    [DataRow(SKColorType.Bgra8888)]
    [DataRow(SKColorType.Rgb565)]
    [DataRow(SKColorType.Gray8)]
    public void Video_SkiaSharpFrames_KeepTheirColours(SKColorType colorType)
    {
        using var outputFile = new TemporaryFile("out.png");
        using var bitmap = new SKBitmap(new SKImageInfo(16, 16, colorType, SKAlphaType.Opaque));
        bitmap.Erase(new SKColor(200, 40, 90));
        var expected = bitmap.GetPixel(0, 0);

        FFMpegArguments
            .FromPipeInput(new RawVideoPipeSource(new[] { new SkiaSharpVideoFrame(bitmap) }))
            .OutputToFile(outputFile, true, o => o.WithFrameCount(1))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        using var output = SKBitmap.Decode((string)outputFile);
        var actual = output.GetPixel(8, 8);
        Assert.IsTrue(Math.Abs(expected.Red - actual.Red) <= 8 && Math.Abs(expected.Green - actual.Green) <= 8 &&
                      Math.Abs(expected.Blue - actual.Blue) <= 8, $"expected {expected}, got {actual}");
    }

    [TestMethod]
    public void RawVideoPipeSource_AcceptsAnArrayOfFrames()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(16, 8, SKColorType.Bgra8888));
        var source = new RawVideoPipeSource(new IVideoFrame[] { new SkiaSharpVideoFrame(bitmap) });

        Assert.AreEqual("-f rawvideo -r 25 -pix_fmt bgra -s 16x8", source.GetStreamArguments());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_InMemory_SkiaSharp()
    {
        using var bitmap = SkiaSharpImage.Snapshot(TestResources.Mp4Video);

        var input = FFProbe.Analyse(TestResources.Mp4Video);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        Assert.AreEqual(255, bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2).Alpha);
        // Note: The resulting ColorType is dependent on the execution environment and therefore not assessed,
        // e.g. Bgra8888 on Windows and Rgba8888 on macOS.
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_SnapshotAsync_InMemory_SkiaSharp()
    {
        using var bitmap = await SkiaSharpImage.SnapshotAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);

        var input = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        Assert.AreEqual(255, bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2).Alpha);
        // Note: The resulting ColorType is dependent on the execution environment and therefore not assessed,
        // e.g. Bgra8888 on Windows and Rgba8888 on macOS.
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Png_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.png");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("png", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_SnapshotAsync_Png_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.png");
        var input = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);

        await FFMpeg.Snapshot(TestResources.Mp4Video, outputPath)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("png", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Jpg_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.jpg");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("mjpeg", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Bmp_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.bmp");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("bmp", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Webp_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.webp");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("webp", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    public void Video_Snapshot_RejectsNonImageExtension()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => FFMpeg.Snapshot(TestResources.Mp4Video, "out.asd"));

        Assert.Contains("needed: .png,.jpg,.bmp,.webp", exception.Message);
    }

    [TestMethod]
    public void Video_GifSnapshot_RejectsNonGifExtension()
    {
        var exception = Assert.ThrowsExactly<ArgumentException>(() => FFMpeg.GifSnapshot(TestResources.Mp4Video, "out.png"));

        Assert.Contains("needed: .gif", exception.Message);
    }

    [TestMethod]
    public void Video_Snapshot_AcceptsAnUpperCaseExtension()
    {
        var arguments = FFMpeg.Snapshot(FFProbe.Analyse(TestResources.Mp4Video), "out.PNG").Arguments;

        Assert.Contains("-c:v png", arguments);
    }

    [TestMethod]
    public void Video_GifSnapshot_KeepsARequestedSizeEqualToTheSource()
    {
        var source = FFProbe.Analyse(TestResources.Mp4Video);
        var size = new Size(source.PrimaryVideoStream!.Width, source.PrimaryVideoStream.Height);

        var arguments = FFMpeg.GifSnapshot(source, "out.gif", size).Arguments;

        Assert.DoesNotContain("scale=", arguments);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Rotated_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.png");

        var size = new Size(360, 0); // half the size of original video, keeping height 0 for keeping aspect ratio
        FFMpeg.Snapshot(TestResources.Mp4VideoRotationNegative, outputPath, size).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(size.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(1280 / 2, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual(0, analysis.PrimaryVideoStream!.Rotation);
        Assert.AreEqual("png", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_HeightOnly_KeepsAspectRatio()
    {
        using var outputPath = new TemporaryFile("out.png");

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath, new Size(0, 360))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(640, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(360, analysis.PrimaryVideoStream!.Height);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_GifSnapshot_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.gif");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.GifSnapshot(TestResources.Mp4Video, outputPath, captureTime: TimeSpan.FromSeconds(0)).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreNotEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreNotEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("gif", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_GifSnapshot_VideoIsNotFirstStream()
    {
        using var audioFirst = new TemporaryFile("audio-first.mp4");
        FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(audioFirst, options => options
                .WithMap(0, StreamType.Audio)
                .WithMap(0, StreamType.Video)
                .CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.AreEqual(1, FFProbe.Analyse(audioFirst).PrimaryVideoStream!.Index);
        using var outputPath = new TemporaryFile("out.gif");

        FFMpeg.GifSnapshot(audioFirst, outputPath, captureTime: TimeSpan.FromSeconds(0))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.AreEqual("gif", FFProbe.Analyse(outputPath).PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_GifPalette_ThroughTheComplexFilterGraph()
    {
        using var outputPath = new TemporaryFile("out.gif");

        var result = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video, options => options.WithDuration(TimeSpan.FromSeconds(1)))
            .OutputToFile(outputPath, options => options
                .WithComplexFilter(graph => graph
                    .From(0, StreamType.Video).Video(f => f.Fps(12).Scale(320, -1)).WithCustomFilter("split").As("a", "b")
                    .From("a").WithCustomFilter("palettegen").As("palette")
                    .From("b").From("palette").WithCustomFilter("paletteuse").As("gif"))
                .WithMap("gif"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("gif", FFProbe.Analyse(outputPath).PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_GifSnapshot_PersistSnapshot_SizeSupplied()
    {
        using var outputPath = new TemporaryFile("out.gif");
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var desiredGifSize = new Size(320, 240);

        FFMpeg.GifSnapshot(TestResources.Mp4Video, outputPath, desiredGifSize, TimeSpan.FromSeconds(0)).ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreNotEqual(input.PrimaryVideoStream!.Width, desiredGifSize.Width);
        Assert.AreNotEqual(input.PrimaryVideoStream.Height, desiredGifSize.Height);
        Assert.AreEqual("gif", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_GifSnapshot_PersistSnapshotAsync()
    {
        using var outputPath = new TemporaryFile("out.gif");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        await FFMpeg.GifSnapshot(TestResources.Mp4Video, outputPath, captureTime: TimeSpan.FromSeconds(0))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreNotEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreNotEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("gif", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_GifSnapshot_PersistSnapshotAsync_SizeSupplied()
    {
        using var outputPath = new TemporaryFile("out.gif");
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var desiredGifSize = new Size(320, 240);

        await FFMpeg.GifSnapshot(TestResources.Mp4Video, outputPath, desiredGifSize, TimeSpan.FromSeconds(0))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreNotEqual(input.PrimaryVideoStream!.Width, desiredGifSize.Width);
        Assert.AreNotEqual(input.PrimaryVideoStream.Height, desiredGifSize.Height);
        Assert.AreEqual("gif", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Join()
    {
        using var inputCopy = new TemporaryFile("copy-input.mp4");
        File.Copy(TestResources.Mp4Video, inputCopy);

        using var outputPath = new TemporaryFile("out.mp4");
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var success = FFMpeg.Join(outputPath, TestResources.Mp4Video, inputCopy).ProcessSynchronously();
        Assert.IsTrue(success.Success);
        Assert.IsTrue(File.Exists(outputPath));

        var expectedDuration = input.Duration * 2;
        var result = FFProbe.Analyse(outputPath);
        Assert.AreEqual(expectedDuration.Days, result.Duration.Days);
        Assert.AreEqual(expectedDuration.Hours, result.Duration.Hours);
        Assert.AreEqual(expectedDuration.Minutes, result.Duration.Minutes);
        Assert.AreEqual(expectedDuration.Seconds, result.Duration.Seconds);
        Assert.AreEqual(input.PrimaryVideoStream!.Height, result.PrimaryVideoStream!.Height);
        Assert.AreEqual(input.PrimaryVideoStream.Width, result.PrimaryVideoStream.Width);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Join_VideoOnly()
    {
        using var outputPath = new TemporaryFile("out.mp4");

        var success = FFMpeg.Join(outputPath, TestResources.Mp4WithoutAudio, TestResources.Mp4WithoutAudio).ProcessSynchronously();
        Assert.IsTrue(success.Success);

        var result = FFProbe.Analyse(outputPath);
        Assert.AreEqual(6, result.Duration.Seconds);
        Assert.IsNull(result.PrimaryAudioStream);
    }

    [TestMethod]
    [Timeout(2 * BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Join_Image_Sequence()
    {
        var imageSet = new List<string>();
        Directory.EnumerateFiles(TestResources.ImageCollection, "*.png")
            .ToList()
            .ForEach(file =>
            {
                for (var i = 0; i < 5; i++)
                {
                    imageSet.Add(file);
                }
            });
        var imageAnalysis = FFProbe.Analyse(imageSet.First());

        using var outputFile = new TemporaryFile("out.mp4");
        var success = FFMpeg.JoinImageSequence(outputFile, 10, imageSet.ToArray()).ProcessSynchronously();
        Assert.IsTrue(success.Success);
        var result = FFProbe.Analyse(outputFile);

        Assert.AreEqual(3, result.Duration.Seconds);
        Assert.AreEqual(imageAnalysis.PrimaryVideoStream!.Width, result.PrimaryVideoStream!.Width);
        Assert.AreEqual(imageAnalysis.PrimaryVideoStream!.Height, result.PrimaryVideoStream.Height);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_With_Only_Audio_Should_Extract_Metadata()
    {
        var video = FFProbe.Analyse(TestResources.Mp4WithoutVideo);
        Assert.IsNull(video.PrimaryVideoStream);
        Assert.AreEqual("aac", video.PrimaryAudioStream!.CodecName);
        Assert.AreEqual(10, video.Duration.TotalSeconds, 0.5);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Duration()
    {
        var video = FFProbe.Analyse(TestResources.Mp4Video);
        using var outputFile = new TemporaryFile("out.mp4");

        FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt.WithDuration(TimeSpan.FromSeconds(video.Duration.TotalSeconds - 2)))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(File.Exists(outputFile));
        var outputVideo = FFProbe.Analyse(outputFile);

        Assert.AreEqual(video.Duration.Days, outputVideo.Duration.Days);
        Assert.AreEqual(video.Duration.Hours, outputVideo.Duration.Hours);
        Assert.AreEqual(video.Duration.Minutes, outputVideo.Duration.Minutes);
        Assert.AreEqual(video.Duration.Seconds - 2, outputVideo.Duration.Seconds);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_UpdatesProgress()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var percentageDone = 0.0;
        var timeDone = TimeSpan.Zero;
        var analysis = FFProbe.Analyse(TestResources.Mp4Video);

        var events = new List<double>();

        void OnPercentageProgess(double percentage)
        {
            events.Add(percentage);
            percentageDone = percentage;
        }

        void OnTimeProgess(TimeSpan time)
        {
            if (time < analysis.Duration)
            {
                timeDone = time;
            }
        }

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .WithDuration(analysis.Duration))
            .NotifyOnPercentageProgress(OnPercentageProgess, analysis.Duration)
            .NotifyOnProgress(OnTimeProgess)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        Assert.IsTrue(File.Exists(outputFile));
        Assert.AreNotEqual(0.0, percentageDone);
        Assert.IsGreaterThan(1, events.Count);
        CollectionAssert.AllItemsAreUnique(events);
        Assert.AreNotEqual(100.0, events.First());
        Assert.AreEqual(100.0, events.Last(), 0.001);
        Assert.AreNotEqual(TimeSpan.Zero, timeDone);
        Assert.AreNotEqual(analysis.Duration, timeDone);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_OutputsData()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var dataReceived = false;

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .WithDuration(TimeSpan.FromSeconds(2)))
            .WithLogLevel(FFMpegLogLevel.Info)
            .NotifyOnStandardError(_ => dataReceived = true)
            .Configure(opt => opt.Encoding = Encoding.UTF8)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(dataReceived);
        Assert.IsTrue(success.Success);
        Assert.IsTrue(File.Exists(outputFile));
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TranscodeInMemory_WindowsOnly()
    {
        Video_TranscodeInMemory_Internal(PixelFormat.Format24bppRgb, TestContext.CancellationToken);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TranscodeInMemory()
    {
        Video_TranscodeInMemory_Internal(SKColorType.Rgb565, TestContext.CancellationToken);
    }

    private static void Video_TranscodeInMemory_Internal(dynamic pixelFormat, CancellationToken cancellationToken)
    {
        using var resStream = new MemoryStream();
        var reader = new StreamPipeSink(resStream);
        var writer = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 128, 128));

        FFMpegArguments
            .FromPipeInput(writer)
            .OutputToPipe(reader, opt => opt
                .WithVideoCodec(VideoCodec.LibVpxVp9)
                .ForceFormat("webm"))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously();

        resStream.Position = 0;
        var vi = FFProbe.Analyse(resStream);
        Assert.AreEqual(128, vi.PrimaryVideoStream!.Width);
        Assert.AreEqual(128, vi.PrimaryVideoStream.Height);
    }

    [TestMethod]
    [Timeout(2 * BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TranscodeToMemory()
    {
        using var memoryStream = new MemoryStream();

        FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToPipe(new StreamPipeSink(memoryStream), opt => opt
                .WithVideoCodec(VideoCodec.LibVpxVp9)
                .ForceFormat("webm"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        memoryStream.Position = 0;
        var vi = FFProbe.Analyse(memoryStream);
        Assert.AreEqual(640, vi.PrimaryVideoStream!.Width);
        Assert.AreEqual(360, vi.PrimaryVideoStream.Height);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Cancel_Async()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(out var cancel)
            .CancellableThrough(TestContext.CancellationToken)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously(false);

        await Task.Delay(300, TestContext.CancellationToken);
        cancel();

        var result = await task;

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Cancelled);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(out var cancel)
            .CancellableThrough(TestContext.CancellationToken);

        Task.Delay(300, TestContext.CancellationToken).ContinueWith(_ => cancel(), TestContext.CancellationToken);

        var result = task.CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously(false);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Cancelled);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Cancel_Async_With_Timeout()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(out var cancel, TimeSpan.FromSeconds(10))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously(false);

        await Task.Delay(300, TestContext.CancellationToken);
        cancel();

        await task;

        var outputInfo = await FFProbe.AnalyseAsync(outputFile, cancellationToken: TestContext.CancellationToken);

        Assert.IsNotNull(outputInfo);
        Assert.AreEqual(320, outputInfo.PrimaryVideoStream!.Width);
        Assert.AreEqual(240, outputInfo.PrimaryVideoStream.Height);
        Assert.AreEqual("h264", outputInfo.PrimaryVideoStream.CodecName);
        Assert.AreEqual("aac", outputInfo.PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Cancel_CancellationToken_Async()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(cts.Token)
            .ProcessAsynchronously(false);

        cts.CancelAfter(300);

        var result = await task;

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Cancelled);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Cancel_CancellationToken_Async_Throws()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(cts.Token)
            .ProcessAsynchronously();

        cts.CancelAfter(300);

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => task);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel_CancellationToken_Throws()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(cts.Token);

        cts.CancelAfter(300);

        Assert.ThrowsExactly<OperationCanceledException>(() => task.ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel_CancellationToken_EveryRegisteredTokenCancels()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        using var first = new CancellationTokenSource();
        using var second = new CancellationTokenSource();

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(first.Token)
            .CancellableThrough(second.Token)
            .CancellableThrough(TestContext.CancellationToken);

        first.CancelAfter(300);

        Assert.ThrowsExactly<OperationCanceledException>(() => task.ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel_CancellationToken_BeforeProcessing_Throws()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(cts.Token);

        cts.Cancel();
        Assert.ThrowsExactly<OperationCanceledException>(() => task.ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel_CancellationToken_BeforePassing_Throws()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        cts.Cancel();

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast));

        Assert.ThrowsExactly<OperationCanceledException>(() => task.CancellableThrough(cts.Token));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Cancel_CancellationToken_Async_With_Timeout()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(cts.Token, TimeSpan.FromSeconds(8))
            .ProcessAsynchronously(false);

        cts.CancelAfter(300);

        await task;

        var outputInfo = await FFProbe.AnalyseAsync(outputFile, cancellationToken: TestContext.CancellationToken);

        Assert.IsNotNull(outputInfo);
        Assert.AreEqual(320, outputInfo.PrimaryVideoStream!.Width);
        Assert.AreEqual(240, outputInfo.PrimaryVideoStream.Height);
        Assert.AreEqual("h264", outputInfo.PrimaryVideoStream.CodecName);
        Assert.AreEqual("aac", outputInfo.PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Cancel_RunToken_FinalisesTheOutput()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);

        var task = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .ProcessAsynchronously(false, cancellationToken: cts.Token);

        cts.CancelAfter(300);

        var result = await task;
        var outputInfo = await FFProbe.AnalyseAsync(outputFile, cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.Cancelled);
        Assert.AreEqual("h264", outputInfo.PrimaryVideoStream!.CodecName);
        Assert.AreEqual("aac", outputInfo.PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel_DefaultGracePeriod_FinalisesTheOutput()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var processor = FFMpegArguments
            .FromFileInput("testsrc2=size=320x240[out0]; sine[out1]", false, args => args
                .WithCustomArgument("-re")
                .ForceFormat("lavfi"))
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoCodec(VideoCodec.LibX264)
                .WithSpeedPreset(EncoderPreset.VeryFast))
            .CancellableThrough(out var cancel)
            .CancellableThrough(TestContext.CancellationToken);

        Task.Delay(300, TestContext.CancellationToken).ContinueWith(_ => cancel(), TestContext.CancellationToken);
        var result = processor.ProcessSynchronously(false);
        var outputInfo = FFProbe.Analyse(outputFile);

        Assert.IsTrue(result.Cancelled);
        Assert.AreEqual("h264", outputInfo.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Cancel_RunTokenAlreadyCancelled_DoesNotStart()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var processor = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, true, opt => opt.CopyStreams());

        Assert.ThrowsExactly<OperationCanceledException>(() => processor.ProcessSynchronously(cancellationToken: cts.Token));
        Assert.IsFalse(File.Exists(outputFile));
        Assert.IsTrue(processor.ProcessSynchronously().Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Trim()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = FFMpeg.Trim(TestResources.Mp4Video, outputFile, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)).ProcessSynchronously();
        Assert.IsTrue(success.Success);

        var analysis = FFProbe.Analyse(outputFile);
        Assert.IsTrue(analysis.Duration >= TimeSpan.FromSeconds(0.9) && analysis.Duration <= TimeSpan.FromSeconds(1.2), $"Unexpected duration {analysis.Duration}");
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_Trim_Async()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = await FFMpeg.Trim(TestResources.Mp4Video, outputFile, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();
        Assert.IsTrue(success.Success);

        var analysis = await FFProbe.AnalyseAsync(outputFile, cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(analysis.Duration >= TimeSpan.FromSeconds(0.9) && analysis.Duration <= TimeSpan.FromSeconds(1.2), $"Unexpected duration {analysis.Duration}");
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TrimAndConcat_KeepEveryStream()
    {
        using var twoAudioTracks = new TemporaryFile("dual.mkv");
        using var trimmed = new TemporaryFile("trimmed.mkv");
        using var joined = new TemporaryFile("joined.mkv");
        FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .AddFileInput(TestResources.Mp3Audio)
            .OutputToFile(twoAudioTracks, true, options => options
                .WithMap(0)
                .WithMap(1, StreamType.Audio)
                .CopyStreams()
                .WithShortest())
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);

        var trimResult = FFMpeg.Trim(twoAudioTracks, trimmed, TimeSpan.Zero, TimeSpan.FromSeconds(2))
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);
        var concatResult = FFMpeg.Concat(joined, twoAudioTracks, twoAudioTracks)
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(trimResult.Success);
        Assert.IsTrue(concatResult.Success);
        Assert.HasCount(2, FFProbe.Analyse(trimmed).AudioStreams);
        Assert.HasCount(2, FFProbe.Analyse(joined).AudioStreams);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Trim_WritesTheRequestedOutput()
    {
        using var requestedOutput = new TemporaryFile("out.mkv");
        var mp4 = Path.ChangeExtension(requestedOutput, ".mp4");

        var success = FFMpeg.Trim(TestResources.Mp4Video, requestedOutput, TimeSpan.Zero, TimeSpan.FromSeconds(1)).ProcessSynchronously();

        Assert.IsTrue(success.Success);
        Assert.IsTrue(File.Exists(requestedOutput));
        Assert.IsFalse(File.Exists(mp4));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow("out.ts")]
    [DataRow("out.mkv")]
    [DataRow("out.mp4")]
    public void Video_SaveStream_RecordsAnyProtocolIntoAnyContainer(string filename)
    {
        using var outputFile = new TemporaryFile(filename);
        var uri = new Uri(Path.GetFullPath(TestResources.Mp4Video));

        var success = FFMpeg.SaveStream(uri, outputFile)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        Assert.IsNotEmpty(FFProbe.Analyse(outputFile).VideoStreams);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(WatermarkPosition.TopLeft)]
    [DataRow(WatermarkPosition.BottomRight)]
    [DataRow(WatermarkPosition.Center)]
    public void Video_Watermark_KeepsTheSizeAndTheAudio(WatermarkPosition position)
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var logo = Path.Combine(TestResources.ImageCollection, "a.png");

        var success = FFMpeg.Watermark(TestResources.Mp4Video, logo, outputFile, position)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var result = FFProbe.Analyse(outputFile);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, result.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, result.PrimaryVideoStream.Height);
        Assert.AreEqual(input.PrimaryAudioStream!.CodecName, result.PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Watermark_WorksWithoutAnAudioStream()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var logo = Path.Combine(TestResources.ImageCollection, "a.png");

        var success = FFMpeg.Watermark(TestResources.Mp4WithoutAudio, logo, outputFile)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        Assert.IsEmpty(FFProbe.Analyse(outputFile).AudioStreams);
    }

    [TestMethod]
    public void Video_ExtractSubtitles_RejectsAnInputWithoutThatSubtitleStream()
    {
        using var subtitled = CreateVideoWithSubtitles();
        var video = FFProbe.Analyse(subtitled).PrimaryVideoStream!.Index;

        Assert.ThrowsExactly<ArgumentException>(() => FFMpeg.ExtractSubtitles(TestResources.Mp4Video, "out.srt"));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => FFMpeg.ExtractSubtitles(subtitled, "out.srt", video));
    }

    private TemporaryFile CreateVideoWithSubtitles()
    {
        var video = new TemporaryFile("subtitled.mkv");
        FFMpeg.AddSubtitles(TestResources.Mp4Video, TestResources.SrtSubtitle, video)
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);
        return video;
    }

    [TestMethod]
    public void Video_AnalysisOverload_BuildsTheSameArgumentsAsThePathOverload()
    {
        using var output = new TemporaryFile("out.mkv");
        var source = FFProbe.Analyse(TestResources.Mp4Video);

        Assert.AreEqual(FFMpeg.Remux(TestResources.Mp4Video, output).Arguments, FFMpeg.Remux(source, output).Arguments);
        Assert.AreEqual(FFMpeg.RemoveAudio(TestResources.Mp4Video, output).Arguments, FFMpeg.RemoveAudio(source, output).Arguments);
        Assert.AreEqual(FFMpeg.ThumbnailSheet(TestResources.Mp4Video, "sheet.png").Arguments,
            FFMpeg.ThumbnailSheet(source, "sheet.png").Arguments);
        Assert.AreEqual(FFMpeg.Watermark(TestResources.Mp4Video, TestResources.PngImage, output).Arguments,
            FFMpeg.Watermark(source, TestResources.PngImage, output).Arguments);
        Assert.AreEqual(FFMpeg.ExtractAudio(TestResources.Mp4Video, "out.m4a").Arguments, FFMpeg.ExtractAudio(source, "out.m4a").Arguments);
        using var subtitled = CreateVideoWithSubtitles();
        Assert.AreEqual(FFMpeg.ExtractSubtitles(subtitled, "out.srt").Arguments, FFMpeg.ExtractSubtitles(FFProbe.Analyse(subtitled), "out.srt").Arguments);
        Assert.AreEqual(FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, output).Arguments,
            FFMpeg.PosterWithAudio(FFProbe.Analyse(TestResources.PngImage), FFProbe.Analyse(TestResources.Mp3Audio), output).Arguments);
    }

    [TestMethod]
    public void Video_EveryHelperButSaveStream_KnowsItsDurationForPercentageProgress()
    {
        using var subtitled = CreateVideoWithSubtitles();
        var processors = new Dictionary<string, FFMpegArgumentProcessor>
        {
            ["Snapshot"] = FFMpeg.Snapshot(TestResources.Mp4Video, "out.png"),
            ["GifSnapshot"] = FFMpeg.GifSnapshot(TestResources.Mp4Video, "out.gif"),
            ["ThumbnailSheet"] = FFMpeg.ThumbnailSheet(TestResources.Mp4Video, "out.png"),
            ["JoinImageSequence"] = FFMpeg.JoinImageSequence("out.mp4", 1, TestResources.PngImage),
            ["PosterWithAudio"] = FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, "out.mp4"),
            ["Watermark"] = FFMpeg.Watermark(TestResources.Mp4Video, TestResources.PngImage, "out.mp4"),
            ["AddSubtitles"] = FFMpeg.AddSubtitles(TestResources.Mp4Video, TestResources.SrtSubtitle, "out.mkv"),
            ["ExtractSubtitles"] = FFMpeg.ExtractSubtitles(subtitled, "out.srt"),
            ["Remux"] = FFMpeg.Remux(TestResources.Mp4Video, "out.mkv"),
            ["Concat"] = FFMpeg.Concat("out.mp4", TestResources.Mp4Video, TestResources.Mp4Video),
            ["Join"] = FFMpeg.Join("out.mp4", TestResources.Mp4Video, TestResources.Mp4Video),
            ["Trim"] = FFMpeg.Trim(TestResources.Mp4Video, "out.mp4", TimeSpan.Zero, TimeSpan.FromSeconds(1)),
            ["RemoveAudio"] = FFMpeg.RemoveAudio(TestResources.Mp4Video, "out.mp4"),
            ["ExtractAudio"] = FFMpeg.ExtractAudio(TestResources.Mp4Video, "out.m4a"),
            ["ReplaceAudio"] = FFMpeg.ReplaceAudio(TestResources.Mp4Video, TestResources.Mp3Audio, "out.mp4")
        };

        foreach (var (helper, processor) in processors)
        {
            try
            {
                processor.NotifyOnPercentageProgress(_ => { });
            }
            catch (InvalidOperationException)
            {
                Assert.Fail($"{helper} does not know its output duration");
            }
        }

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            FFMpeg.SaveStream(new Uri("https://example.com/live.m3u8"), "out.ts").NotifyOnPercentageProgress(_ => { }));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ExtractAudio_ReportsPercentageProgressWithoutADuration()
    {
        using var output = new TemporaryFile("out.m4a");
        var percentages = new List<double>();

        var result = FFMpeg.ExtractAudio(TestResources.Mp4Video, output)
            .NotifyOnPercentageProgress(percentages.Add)
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(100.0, percentages.Last());
    }

    [TestMethod]
    public void Video_AnalysisOverload_BuildsTheSameArgumentsForTheMultiInputHelpers()
    {
        using var output = new TemporaryFile("out.mp4");
        var parts = new[] { TestResources.Mp4Video, TestResources.Mp4Video };
        var sources = parts.Select(part => FFProbe.Analyse(part)).ToArray();

        // The concat demuxer names its list file with a fresh guid per call, so compare without it.
        Assert.AreEqual(WithoutConcatFileName(FFMpeg.Concat(output, parts).Arguments),
            WithoutConcatFileName(FFMpeg.Concat(output, sources).Arguments));
        Assert.AreEqual(FFMpeg.Join(output, parts).Arguments, FFMpeg.Join(output, sources).Arguments);

        static string WithoutConcatFileName(string arguments)
        {
            return System.Text.RegularExpressions.Regex.Replace(arguments, "concat_[0-9a-fA-F-]+", "concat_list");
        }
    }

    [TestMethod]
    public void Video_AnalysisOverload_RejectsAnAnalysisThatCameFromAStream()
    {
        using var output = new TemporaryFile("out.mkv");
        using var stream = File.OpenRead(TestResources.WebmVideo);
        var source = FFProbe.Analyse(stream);

        Assert.IsNull(source.Path);
        var exception = Assert.ThrowsExactly<ArgumentException>(() => FFMpeg.Remux(source, output));
        Assert.Contains("came from a stream", exception.Message);
    }

    [TestMethod]
    public void Video_Subtitles_CodecNameSpellsTheSameThingAsTheConstant()
    {
        using var output = new TemporaryFile("out.mp4");

        var byConstant = FFMpeg.AddSubtitles(TestResources.Mp4Video, TestResources.SrtSubtitle, output, "eng", SubtitleCodec.MovText).Arguments;
        var byName = FFMpeg.AddSubtitles(TestResources.Mp4Video, TestResources.SrtSubtitle, output, "eng", "mov_text").Arguments;

        Assert.AreEqual(byConstant, byName);
        Assert.Contains("-c:s mov_text", byName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Subtitles_RoundTripThroughASoftStream()
    {
        using var withSubtitles = new TemporaryFile("out.mkv");
        using var extracted = new TemporaryFile("out.srt");

        var added = FFMpeg.AddSubtitles(TestResources.Mp4Video, TestResources.SrtSubtitle, withSubtitles, "eng")
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(added.Success);

        var analysis = FFProbe.Analyse(withSubtitles);
        Assert.IsNotEmpty(analysis.SubtitleStreams);
        Assert.IsNotEmpty(analysis.VideoStreams);
        Assert.IsNotEmpty(analysis.AudioStreams);
        Assert.AreEqual("eng", analysis.PrimarySubtitleStream!.Language);

        Assert.AreEqual(2, analysis.PrimarySubtitleStream.Index);
        Assert.AreEqual(FFMpeg.ExtractSubtitles(analysis, extracted).Arguments, FFMpeg.ExtractSubtitles(analysis, extracted, 2).Arguments);

        var pulled = FFMpeg.ExtractSubtitles(analysis, extracted, analysis.PrimarySubtitleStream.Index)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(pulled.Success);

        var cues = File.ReadAllText(extracted);
        Assert.Contains("00:00:0", cues);
        Assert.IsNotEmpty(File.ReadAllText(TestResources.SrtSubtitle));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ScaleToVideoSize_KeepsTheWidthEvenForYuv420Encoders()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4VideoRotation)
            .OutputToFile(outputFile, true, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .WithVideoFilters(filters => filters.Scale(VideoSize.Hd)))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        var video = FFProbe.Analyse(outputFile).PrimaryVideoStream!;
        Assert.AreEqual(0, video.Width % 2);
        Assert.AreEqual(720, video.Height);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ThumbnailSheet_TilesTheSampledFrames()
    {
        using var outputFile = new TemporaryFile("out.png");

        var success = FFMpeg.ThumbnailSheet(TestResources.Mp4Video, outputFile, 3, 2, tileSize: new Size(160, 90))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        var sheet = FFProbe.Analyse(outputFile).PrimaryVideoStream!;
        Assert.AreEqual(480, sheet.Width);
        Assert.AreEqual(180, sheet.Height);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ThumbnailSheet_ScalesTileHeightFromItsWidth()
    {
        using var outputFile = new TemporaryFile("out.jpg");

        var success = FFMpeg.ThumbnailSheet(TestResources.Mp4Video, outputFile, 2, 2, TimeSpan.FromSeconds(1), new Size(320, -1))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        var sheet = FFProbe.Analyse(outputFile).PrimaryVideoStream!;
        Assert.AreEqual(640, sheet.Width);
        Assert.AreEqual(360, sheet.Height);
    }

    [TestMethod]
    public void Video_ThumbnailSheet_RejectsNonImageExtension()
    {
        Assert.ThrowsExactly<ArgumentException>(() => FFMpeg.ThumbnailSheet(TestResources.Mp4Video, "out.mp4"));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow("out.mkv")]
    [DataRow("out.mov")]
    [DataRow("out.ts")]
    public void Video_Remux_KeepsEveryStreamAsItWas(string filename)
    {
        using var outputFile = new TemporaryFile(filename);

        var success = FFMpeg.Remux(TestResources.Mp4Video, outputFile)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var result = FFProbe.Analyse(outputFile);
        Assert.AreEqual(input.PrimaryVideoStream!.CodecName, result.PrimaryVideoStream!.CodecName);
        Assert.AreEqual(input.PrimaryAudioStream!.CodecName, result.PrimaryAudioStream!.CodecName);
        Assert.AreEqual(input.PrimaryVideoStream.Width, result.PrimaryVideoStream.Width);
        Assert.AreEqual(input.Duration.Seconds, result.Duration.Seconds);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Remux_FailsWhenTheContainerCannotMuxTheStreams()
    {
        using var outputFile = new TemporaryFile("out.webm");

        var result = FFMpeg.Remux(TestResources.Mp4Video, outputFile)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously(false);

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Concat_CopiesTheStreams()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = FFMpeg.Concat(outputFile, TestResources.Mp4Video, TestResources.Mp4VideoRotation)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var result = FFProbe.Analyse(outputFile);
        Assert.AreEqual((input.Duration * 2).Seconds, result.Duration.Seconds);
        Assert.AreEqual(input.PrimaryVideoStream!.CodecName, result.PrimaryVideoStream!.CodecName);
        Assert.AreEqual(input.PrimaryAudioStream!.CodecName, result.PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    public void Video_ReencodingHelpers_TakeTheEncodeFromTheirOutputOptions()
    {
        Action<FFMpegOutputOptions> x265 = options => options.WithVideoCodec(VideoCodec.LibX265).WithConstantRateFactor(28);

        var watermark = FFMpeg.Watermark(TestResources.Mp4Video, TestResources.PngImage, "out.mp4", addArguments: x265).Arguments;
        var poster = FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, "out.mp4", addArguments: x265).Arguments;
        var sequence = FFMpeg.JoinImageSequence("out.mp4", new[] { TestResources.PngImage }, 1, x265).Arguments;

        foreach (var arguments in new[] { watermark, poster, sequence })
        {
            StringAssert.Contains(arguments, "-c:v libx265 -crf 28");
            Assert.DoesNotContain("libx264", arguments);
            Assert.DoesNotContain("yuv420p", arguments);
        }

        Assert.DoesNotContain("-c:a copy", watermark);
        StringAssert.Contains(poster, "-c:a copy -shortest");
        StringAssert.Contains(FFMpeg.Watermark(TestResources.Mp4Video, TestResources.PngImage, "out.mp4").Arguments, "-c:a copy");
    }

    [TestMethod]
    public void Video_Join_LeavesTheEncoderToTheContainerByDefault()
    {
        var arguments = FFMpeg.Join("out.webm", TestResources.Mp4Video, TestResources.Mp4Video).Arguments;

        Assert.DoesNotContain("-c:", arguments);
        Assert.DoesNotContain("-b:", arguments);
        Assert.DoesNotContain("-preset", arguments);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Join_TakesOutputOptions()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = FFMpeg.Join(outputFile, new[] { TestResources.Mp4WithoutAudio, TestResources.Mp4WithoutAudio },
                options => options.WithVideoCodec(VideoCodec.LibX264).WithConstantRateFactor(30).WithSpeedPreset(EncoderPreset.UltraFast))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
        Assert.AreEqual("h264", FFProbe.Analyse(outputFile).PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_DemuxConcat()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = FFMpegArguments
            .FromConcatDemuxerInput(new[] { TestResources.Mp4Video, TestResources.Mp4Video })
            .OutputToFile(outputFile, true, options => options.CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);

        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var result = FFProbe.Analyse(outputFile);
        Assert.AreEqual((input.Duration * 2).Seconds, result.Duration.Seconds);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_DemuxConcat_ResolvesRelativePaths_AgainstPerRunWorkingDirectory()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var options = new FFOptions { WorkingDirectory = Path.GetFullPath(TestResources.ImageCollection + "/..") };

        var result = FFMpegArguments
            .FromConcatDemuxerInput(new[] { Path.GetFileName(TestResources.Mp4Video), Path.GetFileName(TestResources.Mp4Video) })
            .OutputToFile(outputFile, true, o => o.CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously(true, options);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(6, FFProbe.Analyse(outputFile).Duration.Seconds);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_FileInputs_ResolveRelativePaths_AgainstPerRunWorkingDirectory()
    {
        var options = new FFOptions { WorkingDirectory = Path.GetFullPath(TestResources.ImageCollection + "/..") };
        var images = Directory.GetFiles(TestResources.ImageCollection).Select(image => Path.Combine("images", Path.GetFileName(image)));

        var result = FFMpegArguments
            .FromFileInput(Path.GetFileName(TestResources.Mp4Video))
            .AddImageSequenceInput(images)
            .OutputToNull(o => o.WithMap(0))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously(true, options);

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_OutputWithoutOverwrite_ChecksThePerRunWorkingDirectory()
    {
        using var existing = new TemporaryFile("out.mp4");
        File.WriteAllText(existing, string.Empty);
        var options = new FFOptions { WorkingDirectory = Path.GetDirectoryName(existing)! };

        Assert.ThrowsExactly<IOException>(() => FFMpegArguments
            .FromFileInput(Path.GetFullPath(TestResources.Mp4Video))
            .OutputToFile(Path.GetFileName(existing), false)
            .ProcessSynchronously(true, options, TestContext.CancellationToken));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_AddMetadata_KeepsSpecialCharactersInValues()
    {
        using var outputFile = new TemporaryFile("out.mkv");
        const string title = @"AC\DC = best; #1";
        const string chapterTitle = @"Part=1\2; #a";

        FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .AddMetadata(new FFMetadataBuilder().WithTitle(title).WithChapter(chapterTitle, TimeSpan.FromSeconds(1)))
            .OutputToFile(outputFile, true, o => o.CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputFile);
        Assert.AreEqual(title, analysis.Format.Tags!["title"]);
        Assert.AreEqual(chapterTitle, analysis.Chapters[0].Title);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ProbedChapterWithoutTitle_HasAnEmptyTitle()
    {
        using var metadataFile = new TemporaryFile("chapters.txt");
        using var outputFile = new TemporaryFile("out.mkv");
        File.WriteAllText(metadataFile, ";FFMETADATA1\n[CHAPTER]\nTIMEBASE=1/1000\nSTART=0\nEND=1000\n");

        FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .AddMetadataFile(metadataFile)
            .OutputToFile(outputFile, true, o => o.CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var chapter = FFProbe.Analyse(outputFile).Chapters.Single();
        Assert.AreEqual(string.Empty, chapter.Title);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TempFileArguments_UsePerRunTemporaryFolder()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempFolder);
        var stderr = new List<string>();
        try
        {
            FFMpegArguments
                .FromConcatDemuxerInput(new[] { TestResources.Mp4Video })
                .AddMetadata(new FFMetadataBuilder().WithTitle("title"))
                .OutputToFile(outputFile, true, o => o.CopyStreams())
                .NotifyOnStandardError(stderr.Add)
                .CancellableThrough(TestContext.CancellationToken)
                .ProcessSynchronously(true, new FFOptions { TemporaryFilesFolder = tempFolder });

            Assert.IsTrue(stderr.Any(line => line.Contains("concat_") && line.Contains(tempFolder)), string.Join("\n", stderr));
            Assert.IsTrue(stderr.Any(line => line.Contains("metadata_") && line.Contains(tempFolder)), string.Join("\n", stderr));
            Assert.IsEmpty(Directory.GetFileSystemEntries(tempFolder));
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TempFileArguments_AreRemovedWhenALaterInputIsMissing()
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempFolder);
        var pipeInput = new InputPipeArgument(new StreamPipeSource(new MemoryStream()));
        try
        {
            Assert.ThrowsExactly<FileNotFoundException>(() => FFMpegArguments
                .FromImageSequenceInput(Directory.GetFiles(TestResources.ImageCollection))
                .AddConcatDemuxerInput(new[] { TestResources.Mp4Video })
                .AddMetadata(new FFMetadataBuilder().WithTitle("title"))
                .AddInput(pipeInput)
                .AddFileInput("missing.mp4")
                .OutputToNull()
                .ProcessSynchronously(true, new FFOptions { TemporaryFilesFolder = tempFolder }, TestContext.CancellationToken));

            Assert.IsEmpty(Directory.GetFileSystemEntries(tempFolder));
            if (!OperatingSystem.IsWindows())
            {
                Assert.IsFalse(File.Exists(pipeInput.PipePath.Substring("unix:".Length)));
            }
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ImageSequence_UsesPerRunTemporaryFolder()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempFolder);
        var stderr = new List<string>();
        try
        {
            var images = Directory.GetFiles(TestResources.ImageCollection).OrderBy(image => image).ToArray();
            FFMpeg.JoinImageSequence(outputFile, 10, images)
                .NotifyOnStandardError(stderr.Add)
                .CancellableThrough(TestContext.CancellationToken)
                .ProcessSynchronously(true, new FFOptions { TemporaryFilesFolder = tempFolder });

            Assert.IsTrue(stderr.Any(line => line.Contains(tempFolder)), string.Join("\n", stderr));
            Assert.IsEmpty(Directory.GetFileSystemEntries(tempFolder));
        }
        finally
        {
            Directory.Delete(tempFolder, true);
        }
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TeeOutput_WritesEveryTarget()
    {
        using var first = new TemporaryFile("tee'first.mp4");
        using var second = new TemporaryFile("second.mp4");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToTee(outputs => outputs
                    .OutputToFile(first, true, options => options.ForceFormat("mp4"))
                    .OutputToFile(second, true, options => options.ForceFormat("mp4")),
                options => options.WithCustomArgument("-map 0").CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);

        Assert.AreEqual(3, FFProbe.Analyse(first).Duration.Seconds);
        Assert.AreEqual(3, FFProbe.Analyse(second).Duration.Seconds);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TeeOutput_SelectsStreamsPerTarget()
    {
        using var everything = new TemporaryFile("everything.mp4");
        using var videoOnly = new TemporaryFile("video-only.mp4");

        var result = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToTee(outputs => outputs
                    .OutputToFile(everything, options => options.ForceFormat(ContainerFormats.Mp4))
                    .OutputToFile(videoOnly, options => options.ForceFormat(ContainerFormats.Mp4).WithSelect(StreamType.Video)),
                options => options.WithMap(0).CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(result.Success);
        Assert.IsNotEmpty(FFProbe.Analyse(everything).AudioStreams);
        Assert.IsEmpty(FFProbe.Analyse(videoOnly).AudioStreams);
        Assert.IsNotEmpty(FFProbe.Analyse(videoOnly).VideoStreams);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_TeeOutput_WritesToAPipe()
    {
        using var file = new TemporaryFile("out.ts");
        using var piped = new MemoryStream();

        var result = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToTee(outputs => outputs
                    .OutputToFile(file, options => options.ForceFormat(ContainerFormats.Ts))
                    .OutputToPipe(new StreamPipeSink(piped), options => options.ForceFormat(ContainerFormats.Ts)),
                options => options.WithMap(0).CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(new FileInfo(file).Length, piped.Length);
    }

    [TestMethod]
    public void Video_TeeOutput_HonoursOverwriteFalseOnEachTarget()
    {
        using var overwritable = new TemporaryFile("first.mp4");
        using var existing = new TemporaryFile("second.mp4");
        File.WriteAllText(existing, "keep me");

        var processor = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToTee(outputs => outputs
                .OutputToFile(overwritable, true, options => options.ForceFormat("mp4"))
                .OutputToFile(existing, false, options => options.ForceFormat("mp4")));

        Assert.ThrowsExactly<IOException>(() => processor.ProcessSynchronously());
        Assert.AreEqual("keep me", File.ReadAllText(existing));
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_AddMetadata_FollowedByAnotherInput()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var result = FFMpegArguments
            .FromFileInput(TestResources.Mp4WithoutAudio)
            .AddMetadata(new FFMetadataBuilder().WithTitle("with audio"))
            .AddFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true, options => options
                .WithMap(0, StreamType.Video)
                .WithMap(2, StreamType.Audio)
                .CopyStreams())
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var analysis = FFProbe.Analyse(outputFile);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("with audio", analysis.Format.Tags!["title"]);
        Assert.HasCount(1, analysis.AudioStreams);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_EncoderTuning_IsAcceptedByTheEncoders()
    {
        using var video = new TemporaryFile("tuned.mp4");
        using var audio = new TemporaryFile("tuned.mp3");

        var videoResult = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(video, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .WithVideoProfile(VideoProfile.Main)
                .WithTune(EncoderTune.FastDecode)
                .WithGopSize(25)
                .WithMaxBitrate(1000)
                .WithBufferSize(2000)
                .WithAudioCodec(AudioCodec.Aac)
                .WithAudioChannels(1))
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);
        var audioResult = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(audio, options => options
                .DisableVideo()
                .WithAudioCodec(AudioCodec.LibMp3Lame)
                .WithAudioQualityScale(4))
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);

        var analysis = FFProbe.Analyse(video);
        Assert.IsTrue(videoResult.Success);
        Assert.IsTrue(audioResult.Success);
        Assert.AreEqual("Main", analysis.PrimaryVideoStream!.Profile);
        Assert.AreEqual(1, analysis.PrimaryAudioStream!.Channels);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_MetadataAndDisposition_ReachTheOutput()
    {
        using var subtitled = new TemporaryFile("subtitled.mkv");
        using var outputFile = new TemporaryFile("out.mkv");
        FFMpeg.AddSubtitles(TestResources.Mp4Video, TestResources.SrtSubtitle, subtitled, "eng").ProcessSynchronously();

        var result = FFMpegArguments
            .FromFileInput(subtitled)
            .OutputToFile(outputFile, options => options
                .WithMap(0)
                .CopyStreams()
                .WithMetadata("title", "Say \"hi\"")
                .WithStreamMetadata("language", "dan", StreamType.Audio, 0)
                .WithDisposition(StreamDisposition.Default + StreamDisposition.Forced, StreamType.Subtitle, 0))
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);

        var analysis = FFProbe.Analyse(outputFile);
        Assert.IsTrue(result.Success);
        Assert.AreEqual("eng", FFProbe.Analyse(subtitled).PrimarySubtitleStream!.Language);
        Assert.AreEqual("Say \"hi\"", analysis.Format.Tags!["title"]);
        Assert.AreEqual("dan", analysis.PrimaryAudioStream!.Language);
        Assert.IsTrue(analysis.PrimarySubtitleStream!.Disposition!["forced"]);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_OutputToMany_WritesEveryOutput()
    {
        using var mp4 = new TemporaryFile("many.mp4");
        using var mkv = new TemporaryFile("many.mkv");

        var result = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToMany(outputs => outputs
                .OutputToFile(mp4, true, options => options.CopyStreams())
                .OutputToFile(mkv, true, options => options.CopyStreams().ForceFormat("matroska")))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, FFProbe.Analyse(mp4).Duration.Seconds);
        Assert.AreEqual(3, FFProbe.Analyse(mkv).Duration.Seconds);
    }
}
