using FFMpegCore.Enums;

namespace FFMpegCore.Arguments.Protocols.Rtsp;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-protocols.html#rtsp" />
/// Set RTSP transport protocols.
/// </summary>
public sealed class RtspTransportArgument(RtspTransportProtocol protocol) : IRtspMuxerArgument, IRtspDemuxerArgument
{
    public string Text => $"-rtsp_transport {protocol}";
}
