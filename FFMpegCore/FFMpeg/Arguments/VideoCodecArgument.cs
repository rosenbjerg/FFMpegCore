using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents video codec parameter
    /// </summary>
    public class VideoCodecArgument : IArgument
    {
        public readonly string Codec;

        public VideoCodecArgument(string codec)
        {
            Codec = codec;
        }

        public VideoCodecArgument(VideoCodec value) : this(value.ToString().ToLower()) { }

        public string Text => $"-c:v {Codec} -pix_fmt yuv420p";
    }
}
