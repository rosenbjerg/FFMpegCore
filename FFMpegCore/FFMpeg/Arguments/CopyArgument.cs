using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class CopyArgument : IArgument
{
    public readonly StreamType StreamType;
    public readonly int? StreamIndex;

    public CopyArgument(StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        StreamType = streamType;
        StreamIndex = streamIndex;
    }

    public string Text => $"-c{StreamType.Specifier()}{(StreamIndex == null ? string.Empty : $":{StreamIndex}")} copy";
}
