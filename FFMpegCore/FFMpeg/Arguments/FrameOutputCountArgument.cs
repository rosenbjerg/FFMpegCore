namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents frame output count parameter
    /// </summary>
    public class FrameOutputCountArgument : IArgument
    {
        public readonly int Frames;
        public FrameOutputCountArgument(int frames)
        {
            Frames = frames;
        }

        public string Text => $"-vframes {Frames}";
    }
}
