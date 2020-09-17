namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents choice of video stream, works with one input file
    /// </summary>
    public class MapStreamArgument : IArgument
    {
        public readonly int VideoStream;

        public MapStreamArgument(int videoStreamNum)
        {
            VideoStream = videoStreamNum;
        }

        public string Text => $"-map 0:{VideoStream}";
    }
}