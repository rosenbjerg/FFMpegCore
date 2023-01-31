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
        private readonly bool _negativeMap;

        public MapStreamArgument(int streamIndex, int inputFileIndex, Channel channel = Channel.All, bool negativeMap = false)
        {
            if (channel == Channel.Both)
            {
                // "Both" is not valid in this case and probably means all stream types
                channel = Channel.All;
            }
            _inputFileIndex = inputFileIndex;
            _streamIndex = streamIndex;
            _channel = channel;
            _negativeMap = negativeMap;
        }

        public string Text => $"-map {(_negativeMap?"-":"")}{_inputFileIndex}{_channel.StreamType()}:{_streamIndex}";
    }
}