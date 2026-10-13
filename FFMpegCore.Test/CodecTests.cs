using System.Runtime.Versioning;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Test.Utilities;

namespace FFMpegCore.Test;

[TestClass]
public class CodecTests
{
    [TestMethod]
    public void Codecs_Enumerate()
    {
        var all = FFMpeg.GetCodecs();
        var video = FFMpeg.GetVideoCodecs();
        var audio = FFMpeg.GetAudioCodecs();
        var subtitle = FFMpeg.GetSubtitleCodecs();
        var data = FFMpeg.GetDataCodecs();

        Assert.IsNotEmpty(all);
        Assert.IsNotEmpty(video);
        Assert.IsNotEmpty(audio);
        Assert.IsNotEmpty(subtitle);
        Assert.IsNotEmpty(data);
        Assert.IsTrue(video.All(codec => codec.Type == CodecType.Video));
        Assert.IsTrue(audio.All(codec => codec.Type == CodecType.Audio));
        Assert.IsTrue(subtitle.All(codec => codec.Type == CodecType.Subtitle));
        Assert.IsTrue(data.All(codec => codec.Type == CodecType.Data));
        CollectionAssert.AreEquivalent(all.ToList(), FFMpeg.GetCodecs(CodecType.Video).Concat(audio).Concat(subtitle).Concat(data).ToList());
    }

    [TestMethod]
    public void Codecs_TryGetExisting()
    {
        Assert.IsTrue(FFMpeg.TryGetCodec("aac", out var codec));
        Assert.AreEqual(CodecType.Audio, codec.Type);
    }

    [TestMethod]
    public void Codecs_TryGetNotExisting()
    {
        Assert.IsFalse(FFMpeg.TryGetCodec("not-a-codec", out _));
    }

    [TestMethod]
    public void Codecs_GetNotExisting()
    {
        Assert.ThrowsExactly<FFMpegException>(() => FFMpeg.GetCodec("not-a-codec"));
    }

    [TestMethod]
    public void ContainerFormats_Enumerate()
    {
        var formats = FFMpeg.GetContainerFormats();

        Assert.IsNotEmpty(formats);
        Assert.IsTrue(formats.Any(format => format.Name == "mp4"));
    }

    [TestMethod]
    public void ContainerFormats_TryGetExisting()
    {
        Assert.IsTrue(FFMpeg.TryGetContainerFormat("mp4", out var format));
        Assert.AreEqual(".mp4", format.GetExtension());
    }

    [TestMethod]
    public void ContainerFormats_ExtensionOverrides_ApplyPerRun()
    {
        var options = new FFOptions { ExtensionOverrides = { ["mpegts"] = ".mts", ["matroska"] = ".mkv" } };

        Assert.AreEqual(".mts", ContainerFormats.Ts.GetExtension(options));
        Assert.AreEqual(".mkv", new ContainerFormat("matroska").GetExtension(options));
        Assert.AreEqual(".ts", ContainerFormats.Ts.GetExtension());
    }

