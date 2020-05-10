using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Transpose argument.
    /// 0 = 90CounterCLockwise and Vertical Flip (default)
    /// 1 = 90Clockwise
    /// 2 = 90CounterClockwise
    /// 3 = 90Clockwise and Vertical Flip
    /// </summary>
    public class TransposeArgument : IArgument
    {
        public readonly Transposition Transposition;
        public TransposeArgument(Transposition transposition)
        {
            Transposition = transposition;
        }

        public string Text => $"-vf \"transpose={(int)Transposition}\"";
    }
}