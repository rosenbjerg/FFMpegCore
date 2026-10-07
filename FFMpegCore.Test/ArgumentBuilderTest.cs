using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace FFMpegCore.Test;

[TestClass]
public class ArgumentBuilderTest
{
    private readonly string[] _concatFiles = { "1.mp4", "2.mp4", "3.mp4", "4.mp4" };
    private readonly int _macOsMaxPipePathLength = 104;
    private readonly string[] _multiFiles = { "1.mp3", "2.mp3", "3.mp3", "4.mp3" };

    [TestMethod]
    public void Builder_BuildString_IO_1()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4").Arguments;
        Assert.AreEqual("-i \"input.mp4\" \"output.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_LambdaStraightAfterThePath_KeepsTheDefaults()
    {
        var withFlags = FFMpegArguments
            .FromFileInput("input.mp4", true, opt => opt.WithStartTime(TimeSpan.FromSeconds(1)))
            .AddFileInput(new[] { "a.mp3", "b.mp3" }, true, opt => opt.WithLoop(1))
            .AddFileInput(new FileInfo("c.mp3"), true, opt => opt.WithLoop(2))
            .OutputToFile("output.mp4", true, opt => opt.CopyStreams())
            .Arguments;
        var withoutFlags = FFMpegArguments
            .FromFileInput("input.mp4", opt => opt.WithStartTime(TimeSpan.FromSeconds(1)))
            .AddFileInput(new[] { "a.mp3", "b.mp3" }, opt => opt.WithLoop(1))
            .AddFileInput(new FileInfo("c.mp3"), opt => opt.WithLoop(2))
            .OutputToFile("output.mp4", opt => opt.CopyStreams())
            .Arguments;
        var many = FFMpegArguments
            .FromFileInput(new FileInfo("input.mp4"), opt => opt.WithLoop(1))
            .OutputToMany(outputs => outputs.OutputToFile("output.mp4", opt => opt.CopyStreams()))
            .Arguments;

        Assert.AreEqual(withFlags, withoutFlags);
        StringAssert.EndsWith(many, "-c copy \"output.mp4\" -y");
    }

    [TestMethod]
    public void Builder_BuildString_Scale()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", true, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Scale(VideoSize.Hd)))
            .Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vf \"scale=-2:720\" \"output.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioCodec()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", true, opt => opt.WithAudioCodec(AudioCodec.Aac)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:a aac \"output.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioBitrate()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", true, opt => opt.WithAudioBitrate(AudioQuality.Normal)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -b:a 128k \"output.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_Bitrates_AreInKilobitsPerSecond()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", opt => opt.WithVideoBitrate(kilobitsPerSecond: 2400).WithAudioBitrate(kilobitsPerSecond: 160)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -b:v 2400k -b:a 160k \"output.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_EncoderTuning()
    {
        var str = FFMpegArguments.FromFileInput("input.raw", false, opt => opt.WithAudioChannels(2))
            .OutputToFile("output.mp4", opt => opt
                .WithVideoProfile(VideoProfile.High)
                .WithTune(EncoderTune.ZeroLatency)
                .WithGopSize(50)
                .WithMaxBitrate(3000)
                .WithBufferSize(6000)
                .WithVideoQualityScale(3)
                .WithAudioQualityScale(2)
                .WithAudioChannels(1))
            .Arguments;

        Assert.AreEqual(
            "-ac 2 -i \"input.raw\" -profile:v high -tune zerolatency -g 50 -maxrate 3000k -bufsize 6000k -q:v 3 -q:a 2 -ac 1 \"output.mp4\" -y",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioCodec_Fluent()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false,
            opt => opt.WithAudioCodec(AudioCodec.Aac).WithAudioBitrate(128)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:a aac -b:a 128k \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_BitStream()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false,
            opt => opt.WithBitstreamFilter(StreamType.Audio, BitstreamFilter.H264_Mp4ToAnnexB)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -bsf:a h264_mp4toannexb \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_BitStream_PassesAnUnlistedFilterThrough()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false,
            opt => opt.WithBitstreamFilter(StreamType.Video, "vp9_superframe")).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -bsf:v vp9_superframe \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(StreamType.All)]
    [DataRow(StreamType.Attachment)]
    [DataRow(StreamType.VideoNoAttachedPic)]
    [DataRow(StreamType.Subtitle)]
    [DataRow(StreamType.Data)]
    public void BitstreamFilter_RejectsStreamTypesWithoutABsfSpelling(StreamType streamType)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new BitstreamFilterArgument(streamType, BitstreamFilter.Aac_AdtsToAsc));
    }

    [TestMethod]
    public void Builder_BuildString_HardwareAcceleration_Auto()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithHardwareAcceleration())
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-hwaccel auto -i \"input.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_HardwareAcceleration_Specific()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithHardwareAcceleration(HardwareAccelerationDevice.CUVID))
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-hwaccel cuvid -i \"input.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_HardwareAcceleration_PassesAnUnlistedDeviceThrough()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithHardwareAcceleration("mediacodec"))
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-hwaccel mediacodec -i \"input.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Concat()
    {
        var str = FFMpegArguments.FromConcatProtocolInput(_concatFiles).OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-i \"concat:1.mp4|2.mp4|3.mp4|4.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DemuxConcat()
    {
        var str = FFMpegArguments.FromConcatDemuxerInput(_concatFiles).OutputToFile("output.mp4", false).Arguments;
        Assert.Contains("-f concat -safe 0 -i", str);
        Assert.Contains("\"output.mp4\"", str);
    }

    public static IEnumerable<object[]> InputSideOptions()
    {
        yield return new object[] { "-s 1920x1080", (Action<FFMpegInputOptions>)(opt => opt.WithFrameSize(new Size(1920, 1080))) };
        yield return new object[] { "-pix_fmt yuv420p", (Action<FFMpegInputOptions>)(opt => opt.WithPixelFormat("yuv420p")) };
        yield return new object[] { "-pix_fmt yuv444p", (Action<FFMpegInputOptions>)(opt => opt.WithPixelFormat(new PixelFormat("yuv444p"))) };
        yield return new object[] { "-ar 48000", (Action<FFMpegInputOptions>)(opt => opt.WithAudioSamplingRate()) };
        yield return new object[] { "-ar 44100", (Action<FFMpegInputOptions>)(opt => opt.WithAudioSamplingRate(44100)) };
        yield return new object[] { "-start_number 7", (Action<FFMpegInputOptions>)(opt => opt.WithStartNumber(7)) };
        yield return new object[] { "-threads 4", (Action<FFMpegInputOptions>)(opt => opt.WithThreads(4)) };
        yield return new object[] { "-vn", (Action<FFMpegInputOptions>)(opt => opt.DisableVideo()) };
        yield return new object[] { "-an", (Action<FFMpegInputOptions>)(opt => opt.DisableAudio()) };
        yield return new object[] { "-sn", (Action<FFMpegInputOptions>)(opt => opt.DisableSubtitles()) };
        yield return new object[] { "-dn", (Action<FFMpegInputOptions>)(opt => opt.DisableData()) };
    }

    [TestMethod]
    [DynamicData(nameof(InputSideOptions))]
    public void Builder_BuildString_InputSideOption_LandsBeforeTheInput(string expected, Action<FFMpegInputOptions> addArguments)
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, addArguments)
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual($"{expected} -i \"input.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_InputFrameSize_NullSizeEmitsNothing()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithFrameSize(null))
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-i \"input.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(StreamType.All, "-c copy")]
    [DataRow(StreamType.Audio, "-c:a copy")]
    [DataRow(StreamType.Video, "-c:v copy")]
    [DataRow(StreamType.VideoNoAttachedPic, "-c:V copy")]
    [DataRow(StreamType.Subtitle, "-c:s copy")]
    [DataRow(StreamType.Data, "-c:d copy")]
    [DataRow(StreamType.Attachment, "-c:t copy")]
    public void Builder_BuildString_Copy_SpellsEveryStreamType(StreamType streamType, string expected)
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.CopyStreams(streamType)).Arguments;
        Assert.AreEqual($"-i \"input.mp4\" {expected} \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Copy_Default_IsAll()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.CopyStreams()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Audio()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableAudio()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -an \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Video()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableVideo()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Subtitle()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableSubtitles()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -sn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Data()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableData()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -dn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Multiple()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
        .OutputToFile("output.mp4", false, opt => opt.DisableAudio().DisableVideo()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -an -vn \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(StreamType.All)]
    [DataRow(StreamType.Attachment)]
    [DataRow(StreamType.VideoNoAttachedPic)]
    public void DisableStream_RejectsStreamTypesFFMpegCannotDisable(StreamType streamType)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DisableStreamArgument(streamType));
    }

    [TestMethod]
    public void Builder_BuildString_DisableSubtitlesAndData()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableSubtitles().DisableData()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -sn -dn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioSamplingRate_Default()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithAudioSamplingRate()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -ar 48000 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioSamplingRate()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithAudioSamplingRate(44000)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -ar 44000 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_VariableBitrate()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVariableBitrate(5)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vbr 5 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Faststart()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithFastStart()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -movflags faststart \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_RemoveMetadata()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithoutMetadata()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -map_metadata -1 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Transpose()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Transpose(Transposition.CounterClockwise90)))
            .Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vf \"transpose=2\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Mirroring()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .HorizontalFlip()))
            .Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vf \"hflip\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_TransposeScale()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Transpose(Transposition.CounterClockwise90)
                    .Scale(200, 300)))
            .Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vf \"transpose=2, scale=200:300\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_ForceFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.ForceFormat(ContainerFormats.Mp4))
            .OutputToFile("output.mp4", false, opt => opt.ForceFormat(ContainerFormats.Mp4)).Arguments;
        Assert.AreEqual("-f mp4 -i \"input.mp4\" -f mp4 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_FrameOutputCount()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithFrameCount(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -frames:v 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_VideoStreamNumber()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.WithMap(0, StreamType.All, 1)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -map 0:1 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_FrameRate()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithFrameRate(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -r 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Loop()
    {
        var str = FFMpegArguments.FromFileInput("input.png", false, opt => opt.WithLoop(50)).OutputToFile("output.mp4", false)
            .Arguments;
        Assert.AreEqual("-loop 50 -i \"input.png\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Seek()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithStartTime(TimeSpan.FromSeconds(10)))
            .OutputToFile("output.mp4", false, opt => opt.WithStartTime(TimeSpan.FromSeconds(10))).Arguments;
        Assert.AreEqual("-ss 00:00:10.000 -i \"input.mp4\" -ss 00:00:10.000 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_EndSeek()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithStopTime(TimeSpan.FromSeconds(10)))
            .OutputToFile("output.mp4", false, opt => opt.WithStopTime(TimeSpan.FromSeconds(10))).Arguments;
        Assert.AreEqual("-to 00:00:10.000 -i \"input.mp4\" -to 00:00:10.000 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Shortest()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithShortest()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -shortest \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Size()
    {
        var str = FFMpegArguments.FromFileInput("input.yuv", false, opt => opt.ForceFormat("rawvideo").WithFrameSize(1920, 1080))
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-f rawvideo -s 1920x1080 -i \"input.yuv\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_InputDecoders()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithVideoDecoder("h264_cuvid").WithAudioDecoder("aac"))
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-c:v h264_cuvid -c:a aac -i \"input.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Speed()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithSpeedPreset(EncoderPreset.Fast)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -preset fast \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Speed_PassesAnUnlistedPresetThrough()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithSpeedPreset("p4")).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -preset p4 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DrawtextFilter()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .DrawText("Stack Overflow", text => text
                        .WithFontFile("/path/to/font.ttf")
                        .WithParameter("fontcolor", "white")
                        .WithParameter("fontsize", "24")
                        .WithParameter("box", "1")
                        .WithParameter("boxcolor", "black@0.5")
                        .WithParameter("boxborderw", "5")
                        .WithParameter("x", "(w-text_w)/2")
                        .WithParameter("y", "(h-text_h)/2"))))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -vf \"drawtext=text='Stack Overflow':fontfile=/path/to/font.ttf:fontcolor=white:fontsize=24:box=1:boxcolor=black@0.5:boxborderw=5:x=(w-text_w)/2:y=(h-text_h)/2\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_DrawtextFilter_TextOnly()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .DrawText("Hello")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"drawtext=text=Hello\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_SubtitleHardBurnFilter()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .HardBurnSubtitle("sample.srt", subtitles => subtitles
                        .WithCharacterEncoding("UTF-8")
                        .WithOriginalSize(1366, 768)
                        .WithSubtitleIndex(0)
                        .WithStyle(style => style
                            .WithParameter("FontName", "DejaVu Serif")
                            .WithParameter("PrimaryColour", "&HAA00FF00")))))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -vf \"subtitles='sample.srt':charenc=UTF-8:original_size=1366x768:stream_index=0:force_style='FontName=DejaVu Serif\\,PrimaryColour=&HAA00FF00'\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_SubtitleHardBurnFilterFixedPaths()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .HardBurnSubtitle(@"sample( \ : [ ] , ' ).srt")))
            .Arguments;

        Assert.AreEqual(@"-i ""input.mp4"" -vf ""subtitles='sample( \\ \: \[ \] \, '\\\'' ).srt'"" ""output.mp4""",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_StartNumber()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithStartNumber(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -start_number 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Threads_1()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithThreads(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -threads 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Threads_2()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithThreads(Environment.ProcessorCount)).Arguments;
        Assert.AreEqual($"-i \"input.mp4\" -threads {Environment.ProcessorCount} \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Codec()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoCodec(VideoCodec.LibX264)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:v libx264 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Codec_Override()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", true,
            opt => opt.WithVideoCodec(VideoCodec.LibX264).WithPixelFormat("yuv420p")).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:v libx264 -pix_fmt yuv420p \"output.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_Duration()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithDuration(TimeSpan.FromSeconds(20))).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -t 00:00:20 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Raw()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.WithCustomArgument(null!))
            .OutputToFile("output.mp4", false, opt => opt.WithCustomArgument(null!)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" \"output.mp4\"", str);

        str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithCustomArgument("-acodec copy")).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -acodec copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_ForcePixelFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithPixelFormat("yuv444p")).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -pix_fmt yuv444p \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_PanAudioFilterChannelNumber()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.Pan(2, "c0=c1", "c1=c1")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"pan=2c|c0=c1|c1=c1\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_PanAudioFilterChannelLayout()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.Pan("stereo", "c0=c0", "c1=c1")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"pan=stereo|c0=c0|c1=c1\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_PanAudioFilterChannelNoOutputDefinition()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.Pan("stereo")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"pan=stereo\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DynamicAudioNormalizerDefaultFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.DynamicAudioNormalizer()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"dynaudnorm=f=500:g=31:p=0.95:m=10.0:r=0.0:n=1:c=0:b=0:s=0.0\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DynamicAudioNormalizerWithValuesFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.DynamicAudioNormalizer(125, 13, 0.9215, 5.124, 0.5458, false, true, true, 0.3333333)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"dynaudnorm=f=125:g=13:p=0.92:m=5.1:r=0.5:n=0:c=1:b=1:s=0.3\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Audible_AAXC_Decryption()
    {
        var str = FFMpegArguments.FromFileInput("input.aaxc", false, x => x.WithAudibleEncryptionKeys("123", "456"))
            .OutputToFile("output.m4b", true, x => x.WithMapMetadata(0).WithId3v2Version().DisableVideo().CopyStreams(StreamType.Audio))
            .Arguments;

        Assert.AreEqual("-audible_key 123 -audible_iv 456 -i \"input.aaxc\" -map_metadata 0 -id3v2_version 3 -vn -c:a copy \"output.m4b\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_PadFilter()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Pad("max(iw,ih)", "ow", pad => pad
                        .WithParameter("x", "(ow-iw)/2")
                        .WithParameter("y", "(oh-ih)/2")
                        .WithParameter("color", "violet")
                        .WithParameter("eval", "frame"))))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -vf \"pad=width=max(iw\\,ih):height=ow:x=(ow-iw)/2:y=(oh-ih)/2:color=violet:eval=frame\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_PadFilter_Alt()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Pad(configure: pad => pad
                        .WithAspectRatio("4/3")
                        .WithParameter("x", "(ow-iw)/2")
                        .WithParameter("y", "(oh-ih)/2")
                        .WithParameter("color", "violet")
                        .WithParameter("eval", "frame"))))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -vf \"pad=aspect=4/3:x=(ow-iw)/2:y=(oh-ih)/2:color=violet:eval=frame\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_PadFilter_NeedsASize()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new PadArgument());
        Assert.ThrowsExactly<ArgumentException>(() => new PadArgument(configure: pad => pad.WithParameter("color", "black")));
    }

    [TestMethod]
    public void Builder_BuildString_GifPalette()
    {
        var streamIndex = 0;
        var size = new Size(640, 480);

        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.gif", false, opt => opt
                .WithGifPalette(streamIndex, size))
            .Arguments;

        Assert.AreEqual($"""
                         -i "input.mp4" -filter_complex "[0:{streamIndex}] fps=12,scale=w={size.Width}:h={size.Height},split [a][b];[a] palettegen=max_colors=32 [p];[b][p] paletteuse=dither=bayer" "output.gif"
                         """, str);
    }

    [TestMethod]
    public void Builder_BuildString_GifPalette_NullSize_FpsSupplied()
    {
        var streamIndex = 1;

        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.gif", false, opt => opt
                .WithGifPalette(streamIndex, null, 10))
            .Arguments;

        Assert.AreEqual($"""
                         -i "input.mp4" -filter_complex "[0:{streamIndex}] fps=10,split [a][b];[a] palettegen=max_colors=32 [p];[b][p] paletteuse=dither=bayer" "output.gif"
                         """, str);
    }

    [TestMethod]
    public void Builder_BuildString_OutputToMany()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToMany(args => args
                .OutputToFile("output.mp4", true, args => args.CopyStreams())
                .OutputToFile("output.ts", false, args => args.CopyStreams().ForceFormat("mpegts"))
                .OutputToUrl("http://server/path", options => options.ForceFormat("webm")))
            .Arguments;
        Assert.AreEqual("""-i "input.mp4" -c copy "output.mp4" -y -c copy -f mpegts "output.ts" -f webm http://server/path""", str);
    }

    [TestMethod]
    public void Builder_BuildString_MBROutput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToMany(args => args
                .OutputToFile("sd.mp4", true, args => args.WithVideoFilters(filters => filters.Scale(1200, 720)))
                .OutputToFile("hd.mp4", false, args => args.WithVideoFilters(filters => filters.Scale(1920, 1080))))
            .Arguments;
        Assert.AreEqual("""
                        -i "input.mp4" -vf "scale=1200:720" "sd.mp4" -y -vf "scale=1920:1080" "hd.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_TeeOutput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToTee(args => args
                .OutputToFile("output.mp4", false, args => args.WithFastStart())
                .OutputToUrl("http://server/path", options => options.ForceFormat("mpegts").WithMap(0, StreamType.Video, 0)))
            .Arguments;
        Assert.AreEqual("""
                        -i "input.mp4" -f tee "[movflags=faststart]output.mp4|[f=mpegts:select=\'0:v:0\']http://server/path"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_TeeOutput_MapByTypeSelectsEveryStream()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToTee(args => args
                .OutputToUrl("http://server/path", options => options.ForceFormat("mpegts").WithMap(0, StreamType.Video)))
            .Arguments;
        Assert.AreEqual("""
                        -i "input.mp4" -f tee "[f=mpegts:select=\'0:v\']http://server/path"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_MultiInput()
    {
        var audioStreams = string.Join("", _multiFiles.Select((item, index) => $"[{index}:0]"));
        var mixFilter = $"{audioStreams}amix=inputs={_multiFiles.Length}:duration=longest:dropout_transition=1:normalize=0[final]";
        var ffmpegArgs = $"-filter_complex \"{mixFilter}\" -map \"[final]\"";
        var str = FFMpegArguments
            .FromFileInput(_multiFiles)
            .OutputToFile("output.mp3", true, options => options
                .WithCustomArgument(ffmpegArgs)
                .WithAudioCodec(AudioCodec.LibMp3Lame) // Set the audio codec to MP3
                .WithAudioBitrate(128) // Set the bitrate to 128kbps
                .WithAudioSamplingRate() // Set the sample rate to 48kHz
                .WithoutMetadata() // Remove metadata
                .WithCustomArgument("-ac 2 -write_xing 0 -id3v2_version 0")) // Force 2 Channels
            .Arguments;
        Assert.AreEqual(
            "-i \"1.mp3\" -i \"2.mp3\" -i \"3.mp3\" -i \"4.mp3\" -filter_complex \"[0:0][1:0][2:0][3:0]amix=inputs=4:duration=longest:dropout_transition=1:normalize=0[final]\" -map \"[final]\" -c:a libmp3lame -b:a 128k -ar 48000 -map_metadata -1 -ac 2 -write_xing 0 -id3v2_version 0 \"output.mp3\" -y",
            str);
    }

    [TestMethod]
    public void Pre_VerifyExists_AllFilesExist()
    {
        // Arrange
        var filePaths = new List<string> { Path.GetTempFileName(), Path.GetTempFileName(), Path.GetTempFileName() };
        var argument = new MultiInputArgument(filePaths, true);
        try
        {
            // Act & Assert
            argument.Pre(new FFOptions()); // No exception should be thrown
        }
        finally
        {
            // Cleanup
            foreach (var filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }
    }

    [TestMethod]
    public void Pre_VerifyExists_SomeFilesNotExist()
    {
        // Arrange
        var filePaths = new List<string> { Path.GetTempFileName(), "file2.mp4", "file3.mp4" };
        var argument = new MultiInputArgument(filePaths, true);
        try
        {
            // Act & Assert
            Assert.ThrowsExactly<FileNotFoundException>(() => argument.Pre(new FFOptions()));
        }
        finally
        {
            // Cleanup
            File.Delete(filePaths[0]);
        }
    }

    [TestMethod]
    public void Pre_VerifyExists_NoFilesExist()
    {
        // Arrange
        var filePaths = new List<string> { "file1.mp4", "file2.mp4", "file3.mp4" };
        var argument = new MultiInputArgument(filePaths, true);
        // Act & Assert
        Assert.ThrowsExactly<FileNotFoundException>(() => argument.Pre(new FFOptions()));
    }

    [TestMethod]
    public void Concat_Escape()
    {
        var arg = new ConcatDemuxerArgument([@"Heaven's River\05 - Investigation.m4b"]);
        var expected = "file '" + Path.GetFullPath(@"Heaven's River\05 - Investigation.m4b").Replace("'", @"'\''") + "'";
        CollectionAssert.AreEquivalent(new[] { expected }, arg.Values.ToArray());
    }

    [TestMethod]
    public void Concat_ResolvesRelativePaths_KeepsAbsolutePathsAndUrls()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "a.mp4");
        var arg = new ConcatDemuxerArgument(["Resources/a.mp4", absolute, "https://host/a.mp4", "concat:a.mp4|b.mp4"]);

        CollectionAssert.AreEqual(new[]
        {
            $"file '{Path.GetFullPath("Resources/a.mp4")}'",
            $"file '{absolute}'",
            "file 'https://host/a.mp4'",
            "file 'concat:a.mp4|b.mp4'"
        }, arg.Values.ToArray());
    }

    [TestMethod]
    [DoNotParallelize]
    public void Concat_ResolvesRelativePaths_AgainstGlobalWorkingDirectory()
    {
        var workingDirectory = Path.GetTempPath();
        try
        {
            GlobalFFOptions.Configure(options => options.WorkingDirectory = workingDirectory);

            var arg = new ConcatDemuxerArgument(["a.mp4"]);

            CollectionAssert.AreEqual(new[] { $"file '{Path.GetFullPath(Path.Combine(workingDirectory, "a.mp4"))}'" }, arg.Values.ToArray());
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    [TestMethod]
    public void Audible_Aaxc_Test()
    {
        var arg = new AudibleEncryptionKeyArgument("123", "456");
        Assert.AreEqual("-audible_key 123 -audible_iv 456", arg.Text);
    }

    [TestMethod]
    public void Audible_Aax_Test()
    {
        var arg = new AudibleEncryptionKeyArgument("62689101");
        Assert.AreEqual("-activation_bytes 62689101", arg.Text);
    }

    [TestMethod]
    public void InputPipe_MaxLength_ShorterThanMacOSMax()
    {
        var pipePath = new InputPipeArgument(new StreamPipeSource(Stream.Null)).PipePath;
        Assert.IsLessThan(104, pipePath.Length);
    }

    [TestMethod]
    public void OutputPipe_MaxLength_ShorterThanMacOSMax()
    {
        var pipePath = new OutputPipeArgument(new StreamPipeSink(Stream.Null)).PipePath;
        Assert.IsLessThan(_macOsMaxPipePathLength, pipePath.Length);
    }

    [TestMethod]
    public void Builder_BuildString_LowPassFilterDefault()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.LowPass()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"lowpass=f=3000.00:p=2:t=q:w=0.71:m=1.00:n=0:r=auto\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_LowPassFilterWithValues()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions
                    .LowPass(5000, 1, "h", 2, 0.5, "FL", true, "svf", "f32", 256)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"lowpass=f=5000.00:p=1:t=h:w=2.00:m=0.50:c=FL:n=1:a=svf:r=f32:b=256\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_HighPassFilterDefault()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.HighPass()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"highpass=f=3000.00:p=2:t=q:w=0.71:m=1.00:n=0:r=auto\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_HighPassFilterWithValues()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions
                    .HighPass(200, 1, "o", 1.5, 0.25, "FR", true, "tdii", "s16", 128)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"highpass=f=200.00:p=1:t=o:w=1.50:m=0.25:c=FR:n=1:a=tdii:r=s16:b=128\" \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(-1.0, 2, "q", 1.0, "auto")]
    [DataRow(3000.0, 3, "q", 1.0, "auto")]
    [DataRow(3000.0, 2, "q", 1.5, "auto")]
    public void Builder_LowPassFilter_Rejects_InvalidArguments(double frequency, int poles, string widthType, double mix, string precision)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new LowPassFilterArgument(frequency, poles, widthType, mix: mix, precision: precision));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new HighPassFilterArgument(frequency, poles, widthType, mix: mix, precision: precision));
    }

    [TestMethod]
    public void Builder_ClosedSetValues_TypedAndPlainStringsRenderTheSame()
    {
        var typed = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", opt => opt.WithAudioFilters(f => f
                .HighPass(200, 2, FilterWidthType.Octave, transform: FilterTransform.StateVariable, precision: FilterPrecision.F32)
                .AudioGate(mode: AudioGateMode.Upward, detection: AudioGateDetection.Peak, link: AudioGateLink.Maximum)
                .SilenceDetect(SilenceDetectNoiseUnit.AmplitudeRatio, 0.01)))
            .Arguments;
        var plain = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", opt => opt.WithAudioFilters(f => f
                .HighPass(200, 2, "o", transform: "svf", precision: "f32")
                .AudioGate(mode: "upward", detection: "peak", link: "maximum")
                .SilenceDetect("ar", 0.01)))
            .Arguments;

        Assert.AreEqual(typed, plain);
    }

    [TestMethod]
    public void Builder_ClosedSetValues_PassUnknownStringsThroughToFFMpeg()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", opt => opt
                .WithVideoFilters(f => f.Fps(30, "some_future_mode"))
                .WithAudioFilters(f => f.LowPass(transform: "some_future_transform")))
            .Arguments;

        StringAssert.Contains(str, "fps=fps=30:round=some_future_mode");
        StringAssert.Contains(str, ":a=some_future_transform:");
    }

    [TestMethod]
    public void Builder_BuildString_AudioGateDefault()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.AudioGate()))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -af \"agate=level_in=1.00:mode=downward:range=0.06:threshold=0.13:ratio=2:attack=20.00:release=250.00:makeup=1:knee=2.83:detection=rms:link=average\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioGateWithValues()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions
                    .AudioGate(0.5, "upward", 0.5, 0.25, 4, 10, 100, 2, 4, "peak", "maximum")))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -af \"agate=level_in=0.50:mode=upward:range=0.50:threshold=0.25:ratio=4:attack=10.00:release=100.00:makeup=2:knee=4.00:detection=peak:link=maximum\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    [DataRow(0.001, "downward", 0.5, 0.5, 2, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.0, 0.5, 2, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 1.5, 2, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 0, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 0.001, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 9001.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 65, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 1, 0.5, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 1, 9.0, "rms", "average")]
    public void Builder_AudioGate_Rejects_InvalidArguments(double levelIn, string mode, double range, double threshold, int ratio, double attack,
        double release, int makeup, double knee, string detection, string link)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new AudioGateArgument(levelIn, mode, range, threshold, ratio, attack, release, makeup, knee, detection, link));
    }

    [TestMethod]
    public void Builder_AudioGate_ReportsTheOffendingParameter()
    {
        var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AudioGateArgument(knee: 9));

        Assert.AreEqual("knee", exception.ParamName);
    }

    [TestMethod]
    [DoNotParallelize]
    public void Builder_BuildString_BlackDetect_IsCultureInvariant()
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");

            Assert.AreEqual("d=1.5:pic_th=0.98:pix_th=0.1", new BlackDetectArgument(1.5).Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [TestMethod]
    public void Builder_BuildString_SilenceDetectDefault()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.SilenceDetect()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"silencedetect=n=-60.0dB:d=2.00:m=0\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_SilenceDetectAmplitudeRatio()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.SilenceDetect("ar", 0.05, 1.5, true)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"silencedetect=n=0.05:d=1.50:m=1\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_SilenceDetect_Rejects_UnknownNoiseType()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SilenceDetectArgument("lufs"));
    }

    [TestMethod]
    public void Builder_BuildString_BlackDetect()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithVideoFilters(filterOptions => filterOptions.BlackDetect()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"blackdetect=d=2:pic_th=0.98:pix_th=0.1\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_BlackFrame()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithVideoFilters(filterOptions => filterOptions.BlackFrame(90, 40)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"blackframe=amount=90:threshold=40\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_CropFilter()
    {
        var bySize = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(filterOptions => filterOptions.Crop(new Size(640, 480), 10, 20)))
            .Arguments;
        var byDimensions = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(filterOptions => filterOptions.Crop(640, 480, 10, 20)))
            .Arguments;
        var topLeft = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(filterOptions => filterOptions.Crop(640, 480)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"crop=640:480:10:20\" \"output.mp4\"", bySize);
        Assert.AreEqual(bySize, byDimensions);
        Assert.AreEqual("-i \"input.mp4\" -vf \"crop=640:480:0:0\" \"output.mp4\"", topLeft);
    }

    [TestMethod]
    public void Builder_BuildString_CropAndScale_ShareOneFilterChain()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(filterOptions => filterOptions
                .Crop(640, 480, 10, 20)
                .Scale(320, 240)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"crop=640:480:10:20, scale=320:240\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_SelectStreams()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithMap(1, StreamType.Video, new[] { 0, 2 }))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map 1:v:0 -map 1:v:2 \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(StreamType.All, "-map 1")]
    [DataRow(StreamType.Audio, "-map 1:a")]
    [DataRow(StreamType.Video, "-map 1:v")]
    [DataRow(StreamType.VideoNoAttachedPic, "-map 1:V")]
    [DataRow(StreamType.Subtitle, "-map 1:s")]
    [DataRow(StreamType.Data, "-map 1:d")]
    [DataRow(StreamType.Attachment, "-map 1:t")]
    public void Builder_BuildString_SelectStreamsByType(StreamType streamType, string expected)
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithMap(1, streamType))
            .Arguments;

        Assert.AreEqual($"-i \"input.mp4\" {expected} \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DeselectStream()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithNegativeMap(0, StreamType.All, 1))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map -0:1 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DeselectStreamsByType()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithNegativeMap(0, StreamType.Subtitle))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map -0:s \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DeselectStreams()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithNegativeMap(0, StreamType.Audio, new[] { 1, 2 }))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map -0:a:1 -map -0:a:2 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_ForcePixelFormat_FromPixelFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithPixelFormat(new PixelFormat("yuv444p")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -pix_fmt yuv444p \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioCodec_FromName()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithAudioCodec("libopus"))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -c:a libopus \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_FpsFilter()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(f => f.Fps(0.5)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"fps=fps=0.5\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_TileFilter()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.png", false, opt => opt.WithVideoFilters(f => f.Tile(4, 3, 2, 1, "black")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"tile=layout=4x3:margin=2:padding=1:color=black\" \"output.png\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_OverlayWithEveryOption()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .AddFileInput("logo.png")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g
                    .From(0, StreamType.Video)
                    .From(1, StreamType.Video)
                    .Overlay("W-w-10", "H-h-10", "endall", true)
                    .As("v"))
                .WithMap("v"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -i "logo.png" -filter_complex "[0:v][1:v]overlay=x=W-w-10:y=H-h-10:eof_action=endall:shortest=1[v]" -map "[v]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_VideoFadeFilter()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(f =>
                f.Fade(FadeDirection.Out, TimeSpan.FromSeconds(9.5), TimeSpan.FromSeconds(0.5), "white")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"fade=t=out:st=9.5:d=0.5:c=white\" \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(2.0, "0.5*PTS")]
    [DataRow(0.5, "2*PTS")]
    [DataRow(1.0, "1*PTS")]
    public void Builder_BuildString_VideoSpeedFilter(double multiplier, string expected)
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(f => f.Speed(multiplier)))
            .Arguments;

        Assert.AreEqual($"-i \"input.mp4\" -vf \"setpts={expected}\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_LoudnormFilter()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithAudioFilters(f => f.Loudnorm(-16, 11, -1.5, true)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"loudnorm=I=-16.0:LRA=11.0:TP=-1.5:dual_mono=true\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudioFadeFilter()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithAudioFilters(f =>
                f.Fade(FadeDirection.In, TimeSpan.Zero, TimeSpan.FromSeconds(2), "qsin")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"afade=t=in:st=0:d=2:curve=qsin\" \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(2.0, "atempo=2")]
    [DataRow(0.75, "atempo=0.75")]
    [DataRow(0.25, "atempo=0.5, atempo=0.5")]
    [DataRow(0.1, "atempo=0.5, atempo=0.5, atempo=0.5, atempo=0.8")]
    public void Builder_BuildString_AudioSpeedFilter_ChainsBeyondOneAtempo(double multiplier, string expected)
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithAudioFilters(f => f.Speed(multiplier)))
            .Arguments;

        Assert.AreEqual($"-i \"input.mp4\" -af \"{expected}\" \"output.mp4\"", str);
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(-1.0)]
    public void SpeedFilters_RejectNonPositiveMultipliers(double multiplier)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new VideoSpeedArgument(multiplier));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AudioFilterOptions().Speed(multiplier));
    }

    [TestMethod]
    [DataRow(0.4)]
    [DataRow(101.0)]
    public void AudioSpeedArgument_RejectsFactorsOneAtempoCannotSpan(double factor)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new AudioSpeedArgument(factor));
    }

    [TestMethod]
    public void Loudnorm_RejectsTargetsOutsideTheStandardRanges()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LoudnormArgument(-80));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LoudnormArgument(loudnessRange: 25));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LoudnormArgument(truePeak: 3));
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_Overlay()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .AddFileInput("logo.png")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g
                    .From(0, StreamType.Video)
                    .From(1, StreamType.Video)
                    .Overlay("W-w-10", "H-h-10")
                    .As("v"))
                .WithMap("v"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -i "logo.png" -filter_complex "[0:v][1:v]overlay=x=W-w-10:y=H-h-10[v]" -map "[v]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_ChainsAreJoinedWithSemicolons()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g
                    .From(0, StreamType.Video)
                    .WithFilter(new ScaleArgument(640, 360))
                    .As("small")
                    .From("small")
                    .WithFilter(FlipArgument.Horizontal)
                    .As("flipped"))
                .WithMap("flipped"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -filter_complex "[0:v]scale=640:360[small];[small]hflip[flipped]" -map "[flipped]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_VideoAndAudioReuseTheFilterBuilders()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g
                    .From(0, StreamType.Video)
                    .Video(f => f.Scale(640, -2).HorizontalFlip())
                    .As("v")
                    .From(0, StreamType.Audio)
                    .Audio(f => f.Loudnorm())
                    .As("a"))
                .WithMap("v")
                .WithMap("a"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -filter_complex "[0:v]scale=640:-2,hflip[v];[0:a]loudnorm=I=-24.0:LRA=7.0:TP=-2.0[a]" -map "[v]" -map "[a]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_ConcatWithAudio()
    {
        var str = FFMpegArguments.FromFileInput(new[] { "a.mp4", "b.mp4" })
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g
                    .From(0, StreamType.Video, 0)
                    .From(0, StreamType.Audio, 0)
                    .From(1, StreamType.Video, 0)
                    .From(1, StreamType.Audio, 0)
                    .Concat(2, 1, 1)
                    .As("v", "a"))
                .WithMap("v")
                .WithMap("a"))
            .Arguments;

        Assert.AreEqual("""
                        -i "a.mp4" -i "b.mp4" -filter_complex "[0:v:0][0:a:0][1:v:0][1:a:0]concat=n=2:v=1:a=1[v][a]" -map "[v]" -map "[a]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_LabelsMayBeBracketed()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g.From(0, StreamType.Video).WithFilter(new FpsArgument(1)).As("[out]"))
                .WithMap("[out]"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -filter_complex "[0:v]fps=fps=1[out]" -map "[out]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void ComplexFilter_RejectsAnEmptyGraph()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ComplexFilterArgument(new FFMpegComplexFilterOptions()));
    }

    [TestMethod]
    public void ComplexFilter_RejectsAChainWithoutFilters()
    {
        var options = new FFMpegComplexFilterOptions();
        options.From(0, StreamType.Video).As("v");

        Assert.ThrowsExactly<ArgumentException>(() => new ComplexFilterArgument(options));
    }

    [TestMethod]
    public void Builder_BuildString_SubtitleCodec()
    {
        var str = FFMpegArguments.FromFileInput("input.mkv")
            .OutputToFile("output.mp4", false, opt => opt.WithSubtitleCodec("mov_text"))
            .Arguments;

        Assert.AreEqual("-i \"input.mkv\" -c:s mov_text \"output.mp4\"", str);
    }

    [TestMethod]
    public void SubtitleCodec_RejectsCodecsOfAnotherType()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new SubtitleCodecArgument(AudioCodec.Aac));
    }

    [TestMethod]
    public void VideoAndAudioCodec_RejectCodecsOfAnotherType()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new VideoCodecArgument(AudioCodec.Aac));
        Assert.ThrowsExactly<ArgumentException>(() => new AudioCodecArgument(VideoCodec.LibX264));
    }

    [TestMethod]
    public void Builder_BuildString_Copy_All()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.CopyStreams())
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -c copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AudibleActivationBytes()
    {
        var str = FFMpegArguments.FromFileInput("input.aax", false, opt => opt.WithAudibleActivationBytes("62689101"))
            .OutputToFile("output.m4b", false)
            .Arguments;

        Assert.AreEqual("-activation_bytes 62689101 -i \"input.aax\" \"output.m4b\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_FileInput_FromFileInfo()
    {
        var fileInfo = new FileInfo("input.mp4");
        var fromInput = FFMpegArguments.FromFileInput(fileInfo).OutputToFile("output.mp4", false).Arguments;
        var addInput = FFMpegArguments.FromFileInput("first.mp4").AddFileInput(fileInfo).OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual($"-i \"{fileInfo.FullName}\" \"output.mp4\"", fromInput);
        Assert.AreEqual($"-i \"first.mp4\" -i \"{fileInfo.FullName}\" \"output.mp4\"", addInput);
    }

    [TestMethod]
    public void Builder_BuildString_UrlInput()
    {
        var uri = new Uri("https://example.com/stream.m3u8");
        var fromInput = FFMpegArguments.FromUrlInput(uri).OutputToFile("output.mp4", false).Arguments;
        var addInput = FFMpegArguments.FromFileInput("first.mp4").AddUrlInput(uri).OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual("-i \"https://example.com/stream.m3u8\" \"output.mp4\"", fromInput);
        Assert.AreEqual("-i \"first.mp4\" -i \"https://example.com/stream.m3u8\" \"output.mp4\"", addInput);
    }

    [TestMethod]
    public void Builder_BuildString_UrlInput_FromString()
    {
        const string uri = "rtsp://example.com/camera";
        var fromInput = FFMpegArguments.FromUrlInput(uri).OutputToFile("output.mp4", false).Arguments;
        var addInput = FFMpegArguments.FromFileInput("first.mp4").AddUrlInput(uri).OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual($"-i \"{uri}\" \"output.mp4\"", fromInput);
        Assert.AreEqual($"-i \"first.mp4\" -i \"{uri}\" \"output.mp4\"", addInput);
    }

    [TestMethod]
    public void Pre_VerifyExists_MissingFileIsReported()
    {
        var verifying = new InputArgument("does-not-exist.mp4", true);
        var notVerifying = new InputArgument("does-not-exist.mp4", false);

        Assert.ThrowsExactly<FileNotFoundException>(() => verifying.Pre(new FFOptions()));
        notVerifying.Pre(new FFOptions());
    }

    [TestMethod]
    public void Builder_FileInfoInput_VerifiesExistenceLikeAPathInput()
    {
        var fileInfo = new FileInfo("input.mp4");

        var byDefault = FFMpegArguments.FromFileInput(fileInfo).Arguments.OfType<InputArgument>().Single();
        var optedOut = FFMpegArguments.FromFileInput(fileInfo, false).Arguments.OfType<InputArgument>().Single();

        Assert.IsTrue(byDefault.VerifyExists);
        Assert.IsFalse(optedOut.VerifyExists);
    }

    [TestMethod]
    public void Builder_BuildString_DeviceInput()
    {
        var fromInput = FFMpegArguments.FromDeviceInput("video=Camera").OutputToFile("output.mp4", false).Arguments;
        var addInput = FFMpegArguments.FromFileInput("first.mp4").AddDeviceInput("video=Camera").OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual("-i video=Camera \"output.mp4\"", fromInput);
        Assert.AreEqual("-i \"first.mp4\" -i video=Camera \"output.mp4\"", addInput);
    }

    [TestMethod]
    public void Builder_BuildString_AddFileInputs()
    {
        var single = FFMpegArguments.FromFileInput("first.mp4").AddFileInput("second.mp4", false).OutputToFile("output.mp4", false).Arguments;
        var multiple = FFMpegArguments.FromFileInput("first.mp4").AddFileInput(_multiFiles, false).OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual("-i \"first.mp4\" -i \"second.mp4\" \"output.mp4\"", single);
        Assert.AreEqual("-i \"first.mp4\" -i \"1.mp3\" -i \"2.mp3\" -i \"3.mp3\" -i \"4.mp3\" \"output.mp4\"", multiple);
    }

    [TestMethod]
    public void Builder_BuildString_AddConcatInput()
    {
        var str = FFMpegArguments.FromFileInput("first.mp4").AddConcatProtocolInput(_concatFiles).OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual("-i \"first.mp4\" -i \"concat:1.mp4|2.mp4|3.mp4|4.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AddPipeInput()
    {
        var pipeSource = new StreamPipeSource(Stream.Null);
        var str = FFMpegArguments.FromFileInput("first.mp4").AddPipeInput(pipeSource).OutputToFile("output.mp4", false).Arguments;

        StringAssert.Matches(str, new Regex("^-i \"first.mp4\" -i \".+\" \"output.mp4\"$"));
    }

    [TestMethod]
    public void Builder_BuildString_StreamPipeFormats_AreForcedOnTheirSide()
    {
        var source = new StreamPipeSource(Stream.Null) { Format = "mpegts" };
        var sink = new StreamPipeSink(Stream.Null) { Format = "matroska" };

        var str = FFMpegArguments.FromPipeInput(source).OutputToPipe(sink, opt => opt.CopyStreams()).Arguments;

        StringAssert.Matches(str, new Regex("^-f mpegts -i \".+\" -c copy -f matroska \".+\" -y$"));
    }

    [TestMethod]
    public void Builder_BuildString_StreamPipesWithoutFormat_AddNothing()
    {
        var str = FFMpegArguments.FromPipeInput(new StreamPipeSource(Stream.Null))
            .OutputToPipe(new StreamPipeSink(Stream.Null), opt => opt.ForceFormat("webm"))
            .Arguments;

        StringAssert.Matches(str, new Regex("^-i \".+\" -f webm \".+\" -y$"));
    }

    [TestMethod]
    public void Builder_BuildString_AddImageSequenceInput()
    {
        var str = FFMpegArguments.FromFileInput("first.mp4")
            .AddImageSequenceInput(["1.png", "2.png"], opt => opt.WithFrameRate(10))
            .OutputToFile("output.mp4", false)
            .Arguments;

        StringAssert.Matches(str, new Regex("^-i \"first.mp4\" -r 10 -i \".*[0-9a-f-]+.%09d\\.png\" \"output.mp4\"$"));
    }

    [TestMethod]
    public void Builder_BuildString_OutputToUrl()
    {
        var byString = FFMpegArguments.FromFileInput("input.mp4").OutputToUrl("rtmp://example.com/live/key").Arguments;
        var byUri = FFMpegArguments.FromFileInput("input.mp4").OutputToUrl(new Uri("rtmp://example.com/live/key")).Arguments;

        Assert.AreEqual("-i \"input.mp4\" rtmp://example.com/live/key", byString);
        Assert.AreEqual(byString, byUri);
    }

    [TestMethod]
    public void Builder_BuildString_AddMetadata_FromBuilder()
    {
        var metadata = new FFMetadataBuilder().WithTitle("Title");
        var str = FFMpegArguments.FromFileInput("input.mp4").AddMetadata(metadata).OutputToFile("output.mp4", false).Arguments;

        StringAssert.Matches(str, new Regex("^-i \"input.mp4\" -i \".*metadata_[0-9a-f-]+\\.txt\" -map_metadata 1 \"output.mp4\"$"));
    }

    [TestMethod]
    public void Builder_EmptyFilterOptions_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(_ => { })));
        Assert.ThrowsExactly<ArgumentException>(() =>
            FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.WithAudioFilters(_ => { })));
    }

    [TestMethod]
    public void Builder_BuildString_OutputToNull()
    {
        var single = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToNull(opt => opt.WithAudioFilters(f => f.SilenceDetect()))
            .Arguments;
        var many = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToMany(outputs => outputs
                .OutputToFile("output.mp4", opt => opt.CopyStreams())
                .OutputToNull(opt => opt.DisableVideo()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"silencedetect=n=-60.0dB:d=2.00:m=0\" -f null -", single);
        Assert.AreEqual("-i \"input.mp4\" -c copy \"output.mp4\" -y -vn -f null -", many);
    }

    [TestMethod]
    public void Builder_BuildString_MetadataAndDisposition()
    {
        var str = FFMpegArguments.FromFileInput("input.mkv")
            .OutputToFile("output.mkv", false, opt => opt
                .WithMetadata("title", "Say \"hi\"")
                .WithStreamMetadata("language", "eng", StreamType.Audio, 1)
                .WithStreamMetadata("handler_name", "Main")
                .WithDisposition(StreamDisposition.Default, StreamType.Subtitle, 0)
                .WithDisposition(StreamDisposition.Default + "forced", StreamType.Audio))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mkv\" -metadata \"title=Say \\\"hi\\\"\" -metadata:s:a:1 \"language=eng\" -metadata:s \"handler_name=Main\" " +
            "-disposition:s:0 default -disposition:a default+forced \"output.mkv\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_MapMetadata_ExplicitIndex()
    {
        var str = FFMpegArguments.FromFileInput("video.mp4")
            .AddFileInput("audio.mp3")
            .OutputToFile("output.mp4", false, opt => opt.WithMapMetadata(1))
            .Arguments;

        Assert.AreEqual("-i \"video.mp4\" -i \"audio.mp3\" -map_metadata 1 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AddMetadata_FollowedByAnotherInput_MapsOnTheOutputSide()
    {
        var str = FFMpegArguments.FromFileInput("video.mp4")
            .AddMetadata(";FFMETADATA1")
            .AddFileInput("audio.mp3")
            .OutputToFile("output.mp4", false, opt => opt.CopyStreams())
            .Arguments;

        StringAssert.Matches(str,
            new Regex("^-i \"video.mp4\" -i \".*metadata_[0-9a-f-]+\\.txt\" -i \"audio.mp3\" -map_metadata 1 -c copy \"output.mp4\"$"));
    }

    [TestMethod]
    public void Builder_BuildString_AddMetadata_CountsEveryFileOfAMultiInput()
    {
        var str = FFMpegArguments.FromFileInput(new[] { "a.mp4", "b.mp4", "c.mp4" }, false)
            .AddMetadata(";FFMETADATA1")
            .OutputToFile("output.mp4", false)
            .Arguments;

        StringAssert.EndsWith(str, "-map_metadata 3 \"output.mp4\"");
    }

    [TestMethod]
    public void Builder_BuildString_AddMetadata_CountsTheConcatDemuxerAsOneInput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .AddConcatDemuxerInput(new[] { "a.mp4", "b.mp4" })
            .AddMetadata(";FFMETADATA1")
            .OutputToFile("output.mp4", false)
            .Arguments;

        StringAssert.EndsWith(str, "-map_metadata 2 \"output.mp4\"");
    }

    [TestMethod]
    public void Builder_BuildString_AddMetadata_ExplicitMappingWins()
    {
        var mapped = FFMpegArguments.FromFileInput("input.mp4")
            .AddMetadata(";FFMETADATA1")
            .OutputToFile("output.mp4", false, opt => opt.WithMapMetadata(0))
            .Arguments;
        var removed = FFMpegArguments.FromFileInput("input.mp4")
            .AddMetadata(";FFMETADATA1")
            .OutputToFile("output.mp4", false, opt => opt.WithoutMetadata())
            .Arguments;

        StringAssert.EndsWith(mapped, "txt\" -map_metadata 0 \"output.mp4\"");
        StringAssert.EndsWith(removed, "txt\" -map_metadata -1 \"output.mp4\"");
    }

    [TestMethod]
    public void Builder_BuildString_AddMetadata_MapsOnEveryOutput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .AddMetadata(";FFMETADATA1")
            .OutputToMany(outputs => outputs
                .OutputToFile("a.mp4", false)
                .OutputToFile("b.mp4", false, opt => opt.WithoutMetadata()))
            .Arguments;

        StringAssert.EndsWith(str, "txt\" -map_metadata 1 \"a.mp4\" -map_metadata -1 \"b.mp4\"");
    }

    [TestMethod]
    public void Builder_BuildString_TeeOutput_OverwriteIsHoistedOutOfBranches()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToTee(args => args
                .OutputToFile("first.mp4", true, options => options.ForceFormat("mp4"))
                .OutputToFile("second.mp4", false, options => options.ForceFormat("mp4")))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -f tee \"[f=mp4]first.mp4|[f=mp4]second.mp4\" -y", str);
    }

    [TestMethod]
    public void Builder_BuildString_TeeOutput_EscapesTargets()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToTee(args => args
                .OutputToFile(@"C:\out\it's.mp4", false, options => options.ForceFormat("mp4"))
                .OutputToFile("a|b.mp4", false, options => options.ForceFormat("mp4")))
            .Arguments;

        Assert.AreEqual(@"-i ""input.mp4"" -f tee ""[f=mp4]C:\\out\\it\'s.mp4|[f=mp4]a\|b.mp4""", str);
    }

    [TestMethod]
    public void Builder_TeeOutput_RequiresAtLeastOneOutput()
    {
        Assert.ThrowsExactly<ArgumentException>(() => FFMpegArguments.FromFileInput("input.mp4").OutputToTee(_ => { }));
    }

    [TestMethod]
    public void Builder_BuildString_OutputToMany_UrlAndPipe()
    {
        var sink = new StreamPipeSink(Stream.Null);
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToMany(outputs => outputs
                .OutputToUrl(new Uri("rtmp://example.com/live"), options => options.ForceFormat("flv"))
                .OutputToPipe(sink, options => options.ForceFormat("mpegts")))
            .Arguments;

        StringAssert.Matches(str, new Regex("^-i \"input.mp4\" -f flv rtmp://example.com/live -f mpegts \".+\" -y$"));
    }

    [TestMethod]
    public void Builder_BuildString_VideoFilter_KeyWithoutValueIsRenderedAsTheBareName()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(f => f.WithFilter(new BareNameVideoFilter("yadif"))))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf \"yadif\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_CustomFiltersJoinTheBuiltInOnes()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(f => f.WithCustomFilter("yadif").Scale(640, 360).WithCustomFilter("eq", "contrast=1.2:brightness=0.05"))
                .WithAudioFilters(f => f.WithCustomFilter("volume", "0.5")))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -vf "yadif, scale=640:360, eq=contrast=1.2:brightness=0.05" -af "volume=0.5" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_VideoFilter_FilterWithNeitherKeyNorValueIsRejected()
    {
        var options = new VideoFilterOptions();
        options.WithFilter(new BareNameVideoFilter(string.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => new VideoFiltersArgument(options));
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_KeyWithoutValueIsRenderedAsTheBareName()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g.From(0, StreamType.Video).WithCustomFilter("yadif").As("v"))
                .WithMap("v"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -filter_complex "[0:v]yadif[v]" -map "[v]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_ComplexFilter_LabelsCanBeNegativelyMapped()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithComplexFilter(g => g.From(0, StreamType.Video).WithCustomFilter("yadif").As("v"))
                .WithMap(0)
                .WithNegativeMap("v"))
            .Arguments;

        Assert.AreEqual("""
                        -i "input.mp4" -filter_complex "[0:v]yadif[v]" -map 0 -map -"[v]" "output.mp4"
                        """, str);
    }

    [TestMethod]
    public void ImageSequenceInput_RejectsMixedExtensions()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ImageSequenceInputArgument(new[] { "a.png", "b.jpg" }));
    }

    private class BareNameVideoFilter : IVideoFilterArgument
    {
        public BareNameVideoFilter(string key)
        {
            Key = key;
        }

        public string Key { get; }
        public string Value => string.Empty;
    }
}
