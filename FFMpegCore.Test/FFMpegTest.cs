using FFMpegCore.FFMPEG;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test
{
    [TestClass]
    public class FFMpegTest
    {
        [TestMethod]
        public void CTOR_Default()
        {
            var encoder = new FFMpeg();
            var probe = new FFProbe();

            Assert.IsNotNull(encoder);
            Assert.IsNotNull(probe);
        }
    }
}
