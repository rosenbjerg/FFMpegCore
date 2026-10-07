using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents the subtitle codec parameter
/// </summary>
public class SubtitleCodecArgument : IArgument
{
    public readonly string SubtitleCodec;

    public SubtitleCodecArgument(Codec subtitleCodec)
    {
        if (subtitleCodec.Type != CodecType.Subtitle)
        {
            throw new ArgumentException($"Codec \"{subtitleCodec.Name}\" is not a subtitle codec", nameof(subtitleCodec));
        }

        SubtitleCodec = subtitleCodec.Name;
    }

    public SubtitleCodecArgument(string subtitleCodec)
    {
        SubtitleCodec = subtitleCodec;
    }

    public string Text => $"-c:s {SubtitleCodec.ToLowerInvariant()}";
}
