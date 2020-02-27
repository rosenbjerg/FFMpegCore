namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents frame rate parameter
    /// </summary>
    public class FrameRateArgument : Argument<double>
    {
        public FrameRateArgument() { }

        public FrameRateArgument(double value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-r {Value}";
        }
    }
}
