using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of audio codec and it's quality
    /// </summary>
    public class AudioBitrateArgument : Argument<int>
    {
        public AudioBitrateArgument(AudioQuality value) : base((int)value) { }
        public AudioBitrateArgument(int bitrate) : base(bitrate) { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-b:a {Value}k";
        }
    }
}