using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents frame rate parameter
    /// </summary>
    public class FrameRateArgument : Argument<double>
    {
        public FrameRateArgument()
        {
        }

        public FrameRateArgument(double value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.FrameRate(Value);
        }
    }
}
