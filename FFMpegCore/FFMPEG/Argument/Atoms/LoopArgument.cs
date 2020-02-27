namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents loop parameter
    /// </summary>
    public class LoopArgument : Argument<int>
    {
        public LoopArgument() { }

        public LoopArgument(int value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-loop {Value}";
        }
    }
}
