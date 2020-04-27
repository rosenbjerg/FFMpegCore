using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegCore.FFMPEG.Pipes
{
    public interface IVideoFrame
    {
        int Width { get; }
        int Height { get; }
        string Format { get; }

        void Serialize(System.IO.Stream pipe);
        Task SerializeAsync(System.IO.Stream pipe);
    }
}
