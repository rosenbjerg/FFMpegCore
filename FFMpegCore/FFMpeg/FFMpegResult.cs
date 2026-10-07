namespace FFMpegCore;

public sealed class FFMpegResult
{
    internal FFMpegResult(int exitCode, IReadOnlyList<string> standardError, bool cancelled)
    {
        ExitCode = exitCode;
        StandardError = standardError;
        Cancelled = cancelled;
    }

    public int ExitCode { get; }
    public IReadOnlyList<string> StandardError { get; }
    public bool Cancelled { get; }
    public bool Success => ExitCode == 0 && !Cancelled;
}
