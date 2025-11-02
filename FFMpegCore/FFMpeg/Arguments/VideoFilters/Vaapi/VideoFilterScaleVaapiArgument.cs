using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments.VideoFilters.Vaapi;

/// <summary>
/// <see href="https://trac.ffmpeg.org/wiki/Hardware/VAAPI#SurfaceFormats" />
/// undocumented in: <see href="https://www.ffmpeg.org/ffmpeg-filters.html#VAAPI-Video-Filters" />
/// may refer to: <see href="https://www.ffmpeg.org/ffmpeg-filters.html#scale-1" />
/// </summary>
public sealed class VideoFilterScaleVaapiArgument : IVaapiVideoFilterArgument
{
    private readonly Size? _size;

    public VideoFilterScaleVaapiArgument(Size? size)
    {
        _size = size;
    }

    public VideoFilterScaleVaapiArgument(int width, int height) : this(new Size(width, height)) { }

    public VideoFilterScaleVaapiArgument(VideoSize videosize)
    {
        _size = videosize == VideoSize.Original ? null : new Size(-1, (int)videosize);
    }

    public string Key => "scale_vaapi";
    public string Value => _size == null ? string.Empty : $"{_size.Value.Width}:{_size.Value.Height}";
}
