namespace FFMpegCore.Exceptions;

public class FFProbeProcessException : FFProbeException
{
    public FFProbeProcessException(string message, IReadOnlyCollection<string> errorOutput, Exception? inner = null)
        : base(FFMpegExceptionType.Process, message, inner, string.Join("\n", errorOutput))
    {
        ErrorOutput = errorOutput;
    }

    public IReadOnlyCollection<string> ErrorOutput { get; }
}
