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

        public SpeedArgument(Speed value) : base(value)
        {
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Speed(Value);
        }
    }
}
