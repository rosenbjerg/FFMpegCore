using System.Runtime.InteropServices;
using FFMpegCore.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    [TestMethod]
    public void GetLatestSuiteTest()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fileNames = FFMpegDownloader.DownloadFFMpegSuite();
            Assert.IsTrue(fileNames.Count == 3);
        }
        else
        {
            Assert.Inconclusive("This test is only for Windows");
        }
    }

    [TestMethod]
    public void GetLatestFFMpegTest()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fileNames = FFMpegDownloader.DownloadFFMpeg();
            Assert.IsTrue(fileNames.Count == 1);
        }
        else
        {
            Assert.Inconclusive("This test is only for Windows");
        }
    }

    [TestMethod]
    public void GetLatestFFProbeTest()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fileNames = FFMpegDownloader.DownloadFFProbe();
            Assert.IsTrue(fileNames.Count == 1);
        }
        else
        {
            Assert.Inconclusive("This test is only for Windows");
        }
    }

    [TestMethod]
    public void GetLatestFFPlayTest()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var fileNames = FFMpegDownloader.DownloadFFPlay();
            Assert.IsTrue(fileNames.Count == 1);
        }
        else
        {
            Assert.Inconclusive("This test is only for Windows");
        }
    }
}
