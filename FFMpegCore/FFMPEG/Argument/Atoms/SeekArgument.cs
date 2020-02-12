using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents seek parameter
    /// </summary>
    public class SeekArgument : Argument<TimeSpan?>
    {
        public SeekArgument()
        {
        }

        public SeekArgument(TimeSpan? value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Seek(Value);
        }
    }
}
