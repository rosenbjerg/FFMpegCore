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
    public int BlockSize { get; set; } = 4096;
    public string Format { get; set; } = string.Empty;

    public string GetStreamArguments()
    {
        return string.IsNullOrEmpty(Format) ? string.Empty : $"-f {Format}";
    }

    public Task WriteAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        return Source.CopyToAsync(outputStream, BlockSize, cancellationToken);
    }
}
