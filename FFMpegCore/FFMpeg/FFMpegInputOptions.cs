using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class FFMpegInputOptions : FFMpegArgumentsBase
{
    internal FFMpegInputOptions() { }

    /// <summary>-c:v, selecting the decoder</summary>
    public FFMpegInputOptions WithVideoDecoder(string decoder)
    {
        return WithArgument(new VideoCodecArgument(decoder));
    }

    /// <summary>-c:a, selecting the decoder</summary>
    public FFMpegInputOptions WithAudioDecoder(string decoder)
    {
        return WithArgument(new AudioCodecArgument(decoder));
    }

    /// <summary>-s</summary>
    public FFMpegInputOptions WithFrameSize(int width, int height)
    {
        return WithArgument(new SizeArgument(width, height));
    }

    /// <summary>-s</summary>
    public FFMpegInputOptions WithFrameSize(Size? size)
    {
        return WithArgument(new SizeArgument(size));
    }

    /// <summary>-hwaccel</summary>
    public FFMpegInputOptions WithHardwareAcceleration(HardwareAccelerationDevice hardwareAccelerationDevice = HardwareAccelerationDevice.Auto)
    {
        return WithArgument(new HardwareAccelerationArgument(hardwareAccelerationDevice));
    }

    /// <summary>-loop</summary>
    public FFMpegInputOptions WithLoop(int times)
    {
        return WithArgument(new LoopArgument(times));
    }

    /// <summary>-audible_key and -audible_iv</summary>
    public FFMpegInputOptions WithAudibleEncryptionKeys(string key, string iv)
    {
        return WithArgument(new AudibleEncryptionKeyArgument(key, iv));
    }

    /// <summary>-activation_bytes</summary>
    public FFMpegInputOptions WithAudibleActivationBytes(string activationBytes)
    {
        return WithArgument(new AudibleEncryptionKeyArgument(activationBytes));
    }

    /// <summary>-ss</summary>
    public FFMpegInputOptions WithStartTime(TimeSpan? startTime)
    {
        return WithArgument(new SeekArgument(startTime));
    }

    /// <summary>-to</summary>
    public FFMpegInputOptions WithStopTime(TimeSpan? stopTime)
    {
        return WithArgument(new EndSeekArgument(stopTime));
    }

    /// <summary>-t</summary>
    public FFMpegInputOptions WithDuration(TimeSpan? duration)
    {
        return WithArgument(new DurationArgument(duration));
    }

    /// <summary>-r</summary>
    public FFMpegInputOptions WithFrameRate(double frameRate)
    {
        return WithArgument(new FrameRateArgument(frameRate));
    }

    /// <summary>-f</summary>
    public FFMpegInputOptions ForceFormat(ContainerFormat format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    /// <summary>-f</summary>
    public FFMpegInputOptions ForceFormat(string format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    /// <summary>-pix_fmt</summary>
    public FFMpegInputOptions WithPixelFormat(string pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    /// <summary>-pix_fmt</summary>
    public FFMpegInputOptions WithPixelFormat(PixelFormat pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    /// <summary>-ar</summary>
    public FFMpegInputOptions WithAudioSamplingRate(int samplingRate = 48000)
    {
        return WithArgument(new AudioSamplingRateArgument(samplingRate));
    }

    /// <summary>-start_number</summary>
    public FFMpegInputOptions WithStartNumber(int startNumber)
    {
        return WithArgument(new StartNumberArgument(startNumber));
    }

    /// <summary>-threads</summary>
    public FFMpegInputOptions WithThreads(int threads)
    {
        return WithArgument(new ThreadsArgument(threads));
    }

    /// <summary>-vn</summary>
    public FFMpegInputOptions DisableVideo()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Video));
    }

    /// <summary>-an</summary>
    public FFMpegInputOptions DisableAudio()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Audio));
    }

    /// <summary>-sn</summary>
    public FFMpegInputOptions DisableSubtitles()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Subtitle));
    }

    /// <summary>-dn</summary>
    public FFMpegInputOptions DisableData()
    {
        return WithArgument(new DisableStreamArgument(StreamType.Data));
    }

    public FFMpegInputOptions WithCustomArgument(string argument)
    {
        return WithArgument(new CustomArgument(argument));
    }

    public FFMpegInputOptions WithArgument(IArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
