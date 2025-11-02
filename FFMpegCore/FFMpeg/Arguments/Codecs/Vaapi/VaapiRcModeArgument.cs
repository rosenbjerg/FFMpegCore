using FFMpegCore.Enums;

namespace FFMpegCore.Arguments.Codecs.Vaapi;

/// <summary>
/// <see href="https://www.ffmpeg.org/ffmpeg-codecs.html#VAAPI-encoders" />
/// Set the rate control mode to use. A given driver may only support a subset of modes.
/// </summary>
public sealed class VaapiRcModeArgument(VaapiRcMode rcMode) : IArgument
{
    public string Text => $"-rc_mode {rcMode}";
}
