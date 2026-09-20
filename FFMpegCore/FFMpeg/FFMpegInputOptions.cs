using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class FFMpegInputOptions : FFMpegArgumentsBase
{
    internal FFMpegInputOptions() { }

    public FFMpegInputOptions WithVideoDecoder(string decoder)
    {
        return WithArgument(new VideoCodecArgument(decoder));
    }

    public FFMpegInputOptions WithAudioDecoder(string decoder)
    {
        return WithArgument(new AudioCodecArgument(decoder));
    }

    public FFMpegInputOptions Resize(int width, int height)
    {
        return WithArgument(new SizeArgument(width, height));
    }

    public FFMpegInputOptions Resize(Size? size)
    {
        return WithArgument(new SizeArgument(size));
    }

    public FFMpegInputOptions WithHardwareAcceleration(HardwareAccelerationDevice hardwareAccelerationDevice = HardwareAccelerationDevice.Auto)
    {
        return WithArgument(new HardwareAccelerationArgument(hardwareAccelerationDevice));
    }

    public FFMpegInputOptions Loop(int times)
    {
        return WithArgument(new LoopArgument(times));
    }

    public FFMpegInputOptions WithAudibleEncryptionKeys(string key, string iv)
    {
        return WithArgument(new AudibleEncryptionKeyArgument(key, iv));
    }

    public FFMpegInputOptions WithAudibleActivationBytes(string activationBytes)
    {
        return WithArgument(new AudibleEncryptionKeyArgument(activationBytes));
    }

    public FFMpegInputOptions WithDuration(TimeSpan? duration)
    {
        return WithArgument(new DurationArgument(duration));
    }

    public FFMpegInputOptions WithFramerate(double framerate)
    {
        return WithArgument(new FrameRateArgument(framerate));
    }

    public FFMpegInputOptions WithStartNumber(int startNumber)
    {
        return WithArgument(new StartNumberArgument(startNumber));
    }

    public FFMpegInputOptions UsingThreads(int threads)
    {
        return WithArgument(new ThreadsArgument(threads));
    }

    public FFMpegInputOptions Seek(TimeSpan? seekTo)
    {
        return WithArgument(new SeekArgument(seekTo));
    }

    public FFMpegInputOptions EndSeek(TimeSpan? seekTo)
    {
        return WithArgument(new EndSeekArgument(seekTo));
    }

    public FFMpegInputOptions ForceFormat(ContainerFormat format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    public FFMpegInputOptions ForceFormat(string format)
    {
        return WithArgument(new ForceFormatArgument(format));
    }

    public FFMpegInputOptions ForcePixelFormat(string pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    public FFMpegInputOptions ForcePixelFormat(PixelFormat pixelFormat)
    {
        return WithArgument(new ForcePixelFormatArgument(pixelFormat));
    }

    public FFMpegInputOptions WithAudioSamplingRate(int samplingRate = 48000)
    {
        return WithArgument(new AudioSamplingRateArgument(samplingRate));
    }

    public FFMpegInputOptions DisableChannel(Channel channel)
    {
        return WithArgument(new DisableChannelArgument(channel));
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
