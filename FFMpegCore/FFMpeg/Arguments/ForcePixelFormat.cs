using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class ForcePixelFormat : IArgument
{
    public ForcePixelFormat(string format)
    {
        PixelFormat = format;
    }

    public ForcePixelFormat(PixelFormat format) : this(format.Name) { }
    public string PixelFormat { get; }
    public string Text => $"-pix_fmt {PixelFormat}";
}
