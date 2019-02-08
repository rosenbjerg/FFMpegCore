using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents speed parameter
    /// </summary>
    public class SpeedArgument : Argument<Speed>
    {
        public SpeedArgument()
        {            
        }

        public SpeedArgument(Speed value) : base(value)
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
