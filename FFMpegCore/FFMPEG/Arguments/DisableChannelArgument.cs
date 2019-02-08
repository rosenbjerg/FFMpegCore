using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Arguments
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class DisableChannelArgument : Argument<Channel>
    {
        public DisableChannelArgument()
        {
        }

        public DisableChannelArgument(Channel value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentsStringifier.Disable(Value);
        }
    }
}
