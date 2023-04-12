using System.Runtime.InteropServices;
using FFMpegCore.Helpers;

namespace FFMpegCore.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

public class DownloaderTests
{
    [TestClass]
    public class FFMpegDownloaderTest
    {
        [TestMethod]
        public void GetLatestVersionTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var files = FFMpegDownloader.GetLatestVersion();
                Assert.IsTrue(files.Count == 3);
            }
            else
            {
                Assert.Inconclusive("This test is only for Windows");
            }
            
        }
    }
}
