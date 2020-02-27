using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents video codec parameter
    /// </summary>
    public class VideoCodecArgument : Argument<VideoCodec>
    {
        public int Bitrate { get; protected set; } = 0;

        public VideoCodecArgument() { }

        public VideoCodecArgument(VideoCodec value) : base(value) { }

        public VideoCodecArgument(VideoCodec value, int bitrate) : base(value)
        {
            Bitrate = bitrate;
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            var video = $"-c:v {Value.ToString().ToLower()} -pix_fmt yuv420p";

            if (Bitrate != default)
            {
                video += $" -b:v {Bitrate}k";
            }

            return video;
        }
    }
}
