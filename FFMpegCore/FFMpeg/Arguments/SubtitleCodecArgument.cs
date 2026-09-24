using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

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
            throw new FFMpegException(FFMpegExceptionType.Operation, $"Codec \"{subtitleCodec.Name}\" is not a subtitle codec");
        }

        SubtitleCodec = subtitleCodec.Name;
    }

    public SubtitleCodecArgument(string subtitleCodec)
    {
        SubtitleCodec = subtitleCodec;
    }

    public string Text => $"-c:s {SubtitleCodec.ToLowerInvariant()}";
}
