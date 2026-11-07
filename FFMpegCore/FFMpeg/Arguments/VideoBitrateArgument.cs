namespace FFMpegCore.Arguments;

/// <summary>
///     Represents video bitrate parameter
/// </summary>
public class VideoBitrateArgument : IArgument
{
    public readonly int Bitrate;
    public readonly int? StreamIndex;

    public VideoBitrateArgument(int kilobitsPerSecond, int? streamIndex = null)
    {
        Bitrate = kilobitsPerSecond;
        StreamIndex = streamIndex;
    }

    public string Text => $"-b:v{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} {Bitrate}k";
}
