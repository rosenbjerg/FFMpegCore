using FFMpegCore.FFMPEG.Enums;
using System.Drawing;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents scale parameter
    /// </summary>
    public class ScaleArgument : Argument<Size>
    {
        public ScaleArgument() { }

        public ScaleArgument(Size value) : base(value) { }

        public ScaleArgument(int width, int height) : base(new Size(width, height)) { }

        public ScaleArgument(VideoSize videosize)
        {
            Value = videosize == VideoSize.Original ? new Size(-1, -1) : new Size(-1, (int)videosize);
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-vf scale={Value.Width}:{Value.Height}";
        }
    }
}
