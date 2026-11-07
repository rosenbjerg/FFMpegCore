using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class MetadataTagArgument : IArgument
{
    public readonly string Key;
    public readonly int? StreamIndex;
    public readonly StreamType? StreamType;
    public readonly string Value;

    public MetadataTagArgument(string key, string value, StreamType? streamType = null, int? streamIndex = null)
    {
        Key = key;
        Value = value;
        StreamType = streamType;
        StreamIndex = streamIndex;
    }

    public string Text => $"-metadata{Specifier} \"{Key}={Value.Replace("\"", "\\\"")}\"";

    private string Specifier => StreamType == null
        ? string.Empty
        : $":s{StreamType.Value.Specifier()}{(StreamIndex == null ? "" : $":{StreamIndex}")}";
}
