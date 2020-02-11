namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class CpuSpeedArgument : Argument<int>
    {
        public CpuSpeedArgument()
        {
        }

        public CpuSpeedArgument(int value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Speed(Value);
        }
    }
}
