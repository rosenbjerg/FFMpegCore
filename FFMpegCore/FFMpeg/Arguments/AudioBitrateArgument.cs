using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class AudioBitrateArgument : IArgument
{
    public readonly int Bitrate;
    public readonly int? StreamIndex;

    public AudioBitrateArgument(AudioQuality value, int? streamIndex = null) : this((int)value, streamIndex) { }

    public AudioBitrateArgument(int kilobitsPerSecond, int? streamIndex = null)
    {
        Bitrate = kilobitsPerSecond;
        StreamIndex = streamIndex;
    }

    public string Text => $"-b:a{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} {Bitrate}k";
}
