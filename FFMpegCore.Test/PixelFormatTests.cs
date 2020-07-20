using FFMpegCore.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test
{
    [TestClass]
    public class PixelFormatTests
    {
        [TestMethod]
        public void PixelFormats_Enumerate()
        {
            var formats = FFMpeg.GetPixelFormats();
            Assert.IsTrue(formats.Count > 0);
        }

        [TestMethod]
        public void PixelFormats_TryGetExisting()
        {
            Assert.IsTrue(FFMpeg.TryGetPixelFormat("yuv420p", out _));
        }

        [TestMethod]
        public void PixelFormats_TryGetNotExisting()
        {
            Assert.IsFalse(FFMpeg.TryGetPixelFormat("yuv420pppUnknown", out _));
        }
        
        [TestMethod]
        public void PixelFormats_GetExisting()
        {
            var fmt = FFMpeg.GetPixelFormat("yuv420p");
            Assert.IsTrue(fmt.Components == 3 && fmt.BitsPerPixel == 12);
        }

        [TestMethod]
        public void PixelFormats_GetNotExisting()
        {
            Assert.ThrowsException<FFMpegException>(() => FFMpeg.GetPixelFormat("yuv420pppUnknown"));
        }
    }
}
