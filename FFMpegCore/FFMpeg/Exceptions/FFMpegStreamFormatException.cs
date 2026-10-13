namespace FFMpegCore.Exceptions;

public class FFMpegStreamFormatException : FFMpegException
{
    public FFMpegStreamFormatException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
