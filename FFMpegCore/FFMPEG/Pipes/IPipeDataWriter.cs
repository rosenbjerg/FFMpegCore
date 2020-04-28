using FFMpegCore.FFMPEG.Argument;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    /// <summary>
    /// Interface for ffmpeg pipe source data IO
    /// </summary>
    public interface IPipeDataWriter
    {
        string GetFormat();
        void WriteData(System.IO.Stream pipe);
        Task WriteDataAsync(System.IO.Stream pipe);
    }
}
