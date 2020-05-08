using FFMpegCore.FFMPEG.Enums;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents parameter of copy parameter
    /// Defines if channel (audio, video or both) should be copied to output file
    /// </summary>
    public class CopyArgument : IArgument
    {
        public readonly Channel Channel;
        public CopyArgument(Channel channel = Channel.Both)
        {
            Channel = channel;
        }

        public string Text => Channel switch
        {
            Channel.Audio => "-c:a copy",
            Channel.Video => "-c:v copy",
            _ => "-c copy"
        };
    }
}
