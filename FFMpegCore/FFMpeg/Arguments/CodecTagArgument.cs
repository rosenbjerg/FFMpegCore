using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class CodecTagArgument : IArgument
{
    public readonly int? StreamIndex;
    public readonly StreamType StreamType;
    public readonly string CodecTag;

    public CodecTagArgument(string codecTag, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        CodecTag = codecTag;
        StreamType = streamType;
        StreamIndex = streamIndex;
    }

    public string Text => $"-tag{StreamType.Specifier()}{(StreamIndex == null ? "" : $":{StreamIndex}")} {CodecTag}";
}
