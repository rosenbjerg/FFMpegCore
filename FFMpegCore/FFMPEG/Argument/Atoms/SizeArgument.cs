using System.Drawing;
using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class SizeArgument : ScaleArgument
    {
        public SizeArgument() { }

        public SizeArgument(Size? value) : base(value ?? default) { }

        public SizeArgument(VideoSize videosize) : base(videosize) { }

        public SizeArgument(int width, int height) : base(width, height) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return Value == default ? string.Empty : $"-s {Value.Width}x{Value.Height}";
        }
    }
}
