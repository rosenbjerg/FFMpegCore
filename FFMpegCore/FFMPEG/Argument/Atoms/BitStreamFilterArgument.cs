using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of bitstream filter
    /// </summary>
    public class BitStreamFilterArgument : Argument<Channel, Filter>
    {
        public BitStreamFilterArgument()
        {
        }

        public BitStreamFilterArgument(Channel first, Filter second) : base(first, second)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.BitStreamFilter(First, Second);
        }
    }
}
