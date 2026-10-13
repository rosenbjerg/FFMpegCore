namespace FFMpegCore.Exceptions;

public class FFProbeException : FFMpegException
{
    public FFProbeException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
