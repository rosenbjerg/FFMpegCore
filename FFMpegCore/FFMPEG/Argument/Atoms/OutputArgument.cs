using System;
using System.IO;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents output parameter
    /// </summary>
    public class OutputArgument : Argument<string>
    {
        public OutputArgument()
        {
        }

        public OutputArgument(string value) : base(value)
        {
        }

        public OutputArgument(VideoInfo value) : base(value.FullName)
        {
        }

        public OutputArgument(FileInfo value) : base(value.FullName)
        {
        }

        public OutputArgument(Uri value) : base(value.AbsolutePath)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Output(Value);
        }

        public FileInfo GetAsFileInfo()
        {
            return new FileInfo(Value);
        }
    }
}
