namespace FFMpegCore.Exceptions;

public class FFMpegException : Exception
{
    public FFMpegException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
