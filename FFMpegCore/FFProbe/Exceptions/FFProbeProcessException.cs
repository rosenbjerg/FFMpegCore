namespace FFMpegCore.Exceptions;

public class FFProbeProcessException : FFProbeException
{
    public FFProbeProcessException(string message, IReadOnlyCollection<string> processErrors, Exception? inner = null)
        : base(FFMpegExceptionType.Process, message, inner, string.Join("\n", processErrors))
    {
        ProcessErrors = processErrors;
    }

    public IReadOnlyCollection<string> ProcessErrors { get; }
}
