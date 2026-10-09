using System.Drawing;

namespace FFMpegCore.Arguments;

public class CropArgument : IVideoFilterArgument
{
    public readonly int Left;
    public readonly Size Size;
    public readonly int Top;

    public CropArgument(Size size, int left = 0, int top = 0)
    {
        Size = size;
        Left = left;
        Top = top;
    }

    public CropArgument(int width, int height, int left = 0, int top = 0) : this(new Size(width, height), left, top) { }

    public string Key => "crop";
    public string Value => $"{Size.Width}:{Size.Height}:{Left}:{Top}";
}
