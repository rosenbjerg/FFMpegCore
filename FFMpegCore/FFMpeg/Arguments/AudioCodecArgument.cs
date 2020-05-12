using FFMpegCore.Enums;
using FFMpegCore.Models;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents parameter of audio codec and it's quality
    /// </summary>
    public class AudioCodecArgument : IArgument
    {
        public readonly string AudioCodec;

        public AudioCodecArgument(Codec audioCodec)
        {
            if (audioCodec.Type != CodecType.Audio)
                throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{audioCodec.Name}\" is not an audio codec");

            AudioCodec = audioCodec.Name;
        }

        public AudioCodecArgument(string audioCodec)
        {
            AudioCodec = audioCodec;
        }

        public string Text => $"-c:a {AudioCodec.ToString().ToLowerInvariant()}";
    }
}
