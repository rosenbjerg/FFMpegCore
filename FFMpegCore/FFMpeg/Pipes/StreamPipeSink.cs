using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public class StreamPipeSink : IPipeSink
    {
        public System.IO.Stream Destination { get; }
        public int BlockSize { get; set; } = 4096;
        public string Format { get; set; } = string.Empty;

        public StreamPipeSink(System.IO.Stream destination)
        {
            Destination = destination;
        }

        public Task CopyAsync(System.IO.Stream inputStream, CancellationToken cancellationToken) =>
            inputStream.CopyToAsync(Destination, BlockSize, cancellationToken);

        public string GetFormat() => Format;
    }
}
