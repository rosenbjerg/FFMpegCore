using System.Drawing;
using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class SizeArgument : ScaleArgument
    {
        public SizeArgument(Size? value) : base(value) { }

        public SizeArgument(VideoSize videosize) : base(videosize) { }

        public SizeArgument(int width, int height) : base(width, height) { }

        public override string Text => Size.HasValue ? $"-s {Size.Value.Width}x{Size.Value.Height}" : string.Empty;
    }
}
