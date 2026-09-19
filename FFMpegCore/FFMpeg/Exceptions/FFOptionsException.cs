namespace FFMpegCore.Exceptions;

public class FFOptionsException : Exception
{
    public FFOptionsException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
