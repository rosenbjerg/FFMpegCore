namespace FFMpegCore.Exceptions;

public class FFProbeProcessException : FFProbeException
{
    public FFProbeProcessException(int exitCode, IReadOnlyList<string> errorOutput, Exception? inner = null)
        : base(FFMpegExceptionType.Process, $"ffprobe exited with non-zero exit-code ({exitCode} - {string.Join("\n", errorOutput)})", inner)
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
    }

    public int ExitCode { get; }
    public IReadOnlyList<string> ErrorOutput { get; }
}
