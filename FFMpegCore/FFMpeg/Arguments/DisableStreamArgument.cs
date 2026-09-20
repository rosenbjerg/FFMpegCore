using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

public class DisableStreamArgument : IArgument
{
    public readonly StreamType StreamType;

    public DisableStreamArgument(StreamType streamType)
    {
        if (streamType is StreamType.All or StreamType.Attachments or StreamType.VideoNoAttachedPic)
        {
            throw new FFMpegException(FFMpegExceptionType.Operation, $"{streamType} streams cannot be disabled");
        }

        StreamType = streamType;
    }

    public string Text => StreamType switch
    {
        StreamType.Video => "-vn",
        StreamType.Audio => "-an",
        StreamType.Subtitle => "-sn",
        StreamType.Data => "-dn",
        _ => string.Empty
    };
}
