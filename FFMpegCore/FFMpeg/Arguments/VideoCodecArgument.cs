using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents video codec parameter
/// </summary>
public class VideoCodecArgument : IArgument
{
    public readonly string Codec;

    public VideoCodecArgument(string codec)
    {
        Codec = codec;
    }

    public VideoCodecArgument(Codec value)
    {
        if (value.Type != CodecType.Video)
        {
            throw new ArgumentException($"Codec \"{value.Name}\" is not a video codec", nameof(value));
        }

        Codec = value.Name;
    }

    public string Text => $"-c:v {Codec}";
}
