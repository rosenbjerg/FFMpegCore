using FFMpegCore.Extensions.Downloader;
using FFMpegCore.Extensions.Downloader.Enums;
using FFMpegCore.Extensions.Downloader.Exceptions;

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
    public async Task DownloadBinaries_RequiresABinaryFolderBeforeGoingOnline()
    {
        var exception = await Assert.ThrowsExactlyAsync<FFMpegDownloaderException>(() =>
            FFMpegDownloader.DownloadBinariesAsync(ffOptions: new FFOptions(), cancellationToken: TestContext.CancellationToken));

        StringAssert.Contains(exception.Message, "BinaryFolder");
    }

    [TestMethod]
    public async Task DownloadBinaries_CreatesAMissingBinaryFolder()
    {
        var binaryFolder = Path.Combine(_ffOptions.BinaryFolder, "not-yet-created");

        var binaries = await FFMpegDownloader.DownloadBinariesAsync(FFMpegVersions.V6_1, FFMpegBinaries.FFProbe,
            new FFOptions { BinaryFolder = binaryFolder }, cancellationToken: TestContext.CancellationToken);

        Assert.HasCount(1, binaries);
        Assert.IsTrue(File.Exists(binaries[0]));
        if (!OperatingSystem.IsWindows())
        {
            Assert.IsTrue(File.GetUnixFileMode(binaries[0]).HasFlag(UnixFileMode.UserExecute));
        }
    }

    [TestMethod]
    public async Task GetSpecificVersionTest()
    {
        var binaries = await FFMpegDownloader.DownloadBinariesAsync(FFMpegVersions.V6_1, ffOptions: _ffOptions,
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
        var binaries = await FFMpegDownloader.DownloadBinariesAsync(ffOptions: _ffOptions,
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
