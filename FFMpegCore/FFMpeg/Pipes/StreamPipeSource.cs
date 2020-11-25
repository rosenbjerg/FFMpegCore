using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeSource"/> used for stream redirection
    /// </summary>
    public class StreamPipeSource : IPipeSource
    {
        public System.IO.Stream Source { get; }
        public int BlockSize { get; } = 4096;
        public string StreamFormat { get; } = string.Empty;

        public StreamPipeSource(System.IO.Stream source)
        {
            Source = source;
        }

        public Task WriteAsync(System.IO.Stream outputStream, CancellationToken cancellationToken) => Source.CopyToAsync(outputStream, BlockSize, cancellationToken);

        public string GetFormat() => StreamFormat;
    }
}
