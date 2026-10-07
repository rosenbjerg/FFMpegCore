namespace FFMpegCore.Exceptions;

public class FFProbeProcessException : FFProbeException
{
    public FFProbeProcessException(int exitCode, IReadOnlyList<string> standardError, Exception? inner = null)
        : base(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({exitCode} - {string.Join("\n", standardError)})", inner)
    {
        ExitCode = exitCode;
        StandardError = standardError;
    }

    public int ExitCode { get; }
    public IReadOnlyList<string> StandardError { get; }
}
