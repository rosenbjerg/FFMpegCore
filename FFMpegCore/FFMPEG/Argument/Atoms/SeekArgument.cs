using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents seek parameter
    /// </summary>
    public class SeekArgument : Argument<TimeSpan?>
    {
        public SeekArgument() { }

        public SeekArgument(TimeSpan? value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return !Value.HasValue ? string.Empty : $"-ss {Value}";
        }
    }
}
