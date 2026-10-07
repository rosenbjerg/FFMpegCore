using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class BitstreamFilterArgument : IArgument
{
    public readonly BitstreamFilter Filter;
    public readonly StreamType StreamType;

    public BitstreamFilterArgument(StreamType streamType, BitstreamFilter filter)
    {
        if (streamType is not (StreamType.Audio or StreamType.Video))
        {
            throw new ArgumentOutOfRangeException(nameof(streamType), streamType, $"{streamType} streams cannot be bitstream-filtered");
        }

        StreamType = streamType;
        Filter = filter;
    }

    public string Text => $"-bsf{StreamType.Specifier()} {Filter}";
}
