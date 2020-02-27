namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents override parameter
    /// If output file should be overrided if exists
    /// </summary>
    public class OverrideArgument : Argument
    {
        public OverrideArgument() { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return "-y";
        }
    }
}
