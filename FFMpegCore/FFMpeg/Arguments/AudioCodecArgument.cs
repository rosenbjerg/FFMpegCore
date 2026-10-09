using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents parameter of audio codec and it's quality
/// </summary>
public class AudioCodecArgument : IArgument
{
    public readonly string AudioCodec;
    public readonly int? StreamIndex;

    public AudioCodecArgument(Codec audioCodec, int? streamIndex = null)
    {
        if (audioCodec.Type != CodecType.Audio)
        {
            throw new ArgumentException($"Codec \"{audioCodec.Name}\" is not an audio codec", nameof(audioCodec));
        }

        AudioCodec = audioCodec.Name;
        StreamIndex = streamIndex;
    }

    public AudioCodecArgument(string audioCodec, int? streamIndex = null)
    {
        AudioCodec = audioCodec;
        StreamIndex = streamIndex;
    }

    public string Text => $"-c:a{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} {AudioCodec.ToLowerInvariant()}";
}
