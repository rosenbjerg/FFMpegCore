namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents shortest parameter
    /// </summary>
    public class ShortestArgument : IArgument
    {
        public readonly bool Shortest;

        public ShortestArgument(bool shortest)
        {
            Shortest = shortest;
        }

        public string Text => Shortest ? "-shortest" : string.Empty;
    }
}
