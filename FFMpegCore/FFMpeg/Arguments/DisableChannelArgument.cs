using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents channel disabling parameter
///     Used to disable processing of all streams of a particular type so that they are not filtered, automatically selected or mapped for any input or output
/// </summary>
public class DisableChannelArgument : IArgument
{
    public readonly Channel Channel;

    public DisableChannelArgument(Channel channel)
    {
        if (channel == Channel.All)
        {
            throw new FFMpegException(FFMpegExceptionType.Conversion, "Cannot disable all channels");
        }

        if (channel == Channel.Both)
        {
            throw new FFMpegException(FFMpegExceptionType.Conversion, "Cannot disable both video and audio channels at once");
        }

        if (channel == Channel.Attachments)
        {
            // ffmpeg does not support disabling attachment streams
            throw new FFMpegException(FFMpegExceptionType.Operation, $"{nameof(Channel.Attachments)} channel cannot be disabled");
        }

        if (channel == Channel.VideoNoAttachedPic)
        {
            // ffmpeg does not support disabling no-picture-filtered video steams
            throw new FFMpegException(FFMpegExceptionType.Operation, $"{nameof(Channel.VideoNoAttachedPic)} channel cannot be disabled");
        }

        Channel = channel;
    }

    public string Text => Channel switch
    {
        Channel.Video => "-vn",
        Channel.Audio => "-an",
        Channel.Subtitle => "-sn",
        Channel.Data => "-dn",
        _ => string.Empty
    };
}
