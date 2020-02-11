using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of audio codec and it's quality
    /// </summary>
    public class AudioCodecArgument : Argument<AudioCodec>
    {
        /// <summary>
        /// Bitrate of audio channel
        /// </summary>
        public int Bitrate { get; protected set; } = (int)AudioQuality.Normal;

        public AudioCodecArgument()
        {
        }

        public AudioCodecArgument(AudioCodec value) : base(value)
        {
        }

        public AudioCodecArgument(AudioCodec value, AudioQuality bitrate) : base(value)
        {
            Bitrate = (int)bitrate;
        }

        public AudioCodecArgument(AudioCodec value, int bitrate) : base(value)
        {
            Bitrate = bitrate;
        }

        /// <summary>
        /// String representation of the argument
        /// </summary>
        /// <returns>String representation of the argument</returns>
        public override string GetStringValue()
        {
            return ArgumentStringifier.Audio(Value, Bitrate);
        }
    }
}
