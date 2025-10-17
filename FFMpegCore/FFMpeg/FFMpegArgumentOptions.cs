using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class FFMpegArgumentOptions : FFMpegArgumentsBase
{
    internal FFMpegArgumentOptions() { }

    public FFMpegArgumentOptions WithAudioCodec(Codec audioCodec)
    {
        return WithArgument(new AudioCodecArgument(audioCodec));
    }

    public FFMpegArgumentOptions WithAudioCodec(string audioCodec)
    {
        return WithArgument(new AudioCodecArgument(audioCodec));
    }

    public FFMpegArgumentOptions WithAudioBitrate(AudioQuality audioQuality)
    {
        return WithArgument(new AudioBitrateArgument(audioQuality));
    }

    public FFMpegArgumentOptions WithAudioBitrate(int bitrate)
    {
        return WithArgument(new AudioBitrateArgument(bitrate));
    }

    public FFMpegArgumentOptions WithAudioSamplingRate(int samplingRate = 48000)
    {
        return WithArgument(new AudioSamplingRateArgument(samplingRate));
    }

    public FFMpegArgumentOptions WithVariableBitrate(int vbr)
    {
        return WithArgument(new VariableBitRateArgument(vbr));
    }

    public FFMpegArgumentOptions Resize(int width, int height)
    {
        return WithArgument(new SizeArgument(width, height));
    }

    public FFMpegArgumentOptions Resize(Size? size)
    {
        return WithArgument(new SizeArgument(size));
    }

    public FFMpegArgumentOptions Crop(Size? size, int left, int top)
    {
        return WithArgument(new CropArgument(size, top, left));
    }

    public FFMpegArgumentOptions Crop(int width, int height, int left, int top)
    {
        return WithArgument(new CropArgument(new Size(width, height), top, left));
    }

    public FFMpegArgumentOptions Crop(Size? size)
    {
        return WithArgument(new CropArgument(size, 0, 0));
    }

    public FFMpegArgumentOptions Crop(int width, int height)
    {
        return WithArgument(new CropArgument(new Size(width, height), 0, 0));
    }

    public FFMpegArgumentOptions WithBitStreamFilter(Channel channel, Filter filter)
    {
        return WithArgument(new BitStreamFilterArgument(channel, filter));
    }

    public FFMpegArgumentOptions WithConstantRateFactor(int crf)
    {
        return WithArgument(new ConstantRateFactorArgument(crf));
    }

    public FFMpegArgumentOptions CopyChannel(Channel channel = Channel.Both)
    {
        return WithArgument(new CopyArgument(channel));
    }

    public FFMpegArgumentOptions DisableChannel(Channel channel)
    {
        return WithArgument(new DisableChannelArgument(channel));
    }

    public FFMpegArgumentOptions WithDuration(TimeSpan? duration)
    {
        return WithArgument(new DurationArgument(duration));
    }

    public FFMpegArgumentOptions WithFastStart()
    {
        return WithArgument(new FaststartArgument());
    }

    public FFMpegArgumentOptions WithFrameOutputCount(int frames)
    {
        return WithArgument(new FrameOutputCountArgument(frames));
    }

    public FFMpegArgumentOptions WithHardwareAcceleration(HardwareAccelerationDevice hardwareAccelerationDevice = HardwareAccelerationDevice.Auto)
    {
        return WithArgument(new HardwareAccelerationArgument(hardwareAccelerationDevice));
    }

    public FFMpegArgumentOptions UsingShortest(bool shortest = true)
    {
        return WithArgument(new ShortestArgument(shortest));
    }

    public FFMpegArgumentOptions UsingMultithreading(bool multithread)
    {
        return WithArgument(new ThreadsArgument(multithread));
    }

    public FFMpegArgumentOptions UsingThreads(int threads)
    {
        return WithArgument(new ThreadsArgument(threads));
    }

    public FFMpegArgumentOptions WithVideoCodec(Codec videoCodec)
    {
        return WithArgument(new VideoCodecArgument(videoCodec));
    }

    public FFMpegArgumentOptions WithVideoCodec(string videoCodec)
    {
        return WithArgument(new VideoCodecArgument(videoCodec));
    }

    public FFMpegArgumentOptions WithVideoBitrate(int bitrate)
    {
        return WithArgument(new VideoBitrateArgument(bitrate));
    }

    public FFMpegArgumentOptions WithVideoFilters(Action<VideoFilterOptions> videoFilterOptions)
    {
        var videoFilterOptionsObj = new VideoFilterOptions();
        videoFilterOptions(videoFilterOptionsObj);
        return WithArgument(new VideoFiltersArgument(videoFilterOptionsObj));
    }

    public FFMpegArgumentOptions WithAudioFilters(Action<AudioFilterOptions> audioFilterOptions)
    {
        var audioFilterOptionsObj = new AudioFilterOptions();
        audioFilterOptions(audioFilterOptionsObj);
        return WithArgument(new AudioFiltersArgument(audioFilterOptionsObj));
    }

    public FFMpegArgumentOptions WithFramerate(double framerate)
    {
        return WithArgument(new FrameRateArgument(framerate));
    }

    public FFMpegArgumentOptions WithoutMetadata()
    {
        return WithArgument(new RemoveMetadataArgument());
    }

    public FFMpegArgumentOptions WithSpeedPreset(Speed speed)
    {
        return WithArgument(new SpeedPresetArgument(speed));
    }

    public FFMpegArgumentOptions WithStartNumber(int startNumber)
    {
        return WithArgument(new StartNumberArgument(startNumber));
    }

    public FFMpegArgumentOptions WithCustomArgument(string argument)
    {
        return WithArgument(new CustomArgument(argument));
    }

    public FFMpegArgumentOptions Seek(TimeSpan? seekTo)
    {
        return WithArgument(new SeekArgument(seekTo));
    }

    public FFMpegArgumentOptions EndSeek(TimeSpan? seekTo)
    {
        return WithArgument(new EndSeekArgument(seekTo));
    }

    public FFMpegArgumentOptions Loop(int times)
    {
        return WithArgument(new LoopArgument(times));
    }

    public FFMpegArgumentOptions OverwriteExisting()
    {
        return WithArgument(new OverwriteArgument());
    }

    public FFMpegArgumentOptions SelectStream(int streamIndex, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, channel));
    }

    public FFMpegArgumentOptions SelectStreams(IEnumerable<int> streamIndices, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return streamIndices.Aggregate(this,
            (options, streamIndex) => options.SelectStream(streamIndex, inputFileIndex, channel));
    }

    public FFMpegArgumentOptions DeselectStream(int streamIndex, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, channel, true));
    }

    public FFMpegArgumentOptions DeselectStreams(IEnumerable<int> streamIndices, int inputFileIndex = 0,
        Channel channel = Channel.All)
    {
        return streamIndices.Aggregate(this,
            (options, streamIndex) => options.DeselectStream(streamIndex, inputFileIndex, channel));
    }

    public FFMpegArgumentOptions ForceFormat(ContainerFormat format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    public FFMpegArgumentOptions ForceFormat(string format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    public FFMpegArgumentOptions ForcePixelFormat(string pixelFormat)
    {
        return WithArgument(new ForcePixelFormat(pixelFormat));
    }

    public FFMpegArgumentOptions ForcePixelFormat(PixelFormat pixelFormat)
    {
        return WithArgument(new ForcePixelFormat(pixelFormat));
    }

    public FFMpegArgumentOptions WithAudibleEncryptionKeys(string key, string iv)
    {
        return WithArgument(new AudibleEncryptionKeyArgument(key, iv));
    }

    public FFMpegArgumentOptions WithAudibleActivationBytes(string activationBytes)
    {
        return WithArgument(new AudibleEncryptionKeyArgument(activationBytes));
    }

    public FFMpegArgumentOptions WithTagVersion(int id3v2Version = 3)
    {
        return WithArgument(new ID3V2VersionArgument(id3v2Version));
    }

    public FFMpegArgumentOptions WithGifPaletteArgument(int streamIndex, Size? size, double fps = 12)
    {
        return WithArgument(new GifPaletteArgument(streamIndex, fps, size));
    }

    public FFMpegArgumentOptions WithCopyCodec()
    {
        return WithArgument(new CopyCodecArgument());
    }

    public FFMpegArgumentOptions WithArgument(IArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
