using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents force format parameter
    /// </summary>
    public class ForceFormatArgument : Argument<VideoCodec>
    {
        public ForceFormatArgument() { }

        public ForceFormatArgument(VideoCodec value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-f {Value.ToString().ToLower()}";
        }
    }
}
