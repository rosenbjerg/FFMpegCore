using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    public interface IPipeDataReader
    {
        void ReadData(System.IO.Stream stream);
        Task ReadDataAsync(System.IO.Stream stream);
        string GetFormat();
    }
}
