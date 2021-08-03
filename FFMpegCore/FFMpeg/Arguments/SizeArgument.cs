using System.Drawing;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class SizeArgument : IArgument
    {
        public readonly Size? Size;
        public SizeArgument(Size? size)
        {
            Size = size;
        }

        public SizeArgument(int width, int height) : this(new Size(width, height)) { }

        public string Text => Size == null ? string.Empty : $"-s {Size.Value.Width}x{Size.Value.Height}";
    }
}
