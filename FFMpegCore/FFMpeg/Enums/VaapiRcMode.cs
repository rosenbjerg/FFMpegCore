namespace FFMpegCore.Enums;

/// <summary>
/// <see href="https://www.ffmpeg.org/ffmpeg-codecs.html#VAAPI-encoders" />
/// </summary>
public enum VaapiRcMode
{
    /// <summary>
    /// Choose the mode automatically based on driver support and the other options. This is the default.
    /// </summary>
    auto,

    /// <summary>
    /// Constant-quality.
    /// </summary>
    CQP,

    /// <summary>
    /// Constant-bitrate.
    /// </summary>
    CBR,

    /// <summary>
    /// Variable-bitrate.
    /// </summary>
    VBR,

    /// <summary>
    /// Intelligent constant-quality.
    /// </summary>
    ICQ,

    /// <summary>
    /// Quality-defined variable-bitrate.
    /// </summary>
    QVBR,

    /// <summary>
    /// Average variable bitrate.
    /// </summary>
    AVBR,
}
