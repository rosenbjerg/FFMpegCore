using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using FFMpegCore.Test.Resources;

namespace FFMpegCore.Test;

[TestClass]
public class FFProbeTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Audio_FromStream_Duration()
    {
        var fileAnalysis = await FFProbe.AnalyseAsync(TestResources.WebmVideo, cancellationToken: TestContext.CancellationToken);
        await using var inputStream = File.OpenRead(TestResources.WebmVideo);
        var streamAnalysis = await FFProbe.AnalyseAsync(inputStream, cancellationToken: TestContext.CancellationToken);
        Assert.IsTrue(fileAnalysis.Duration == streamAnalysis.Duration);
    }

    [TestMethod]
    public void FrameAnalysis_Sync()
    {
        var frameAnalysis = FFProbe.GetFrames(TestResources.WebmVideo);

        Assert.HasCount(90, frameAnalysis.Frames);
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.PixelFormat == "yuv420p"));
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.Height == 360));
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.Width == 640));
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.MediaType == "video"));
    }

    [TestMethod]
    public async Task FrameAnalysis_Async()
    {
        var frameAnalysis = await FFProbe.GetFramesAsync(TestResources.WebmVideo, cancellationToken: TestContext.CancellationToken);

        Assert.HasCount(90, frameAnalysis.Frames);
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.PixelFormat == "yuv420p"));
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.Height == 360));
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.Width == 640));
        Assert.IsTrue(frameAnalysis.Frames.All(f => f.MediaType == "video"));
    }

    [TestMethod]
    public async Task PacketAnalysis_Async()
    {
        var packetAnalysis = await FFProbe.GetPacketsAsync(TestResources.WebmVideo, cancellationToken: TestContext.CancellationToken);
        var packets = packetAnalysis.Packets;
        Assert.HasCount(96, packets);
        Assert.IsTrue(packets.All(f => f.CodecType == "video"));
        Assert.StartsWith("K_", packets[0].Flags);
        Assert.AreEqual(1362, packets.Last().Size);
    }

    [TestMethod]
    public void PacketAnalysis_Sync()
    {
        var packets = FFProbe.GetPackets(TestResources.WebmVideo).Packets;

        Assert.HasCount(96, packets);
        Assert.IsTrue(packets.All(f => f.CodecType == "video"));
        Assert.StartsWith("K_", packets[0].Flags);
        Assert.AreEqual(1362, packets.Last().Size);
    }

    [TestMethod]
    public void PacketAnalysisAudioVideo_Sync()
    {
        var packets = FFProbe.GetPackets(TestResources.Mp4Video).Packets;

        Assert.HasCount(216, packets);
        var actual = packets.Select(f => f.CodecType).Distinct().ToList();
        var expected = new List<string> { "audio", "video" };
        CollectionAssert.AreEquivalent(expected, actual);
        Assert.IsTrue(packets.Where(t => t.CodecType == "audio").All(f => f.Flags.StartsWith("K_")));
        Assert.AreEqual(75, packets.Count(t => t.CodecType == "video"));
        Assert.AreEqual(141, packets.Count(t => t.CodecType == "audio"));
    }

    [TestMethod]
    [DataRow("0:00:03.008000", 0, 0, 0, 3, 8)]
    [DataRow("05:12:59.177", 0, 5, 12, 59, 177)]
    [DataRow("149:07:50.911750", 6, 5, 7, 50, 911)]
    [DataRow("00:00:00.83", 0, 0, 0, 0, 830)]
    public void MediaAnalysis_ParseDuration(string duration, int expectedDays, int expectedHours, int expectedMinutes, int expectedSeconds,
        int expectedMilliseconds)
    {
        var ffprobeStream = new FFProbeStream { Duration = duration };

        var parsedDuration = MediaAnalysisUtils.ParseDuration(ffprobeStream.Duration);

        Assert.AreEqual(expectedDays, parsedDuration.Days);
        Assert.AreEqual(expectedHours, parsedDuration.Hours);
        Assert.AreEqual(expectedMinutes, parsedDuration.Minutes);
        Assert.AreEqual(expectedSeconds, parsedDuration.Seconds);
        Assert.AreEqual(expectedMilliseconds, parsedDuration.Milliseconds);
    }

    [TestMethod]
    [Ignore("Consistently fails on GitHub Workflow ubuntu agents")]
    public async Task Uri_Duration()
    {
        var fileAnalysis = await FFProbe.AnalyseAsync(new Uri("https://github.com/rosenbjerg/FFMpegCore/raw/master/FFMpegCore.Test/Resources/input_3sec.webm"),
            cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(fileAnalysis);
    }

    [TestMethod]
    public void Probe_Success()
    {
        var info = FFProbe.Analyse(TestResources.Mp4Video);
        Assert.AreEqual(3, info.Duration.Seconds);
        Assert.IsEmpty(info.Chapters);

        Assert.AreEqual("5.1", info.PrimaryAudioStream!.ChannelLayout);
        Assert.AreEqual(6, info.PrimaryAudioStream.Channels);
        Assert.AreEqual("AAC (Advanced Audio Coding)", info.PrimaryAudioStream.CodecLongName);
        Assert.AreEqual("aac", info.PrimaryAudioStream.CodecName);
        Assert.AreEqual("LC", info.PrimaryAudioStream.Profile);
        Assert.AreEqual(377351, info.PrimaryAudioStream.BitRate);
        Assert.AreEqual(48000, info.PrimaryAudioStream.SampleRateHz);
        Assert.AreEqual("mp4a", info.PrimaryAudioStream.CodecTagString);
        Assert.AreEqual("0x6134706d", info.PrimaryAudioStream.CodecTag);

        Assert.AreEqual(1471810, info.PrimaryVideoStream!.BitRate);
        Assert.AreEqual(16, info.PrimaryVideoStream.DisplayAspectRatio.Width);
        Assert.AreEqual(9, info.PrimaryVideoStream.DisplayAspectRatio.Height);
        Assert.AreEqual(1, info.PrimaryVideoStream.SampleAspectRatio.Width);
        Assert.AreEqual(1, info.PrimaryVideoStream.SampleAspectRatio.Height);
        Assert.AreEqual("yuv420p", info.PrimaryVideoStream.PixelFormat);
        Assert.AreEqual(31, info.PrimaryVideoStream.Level);
        Assert.AreEqual(1280, info.PrimaryVideoStream.Width);
        Assert.AreEqual(720, info.PrimaryVideoStream.Height);
        Assert.AreEqual(25, info.PrimaryVideoStream.AvgFrameRate);
        Assert.AreEqual(25, info.PrimaryVideoStream.FrameRate);
        Assert.AreEqual("H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10", info.PrimaryVideoStream.CodecLongName);
        Assert.AreEqual("h264", info.PrimaryVideoStream.CodecName);
        Assert.AreEqual(8, info.PrimaryVideoStream.BitsPerRawSample);
        Assert.AreEqual("Main", info.PrimaryVideoStream.Profile);
        Assert.AreEqual("avc1", info.PrimaryVideoStream.CodecTagString);
        Assert.AreEqual("0x31637661", info.PrimaryVideoStream.CodecTag);
    }

    [TestMethod]
    public void Probe_Rotation()
    {
        var info = FFProbe.Analyse(TestResources.Mp4Video);
        Assert.IsNotNull(info.PrimaryVideoStream);
        Assert.AreEqual(0, info.PrimaryVideoStream.Rotation);

        info = FFProbe.Analyse(TestResources.Mp4VideoRotation);
        Assert.IsNotNull(info.PrimaryVideoStream);
        Assert.AreEqual(90, info.PrimaryVideoStream.Rotation);
    }

    [TestMethod]
    public void Probe_Rotation_Negative_Value()
    {
        var info = FFProbe.Analyse(TestResources.Mp4VideoRotationNegative);
        Assert.IsNotNull(info.PrimaryVideoStream);
        Assert.AreEqual(-90, info.PrimaryVideoStream.Rotation);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Async_Success()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(3, info.Duration.Seconds);
        Assert.IsNotNull(info.PrimaryVideoStream);
        Assert.AreEqual(8, info.PrimaryVideoStream.BitDepth);
        // This video's audio stream is AAC, which is lossy, so bit depth is meaningless.
        Assert.IsNotNull(info.PrimaryAudioStream);
        Assert.IsNull(info.PrimaryAudioStream.BitDepth);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Probe_Success_FromStream()
    {
        using var stream = File.OpenRead(TestResources.WebmVideo);
        var info = FFProbe.Analyse(stream);
        Assert.AreEqual(3, info.Duration.Seconds);
        // This video has no audio stream.
        Assert.IsNull(info.PrimaryAudioStream);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_FromStream_Async()
    {
        await using var stream = File.OpenRead(TestResources.WebmVideo);
        var info = await FFProbe.AnalyseAsync(stream, cancellationToken: TestContext.CancellationToken);
        Assert.AreEqual(3, info.Duration.Seconds);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Probe_HDR()
    {
        var info = FFProbe.Analyse(TestResources.HdrVideo);

        Assert.IsNotNull(info.PrimaryVideoStream);
        Assert.AreEqual("tv", info.PrimaryVideoStream.ColorRange);
        Assert.AreEqual("bt2020nc", info.PrimaryVideoStream.ColorSpace);
        Assert.AreEqual("arib-std-b67", info.PrimaryVideoStream.ColorTransfer);
        Assert.AreEqual("bt2020", info.PrimaryVideoStream.ColorPrimaries);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_Subtitle_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.SrtSubtitle, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimarySubtitleStream);
        Assert.HasCount(1, info.SubtitleStreams);
        Assert.IsEmpty(info.AudioStreams);
        Assert.IsEmpty(info.VideoStreams);
        // BitDepth is meaningless for subtitles
        Assert.IsNull(info.SubtitleStreams[0].BitDepth);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_Disposition_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.Mp4Video, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimaryAudioStream);
        Assert.IsNotNull(info.PrimaryAudioStream.Disposition);
        Assert.IsTrue(info.PrimaryAudioStream.Disposition["default"]);
        Assert.IsFalse(info.PrimaryAudioStream.Disposition["forced"]);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_Mp3AudioBitDepthNull_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.Mp3Audio, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimaryAudioStream);
        // mp3 is lossy, so bit depth is meaningless.
        Assert.IsNull(info.PrimaryAudioStream.BitDepth);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_VocAudioBitDepth_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.AiffAudio, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimaryAudioStream);
        Assert.AreEqual(16, info.PrimaryAudioStream.BitDepth);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_MkvVideoBitDepth_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.MkvVideo, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimaryVideoStream);
        Assert.AreEqual(8, info.PrimaryVideoStream.BitDepth);

        Assert.IsNotNull(info.PrimaryAudioStream);
        Assert.IsNull(info.PrimaryAudioStream.BitDepth);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_24BitWavBitDepth_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.Wav24Bit, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimaryAudioStream);
        Assert.AreEqual(24, info.PrimaryAudioStream.BitDepth);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Probe_Success_32BitWavBitDepth_Async()
    {
        var info = await FFProbe.AnalyseAsync(TestResources.Wav32Bit, cancellationToken: TestContext.CancellationToken);
        Assert.IsNotNull(info.PrimaryAudioStream);
        Assert.AreEqual(32, info.PrimaryAudioStream.BitDepth);
    }

    [TestMethod]
    public void Probe_Success_Custom_Arguments()
    {
        var info = FFProbe.Analyse(TestResources.Mp4Video, customArguments: "-headers \"Hello: World\"");
        Assert.AreEqual(3, info.Duration.Seconds);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Parallel_FFProbe_Cancellation_Should_Throw_Only_OperationCanceledException()
    {
        // Warm up FFMpegCore environment
        FFProbeHelper.VerifyFFProbeExists(GlobalFFOptions.Current);

        var mp4 = TestResources.Mp4Video;
        if (!File.Exists(mp4))
        {
            Assert.Inconclusive($"Test video not found: {mp4}");
            return;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        using var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
        var tasks = Enumerable.Range(0, 50).Select(x => Task.Run(async () =>
        {
            await semaphore.WaitAsync(cts.Token);
            try
            {
                var analysis = await FFProbe.AnalyseAsync(mp4, cancellationToken: cts.Token);
                return analysis;
            }
            finally
            {
                semaphore.Release();
            }
        }, cts.Token)).ToList();

        // Wait for 2 tasks to finish, then cancel all
        await Task.WhenAny(tasks);
        await Task.WhenAny(tasks);
        await cts.CancelAsync();

        var exceptions = new List<Exception>();
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        Assert.IsNotEmpty(exceptions, "No exceptions were thrown on cancellation. Test was useless. " +
                                      ".Try adjust cancellation timings to make cancellation at the moment, when ffprobe is still running.");

        // Check that all exceptions are OperationCanceledException
        CollectionAssert.AllItemsAreInstancesOfType(exceptions, typeof(OperationCanceledException));
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task FFProbe_Should_Throw_FFMpegException_When_Exits_With_Non_Zero_Code()
    {
        var input = TestResources.SrtSubtitle; //non media file
        await Assert.ThrowsAsync<FFMpegException>(async () => await FFProbe.AnalyseAsync(input,
            cancellationToken: TestContext.CancellationToken, customArguments: "--some-invalid-argument"));
    }
}
