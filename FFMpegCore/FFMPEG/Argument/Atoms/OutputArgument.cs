using System;
using System.IO;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents output parameter
    /// </summary>
    public class OutputArgument : Argument<string>
    {
        public OutputArgument() { }

        public OutputArgument(string value) : base(value) { }

        public OutputArgument(VideoInfo value) : base(value.FullName) { }

        public OutputArgument(FileInfo value) : base(value.FullName) { }

        public OutputArgument(Uri value) : base(value.AbsolutePath) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"\"{Value}\"";
        }

        public FileInfo GetAsFileInfo()
        {
            return new FileInfo(Value);
        }
    }
}
