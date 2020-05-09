using FFMpegCore.Test.Resources;
using System.IO;

namespace FFMpegCore.Test
{
    public class BaseTest
    {
        protected FileInfo Input;

        public BaseTest()
        {
            Input = VideoLibrary.LocalVideo;
        }
    }
}