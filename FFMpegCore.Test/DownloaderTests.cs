using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;

namespace FFMpegCore.Test;

[TestClass]
public class DownloaderTests
{
    private FFOptions _ffOptions;

    public TestContext TestContext { get; set; }

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

    [TestMethod]
    public async Task GetSpecificVersionTest()
    {
        var binaries = await FFMpegDownloader.DownloadBinariesAsync(FFMpegVersions.V6_1, options: _ffOptions,
            cancellationToken: TestContext.CancellationToken);
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
        var binaries = await FFMpegDownloader.DownloadBinariesAsync(options: _ffOptions,
            cancellationToken: TestContext.CancellationToken);
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
