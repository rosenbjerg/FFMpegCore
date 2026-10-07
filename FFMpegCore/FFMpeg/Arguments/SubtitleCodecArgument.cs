using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents the subtitle codec parameter
/// </summary>
public class SubtitleCodecArgument : IArgument
{
    public readonly string SubtitleCodec;
    public readonly int? StreamIndex;

    public SubtitleCodecArgument(Codec subtitleCodec, int? streamIndex = null)
    {
        if (subtitleCodec.Type != CodecType.Subtitle)
        {
            throw new ArgumentException($"Codec \"{subtitleCodec.Name}\" is not a subtitle codec", nameof(subtitleCodec));
        }

        SubtitleCodec = subtitleCodec.Name;
        StreamIndex = streamIndex;
    }

    public SubtitleCodecArgument(string subtitleCodec, int? streamIndex = null)
    {
        SubtitleCodec = subtitleCodec;
        StreamIndex = streamIndex;
    }

    public string Text => $"-c:s{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} {SubtitleCodec.ToLowerInvariant()}";
}
