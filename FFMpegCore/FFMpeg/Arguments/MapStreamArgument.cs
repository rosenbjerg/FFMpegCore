namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents choice of video stream
    /// </summary>
    public class MapStreamArgument : IArgument
    {
        private readonly int _inputFileIndex;
        private readonly int _streamIndex;

        public MapStreamArgument(int inputFileIndex, int streamIndex)
        {
            _inputFileIndex = inputFileIndex;
            _streamIndex = streamIndex;
        }

        public string Text => $"-map {_inputFileIndex}:{_streamIndex}";
    }
}