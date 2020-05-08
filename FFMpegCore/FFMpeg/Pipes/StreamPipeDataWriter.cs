using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeDataWriter"/> used for stream redirection
    /// </summary>
    public class StreamPipeDataWriter : IPipeDataWriter
    {
        public System.IO.Stream Source { get; }
        public int BlockSize { get; } = 4096;
        public string StreamFormat { get; } = string.Empty;

        public StreamPipeDataWriter(System.IO.Stream stream)
        {
            Source = stream;
        }

        public void WriteData(System.IO.Stream pipe) => Source.CopyTo(pipe, BlockSize);

        public Task WriteDataAsync(System.IO.Stream pipe) => Source.CopyToAsync(pipe, BlockSize);

        public string GetFormat() => StreamFormat;
    }
}
