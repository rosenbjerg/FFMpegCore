namespace FFMpegCore.Arguments.Formats;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#Format-Options" />
/// Use wallclock as timestamps if set to 1. Default is 0.
/// </summary>
public sealed class UseWallclockAsTimestampsArgument(bool value) : BaseBoolArgument(value)
{
    protected override string ArgumentName => "use_wallclock_as_timestamps";
}
