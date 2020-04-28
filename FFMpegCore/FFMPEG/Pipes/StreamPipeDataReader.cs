using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    public class StreamPipedataReader : IPipeDataReader
    {
        public System.IO.Stream DestanationStream { get; private set; }
        public int BlockSize { get; set; } = 4096;

        public StreamPipedataReader(System.IO. Stream destanationStream)
        {
            DestanationStream = destanationStream;
        }

        public void ReadData(System.IO.Stream stream)
        {
            int read;
            var buffer = new byte[BlockSize];
            while((read = stream.Read(buffer, 0, buffer.Length)) != 0)
                DestanationStream.Write(buffer, 0, buffer.Length);
        }

        public async Task ReadDataAsync(System.IO.Stream stream)
        {
            int read;
            var buffer = new byte[BlockSize];
            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                await DestanationStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
