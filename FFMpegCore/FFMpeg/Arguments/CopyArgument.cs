using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
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
            Channel.Both => "-c:a copy -c:v copy",
            _ => $"-c{Channel.StreamType()} copy"
        };
    }
}
