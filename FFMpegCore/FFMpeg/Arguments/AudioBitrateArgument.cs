using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of audio codec and it's quality
    /// </summary>
    public class AudioBitrateArgument : IArgument
    {
        public readonly int Bitrate;
        public AudioBitrateArgument(AudioQuality value) : this((int)value) { }
        public AudioBitrateArgument(int bitrate)
        {
            Bitrate = bitrate;
        }


        public string Text => $"-b:a {Bitrate}k";
    }
}