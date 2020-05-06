using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
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

        public Task ReadDataAsync(System.IO.Stream stream) =>
            stream.CopyToAsync(DestanationStream, BlockSize);

        public string GetFormat()
        {
            return Format;
        }
    }
}
