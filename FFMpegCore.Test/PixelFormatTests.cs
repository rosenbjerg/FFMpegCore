using Microsoft.VisualStudio.TestTools.UnitTesting;
using FFMpegCore.Models;

namespace FFMpegCore.Test
{
    [TestClass]
    public class PixelFormatTests
    {
        [TestMethod]
        public void PixelFormats_Enumerate()
        {
            var formats = FFMpegUtils.GetPixelFormats();
            Assert.IsTrue(formats.Count > 0);
        }

        [TestMethod]
        public void PixelFormats_TryGetExisting()
        {
            Assert.IsTrue(FFMpegUtils.TryGetPixelFormat("yuv420p", out _));
        }

        [TestMethod]
        public void PixelFormats_TryGetNotExisting()
        {
            Assert.IsFalse(FFMpegUtils.TryGetPixelFormat("yuv420pppUnknown", out _));
        }
        
        [TestMethod]
        public void PixelFormats_GetExisting()
        {
            var fmt = FFMpegUtils.GetPixelFormat("yuv420p");
            Assert.IsTrue(fmt.Components == 3 && fmt.BitsPerPixel == 12);
        }

        [TestMethod]
        public void PixelFormats_GetNotExisting()
        {
            Assert.ThrowsException<FFMpegException>(() => FFMpegUtils.GetPixelFormat("yuv420pppUnknown"));
        }
    }
}
