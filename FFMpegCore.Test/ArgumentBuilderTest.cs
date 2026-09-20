using System.Drawing;
using System.Text.RegularExpressions;
using FFMpegCore.Arguments;
using FFMpegCore.Builders.MetaData;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
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
    public void Builder_BuildString_Scale()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", true, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .Scale(VideoSize.Hd)))
            .Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vf \"scale=-1:720\" \"output.mp4\" -y", str);
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
    public void Builder_BuildString_Quiet()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").WithGlobalOptions(opt => opt.WithVerbosityLevel())
            .OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-hide_banner -loglevel error -i \"input.mp4\" \"output.mp4\"", str);
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
            opt => opt.WithBitStreamFilter(Channel.Audio, Filter.H264_Mp4ToAnnexB)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -bsf:a h264_mp4toannexb \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_HardwareAcceleration_Auto()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithHardwareAcceleration()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -hwaccel auto \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_HardwareAcceleration_Specific()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false,
            opt => opt.WithHardwareAcceleration(HardwareAccelerationDevice.CUVID)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -hwaccel cuvid \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Concat()
    {
        var str = FFMpegArguments.FromConcatInput(_concatFiles).OutputToFile("output.mp4", false).Arguments;
        Assert.AreEqual("-i \"concat:1.mp4|2.mp4|3.mp4|4.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DemuxConcat()
    {
        var str = FFMpegArguments.FromDemuxConcatInput(_concatFiles).OutputToFile("output.mp4", false).Arguments;
        Assert.Contains("-f concat -safe 0 -i", str);
        Assert.Contains("\"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Copy_Audio()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.CopyChannel(Channel.Audio)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:a copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Copy_Video()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.CopyChannel(Channel.Video)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:v copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Copy_Both()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.CopyChannel()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -c:a copy -c:v copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Audio()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Audio)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -an \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Video()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Video)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Subtitle()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Subtitle)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -sn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Data()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Data)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -dn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Multiple()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
        .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Audio).DisableChannel(Channel.Video)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -an -vn \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Both_InvalidChannel()
    {
        Assert.ThrowsExactly<FFMpegException>(() => FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Both)).Arguments);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_All_InvalidChannel()
    {
        Assert.ThrowsExactly<FFMpegException>(() => FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.All)).Arguments);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_Attachments_UnsupportedChannel()
    {
        Assert.ThrowsExactly<FFMpegException>(() => FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.Attachments)).Arguments);
    }

    [TestMethod]
    public void Builder_BuildString_DisableChannel_VideoNoAttachedPic_UnsupportedChannel()
    {
        Assert.ThrowsExactly<FFMpegException>(() => FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DisableChannel(Channel.VideoNoAttachedPic)).Arguments);
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
    public void Builder_BuildString_Overwrite()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.OverwriteExisting()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -y \"output.mp4\"", str);
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
                    .Mirror(Mirroring.Horizontal)))
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
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.ForceFormat(VideoType.Mp4))
            .OutputToFile("output.mp4", false, opt => opt.ForceFormat(VideoType.Mp4)).Arguments;
        Assert.AreEqual("-f mp4 -i \"input.mp4\" -f mp4 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_FrameOutputCount()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithFrameOutputCount(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -vframes 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_VideoStreamNumber()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.SelectStream(1)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -map 0:1 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_FrameRate()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithFramerate(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -r 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Loop()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.Loop(50))
            .Arguments;
        Assert.AreEqual("-i \"input.mp4\" -loop 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Seek()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.Seek(TimeSpan.FromSeconds(10)))
            .OutputToFile("output.mp4", false, opt => opt.Seek(TimeSpan.FromSeconds(10))).Arguments;
        Assert.AreEqual("-ss 00:00:10.000 -i \"input.mp4\" -ss 00:00:10.000 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_EndSeek()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4", false, opt => opt.EndSeek(TimeSpan.FromSeconds(10)))
            .OutputToFile("output.mp4", false, opt => opt.EndSeek(TimeSpan.FromSeconds(10))).Arguments;
        Assert.AreEqual("-to 00:00:10.000 -i \"input.mp4\" -to 00:00:10.000 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Shortest()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.UsingShortest()).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -shortest \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Size()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.Resize(1920, 1080)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -s 1920x1080 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Speed()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithSpeedPreset(Speed.Fast)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -preset fast \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DrawtextFilter()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .DrawText(DrawTextOptions
                        .Create("Stack Overflow", "/path/to/font.ttf")
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
    public void Builder_BuildString_DrawtextFilter_Alt()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .DrawText(DrawTextOptions
                        .Create("Stack Overflow", "/path/to/font.ttf", ("fontcolor", "white"), ("fontsize", "24")))))
            .Arguments;

        Assert.AreEqual(
            "-i \"input.mp4\" -vf \"drawtext=text='Stack Overflow':fontfile=/path/to/font.ttf:fontcolor=white:fontsize=24\" \"output.mp4\"",
            str);
    }

    [TestMethod]
    public void Builder_BuildString_SubtitleHardBurnFilter()
    {
        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt
                .WithVideoFilters(filterOptions => filterOptions
                    .HardBurnSubtitle(SubtitleHardBurnOptions
                        .Create("sample.srt")
                        .SetCharacterEncoding("UTF-8")
                        .SetOriginalSize(1366, 768)
                        .SetSubtitleIndex(0)
                        .WithStyle(StyleOptions.Create()
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
                    .HardBurnSubtitle(SubtitleHardBurnOptions
                        .Create(@"sample( \ : [ ] , ' ).srt"))))
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
            .OutputToFile("output.mp4", false, opt => opt.UsingThreads(50)).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -threads 50 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Threads_2()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.UsingMultithreading(true)).Arguments;
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
            opt => opt.WithVideoCodec(VideoCodec.LibX264).ForcePixelFormat("yuv420p")).Arguments;
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
        Assert.AreEqual(" -i \"input.mp4\"  \"output.mp4\"", str);

        str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithCustomArgument("-acodec copy")).Arguments;
        Assert.AreEqual("-i \"input.mp4\" -acodec copy \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_ForcePixelFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.ForcePixelFormat("yuv444p")).Arguments;
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
                opt => opt.WithAudioFilters(filterOptions => filterOptions.DynamicNormalizer()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"dynaudnorm=f=500:g=31:p=0.95:m=10.0:r=0.0:n=1:c=0:b=0:s=0.0\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DynamicAudioNormalizerWithValuesFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.DynamicNormalizer(125, 13, 0.9215, 5.124, 0.5458, false, true, true, 0.3333333)))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"dynaudnorm=f=125:g=13:p=0.92:m=5.1:r=0.5:n=0:c=1:b=1:s=0.3\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_Audible_AAXC_Decryption()
    {
        var str = FFMpegArguments.FromFileInput("input.aaxc", false, x => x.WithAudibleEncryptionKeys("123", "456"))
            .MapMetaData()
            .OutputToFile("output.m4b", true, x => x.WithTagVersion().DisableChannel(Channel.Video).CopyChannel(Channel.Audio))
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
                    .Pad(PadOptions
                        .Create("max(iw,ih)", "ow")
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
                    .Pad(PadOptions
                        .Create("4/3")
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
    public void Builder_BuildString_GifPalette()
    {
        var streamIndex = 0;
        var size = new Size(640, 480);

        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.gif", false, opt => opt
                .WithGifPaletteArgument(streamIndex, size))
            .Arguments;

        Assert.AreEqual($"""
                         -i "input.mp4" -filter_complex "[0:v] fps=12,scale=w={size.Width}:h={size.Height},split [a][b];[a] palettegen=max_colors=32 [p];[b][p] paletteuse=dither=bayer" "output.gif"
                         """, str);
    }

    [TestMethod]
    public void Builder_BuildString_GifPalette_NullSize_FpsSupplied()
    {
        var streamIndex = 1;

        var str = FFMpegArguments
            .FromFileInput("input.mp4")
            .OutputToFile("output.gif", false, opt => opt
                .WithGifPaletteArgument(streamIndex, null, 10))
            .Arguments;

        Assert.AreEqual($"""
                         -i "input.mp4" -filter_complex "[{streamIndex}:v] fps=10,split [a][b];[a] palettegen=max_colors=32 [p];[b][p] paletteuse=dither=bayer" "output.gif"
                         """, str);
    }

    [TestMethod]
    public void Builder_BuildString_MultiOutput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .MultiOutput(args => args
                .OutputToFile("output.mp4", true, args => args.CopyChannel())
                .OutputToFile("output.ts", false, args => args.CopyChannel().ForceFormat("mpegts"))
                .OutputToUrl("http://server/path", options => options.ForceFormat("webm")))
            .Arguments;
        Assert.AreEqual("""-i "input.mp4" -c:a copy -c:v copy "output.mp4" -y -c:a copy -c:v copy -f mpegts "output.ts" -f webm http://server/path""", str);
    }

    [TestMethod]
    public void Builder_BuildString_MBROutput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .MultiOutput(args => args
                .OutputToFile("sd.mp4", true, args => args.Resize(1200, 720))
                .OutputToFile("hd.mp4", false, args => args.Resize(1920, 1080)))
            .Arguments;
        Assert.AreEqual("""
                        -i "input.mp4" -s 1200x720 "sd.mp4" -y -s 1920x1080 "hd.mp4"
                        """, str);
    }

    [TestMethod]
    public void Builder_BuildString_TeeOutput()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToTee(args => args
                .OutputToFile("output.mp4", false, args => args.WithFastStart())
                .OutputToUrl("http://server/path", options => options.ForceFormat("mpegts").SelectStream(0, channel: Channel.Video)))
            .Arguments;
        Assert.AreEqual("""
                        -i "input.mp4" -f tee "[movflags=faststart]output.mp4|[f=mpegts:select=\'0:v:0\']http://server/path"
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
        var argument = new MultiInputArgument(true, filePaths);
        try
        {
            // Act & Assert
            argument.Pre(); // No exception should be thrown
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
        var argument = new MultiInputArgument(true, filePaths);
        try
        {
            // Act & Assert
            Assert.ThrowsExactly<FileNotFoundException>(() => argument.Pre());
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
        var argument = new MultiInputArgument(true, filePaths);
        // Act & Assert
        Assert.ThrowsExactly<FileNotFoundException>(() => argument.Pre());
    }

    [TestMethod]
    public void Concat_Escape()
    {
        var arg = new DemuxConcatArgument([@"Heaven's River\05 - Investigation.m4b"]);
        var expected = "file '" + Path.GetFullPath(@"Heaven's River\05 - Investigation.m4b").Replace("'", @"'\''") + "'";
        CollectionAssert.AreEquivalent(new[] { expected }, arg.Values.ToArray());
    }

    [TestMethod]
    public void Concat_ResolvesRelativePaths_KeepsAbsolutePathsAndUrls()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "a.mp4");
        var arg = new DemuxConcatArgument(["Resources/a.mp4", absolute, "https://host/a.mp4", "concat:a.mp4|b.mp4"]);

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

            var arg = new DemuxConcatArgument(["a.mp4"]);

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
    [DataRow(3000.0, 2, "x", 1.0, "auto")]
    [DataRow(3000.0, 2, "q", 1.5, "auto")]
    [DataRow(3000.0, 2, "q", 1.0, "s8")]
    public void Builder_LowPassFilter_Rejects_InvalidArguments(double frequency, int poles, string widthType, double mix, string precision)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new LowPassFilterArgument(frequency, poles, widthType, mix: mix, precision: precision));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new HighPassFilterArgument(frequency, poles, widthType, mix: mix, precision: precision));
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
    [DataRow(1.0, "sideways", 0.5, 0.5, 2, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.0, 0.5, 2, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 1.5, 2, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 0, 20.0, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 0.001, 250.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 9001.0, 1, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 65, 2.0, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 1, 0.5, "rms", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 1, 2.0, "loudness", "average")]
    [DataRow(1.0, "downward", 0.5, 0.5, 2, 20.0, 250.0, 1, 2.0, "rms", "minimum")]
    public void Builder_AudioGate_Rejects_InvalidArguments(double levelIn, string mode, double range, double threshold, int ratio, double attack,
        double release, int makeup, double knee, string detection, string link)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new AudioGateArgument(levelIn, mode, range, threshold, ratio, attack, release, makeup, knee, detection, link));
    }

    [TestMethod]
    public void Builder_BuildString_SilenceDetectDefault()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false,
                opt => opt.WithAudioFilters(filterOptions => filterOptions.SilenceDetect()))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -af \"silencedetect=n=60.0dB:d=2.00:m=0\" \"output.mp4\"", str);
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
    public void Builder_BuildString_Crop()
    {
        var bySize = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.Crop(new Size(640, 480), 10, 20))
            .Arguments;
        var byDimensions = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.Crop(640, 480, 10, 20))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -vf crop=640:480:10:20 \"output.mp4\"", bySize);
        Assert.AreEqual(bySize, byDimensions);
    }

    [TestMethod]
    public void Builder_Crop_WithoutSize_IsEmpty()
    {
        Assert.AreEqual(string.Empty, new CropArgument(null, 0, 0).Text);
    }

    [TestMethod]
    public void Builder_BuildString_SelectStreams()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.SelectStreams(new[] { 0, 2 }, 1, Channel.Video))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map 1:v:0 -map 1:v:2 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DeselectStream()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DeselectStream(1))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map -0:1 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_DeselectStreams()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.DeselectStreams(new[] { 1, 2 }, 0, Channel.Audio))
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -map -0:a:1 -map -0:a:2 \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_MapStream_TreatsBothAsAll()
    {
        Assert.AreEqual("-map 0:1", new MapStreamArgument(1, 0, Channel.Both).Text);
    }

    [TestMethod]
    public void Builder_BuildString_ForcePixelFormat_FromPixelFormat()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.ForcePixelFormat(new PixelFormat("yuv444p")))
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
    public void Builder_BuildString_CopyCodec()
    {
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .OutputToFile("output.mp4", false, opt => opt.WithCopyCodec())
            .Arguments;

        Assert.AreEqual("-i \"input.mp4\" -codec copy \"output.mp4\"", str);
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
        var str = FFMpegArguments.FromFileInput("first.mp4").AddConcatInput(_concatFiles).OutputToFile("output.mp4", false).Arguments;

        Assert.AreEqual("-i \"first.mp4\" -i \"concat:1.mp4|2.mp4|3.mp4|4.mp4\" \"output.mp4\"", str);
    }

    [TestMethod]
    public void Builder_BuildString_AddPipeInput()
    {
        var pipeSource = new StreamPipeSource(Stream.Null);
        var str = FFMpegArguments.FromFileInput("first.mp4").AddPipeInput(pipeSource).OutputToFile("output.mp4", false).Arguments;

        StringAssert.Matches(str, new Regex("^-i \"first.mp4\"\\s+-i \".+\" \"output.mp4\"$"));
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
    public void Builder_BuildString_AddMetaData_FromReadOnlyMetaData()
    {
        var metaData = new MetaDataBuilder().WithTitle("Title").Build();
        var str = FFMpegArguments.FromFileInput("input.mp4").AddMetaData(metaData).OutputToFile("output.mp4", false).Arguments;

        StringAssert.Matches(str, new Regex("^-i \"input.mp4\" -i \".*metadata_[0-9a-f-]+\\.txt\" -map_metadata 1 \"output.mp4\"$"));
    }

    [TestMethod]
    public void Builder_EmptyFilterOptions_Throw()
    {
        var video = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.WithVideoFilters(_ => { }));
        var audio = FFMpegArguments.FromFileInput("input.mp4").OutputToFile("output.mp4", false, opt => opt.WithAudioFilters(_ => { }));

        Assert.ThrowsExactly<FFMpegArgumentException>(() => video.Arguments);
        Assert.ThrowsExactly<FFMpegArgumentException>(() => audio.Arguments);
    }

    [TestMethod]
    public void Builder_BuildString_MapMetaData_ExplicitIndex()
    {
        var str = FFMpegArguments.FromFileInput("video.mp4")
            .AddFileInput("audio.mp3")
            .MapMetaData(1)
            .OutputToFile("output.mp4", false)
            .Arguments;

        Assert.AreEqual("-i \"video.mp4\" -i \"audio.mp3\" -map_metadata 1 \"output.mp4\"", str);
        Assert.AreEqual("-map_metadata 0", new MapMetadataArgument().Text);
        Assert.AreEqual("-map_metadata 2", new MapMetadataArgument(2).Text);
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
    public void Builder_BuildString_MultiOutput_UrlAndPipe()
    {
        var sink = new StreamPipeSink(Stream.Null);
        var str = FFMpegArguments.FromFileInput("input.mp4")
            .MultiOutput(outputs => outputs
                .OutputToUrl(new Uri("rtmp://example.com/live"), options => options.ForceFormat("flv"))
                .OutputToPipe(sink, options => options.ForceFormat("mpegts")))
            .Arguments;

        StringAssert.Matches(str, new Regex("^-i \"input.mp4\" -f flv rtmp://example.com/live -f mpegts \".+\" -y$"));
    }
}
