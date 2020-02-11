using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents threads parameter
    /// Number of threads used for video encoding
    /// </summary>
    public class ThreadsArgument : Argument<int>
    {
        public ThreadsArgument()
        {
        }

        public ThreadsArgument(int value) : base(value)
        {
        }

        public ThreadsArgument(bool isMultiThreaded) : 
            base(isMultiThreaded
                ? Environment.ProcessorCount
                : 1)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Threads(Value);
        }
    }
}
