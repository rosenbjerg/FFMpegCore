using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class FFMpegOutputOptions : FFMpegArgumentsBase
{
    internal FFMpegOutputOptions() { }

    /// <summary>-c:v</summary>
    public FFMpegOutputOptions WithVideoCodec(Codec videoCodec)
    {
        return WithArgument(new VideoCodecArgument(videoCodec));
    }

    /// <summary>-c:v</summary>
    public FFMpegOutputOptions WithVideoCodec(string videoCodec)
    {
        return WithArgument(new VideoCodecArgument(videoCodec));
    }

    /// <summary>-c:a</summary>
    public FFMpegOutputOptions WithAudioCodec(Codec audioCodec)
    {
        return WithArgument(new AudioCodecArgument(audioCodec));
    }

    /// <summary>-c:a</summary>
    public FFMpegOutputOptions WithAudioCodec(string audioCodec)
    {
        return WithArgument(new AudioCodecArgument(audioCodec));
    }

    /// <summary>-c copy, or -c:v / -c:a / -c:s copy for one stream type</summary>
    public FFMpegOutputOptions CopyStreams(StreamType streamType = StreamType.All)
    {
        return WithArgument(new CopyArgument(streamType));
    }

    /// <summary>-b:v</summary>
    public FFMpegOutputOptions WithVideoBitrate(int bitrate)
    {
        return WithArgument(new VideoBitrateArgument(bitrate));
    }

    /// <summary>-b:a</summary>
    public FFMpegOutputOptions WithAudioBitrate(AudioQuality audioQuality)
    {
        return WithArgument(new AudioBitrateArgument(audioQuality));
    }

    /// <summary>-b:a</summary>
    public FFMpegOutputOptions WithAudioBitrate(int bitrate)
    {
        return WithArgument(new AudioBitrateArgument(bitrate));
    }

    /// <summary>-vbr</summary>
    public FFMpegOutputOptions WithVariableBitrate(int vbr)
    {
        return WithArgument(new VariableBitRateArgument(vbr));
    }

    /// <summary>-crf</summary>
    public FFMpegOutputOptions WithConstantRateFactor(int crf)
    {
        return WithArgument(new ConstantRateFactorArgument(crf));
    }

    /// <summary>-preset</summary>
    public FFMpegOutputOptions WithSpeedPreset(Speed speed)
    {
        return WithArgument(new SpeedPresetArgument(speed));
    }

    /// <summary>-frames:v</summary>
    public FFMpegOutputOptions WithFrameOutputCount(int frames)
    {
        return WithArgument(new FrameOutputCountArgument(frames));
    }

    /// <summary>-bsf:v or -bsf:a</summary>
    public FFMpegOutputOptions WithBitstreamFilter(StreamType streamType, BitstreamFilter filter)
    {
        return WithArgument(new BitstreamFilterArgument(streamType, filter));
    }

    /// <summary>-movflags faststart</summary>
    public FFMpegOutputOptions WithFastStart()
    {
        return WithArgument(new FaststartArgument());
    }

    /// <summary>-shortest</summary>
    public FFMpegOutputOptions WithShortest(bool shortest = true)
    {
        return WithArgument(new ShortestArgument(shortest));
    }

    /// <summary>-y</summary>
    public FFMpegOutputOptions OverwriteExisting()
    {
        return WithArgument(new OverwriteArgument());
    }

    /// <summary>-map</summary>
    public FFMpegOutputOptions WithMap(int streamIndex, int inputFileIndex = 0, StreamType streamType = StreamType.All)
    {
        return WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, streamType));
    }

    /// <summary>-map, once per index</summary>
    public FFMpegOutputOptions WithMap(IEnumerable<int> streamIndices, int inputFileIndex = 0, StreamType streamType = StreamType.All)
    {
        return streamIndices.Aggregate(this, (options, streamIndex) => options.WithMap(streamIndex, inputFileIndex, streamType));
    }

    /// <summary>-map -, a negative mapping that excludes the stream</summary>
    public FFMpegOutputOptions WithNegativeMap(int streamIndex, int inputFileIndex = 0, StreamType streamType = StreamType.All)
    {
        return WithArgument(new MapStreamArgument(streamIndex, inputFileIndex, streamType, true));
    }

    /// <summary>-map -, once per index</summary>
    public FFMpegOutputOptions WithNegativeMap(IEnumerable<int> streamIndices, int inputFileIndex = 0, StreamType streamType = StreamType.All)
    {
        return streamIndices.Aggregate(this, (options, streamIndex) => options.WithNegativeMap(streamIndex, inputFileIndex, streamType));
    }

    /// <summary>-map_metadata -1</summary>
    public FFMpegOutputOptions WithoutMetadata()
    {
        return WithArgument(new RemoveMetadataArgument());
    }

    /// <summary>-id3v2_version</summary>
    public FFMpegOutputOptions WithId3v2Version(int version = 3)
    {
        return WithArgument(new ID3V2VersionArgument(version));
    }

    /// <summary>-filter_complex with palettegen and paletteuse</summary>
    public FFMpegOutputOptions WithGifPalette(int streamIndex, Size? size, double fps = 12)
    {
        return WithArgument(new GifPaletteArgument(streamIndex, fps, size));
    }

    /// <summary>-vf</summary>
    public FFMpegOutputOptions WithVideoFilters(Action<VideoFilterOptions> videoFilterOptions)
    {
        var videoFilterOptionsObj = new VideoFilterOptions();
        videoFilterOptions(videoFilterOptionsObj);
        return WithArgument(new VideoFiltersArgument(videoFilterOptionsObj));
    }

    /// <summary>-af</summary>
    public FFMpegOutputOptions WithAudioFilters(Action<AudioFilterOptions> audioFilterOptions)
    {
        var audioFilterOptionsObj = new AudioFilterOptions();
        audioFilterOptions(audioFilterOptionsObj);
        return WithArgument(new AudioFiltersArgument(audioFilterOptionsObj));
    }

    /// <summary>-ss</summary>
    public FFMpegOutputOptions WithStartTime(TimeSpan? startTime)
    {
        return WithArgument(new SeekArgument(startTime));
    }

    /// <summary>-to</summary>
    public FFMpegOutputOptions WithStopTime(TimeSpan? stopTime)
    {
        return WithArgument(new EndSeekArgument(stopTime));
    }

    /// <summary>-t</summary>
    public FFMpegOutputOptions WithDuration(TimeSpan? duration)
    {
        return WithArgument(new DurationArgument(duration));
    }

    /// <summary>-r</summary>
    public FFMpegOutputOptions WithFrameRate(double frameRate)
    {
        return WithArgument(new FrameRateArgument(frameRate));
    }

    /// <summary>-f</summary>
    public FFMpegOutputOptions ForceFormat(ContainerFormat format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    /// <summary>-f</summary>
    public FFMpegOutputOptions ForceFormat(string format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    /// <summary>-pix_fmt</summary>
    public FFMpegOutputOptions WithPixelFormat(string pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    /// <summary>-pix_fmt</summary>
    public FFMpegOutputOptions WithPixelFormat(PixelFormat pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    /// <summary>-ar</summary>
    public FFMpegOutputOptions WithAudioSamplingRate(int samplingRate = 48000)
    {
        return WithArgument(new AudioSamplingRateArgument(samplingRate));
    }

    /// <summary>-start_number</summary>
    public FFMpegOutputOptions WithStartNumber(int startNumber)
    {
        return WithArgument(new StartNumberArgument(startNumber));
    }

    /// <summary>-threads</summary>
    public FFMpegOutputOptions WithThreads(int threads)
    {
        return WithArgument(new ThreadsArgument(threads));
    }

    /// <summary>-vn</summary>
    public FFMpegOutputOptions DisableVideo()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Video));
    }

    /// <summary>-an</summary>
    public FFMpegOutputOptions DisableAudio()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Audio));
    }

    /// <summary>-sn</summary>
    public FFMpegOutputOptions DisableSubtitles()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Subtitle));
    }

    /// <summary>-dn</summary>
    public FFMpegOutputOptions DisableData()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Data));
    }

    public FFMpegOutputOptions WithCustomArgument(string argument)
    {
        return WithArgument(new CustomArgument(argument));
    }

    public FFMpegOutputOptions WithArgument(IArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
