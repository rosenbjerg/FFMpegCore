using System.Drawing;

namespace FFMpegCore.Arguments;

public class CropArgument : IArgument
{
    public readonly int Left;
    public readonly Size? Size;
    public readonly int Top;

    public CropArgument(Size? size, int top, int left)
    {
        Size = size;
        Top = top;
        Left = left;
    }

    public CropArgument(int width, int height, int top, int left) : this(new Size(width, height), top, left) { }

    public string Text => Size == null ? string.Empty : $"-vf crop={Size.Value.Width}:{Size.Value.Height}:{Left}:{Top}";
}
