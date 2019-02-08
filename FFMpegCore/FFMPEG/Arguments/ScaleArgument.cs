using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents scale parameter
    /// </summary>
    public class ScaleArgument : Argument<Size>
    {
        public ScaleArgument()
        {
        }

        public ScaleArgument(Size value) : base(value)
        {
        }

        public ScaleArgument(int width, int heignt) : base(new Size(width, heignt))
        {
        }

        public ScaleArgument(VideoSize videosize)
        {
            Value = videosize == VideoSize.Original ? new Size(-1, -1) : new Size(-1, (int)videosize);
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.Scale(Value);
        }
    }
}