    [TestMethod]
    public void ContainerFormats_ExtensionsOfFormatsNamedOtherThanTheirFiles()
    {
        Assert.AreEqual(".mkv", ContainerFormats.Matroska.GetExtension());
        Assert.AreEqual(".m3u8", ContainerFormats.Hls.GetExtension());
        Assert.AreEqual(".flac", ContainerFormats.Flac.GetExtension());
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void ContainerFormats_DemuxersListedUnderSeveralNames_AreFoundUnderEach(bool useCache)
    {
        var options = new FFOptions { UseCache = useCache };

        var mov = FFMpeg.GetContainerFormat("mov", options);
        Assert.IsTrue(mov.DemuxingSupported);
        Assert.IsTrue(mov.MuxingSupported);
        Assert.IsTrue(FFMpeg.GetContainerFormat("m4a", options).DemuxingSupported);
        Assert.IsTrue(FFMpeg.GetContainerFormat("matroska", options).DemuxingSupported);
        Assert.AreEqual(1, FFMpeg.GetContainerFormats(options).Count(format => format.Name == "mov"));
    }

    [TestMethod]
    public void ContainerFormats_TryGetNotExisting()
    {
        Assert.IsFalse(FFMpeg.TryGetContainerFormat("not-a-container", out _));
    }

    [TestMethod]
    public void ContainerFormats_GetNotExisting()
    {
        Assert.ThrowsExactly<FFMpegException>(() => FFMpeg.GetContainerFormat("not-a-container"));
    }

    [TestMethod]
    [DoNotParallelize]
    public void ContainerFormats_Query_DoesNotRequireThreadPoolThreads()
    {
        ThreadPool.GetMinThreads(out var minWorker, out var minIo);
        ThreadPool.GetMaxThreads(out var maxWorker, out var maxIo);
        try
        {
            Assert.IsTrue(ThreadPool.SetMinThreads(4, minIo));
            Assert.IsTrue(ThreadPool.SetMaxThreads(4, maxIo));

            var gate = new TaskCompletionSource<bool>();
            var lookups = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
            {
                await gate.Task;
                return FFMpeg.GetContainersFormatsInternal(GlobalFFOptions.Current);
            })).ToArray();
            gate.SetResult(true);

            Assert.IsTrue(Task.WaitAll(lookups, TimeSpan.FromSeconds(30)));
            Assert.IsTrue(lookups.All(lookup => lookup.Result.Any(format => format.Name == "mp4")));
        }
        finally
        {
            ThreadPool.SetMaxThreads(maxWorker, maxIo);
            ThreadPool.SetMinThreads(minWorker, minIo);
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void Lookups_IgnoreCaseAndSurroundingSpace_WhetherCachedOrNot(bool useCache)
    {
        var options = new FFOptions { UseCache = useCache };

        Assert.IsTrue(FFMpeg.TryGetCodec(" PNG ", out var codec, options));
        Assert.AreEqual("png", codec.Name);
        Assert.IsTrue(FFMpeg.TryGetPixelFormat(" YUV420P ", out _, options));
        Assert.IsTrue(FFMpeg.TryGetContainerFormat(" MP4 ", out _, options));
    }

    [TestMethod]
    [DoNotParallelize]
    public void Lookups_BypassCache_WhenDisabled()
    {
        try
        {
            GlobalFFOptions.Configure(options => options.UseCache = false);

            Assert.IsNotEmpty(FFMpeg.GetPixelFormats());
            Assert.IsNotEmpty(FFMpeg.GetCodecs());
            Assert.IsNotEmpty(FFMpeg.GetCodecs(CodecType.Audio));
            Assert.IsNotEmpty(FFMpeg.GetContainerFormats());
            Assert.IsTrue(FFMpeg.TryGetPixelFormat("yuv420p", out _));
            Assert.IsFalse(FFMpeg.TryGetPixelFormat("nope", out _));
            Assert.IsTrue(FFMpeg.TryGetCodec("aac", out _));
            Assert.IsFalse(FFMpeg.TryGetCodec("nope", out _));
            Assert.IsTrue(FFMpeg.TryGetContainerFormat("mp4", out _));
            Assert.IsFalse(FFMpeg.TryGetContainerFormat("nope", out _));
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    [OsSpecificTestMethod(OsPlatforms.Linux | OsPlatforms.MacOS)]
    [UnsupportedOSPlatform("windows")]
    public void Lookups_QueryTheBinaryTheOptionsPointAt_NotTheOneAlreadyCached()
    {
        Assert.IsNotEmpty(FFMpeg.GetPixelFormats());

        using var binaryFolder = new TemporaryBinaryFolder("ffmpeg");

        var exception = Assert.ThrowsExactly<FFMpegException>(() => FFMpeg.GetPixelFormats(binaryFolder.Options));
        StringAssert.Contains(exception.Message, binaryFolder.BinaryPath);
    }

    [OsSpecificTestMethod(OsPlatforms.Linux | OsPlatforms.MacOS)]
    [UnsupportedOSPlatform("windows")]
    [DoNotParallelize]
    public void WellKnownCodecsAndFormats_DoNotQueryFFMpeg()
    {
        using var binaryFolder = new TemporaryBinaryFolder("ffmpeg");
        var original = GlobalFFOptions.Current;

        try
        {
            GlobalFFOptions.Configure(binaryFolder.Options);

            Assert.AreEqual("libx264", VideoCodec.LibX264.Name);
            Assert.AreEqual(CodecType.Video, VideoCodec.LibX264.Type);
            Assert.AreEqual("mjpeg", VideoCodec.Image.Jpg.Name);
            Assert.AreEqual("libvpx-vp9", VideoCodec.LibVpxVp9.Name);
            Assert.AreEqual("libsvtav1", VideoCodec.LibSvtAv1.Name);
            Assert.AreEqual("libaom-av1", VideoCodec.LibAomAv1.Name);
            Assert.AreEqual("libopus", AudioCodec.LibOpus.Name);
            Assert.AreEqual("pcm_s16le", AudioCodec.PcmS16Le.Name);
            Assert.AreEqual("aac", AudioCodec.Aac.Name);
            Assert.AreEqual(CodecType.Audio, AudioCodec.Aac.Type);
            Assert.AreEqual("mov_text", SubtitleCodec.MovText.Name);
            Assert.AreEqual(CodecType.Subtitle, SubtitleCodec.MovText.Type);
            Assert.AreEqual(".mp4", ContainerFormats.Mp4.GetExtension());
        }
        finally
        {
            GlobalFFOptions.Configure(original);
        }
    }
}
