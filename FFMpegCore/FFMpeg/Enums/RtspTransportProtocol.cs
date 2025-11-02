namespace FFMpegCore.Enums;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-protocols.html#rtsp" />
/// RTSP transport protocols
/// </summary>
public enum RtspTransportProtocol
{
    udp,
    tcp,
    udp_multicast,
    http,
    https,
}
