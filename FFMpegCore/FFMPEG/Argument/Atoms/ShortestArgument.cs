namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents shortest parameter
    /// </summary>
    public class ShortestArgument : Argument<bool>
    {
        public ShortestArgument() { }

        public ShortestArgument(bool value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return Value ? "-shortest" : string.Empty;
        }
    }
}
