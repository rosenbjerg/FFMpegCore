using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents force format parameter
    /// </summary>
    public class ForceFormatArgument : Argument<string>
    {
        public ForceFormatArgument() { }
        public ForceFormatArgument(string format) : base(format) { }

        public ForceFormatArgument(VideoCodec value) : base(value.ToString().ToLower()) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-f {Value}";
        }
    }
}
