using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    [TestMethod]
    public async Task GetSpecificVersionTest()
    {
        var binaries = await FFMpegDownloader.DownloadFFMpegSuite(FFMpegVersions.V6_1);
        Assert.HasCount(2, binaries);
    }

    [TestMethod]
    public async Task GetAllLatestSuiteTest()
    {
        var binaries = await FFMpegDownloader.DownloadFFMpegSuite();
        Assert.HasCount(2, binaries);
    }
}
