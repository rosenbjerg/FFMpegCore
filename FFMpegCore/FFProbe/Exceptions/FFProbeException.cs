namespace FFMpegCore.Exceptions
{
    public class FFProbeException : Exception
    {
        public FFProbeException(string message, Exception? inner = null) : base(message, inner)
        {
        }
    }
}
