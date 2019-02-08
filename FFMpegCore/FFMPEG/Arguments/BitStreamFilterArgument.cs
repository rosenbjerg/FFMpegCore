using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents parameter of bitstream filter
    /// </summary>
    public class BitStreamFilterArgument : Argument<Channel, Filter>
    {
        public BitStreamFilterArgument()
        {
        }

        public BitStreamFilterArgument(Channel first, Filter second) : base(first, second)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.BitStreamFilter(First, Second);
        }
    }
}
