using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public class StreamPipeSink : IPipeSink
    {
        public Func<Stream, CancellationToken, Task> Writer { get; }
        public int BlockSize { get; set; } = 4096;
        public string Format { get; set; } = string.Empty;

        public StreamPipeSink(Func<Stream, CancellationToken, Task> writer)
        {
            Writer = writer;
        }
        public StreamPipeSink(Stream destination)
        {
            Writer = async (inputStream, cancellationToken) => await inputStream.CopyToAsync(destination, BlockSize, cancellationToken).ConfigureAwait(false);
        }

        public Task ReadAsync(Stream inputStream, CancellationToken cancellationToken) 
            => Writer(inputStream, cancellationToken);

        public string GetFormat() => Format;
    }
}
