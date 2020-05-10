using System.Threading;
using System.Threading.Tasks;

namespace FFMpegCore.Pipes
{
    public class StreamPipeDataReader : IPipeDataReader
    {
        public System.IO.Stream DestanationStream { get; private set; }
        public int BlockSize { get; set; } = 4096;
        public string Format { get; set; } = string.Empty;

        public StreamPipeDataReader(System.IO.Stream destanationStream)
        {
            DestanationStream = destanationStream;
        }

        public void ReadData(System.IO.Stream stream) =>
            stream.CopyTo(DestanationStream, BlockSize);

        public Task ReadDataAsync(System.IO.Stream stream, CancellationToken token) =>
            stream.CopyToAsync(DestanationStream, BlockSize, token);

        public string GetFormat()
        {
            return Format;
        }
    }
}
