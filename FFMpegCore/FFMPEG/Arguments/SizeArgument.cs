using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents size parameter
    /// </summary>
    public class SizeArgument : ScaleArgument
    {
        public SizeArgument()
        {
        }

        public SizeArgument(Size value) : base(value)
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
            return ArgumentsStringifier.Size(Value);
        }
    }
}
