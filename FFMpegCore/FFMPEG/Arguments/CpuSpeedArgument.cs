using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class CpuSpeedArgument : Argument<int>
    {
        public CpuSpeedArgument()
        {
        }

        public CpuSpeedArgument(int value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.Speed(Value);
        }
    }
}
