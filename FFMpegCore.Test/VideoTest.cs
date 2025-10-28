using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using System.Text;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
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
        using var outputFile = new TemporaryFile($"out{VideoType.Ogv.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_YUV444p()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264)
                .ForcePixelFormat("yuv444p"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
        var analysis = FFProbe.Analyse(outputFile);
        Assert.AreEqual("yuv444p", analysis.VideoStreams.First().PixelFormat);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToMP4_Args()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_MetadataBuilder()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        await FFMpegArguments
            .FromFileInput(TestResources.WebmVideo)
            .AddMetaData(FFMetadataBuilder.Empty()
                .WithTag("title", "noname")
                .WithTag("artist", "unknown")
                .WithChapter("Chapter 1", 1.1)
                .WithChapter("Chapter 2", 1.23))
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
        Assert.IsTrue(success);
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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));
        var success = FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

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
        using var output = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        var success = FFMpegArguments
            .FromPipeInput(new StreamPipeSource(input))
            .OutputToFile(output, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToMP4_Args_StreamOutputPipe_Async_Failure()
    {
        await Assert.ThrowsExactlyAsync<FFMpegException>(async () =>
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
        Assert.ThrowsExactly<FFMpegException>(() =>
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
        Assert.IsTrue(success);

        output.Position = 0;
        var inputAnalysis = FFProbe.Analyse(TestResources.WebmVideo);
        var outputAnalysis = FFProbe.Analyse(output);
        Assert.AreEqual(inputAnalysis.Duration.TotalSeconds, outputAnalysis.Duration.TotalSeconds, 0.3);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToTS()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.MpegTs.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_ToTS_Args()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.MpegTs.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .CopyChannel()
                .WithBitStreamFilter(Channel.Video, Filter.H264_Mp4ToAnnexB)
                .ForceFormat(VideoType.MpegTs))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
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
        using var output = new TemporaryFile($"out{VideoType.Ts.Extension}");
        var input = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));

        var success = await FFMpegArguments
            .FromPipeInput(input)
            .OutputToFile(output, false, opt => opt
                .ForceFormat(VideoType.Ts))
            .CancellableThrough(cancellationToken)
            .ProcessAsynchronously();
        Assert.IsTrue(success);

        var analysis = await FFProbe.AnalyseAsync(output);
        Assert.AreEqual(VideoType.Ts.Name, analysis.Format.FormatName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_ToOGV_Resize()
    {
        using var outputFile = new TemporaryFile($"out{VideoType.Ogv.Extension}");
        var success = await FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .Resize(200, 200)
                .WithVideoCodec(VideoCodec.LibTheora))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();
        Assert.IsTrue(success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(SKColorType.Rgb565)]
    [DataRow(SKColorType.Bgra8888)]
    public void RawVideoPipeSource_Ogv_Scale(SKColorType pixelFormat)
    {
        using var outputFile = new TemporaryFile($"out{VideoType.Ogv.Extension}");
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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");

        var success = FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(outputFile, false, opt => opt
                .UsingMultithreading(true)
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
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
        using var outputFile = new TemporaryFile($"out{VideoType.Mp4.Extension}");
        var videoFramesSource = new RawVideoPipeSource(BitmapSource.CreateBitmaps(64, pixelFormat, 256, 256));

        var success = FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithVideoCodec(VideoCodec.LibX264))
            .CancellableThrough(cancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_InMemory_SystemDrawingCommon()
    {
        using var bitmap = FFMpegImage.Snapshot(TestResources.Mp4Video);

        var input = FFProbe.Analyse(TestResources.Mp4Video);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
    }

    [SupportedOSPlatform("windows")]
    [OsSpecificTestMethod(OsPlatforms.Windows)]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_SnapshotAsync_InMemory_SystemDrawingCommon()
    {
        using var bitmap = await FFMpegImage.SnapshotAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);

        var input = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        Assert.AreEqual(bitmap.RawFormat, ImageFormat.Png);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_InMemory_SkiaSharp()
    {
        using var bitmap = Extensions.SkiaSharp.FFMpegImage.Snapshot(TestResources.Mp4Video);

        var input = FFProbe.Analyse(TestResources.Mp4Video);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        // Note: The resulting ColorType is dependent on the execution environment and therefore not assessed,
        // e.g. Bgra8888 on Windows and Rgba8888 on macOS.
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Video_SnapshotAsync_InMemory_SkiaSharp()
    {
        using var bitmap = await Extensions.SkiaSharp.FFMpegImage.SnapshotAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);

        var input = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, bitmap.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, bitmap.Height);
        // Note: The resulting ColorType is dependent on the execution environment and therefore not assessed,
        // e.g. Bgra8888 on Windows and Rgba8888 on macOS.
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Png_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.png");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath);

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

        await FFMpeg.SnapshotAsync(TestResources.Mp4Video, outputPath, cancellationToken: TestContext.CancellationToken);

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

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath);

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

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath);

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

        FFMpeg.Snapshot(TestResources.Mp4Video, outputPath);

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("webp", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Exception_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.asd");

        try
        {
            FFMpeg.Snapshot(TestResources.Mp4Video, outputPath);
        }
        catch (Exception ex)
        {
            Assert.IsTrue(ex is ArgumentException);
        }
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Snapshot_Rotated_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.png");

        var size = new Size(360, 0); // half the size of original video, keeping height 0 for keeping aspect ratio
        FFMpeg.Snapshot(TestResources.Mp4VideoRotationNegative, outputPath, size);

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreEqual(size.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreEqual(1280 / 2, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual(0, analysis.PrimaryVideoStream!.Rotation);
        Assert.AreEqual("png", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_GifSnapshot_PersistSnapshot()
    {
        using var outputPath = new TemporaryFile("out.gif");
        var input = FFProbe.Analyse(TestResources.Mp4Video);

        FFMpeg.GifSnapshot(TestResources.Mp4Video, outputPath, captureTime: TimeSpan.FromSeconds(0));

        var analysis = FFProbe.Analyse(outputPath);
        Assert.AreNotEqual(input.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Width);
        Assert.AreNotEqual(input.PrimaryVideoStream.Height, analysis.PrimaryVideoStream!.Height);
        Assert.AreEqual("gif", analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_GifSnapshot_PersistSnapshot_SizeSupplied()
    {
        using var outputPath = new TemporaryFile("out.gif");
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var desiredGifSize = new Size(320, 240);

        FFMpeg.GifSnapshot(TestResources.Mp4Video, outputPath, desiredGifSize, TimeSpan.FromSeconds(0));

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

        await FFMpeg.GifSnapshotAsync(TestResources.Mp4Video, outputPath, captureTime: TimeSpan.FromSeconds(0),
            cancellationToken: TestContext.CancellationToken);

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

        await FFMpeg.GifSnapshotAsync(TestResources.Mp4Video, outputPath, desiredGifSize, TimeSpan.FromSeconds(0),
            cancellationToken: TestContext.CancellationToken);

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
        var success = FFMpeg.Join(outputPath, TestResources.Mp4Video, inputCopy);
        Assert.IsTrue(success);
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
    public void Video_Convert_Webm()
    {
        using var outputPath = new TemporaryFile("out.webm");
        
        var success = FFMpeg.Convert(TestResources.Mp4Video, outputPath, VideoType.WebM);
        Assert.IsTrue(success);
        Assert.IsTrue(File.Exists(outputPath));
        
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var result = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.Duration.Days, result.Duration.Days);
        Assert.AreEqual(input.Duration.Hours, result.Duration.Hours);
        Assert.AreEqual(input.Duration.Minutes, result.Duration.Minutes);
        Assert.AreEqual(input.Duration.Seconds, result.Duration.Seconds);
        Assert.AreEqual(input.PrimaryVideoStream!.Height, result.PrimaryVideoStream!.Height);
        Assert.AreEqual(input.PrimaryVideoStream.Width, result.PrimaryVideoStream.Width);
    }
    
    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Video_Convert_Ogv()
    {
        using var outputPath = new TemporaryFile("out.ogv");
        
        var success = FFMpeg.Convert(TestResources.Mp4Video, outputPath, VideoType.Ogv);
        Assert.IsTrue(success);
        Assert.IsTrue(File.Exists(outputPath));
        
        var input = FFProbe.Analyse(TestResources.Mp4Video);
        var result = FFProbe.Analyse(outputPath);
        Assert.AreEqual(input.Duration.Days, result.Duration.Days);
        Assert.AreEqual(input.Duration.Hours, result.Duration.Hours);
        Assert.AreEqual(input.Duration.Minutes, result.Duration.Minutes);
        Assert.AreEqual(input.Duration.Seconds, result.Duration.Seconds);
        Assert.AreEqual(input.PrimaryVideoStream!.Height, result.PrimaryVideoStream!.Height);
        Assert.AreEqual(input.PrimaryVideoStream.Width, result.PrimaryVideoStream.Width);
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
        var success = FFMpeg.JoinImageSequence(outputFile, 10, imageSet.ToArray());
        Assert.IsTrue(success);
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
            .NotifyOnProgress(OnPercentageProgess, analysis.Duration)
            .NotifyOnProgress(OnTimeProgess)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success);
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
            .WithGlobalOptions(options => options
                .WithVerbosityLevel(VerbosityLevel.Info))
            .OutputToFile(outputFile, false, opt => opt
                .WithDuration(TimeSpan.FromSeconds(2)))
            .NotifyOnError(_ => dataReceived = true)
            .Configure(opt => opt.Encoding = Encoding.UTF8)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(dataReceived);
        Assert.IsTrue(success);
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
                .WithVideoCodec("vp9")
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
                .WithVideoCodec("vp9")
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
                .WithSpeedPreset(Speed.VeryFast))
            .CancellableThrough(out var cancel)
            .CancellableThrough(TestContext.CancellationToken)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously(false);

        await Task.Delay(300, TestContext.CancellationToken);
        cancel();

        var result = await task;

        Assert.IsFalse(result);
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
                .WithSpeedPreset(Speed.VeryFast))
            .CancellableThrough(out var cancel)
            .CancellableThrough(TestContext.CancellationToken);

        Task.Delay(300, TestContext.CancellationToken).ContinueWith(_ => cancel(), TestContext.CancellationToken);

        var result = task.CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously(false);

        Assert.IsFalse(result);
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
                .WithSpeedPreset(Speed.VeryFast))
            .CancellableThrough(out var cancel, 10000)
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
                .WithSpeedPreset(Speed.VeryFast))
            .CancellableThrough(cts.Token)
            .ProcessAsynchronously(false);

        cts.CancelAfter(300);

        var result = await task;

        Assert.IsFalse(result);
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
                .WithSpeedPreset(Speed.VeryFast))
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
                .WithSpeedPreset(Speed.VeryFast))
            .CancellableThrough(cts.Token);

        cts.CancelAfter(300);

        Assert.ThrowsExactly<OperationCanceledException>(() => task.CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
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
                .WithSpeedPreset(Speed.VeryFast))
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
                .WithSpeedPreset(Speed.VeryFast));

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
                .WithSpeedPreset(Speed.VeryFast))
            .CancellableThrough(cts.Token, 8000)
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
}
