using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents shortest parameter
    /// </summary>
    public class ShortestArgument : Argument<bool>
    {
        public ShortestArgument()
        {
        }

        public ShortestArgument(bool value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.FinalizeAtShortestInput(Value);
        }
    }
}
