namespace FFMpegCore.Exceptions;

public enum FFMpegExceptionType
{
    Conversion,
    File,
    Operation,
    Process
}

public class FFMpegException : Exception
{
    public FFMpegException(FFMpegExceptionType type, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Type = type;
    }

    public FFMpegExceptionType Type { get; }
}
