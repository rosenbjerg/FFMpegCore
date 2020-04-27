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

        void Serialize(IInputPipe pipe);
        Task SerializeAsync(IInputPipe pipe);
    }
}
