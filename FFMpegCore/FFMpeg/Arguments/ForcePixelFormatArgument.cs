using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class ForcePixelFormatArgument : IArgument
{
    public ForcePixelFormatArgument(PixelFormat format)
    {
        PixelFormat = format.Name;
    }

    public string PixelFormat { get; }
    public string Text => $"-pix_fmt {PixelFormat}";
}
