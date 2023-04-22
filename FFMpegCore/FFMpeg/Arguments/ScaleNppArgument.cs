using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class ScaleNppArgument : IVideoFilterArgument
{
    public readonly Size? Size;
    public ScaleNppArgument(Size? size)
    {
        Size = size;
    }

    public ScaleNppArgument(int width, int height) : this(new Size(width, height)) { }

    public ScaleNppArgument(VideoSize videosize)
    {
        Size = videosize == VideoSize.Original ? null : (Size?)new Size(-1, (int)videosize);
    }

    public string Key { get; } = "scale_npp";
    public string Value => Size == null ? string.Empty : $"{Size.Value.Width}:{Size.Value.Height}";
}
