namespace FFMpegCore.Arguments.GenericOptions;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg.html#Generic-options" />
/// Suppress printing banner.
/// All FFmpeg tools will normally show a copyright notice, build options and library versions.
/// This option can be used to suppress printing this information.
/// </summary>
public sealed class HideBanner : IArgument
{
    public string Text => "-hide_banner";
}
