using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents start number parameter
    /// </summary>
    public class StartNumberArgument : Argument<int>
    {
        public StartNumberArgument()
        {
        }

        public StartNumberArgument(int value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.StartNumber(Value);
        }
    }
}
