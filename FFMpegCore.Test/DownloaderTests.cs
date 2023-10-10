using FFMpegCore.Extensions.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    [TestMethod]
    public void GetAllLatestSuiteTest()
    {
        var binaries = FFMpegDownloader.DownloadFFMpegSuite(binaries: FFMpegDownloader.FFMpegBinaries.FFMpeg).Result;
        Assert.IsTrue(binaries.Count == 1); // many platforms have only ffmpeg and ffprobe
    }

    [TestMethod]
    public void GetSpecificVersionTest()
    {
        var binaries = FFMpegDownloader.DownloadFFMpegSuite(FFMpegDownloader.FFMpegVersions.V4_0, binaries: FFMpegDownloader.FFMpegBinaries.FFMpeg).Result;
        Assert.IsTrue(binaries.Count == 1);
    }
}
