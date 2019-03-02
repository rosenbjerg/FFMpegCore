using FFMpegCore.FFMPEG;
using FFMpegCore.Test.Resources;
using System.IO;

namespace FFMpegCore.Test
{
    public class BaseTest
    {
        protected FFMpeg Encoder;
        protected FileInfo Input;

        public BaseTest()
        {
            Encoder = new FFMpeg();
            Input = VideoLibrary.LocalVideo;
        }
    }
}