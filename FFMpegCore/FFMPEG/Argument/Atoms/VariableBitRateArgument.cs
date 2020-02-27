using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Variable Bitrate Argument (VBR) argument
    /// </summary>
    public class VariableBitRateArgument : Argument<int>
    {
        public VariableBitRateArgument(int vbr) : base(vbr)
        {
            if (vbr < 0 || vbr > 5)
            {
                throw new ArgumentException("Argument is outside range (0 - 5)", nameof(vbr));
            }
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-vbr {Value}";
        }
    }
}