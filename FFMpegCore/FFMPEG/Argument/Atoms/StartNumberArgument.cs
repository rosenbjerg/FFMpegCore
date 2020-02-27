namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents start number parameter
    /// </summary>
    public class StartNumberArgument : Argument<int>
    {
        public StartNumberArgument() { }

        public StartNumberArgument(int value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-start_number {Value}";
        }
    }
}
