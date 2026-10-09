using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents video codec parameter
/// </summary>
public class VideoCodecArgument : IArgument
{
    public readonly string Codec;
    public readonly int? StreamIndex;

    public VideoCodecArgument(string codec, int? streamIndex = null)
    {
        Codec = codec;
        StreamIndex = streamIndex;
    }

    public VideoCodecArgument(Codec value, int? streamIndex = null)
    {
        if (value.Type != CodecType.Video)
        {
            throw new ArgumentException($"Codec \"{value.Name}\" is not a video codec", nameof(value));
        }

        Codec = value.Name;
        StreamIndex = streamIndex;
    }

    public string Text => $"-c:v{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} {Codec}";
}
