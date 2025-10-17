namespace FFMpegCore.Pipes;

/// <summary>
///     Implementation of <see cref="IPipeSource" /> used for stream redirection
/// </summary>
public class StreamPipeSource : IPipeSource
{
    public StreamPipeSource(Stream source)
    {
        Source = source;
    }

    public Stream Source { get; }
    public int BlockSize { get; } = 4096;
    public string StreamFormat { get; } = string.Empty;

    public string GetStreamArguments()
    {
        return StreamFormat;
    }

    public Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        return Source.CopyToAsync(outputStream, BlockSize, cancellationToken);
    }
}
