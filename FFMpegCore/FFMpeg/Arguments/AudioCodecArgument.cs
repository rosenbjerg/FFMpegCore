using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents parameter of audio codec and it's quality
    /// </summary>
    public class AudioCodecArgument : IArgument
    {
        public readonly AudioCodec AudioCodec;
        public AudioCodecArgument(AudioCodec audioCodec)
        {
            AudioCodec = audioCodec;
        }

        public string Text => $"-c:a {AudioCodec.ToString().ToLower()}";
    }
}
