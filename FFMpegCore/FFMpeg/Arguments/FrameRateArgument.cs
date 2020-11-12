namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents frame rate parameter
    /// </summary>
    public class FrameRateArgument : IArgument
    {
        public readonly double Framerate;

        public FrameRateArgument(double framerate)
        {
            Framerate = framerate;
        }

        public string Text => $"-r {Framerate.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
