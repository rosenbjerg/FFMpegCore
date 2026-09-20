using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

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
        Assert.AreEqual(".mp4", format.Extension);
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
                return FFMpeg.GetContainersFormatsInternal();
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
}
