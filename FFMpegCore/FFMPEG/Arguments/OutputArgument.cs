using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents output parameter
    /// </summary>
    public class OutputArgument : InputArgument
    {
        public OutputArgument()
        {
        }

        public OutputArgument(string value) : base(value)
        {
        }

        public OutputArgument(VideoInfo value) : base(value)
        {
        }

        public OutputArgument(FileInfo value) : base(value)
        {
        }

        public OutputArgument(Uri value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.Output(Value);
        }

        public FileInfo GetAsFileInfo()
        {
            return new FileInfo(Value);
        }
    }
}
