using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents seek parameter
    /// </summary>
    public class SeekArgument : Argument<TimeSpan>
    {
        public SeekArgument()
        {
        }

        public SeekArgument(TimeSpan value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.Seek(Value);
        }
    }
}
