using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents choice of stream by the stream specifier 
    /// </summary>
    public class MapStreamArgument : IArgument
    {
        private readonly int _inputFileIndex;
        private readonly int _streamIndex;
        private readonly Channel _channel;

        public MapStreamArgument(int streamIndex, int inputFileIndex, Channel channel = Channel.All)
        {
            if (channel == Channel.Both)
            {
                // "Both" is not valid in this case and probably means all stream types
                channel = Channel.All;
            }
            _inputFileIndex = inputFileIndex;
            _streamIndex = streamIndex;
            _channel = channel;
        }

        public string Text => $"-map {_inputFileIndex}{_channel.StreamType()}:{_streamIndex}";
    }
}