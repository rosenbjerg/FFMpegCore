using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;
using FFMpegCore.Test.Utilities;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    [OsSpecificTestMethod(OsPlatforms.Windows | OsPlatforms.Linux)]
    public async Task GetSpecificVersionTest()
    {
        var options = new FFOptions { BinaryFolder = Path.GetTempPath() };
        var binaries = await FFMpegDownloader.DownloadFFMpegSuite(FFMpegVersions.V6_1, options: options);
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
        var options = new FFOptions { BinaryFolder = Path.GetTempPath() };
        var binaries = await FFMpegDownloader.DownloadFFMpegSuite(options: options);
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
