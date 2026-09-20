using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class CopyArgument : IArgument
{
    public readonly StreamType StreamType;

    public CopyArgument(StreamType streamType = StreamType.All)
    {
        StreamType = streamType;
    }

    public string Text => $"-c{StreamType.Specifier()} copy";
}
