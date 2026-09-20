namespace FFMpegCore;

public sealed class FFMpegResult
{
    internal FFMpegResult(int exitCode, IReadOnlyList<string> errorOutput, bool cancelled)
    {
        ExitCode = exitCode;
        ErrorOutput = errorOutput;
        Cancelled = cancelled;
    }

    public int ExitCode { get; }
    public IReadOnlyList<string> ErrorOutput { get; }
    public bool Cancelled { get; }
    public bool Success => ExitCode == 0 && !Cancelled;
}
