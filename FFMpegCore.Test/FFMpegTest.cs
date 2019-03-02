using FFMpegCore.FFMPEG;
using FFMpegCore.FFMPEG.Exceptions;
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

        [TestMethod]
        public void CTOR_Options()
        {
            var encoder = new FFMpeg(new FFMpegOptions { RootDirectory = ".\\FFMPEG\\bin" });
            var probe = new FFProbe(new FFMpegOptions { RootDirectory = ".\\FFMPEG\\bin" });

            Assert.IsNotNull(encoder);
            Assert.IsNotNull(probe);
        }

        [TestMethod]
        [ExpectedException(typeof(FFMpegException))]
        public void CTOR_Encoder_Options_Invalid()
        {
            var encoder = new FFMpeg(new FFMpegOptions { RootDirectory = "INVALID_DIR" });
        }

        [TestMethod]
        [ExpectedException(typeof(FFMpegException))]
        public void CTOR_Probe_Options_Invalid()
        {
            var encoder = new FFProbe(new FFMpegOptions { RootDirectory = "INVALID_DIR" });
        }
    }
}
