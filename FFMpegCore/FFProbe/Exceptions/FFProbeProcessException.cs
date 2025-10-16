namespace FFMpegCore.Exceptions;

public class FFProbeProcessException : FFProbeException
{
    public FFProbeProcessException(string message, IReadOnlyCollection<string> processErrors, Exception? inner = null) : base(message, inner)
    {
        ProcessErrors = processErrors;
    }

    public IReadOnlyCollection<string> ProcessErrors { get; }
}
