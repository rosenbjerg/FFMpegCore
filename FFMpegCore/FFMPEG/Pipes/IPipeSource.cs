using FFMpegCore.FFMPEG.Argument;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    public interface IPipeSource
    {
        string GetFormat();
        void FlushData(IInputPipe pipe);
        Task FlushDataAsync(IInputPipe pipe);
    }
}
