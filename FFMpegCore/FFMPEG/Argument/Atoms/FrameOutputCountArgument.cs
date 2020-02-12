namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents frame output count parameter
    /// </summary>
    public class FrameOutputCountArgument : Argument<int>
    {
        public FrameOutputCountArgument()
        {
        }

        public FrameOutputCountArgument(int value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.FrameOutputCount(Value);
        }
    }
}
