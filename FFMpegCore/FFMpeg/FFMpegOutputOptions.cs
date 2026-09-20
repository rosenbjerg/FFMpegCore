using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class FFMpegOutputOptions : FFMpegArgumentsBase
{
    internal FFMpegOutputOptions() { }

    public FFMpegOutputOptions WithAudioCodec(Codec audioCodec)
    {
        return WithArgument(new AudioCodecArgument(audioCodec));
    }

    public FFMpegOutputOptions WithAudioCodec(string audioCodec)
    {
        return WithArgument(new AudioCodecArgument(audioCodec));
    }

    public FFMpegOutputOptions WithAudioBitrate(AudioQuality audioQuality)
    {
        return WithArgument(new AudioBitrateArgument(audioQuality));
    }

    public FFMpegOutputOptions WithAudioBitrate(int bitrate)
    {
        return WithArgument(new AudioBitrateArgument(bitrate));
    }

    public FFMpegOutputOptions WithAudioSamplingRate(int samplingRate = 48000)
    {
        return WithArgument(new AudioSamplingRateArgument(samplingRate));
    }

    public FFMpegOutputOptions WithVariableBitrate(int vbr)
    {
        return WithArgument(new VariableBitRateArgument(vbr));
    }

    public FFMpegOutputOptions WithBitStreamFilter(Channel channel, Filter filter)
    {
        return WithArgument(new BitStreamFilterArgument(channel, filter));
    }

    public FFMpegOutputOptions WithConstantRateFactor(int crf)
    {
        return WithArgument(new ConstantRateFactorArgument(crf));
    }

    public FFMpegOutputOptions CopyChannel(Channel channel = Channel.Both)
    {
        return WithArgument(new CopyArgument(channel));
    }

    public FFMpegOutputOptions DisableChannel(Channel channel)
    {
        return WithArgument(new DisableChannelArgument(channel));
    }

    public FFMpegOutputOptions WithDuration(TimeSpan? duration)
    {
        return WithArgument(new DurationArgument(duration));
    }

    public FFMpegOutputOptions WithFastStart()
    {
        return WithArgument(new FaststartArgument());
    }

    public FFMpegOutputOptions WithFrameOutputCount(int frames)
    {
        return WithArgument(new FrameOutputCountArgument(frames));
    }

    public FFMpegOutputOptions UsingShortest(bool shortest = true)
    {
        return WithArgument(new ShortestArgument(shortest));
    }

    public FFMpegOutputOptions UsingMultithreading(bool multithread)
    {
        return WithArgument(new ThreadsArgument(multithread));
    }

    public FFMpegOutputOptions UsingThreads(int threads)
    {
        return WithArgument(new ThreadsArgument(threads));
    }

    public FFMpegOutputOptions WithVideoCodec(Codec videoCodec)
    {
        return WithArgument(new VideoCodecArgument(videoCodec));
    }

    public FFMpegOutputOptions WithVideoCodec(string videoCodec)
    {
        return WithArgument(new VideoCodecArgument(videoCodec));
    }

    public FFMpegOutputOptions WithVideoBitrate(int bitrate)
    {
        return WithArgument(new VideoBitrateArgument(bitrate));
    }

    public FFMpegOutputOptions WithVideoFilters(Action<VideoFilterOptions> videoFilterOptions)
    {
        var videoFilterOptionsObj = new VideoFilterOptions();
        videoFilterOptions(videoFilterOptionsObj);
        return WithArgument(new VideoFiltersArgument(videoFilterOptionsObj));
    }

    public FFMpegOutputOptions WithAudioFilters(Action<AudioFilterOptions> audioFilterOptions)
    {
        var audioFilterOptionsObj = new AudioFilterOptions();
        audioFilterOptions(audioFilterOptionsObj);
        return WithArgument(new AudioFiltersArgument(audioFilterOptionsObj));
    }

    public FFMpegOutputOptions WithFramerate(double framerate)
    {
        return WithArgument(new FrameRateArgument(framerate));
    }

    public FFMpegOutputOptions WithoutMetadata()
    {
        return WithArgument(new RemoveMetadataArgument());
    }

    public FFMpegOutputOptions WithSpeedPreset(Speed speed)
    {
        return WithArgument(new SpeedPresetArgument(speed));
    }

    public FFMpegOutputOptions WithStartNumber(int startNumber)
    {
        return WithArgument(new StartNumberArgument(startNumber));
    }

    public FFMpegOutputOptions WithCustomArgument(string argument)
    {
        return WithArgument(new CustomArgument(argument));
    }

    public FFMpegOutputOptions Seek(TimeSpan? seekTo)
    {
        return WithArgument(new SeekArgument(seekTo));
    }

    public FFMpegOutputOptions EndSeek(TimeSpan? seekTo)
    {
        return WithArgument(new EndSeekArgument(seekTo));
    }

    public FFMpegOutputOptions OverwriteExisting()
    {
        return WithArgument(new OverwriteArgument());
    }

    public FFMpegOutputOptions SelectStream(int streamIndex, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, channel));
    }

    public FFMpegOutputOptions SelectStreams(IEnumerable<int> streamIndices, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return streamIndices.Aggregate(this,
            (options, streamIndex) => options.SelectStream(streamIndex, inputFileIndex, channel));
    }

    public FFMpegOutputOptions DeselectStream(int streamIndex, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, channel, true));
    }

    public FFMpegOutputOptions DeselectStreams(IEnumerable<int> streamIndices, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return streamIndices.Aggregate(this,
            (options, streamIndex) => options.DeselectStream(streamIndex, inputFileIndex, channel));
    }

    public FFMpegOutputOptions ForceFormat(ContainerFormat format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    public FFMpegOutputOptions ForceFormat(string format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    public FFMpegOutputOptions ForcePixelFormat(string pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    public FFMpegOutputOptions ForcePixelFormat(PixelFormat pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    public FFMpegOutputOptions WithTagVersion(int id3v2Version = 3)
    {
        return WithArgument(new ID3V2VersionArgument(id3v2Version));
    }

    public FFMpegOutputOptions WithGifPaletteArgument(int streamIndex, Size? size, double fps = 12)
    {
        return WithArgument(new GifPaletteArgument(streamIndex, fps, size));
    }

    public FFMpegOutputOptions WithArgument(IArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
