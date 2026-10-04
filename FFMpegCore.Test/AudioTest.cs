using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Extensions.SkiaSharp;
using FFMpegCore.Pipes;
using FFMpegCore.Test.Resources;
using SkiaSharp;

namespace FFMpegCore.Test;

[TestClass]
public class AudioTest
{
    private const int BaseTimeoutMilliseconds = 30_000;

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Audio_Remove()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        FFMpeg.RemoveAudio(TestResources.Mp4Video, outputFile).ProcessSynchronously();
        var source = FFProbe.Analyse(TestResources.Mp4Video);
        var analysis = FFProbe.Analyse(outputFile);

        Assert.IsNotEmpty(analysis.VideoStreams);
        Assert.IsEmpty(analysis.AudioStreams);
        Assert.AreEqual(source.PrimaryVideoStream!.CodecName, analysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    public void Audio_Save()
    {
        using var outputFile = new TemporaryFile("out.mp3");

        FFMpeg.ExtractAudio(TestResources.Mp4Video, outputFile).ProcessSynchronously();
        var analysis = FFProbe.Analyse(outputFile);

        Assert.IsNotEmpty(analysis.AudioStreams);
        Assert.IsEmpty(analysis.VideoStreams);
    }

    [TestMethod]
    [DataRow("out.m4a")]
    [DataRow("out.wav")]
    [DataRow("out.flac")]
    public void Audio_Save_ToContainersOtherThanMp3(string filename)
    {
        using var outputFile = new TemporaryFile(filename);

        FFMpeg.ExtractAudio(TestResources.Mp4Video, outputFile).ProcessSynchronously();
        var analysis = FFProbe.Analyse(outputFile);

        Assert.IsNotEmpty(analysis.AudioStreams);
        Assert.IsEmpty(analysis.VideoStreams);
    }

    [TestMethod]
    public void Audio_Save_CopyingTheStream()
    {
        using var outputFile = new TemporaryFile("out.m4a");

        FFMpeg.ExtractAudio(TestResources.Mp4Video, outputFile, AudioCodec.Copy).ProcessSynchronously();
        var source = FFProbe.Analyse(TestResources.Mp4Video);
        var analysis = FFProbe.Analyse(outputFile);

        Assert.AreEqual(source.PrimaryAudioStream!.CodecName, analysis.PrimaryAudioStream!.CodecName);
        Assert.AreEqual(source.PrimaryAudioStream.Channels, analysis.PrimaryAudioStream.Channels);
    }

    [TestMethod]
    public void Audio_Save_CodecNameSpellsTheSameThingAsTheConstant()
    {
        using var outputFile = new TemporaryFile("out.m4a");

        var byConstant = FFMpeg.ExtractAudio(TestResources.Mp4Video, outputFile, AudioCodec.Copy).Arguments;
        var byName = FFMpeg.ExtractAudio(TestResources.Mp4Video, outputFile, "copy").Arguments;

        Assert.AreEqual(byConstant, byName);
        Assert.Contains("-c:a copy", byName);
    }

    [TestMethod]
    public void Audio_Poster_CodecNameSpellsTheSameThingAsTheConstant()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var byConstant = FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile, AudioCodec.Aac).Arguments;
        var byName = FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile, "aac").Arguments;

        Assert.AreEqual(byConstant, byName);
        Assert.Contains("-c:a aac", byName);
    }

    [TestMethod]
    public async Task Audio_FromRaw()
    {
        await using var file = File.Open(TestResources.RawAudio, FileMode.Open);
        var memoryStream = new MemoryStream();
        await FFMpegArguments
            .FromPipeInput(new StreamPipeSource(file), options => options.ForceFormat("s16le"))
            .OutputToPipe(new StreamPipeSink(memoryStream), options => options.ForceFormat("mp3"))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();
    }

    [TestMethod]
    public void Audio_Add()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var success = FFMpeg.ReplaceAudio(TestResources.Mp4WithoutAudio, TestResources.Mp3Audio, outputFile).ProcessSynchronously();
        var videoAnalysis = FFProbe.Analyse(TestResources.Mp4WithoutAudio);
        var audioAnalysis = FFProbe.Analyse(TestResources.Mp3Audio);
        var outputAnalysis = FFProbe.Analyse(outputFile);

        Assert.IsTrue(success.Success);
        Assert.AreEqual(Math.Max(videoAnalysis.Duration.TotalSeconds, audioAnalysis.Duration.TotalSeconds), outputAnalysis.Duration.TotalSeconds, 0.15);
        Assert.IsTrue(File.Exists(outputFile));
    }

    [TestMethod]
    public void Audio_Replace_TakesTheNewTrackOverTheExistingOne()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var result = FFMpeg.ReplaceAudio(TestResources.Mp4Video, TestResources.Mp3Audio, outputFile).ProcessSynchronously();
        var outputAnalysis = FFProbe.Analyse(outputFile);

        Assert.IsTrue(result.Success);
        Assert.HasCount(1, outputAnalysis.AudioStreams);
        Assert.AreEqual("mp3", outputAnalysis.PrimaryAudioStream!.CodecName);
        Assert.AreEqual(FFProbe.Analyse(TestResources.Mp4Video).PrimaryVideoStream!.CodecName, outputAnalysis.PrimaryVideoStream!.CodecName);
    }

    [TestMethod]
    public void Audio_Replace_ReencodesWhenGivenACodec()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        var result = FFMpeg.ReplaceAudio(TestResources.Mp4Video, TestResources.Mp3Audio, outputFile, AudioCodec.Aac).ProcessSynchronously();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("aac", FFProbe.Analyse(outputFile).PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_SilenceDetect_ThroughANullOutput()
    {
        var result = FFMpegArguments
            .FromFileInput("anullsrc=d=3", false, options => options.ForceFormat("lavfi"))
            .OutputToNull(options => options
                .WithAudioFilters(filters => filters.SilenceDetect(duration: 1)))
            .ProcessSynchronously(cancellationToken: TestContext.CancellationToken);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ErrorOutput.Any(line => line.Contains("silence_start: 0")));
    }

    [TestMethod]
    public void Image_AddAudio_IntoAnyContainer()
    {
        using var outputFile = new TemporaryFile("out.mkv");

        var result = FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile).ProcessSynchronously();

        Assert.IsTrue(result.Success);
        Assert.AreEqual("mp3", FFProbe.Analyse(outputFile).PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    public void Image_AddAudio()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile).ProcessSynchronously();
        var analysis = FFProbe.Analyse(TestResources.Mp3Audio);
        Assert.IsGreaterThan(0, analysis.Duration.TotalSeconds);
        Assert.IsTrue(File.Exists(outputFile));
    }

    [TestMethod]
    public void Image_AddAudio_CopiesTheTrackByDefault()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile).ProcessSynchronously();

        var source = FFProbe.Analyse(TestResources.Mp3Audio);
        var analysis = FFProbe.Analyse(outputFile);
        Assert.AreEqual(source.PrimaryAudioStream!.CodecName, analysis.PrimaryAudioStream!.CodecName);
        Assert.AreEqual(source.PrimaryAudioStream.SampleRateHz, analysis.PrimaryAudioStream.SampleRateHz);
    }

    [TestMethod]
    public void Image_AddAudio_ReencodesWhenGivenACodec()
    {
        using var outputFile = new TemporaryFile("out.mp4");

        FFMpeg.PosterWithAudio(TestResources.PngImage, TestResources.Mp3Audio, outputFile, AudioCodec.Aac).ProcessSynchronously();

        Assert.AreEqual("aac", FFProbe.Analyse(outputFile).PrimaryAudioStream!.CodecName);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Image_AddAudio_ReturnsAProcessorThatCleansUpThePoster()
    {
        using var outputFile = new TemporaryFile("out.mp4");
        using var poster = SKBitmap.Decode(TestResources.PngImage);
        var temporaryFiles = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var percentages = new List<double>();

        try
        {
            var result = await poster.AddAudio(TestResources.Mp3Audio, outputFile, ffOptions: new FFOptions { TemporaryFilesFolder = temporaryFiles.FullName })
                .NotifyOnPercentageProgress(percentages.Add)
                .ProcessAsynchronously(cancellationToken: TestContext.CancellationToken);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(100.0, percentages.Last());
            Assert.IsEmpty(temporaryFiles.GetFiles());
        }
        finally
        {
            temporaryFiles.Delete(true);
        }
    }

    [TestMethod]
    public void Image_AddAudio_WritesThePosterToTheRunTemporaryFilesFolder()
    {
        var missingFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        using var poster = SKBitmap.Decode(TestResources.PngImage);
        var processor = poster.AddAudio(TestResources.Mp3Audio, "out.mp4", ffOptions: new FFOptions { TemporaryFilesFolder = missingFolder });

        Assert.ThrowsExactly<DirectoryNotFoundException>(() => processor.ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_ToAAC_Args_Pipe()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var samples = new List<IAudioSample> { new PcmAudioSampleWrapper([0, 0]), new PcmAudioSampleWrapper([0, 0]) };

        var audioSamplesSource = new RawAudioPipeSource(samples) { Channels = 2, Format = "s8", SampleRate = 8000 };

        var success = FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_ToLibVorbis_Args_Pipe()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var samples = new List<IAudioSample> { new PcmAudioSampleWrapper([0, 0]), new PcmAudioSampleWrapper([0, 0]) };

        var audioSamplesSource = new RawAudioPipeSource(samples) { Channels = 2, Format = "s8", SampleRate = 8000 };

        var success = FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.LibVorbis))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public async Task Audio_ToAAC_Args_Pipe_Async()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var samples = new List<IAudioSample> { new PcmAudioSampleWrapper([0, 0]), new PcmAudioSampleWrapper([0, 0]) };

        var audioSamplesSource = new RawAudioPipeSource(samples) { Channels = 2, Format = "s8", SampleRate = 8000 };

        var success = await FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_ToAAC_Args_Pipe_ValidDefaultConfiguration()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var samples = new List<IAudioSample> { new PcmAudioSampleWrapper([0, 0]), new PcmAudioSampleWrapper([0, 0]) };

        var audioSamplesSource = new RawAudioPipeSource(samples);

        var success = FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();
        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_ToAAC_Args_Pipe_InvalidChannels()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var audioSamplesSource = new RawAudioPipeSource(new List<IAudioSample>()) { Channels = 0 };

        Assert.ThrowsExactly<FFMpegProcessException>(() => FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_ToAAC_Args_Pipe_InvalidFormat()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var audioSamplesSource = new RawAudioPipeSource(new List<IAudioSample>()) { Format = "s8le" };

        Assert.ThrowsExactly<FFMpegProcessException>(() => FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_ToAAC_Args_Pipe_InvalidSampleRate()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var audioSamplesSource = new RawAudioPipeSource(new List<IAudioSample>()) { SampleRate = 0 };

        Assert.ThrowsExactly<FFMpegProcessException>(() => FFMpegArguments
            .FromPipeInput(audioSamplesSource)
            .OutputToFile(outputFile, false, opt => opt
                .WithAudioCodec(AudioCodec.Aac))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_Pan_ToMono()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments.FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.Pan(1, "c0 < 0.9 * c0 + 0.1 * c1")))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var mediaAnalysis = FFProbe.Analyse(outputFile);

        Assert.IsTrue(success.Success);
        Assert.HasCount(1, mediaAnalysis.AudioStreams);
        Assert.AreEqual("mono", mediaAnalysis.PrimaryAudioStream!.ChannelLayout);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_Pan_ToMonoNoDefinitions()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments.FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.Pan(1)))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        var mediaAnalysis = FFProbe.Analyse(outputFile);

        Assert.IsTrue(success.Success);
        Assert.HasCount(1, mediaAnalysis.AudioStreams);
        Assert.AreEqual("mono", mediaAnalysis.PrimaryAudioStream!.ChannelLayout);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_Pan_ToMonoChannelsToOutputDefinitionsMismatch()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        Assert.ThrowsExactly<ArgumentException>(() => FFMpegArguments.FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.Pan(1, "c0=c0", "c1=c1")))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_Pan_ToMonoChannelsLayoutToOutputDefinitionsMismatch()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        Assert.ThrowsExactly<FFMpegProcessException>(() => FFMpegArguments.FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.Pan("mono", "c0=c0", "c1=c1")))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_DynamicNormalizer_WithDefaultValues()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments.FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.DynamicAudioNormalizer()))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    public void Audio_DynamicNormalizer_WithNonDefaultValues()
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        var success = FFMpegArguments.FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.DynamicAudioNormalizer(250, 7, 0.9, 2, 1, false, true, true, 0.5)))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously();

        Assert.IsTrue(success.Success);
    }

    [TestMethod]
    [Timeout(BaseTimeoutMilliseconds, CooperativeCancellation = true)]
    [DataRow(2)]
    [DataRow(32)]
    [DataRow(8)]
    public void Audio_DynamicNormalizer_FilterWindow(int filterWindow)
    {
        using var outputFile = new TemporaryFile($"out{ContainerFormats.Mp4.GetExtension()}");

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => FFMpegArguments
            .FromFileInput(TestResources.Mp3Audio)
            .OutputToFile(outputFile, true,
                argumentOptions => argumentOptions
                    .WithAudioFilters(filter => filter.DynamicAudioNormalizer(filterWindow: filterWindow)))
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessSynchronously());
    }

    [TestMethod]
    public void PcmAudioSampleWrapper_WritesBytes_SyncAndAsync()
    {
        var sample = new PcmAudioSampleWrapper(new byte[] { 1, 2, 3, 4 });
        using var sync = new MemoryStream();
        using var async = new MemoryStream();

        sample.Serialize(sync);
        sample.SerializeAsync(async, CancellationToken.None).GetAwaiter().GetResult();

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, sync.ToArray());
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4 }, async.ToArray());
    }
}
