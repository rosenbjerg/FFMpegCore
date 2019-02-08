using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents frame output count parameter
    /// </summary>
    public class FrameOutputCountArgument : Argument<int>
    {
        public FrameOutputCountArgument()
        {
        }

        public FrameOutputCountArgument(int value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.FrameOutputCount(Value);
        }
    }
}
