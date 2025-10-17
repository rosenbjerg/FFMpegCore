using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;
using FFMpegCore.Test.Utilities;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    private FFOptions _ffOptions;

    [TestInitialize]
    public void InitializeTestFolder()
    {
        var tempDownloadFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDownloadFolder);
        _ffOptions = new FFOptions { BinaryFolder = tempDownloadFolder };
    }

    [TestCleanup]
    public void DeleteTestFolder()
    {
        Directory.Delete(_ffOptions.BinaryFolder, true);
    }

    [OsSpecificTestMethod(OsPlatforms.Windows | OsPlatforms.Linux)]
    public async Task GetSpecificVersionTest()
    {
        var binaries = await FFMpegDownloader.DownloadBinaries(FFMpegVersions.V6_1, options: _ffOptions);
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
        var binaries = await FFMpegDownloader.DownloadBinaries(options: _ffOptions);
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
