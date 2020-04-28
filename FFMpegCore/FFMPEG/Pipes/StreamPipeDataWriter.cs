using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    /// <summary>
    /// Implementation of <see cref="IPipeDataWriter"/> used for stream redirection
    /// </summary>
    public class StreamPipeDataWriter : IPipeDataWriter
    {
        public System.IO.Stream Source { get; private set; }
        public int BlockSize { get; set; } = 4096;
        public string StreamFormat { get; set; } = string.Empty;

        public StreamPipeDataWriter(System.IO.Stream stream)
        {
            Source = stream;
        }

        public void WriteData(System.IO.Stream pipe)
        {
            var buffer = new byte[BlockSize];
            int read;
            while ((read = Source.Read(buffer, 0, buffer.Length)) != 0)
            {
                pipe.Write(buffer, 0, read);
            }
        }

        public async Task WriteDataAsync(System.IO.Stream pipe)
        {
            var buffer = new byte[BlockSize];
            int read;
            while ((read = await Source.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await pipe.WriteAsync(buffer, 0, read);
            }
        }

        public string GetFormat()
        {
            return StreamFormat;
        }
    }
}
