using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents duration parameter
    /// </summary>
    public class DurationArgument : Argument<TimeSpan?>
    {
        public DurationArgument()
        {
        }

        public DurationArgument(TimeSpan? value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Duration(Value);
        }
    }
}
