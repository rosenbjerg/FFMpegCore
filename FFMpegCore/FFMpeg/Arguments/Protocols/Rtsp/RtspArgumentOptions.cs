using FFMpegCore.Enums;

namespace FFMpegCore.Arguments.Protocols.Rtsp;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-protocols.html#rtsp" />
/// </summary>
public sealed class RtspArgumentOptions
{
    private readonly FFMpegArgumentOptions _options;

    internal RtspArgumentOptions(FFMpegArgumentOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// <inheritdoc cref="RtspTransportArgument"/>
    /// </summary>
    /// <param name="rtspTransportProtocol"></param>
    /// <returns></returns>
    public RtspArgumentOptions WithRtspTransport(RtspTransportProtocol rtspTransportProtocol)
    {
        return WithArgument(new RtspTransportArgument(rtspTransportProtocol));
    }

    public RtspArgumentOptions WithArgument(IRtspArgument argument)
    {
        _options.WithArgument(argument);
        return this;
    }
}
