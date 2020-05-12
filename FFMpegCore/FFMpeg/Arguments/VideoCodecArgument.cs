using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

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

        public VideoCodecArgument(Codec value) 
        {
            if (value.Type != CodecType.Video)
                throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{value.Name}\" is not a video codec");

            Codec = value.Name;
        }

        public string Text => $"-c:v {Codec}";
    }
}
