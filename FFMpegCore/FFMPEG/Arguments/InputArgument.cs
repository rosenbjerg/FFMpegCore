using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents input parameter
    /// </summary>
    public class InputArgument : Argument<string>
    {
        public InputArgument()
        {
        }

        public InputArgument(string value) : base(value)
        {
        }

        public InputArgument(VideoInfo value) : base(value.FullName)
        {
        }

        public InputArgument(FileInfo value) : base(value.FullName)
        {
        }

        public InputArgument(Uri value) : base(value.AbsolutePath)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.Input(Value);
        }
    }
}
