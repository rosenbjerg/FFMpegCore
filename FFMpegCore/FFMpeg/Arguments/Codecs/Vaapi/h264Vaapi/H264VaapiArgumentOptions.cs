namespace FFMpegCore.Arguments.Codecs.Vaapi.h264Vaapi;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#image2_002c-image2pipe" />
/// </summary>
public sealed class H264VaapiArgumentOptions
{
    private readonly FFMpegArgumentOptions _options;

    internal H264VaapiArgumentOptions(FFMpegArgumentOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// <inheritdoc cref="VaapiQpArgument"/>
    /// </summary>
    /// <param name="quantizer"><c>0</c> - <c>52</c></param>
    /// <returns></returns>
    public H264VaapiArgumentOptions WithQuantizer(sbyte quantizer)
    {
        return WithArgument(new VaapiQpArgument(quantizer));
    }

    public H264VaapiArgumentOptions WithArgument(IH264VaapiArgument argument)
    {
        _options.WithArgument(argument);
        return this;
    }
}
