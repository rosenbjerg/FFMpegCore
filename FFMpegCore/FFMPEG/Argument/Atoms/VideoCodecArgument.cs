using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents video codec parameter
    /// </summary>
    public class VideoCodecArgument : Argument<string>
    {
        public int Bitrate { get; protected set; } = 0;

        public VideoCodecArgument() { }

        public VideoCodecArgument(string codec) : base(codec) { }

        public VideoCodecArgument(VideoCodec value) : base(value.ToString().ToLower()) { }

        public VideoCodecArgument(VideoCodec value, int bitrate) : base(value.ToString().ToLower())
        {
            Bitrate = bitrate;
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            var video = $"-c:v {Value} -pix_fmt yuv420p";

            if (Bitrate != default)
            {
                video += $" -b:v {Bitrate}k";
            }

            return video;
        }
    }
}
