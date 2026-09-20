namespace FFMpegCore.Exceptions;

public class FFProbeException : FFMpegException
{
    public FFProbeException(FFMpegExceptionType type, string message, Exception? inner = null, string ffProbeErrorOutput = "")
        : base(type, message, inner, ffProbeErrorOutput)
    {
    }
}
