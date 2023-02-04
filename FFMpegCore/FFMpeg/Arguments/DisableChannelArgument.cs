using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents cpu speed parameter
    /// </summary>
    public class DisableChannelArgument : IArgument
    {
        public readonly Channel Channel;

        public DisableChannelArgument(Channel channel)
        {
            if (channel == Channel.Both)
            {
                throw new FFMpegException(FFMpegExceptionType.Conversion, "Cannot disable both channels");
            }

            Channel = channel;
        }

        public string Text => Channel switch
        {
            Channel.Video => "-vn",
            Channel.Audio => "-an",
            _ => string.Empty
        };
    }
}
