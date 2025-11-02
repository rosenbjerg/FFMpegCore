using FFMpegCore.Enums;

namespace FFMpegCore.Arguments.VideoOptions;

/// <summary>
/// <see href="https://trac.ffmpeg.org/wiki/Hardware/VAAPI#SurfaceFormats" />
/// undocumented in <see href="https://www.ffmpeg.org/ffmpeg.html#Advanced-Video-options" />
/// </summary>
public sealed class HardwareAccelerationOutputFormatArgument(HardwareAccelerationDevice hardwareAccelerationDevice) : IArgument
{
    public string Text => $"-hwaccel_output_format {hardwareAccelerationDevice.ToString().ToLowerInvariant()}";
}
