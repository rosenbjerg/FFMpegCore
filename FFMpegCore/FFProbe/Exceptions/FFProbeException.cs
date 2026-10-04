namespace FFMpegCore.Exceptions;

public class FFProbeException : FFMpegException
{
    public FFProbeException(FFMpegExceptionType type, string message, Exception? inner = null)
        : base(type, message, inner)
    {
    }
}
