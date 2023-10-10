using FFMpegCore.Extensions.Downloader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    [Ignore]
    [TestMethod]
    public void GetAllLatestSuiteTest()
    {
        var binaries = FFMpegDownloader.DownloadFFMpegSuite(binaries: FFMpegDownloader.FFMpegBinaries.FFProbe |
                                                                      FFMpegDownloader.FFMpegBinaries.FFMpeg |
                                                                      FFMpegDownloader.FFMpegBinaries.FFPlay).Result;
        Assert.IsTrue(binaries.Count >= 2); // many platforms have only ffmpeg and ffprobe
    }

    [Ignore]
    [TestMethod]
    public void GetSpecificVersionTest()
    {
        var binaries = FFMpegDownloader.DownloadFFMpegSuite(FFMpegDownloader.FFMpegVersions.V4_0).Result;
        Assert.IsTrue(binaries.Count == 2);
    }
}
