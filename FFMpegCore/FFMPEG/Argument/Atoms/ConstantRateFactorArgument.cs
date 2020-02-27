using System;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Constant Rate Factor (CRF) argument
    /// </summary>
    public class ConstantRateFactorArgument : Argument<int>
    {
        public ConstantRateFactorArgument(int crf) : base(crf)
        {
            if (crf < 0 || crf > 63)
            {
                throw new ArgumentException("Argument is outside range (0 - 63)", nameof(crf));
            }
        }

        public override string GetStringValue()
        {
            return $"-crf {Value} ";
        }
    }
    /// <summary>
    /// Constant Rate Factor (CRF) argument
    /// </summary>
    public class Variable : Argument<int>
    {
        public ConstantRateFactorArgument(int crf) : base(crf)
        {
            if (crf < 0 || crf > 63)
            {
                throw new ArgumentException("Argument is outside range (0 - 63)", nameof(crf));
            }
        }

        public override string GetStringValue()
        {
            return $"-crf {Value} ";
        }
    }
}