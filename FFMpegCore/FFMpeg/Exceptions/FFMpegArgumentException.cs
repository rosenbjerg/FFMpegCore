namespace FFMpegCore.Exceptions;

public class FFMpegArgumentException : Exception
{
    public FFMpegArgumentException(string? message = null, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
