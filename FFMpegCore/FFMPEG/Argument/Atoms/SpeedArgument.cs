using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents speed parameter
    /// </summary>
    public class SpeedArgument : Argument<Speed>
    {
        public SpeedArgument()
        {            
        }

        public SpeedArgument(Speed value) : base(value) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-preset {Value.ToString().ToLower()}";
        }
    }
}
