namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents override parameter
    /// If output file should be overrided if exists
    /// </summary>
    public class OverrideArgument : Argument
    {
        public OverrideArgument()
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return "-y";
        }
    }
}
