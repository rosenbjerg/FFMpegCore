using FFMpegCore.FFMPEG.Enums;
using System.Drawing;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents scale parameter
    /// </summary>
    public class ScaleArgument : IArgument
    {
        public readonly Size? Size;
        public ScaleArgument(Size? size)
        {
            Size = size;
        }

        public ScaleArgument(int width, int height) : this(new Size(width, height)) { }

        public ScaleArgument(VideoSize videosize)
        {
            Size = videosize == VideoSize.Original ? new Size(-1, -1) : new Size(-1, (int)videosize);
        }

        public virtual string Text => Size.HasValue ? $"-vf scale={Size.Value.Width}:{Size.Value.Height}" : string.Empty;
    }
}
