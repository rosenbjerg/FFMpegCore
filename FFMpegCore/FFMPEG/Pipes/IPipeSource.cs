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
    public interface IPipeSource
    {
        string GetFormat();
        void FlushData(System.IO.Stream pipe);
        Task FlushDataAsync(System.IO.Stream pipe);
    }
}
