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

        public void WriteData(System.IO.Stream pipe)=>
            Source.CopyTo(pipe, BlockSize);

        public Task WriteDataAsync(System.IO.Stream pipe) =>
            Source.CopyToAsync(pipe, BlockSize);

        public string GetFormat()
        {
            return StreamFormat;
        }
    }
}
