using System.Drawing;
using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class SizeArgument : ScaleArgument
    {
        public SizeArgument()
        {
        }

        public SizeArgument(Size? value) : base(value ?? new Size())
        {
        }

        public SizeArgument(VideoSize videosize) : base(videosize)
        {
        }

        public SizeArgument(int width, int heignt) : base(width, heignt)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Size(Value);
        }
    }
}
