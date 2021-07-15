namespace FFMpegCore.Exceptions
{
    public class FormatNullException : FFProbeException
    {
        public FormatNullException() : base("Format not specified")
        {
        }
    }
}