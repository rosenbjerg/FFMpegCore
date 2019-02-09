using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of copy parameter
    /// Defines if channel (audio, video or both) should be copied to output file
    /// </summary>
    public class CopyArgument : Argument<Channel>
    {
        public CopyArgument()
        {
            Value = Channel.Both;
        }

        public CopyArgument(Channel value = Channel.Both) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Copy(Value);
        }
    }
}
