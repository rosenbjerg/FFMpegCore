using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents duration parameter
    /// </summary>
    public class DurationArgument : Argument<TimeSpan?>
    {
        public DurationArgument() { }

        public DurationArgument(TimeSpan? value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return !Value.HasValue ? string.Empty : $"-t {Value}";
        }
    }
}
