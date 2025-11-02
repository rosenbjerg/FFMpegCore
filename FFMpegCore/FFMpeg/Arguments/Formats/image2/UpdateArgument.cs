namespace FFMpegCore.Arguments.Formats.image2;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg-formats.html#Options-27" />
/// If set to 1, the filename will always be interpreted as just a filename, not a pattern, and the corresponding file will be continuously overwritten with new images. Default value is 0.
/// </summary>
public sealed class UpdateArgument(bool value = false) : BaseBoolArgument(value), IImage2MuxerArgument
{
    protected override string ArgumentName => "update";
}
