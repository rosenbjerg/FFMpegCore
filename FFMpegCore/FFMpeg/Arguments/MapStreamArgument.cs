namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents choice of video stream, works with one input file
    /// </summary>
    public class MapStreamArgument : IArgument
    {
        private readonly int _streamIndex;

        public MapStreamArgument(int index)
        {
            _streamIndex = index;
        }

        public string Text => $"-map 0:{_streamIndex}";
    }
}