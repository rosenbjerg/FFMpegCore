using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents video codec parameter
    /// </summary>
    public class VideoCodecArgument : Argument<VideoCodec>
    {
        public int Bitrate { get; protected set; } = 0;

        public VideoCodecArgument()
        {
        }

        public VideoCodecArgument(VideoCodec value) : base(value)
        {
        }

        public VideoCodecArgument(VideoCodec value, int bitrate) : base(value)
        {
            Bitrate = bitrate;
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Video(Value, Bitrate);
        }
    }
}
