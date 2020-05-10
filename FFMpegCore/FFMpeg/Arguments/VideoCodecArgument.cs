using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
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

        public VideoCodecArgument(VideoCodec value) : this(value.ToString().ToLowerInvariant()) { }

        public string Text => $"-c:v {Codec} -pix_fmt yuv420p";
    }
}
