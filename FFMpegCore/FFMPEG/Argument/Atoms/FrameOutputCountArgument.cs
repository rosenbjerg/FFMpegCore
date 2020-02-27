namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents frame output count parameter
    /// </summary>
    public class FrameOutputCountArgument : Argument<int>
    {
        public FrameOutputCountArgument() { }

        public FrameOutputCountArgument(int value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-vframes {Value}";
        }
    }
}
