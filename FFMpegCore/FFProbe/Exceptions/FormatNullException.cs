namespace FFMpegCore.Exceptions;

public class FormatNullException : FFProbeException
{
    public FormatNullException() : base(FFMpegExceptionType.Process, "Format not specified")
    {
    }
}
