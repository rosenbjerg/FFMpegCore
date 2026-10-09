using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class DisableStreamArgument : IArgument
{
    public readonly StreamType StreamType;

    public DisableStreamArgument(StreamType streamType)
    {
        StreamType = streamType;
        Text = streamType switch
        {
            StreamType.Video => "-vn",
            StreamType.Audio => "-an",
            StreamType.Subtitle => "-sn",
            StreamType.Data => "-dn",
            _ => throw new ArgumentOutOfRangeException(nameof(streamType), streamType, $"{streamType} streams cannot be disabled")
        };
    }

    public string Text { get; }
}
