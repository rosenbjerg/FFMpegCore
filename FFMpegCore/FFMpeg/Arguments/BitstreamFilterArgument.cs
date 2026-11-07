using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class BitstreamFilterArgument : IArgument
{
    public readonly BitstreamFilter Filter;
    public readonly StreamType StreamType;

    public BitstreamFilterArgument(StreamType streamType, BitstreamFilter filter)
    {
        StreamType = streamType;
        Filter = filter;
    }

    public string Text => $"-bsf{StreamType.Specifier()} {Filter}";
}
