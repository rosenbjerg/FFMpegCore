using System.Configuration;
using System.IO;
using FFMpegCore.FFMPEG;
using FFMpegCore.Tests.Resources;

namespace FFMpegCore.Tests
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