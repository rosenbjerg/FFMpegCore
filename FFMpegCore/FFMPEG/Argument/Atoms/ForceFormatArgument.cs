using FFMpegCore.FFMPEG.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents force format parameter
    /// </summary>
    public class ForceFormatArgument : Argument<VideoCodec>
    {
        public ForceFormatArgument()
        {
        }

        public ForceFormatArgument(VideoCodec value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.ForceFormat(Value);
        }
    }
}
