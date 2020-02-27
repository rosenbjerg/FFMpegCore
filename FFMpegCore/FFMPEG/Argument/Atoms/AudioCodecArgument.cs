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
        public int Bitrate { get; } = (int)AudioQuality.Normal;

        public AudioCodecArgument() { }

        public AudioCodecArgument(AudioCodec value) : base(value) { }

        public AudioCodecArgument(AudioCodec value, AudioQuality bitrate) : this(value, (int) bitrate) { }

        public AudioCodecArgument(AudioCodec value, int bitrate) : base(value)
        {
            Bitrate = bitrate;
        }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-c:a {Value.ToString().ToLower()} -b:a {Bitrate}k";
        }
    }
}
