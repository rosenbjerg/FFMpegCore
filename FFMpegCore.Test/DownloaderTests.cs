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
        try
        {
            Assert.HasCount(2, binaries);
        }
        finally
        {
            binaries.ForEach(File.Delete);
        }
    }

    [TestMethod]
    public async Task GetAllLatestSuiteTest()
    {
        var binaries = await FFMpegDownloader.DownloadFFMpegSuite();
        try
        {
            Assert.HasCount(2, binaries);
        }
        finally
        {
            binaries.ForEach(File.Delete);
        }
    }
}
