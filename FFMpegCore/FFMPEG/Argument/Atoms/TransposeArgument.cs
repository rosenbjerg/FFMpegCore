using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Transpose argument.
    /// 0 = 90CounterCLockwise and Vertical Flip (default)
    /// 1 = 90Clockwise
    /// 2 = 90CounterClockwise
    /// 3 = 90Clockwise and Vertical Flip
    /// </summary>
    public class TransposeArgument : Argument<int>
    {
        public TransposeArgument() { }

        public TransposeArgument(int transpose) : base(transpose)
        {
            if (transpose < 0 || transpose > 3)
            {
                throw new ArgumentException("Argument is outside range (0 - 3)", nameof(transpose));
            }
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-vf \"transpose={Value}\"";
        }
    }
}