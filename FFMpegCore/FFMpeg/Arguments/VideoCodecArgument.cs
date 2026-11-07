using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents video codec parameter
/// </summary>
public class VideoCodecArgument : IArgument
{
    public readonly string Codec;
    public readonly int? StreamIndex;

    public VideoCodecArgument(Codec value, int? streamIndex = null)
    {
        if (value.Type is not (CodecType.Video or CodecType.Unknown))
        {
            throw new ArgumentException($"Codec \"{value.Name}\" is not a video codec", nameof(value));
        }

        Codec = value.Name;
        StreamIndex = streamIndex;
    }

    public string Text => $"-c:v{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} {Codec}";
}
