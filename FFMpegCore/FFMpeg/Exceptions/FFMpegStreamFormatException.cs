namespace FFMpegCore.Exceptions;

public class FFMpegStreamFormatException : FFMpegException
{
    public FFMpegStreamFormatException(FFMpegExceptionType type, string message, Exception? innerException = null)
        : base(type, message, innerException)
    {
    }
}
