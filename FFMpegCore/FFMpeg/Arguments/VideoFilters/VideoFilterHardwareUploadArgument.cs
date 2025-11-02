namespace FFMpegCore.Arguments.VideoFilters;

/// <summary>
/// <see href="https://www.ffmpeg.org/ffmpeg-filters.html#hwupload" />
/// Upload system memory frames to hardware surfaces.
/// </summary>
public sealed class VideoFilterHardwareUploadArgument : IVideoFilterArgument
{
    public string Key => string.Empty;
    public string Value => "hwupload";
}
