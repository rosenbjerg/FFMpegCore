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

    [OsSpecificTestMethod(OsPlatforms.Windows | OsPlatforms.Linux)]
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
