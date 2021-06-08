using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeSource"/> used for stream redirection
    /// </summary>
    public class StreamPipeSource : IPipeSource
    {
        public Stream Source { get; }
        public int BlockSize { get; } = 4096;
        public string StreamFormat { get; } = string.Empty;

        public StreamPipeSource(Stream source)
        {
            Source = source;
        }

        public string GetStreamArguments() => StreamFormat;

        public Task WriteAsync(Stream outputStream, CancellationToken cancellationToken) => Source.CopyToAsync(outputStream, BlockSize, cancellationToken);
    }
}
