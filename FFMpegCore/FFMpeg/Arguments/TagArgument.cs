using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class TagArgument : IArgument
{
    public readonly int? StreamIndex;
    public readonly StreamType StreamType;
    public readonly string Tag;

    public TagArgument(string tag, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        Tag = tag;
        StreamType = streamType;
        StreamIndex = streamIndex;
    }

    public string Text => $"-tag{StreamType.Specifier()}{(StreamIndex == null ? "" : $":{StreamIndex}")} {Tag}";
}
