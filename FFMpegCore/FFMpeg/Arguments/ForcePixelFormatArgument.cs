using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class ForcePixelFormatArgument : IArgument
{
    public ForcePixelFormatArgument(string format)
    {
        PixelFormat = format;
    }

    public ForcePixelFormatArgument(PixelFormat format) : this(format.Name) { }
    public string PixelFormat { get; }
    public string Text => $"-pix_fmt {PixelFormat}";
}
