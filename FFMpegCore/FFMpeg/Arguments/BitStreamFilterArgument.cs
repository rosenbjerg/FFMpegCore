using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents parameter of bitstream filter
    /// </summary>
    public class BitStreamFilterArgument : IArgument
    {
        public readonly Channel Channel;
        public readonly Filter Filter;

        public BitStreamFilterArgument(Channel channel, Filter filter)
        {
            Channel = channel;
            Filter = filter;
        }

        public string Text => Channel switch
        {
            Channel.Audio => $"-bsf:a {Filter.ToString().ToLower()}",
            Channel.Video => $"-bsf:v {Filter.ToString().ToLower()}",
            _ => string.Empty
        };
    }
}
