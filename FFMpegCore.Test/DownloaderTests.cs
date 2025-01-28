using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    [TestMethod]
    public void GetAllLatestSuiteTest()
    {
        var binaries = FFMpegDownloader.DownloadFFMpegSuite().Result;
        Assert.IsTrue(binaries.Count == 2); // many platforms have only ffmpeg and ffprobe
    }

    [TestMethod]
    public void GetSpecificVersionTest()
    {
        var binaries = FFMpegDownloader.DownloadFFMpegSuite(FFMpegVersions.V4_0, binaries: FFMpegBinaries.FFMpeg).Result;
        Assert.IsTrue(binaries.Count == 1);
    }
}
