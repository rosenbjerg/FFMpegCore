using System.Runtime.Versioning;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using FFMpegCore.Test.Utilities;

namespace FFMpegCore.Test;

[TestClass]
public class FFMpegHelperTests
{
    [TestMethod]
    public void ExtensionExceptionCheck_AcceptsMatchingExtension_IgnoringCase()
    {
        FFMpegHelper.ExtensionExceptionCheck("output.MP4", ".mp4");
    }

    [TestMethod]
    public void ExtensionExceptionCheck_RejectsMismatch()
    {
        var exception = Assert.ThrowsExactly<FFMpegException>(() => FFMpegHelper.ExtensionExceptionCheck("output.mkv", ".mp4"));
        Assert.AreEqual(FFMpegExceptionType.File, exception.Type);
    }

    [TestMethod]
    [DataRow(640, 480)]
    [DataRow(2, 2)]
    public void ConversionSizeExceptionCheck_AcceptsEvenDimensions(int width, int height)
    {
        FFMpegHelper.ConversionSizeExceptionCheck(width, height);
    }

    [TestMethod]
    [DataRow(641, 480)]
    [DataRow(640, 481)]
    [DataRow(1, 1)]
    public void ConversionSizeExceptionCheck_RejectsOddDimensions(int width, int height)
    {
        Assert.ThrowsExactly<ArgumentException>(() => FFMpegHelper.ConversionSizeExceptionCheck(width, height));
    }

    [OsSpecificTestMethod(OsPlatforms.Linux | OsPlatforms.MacOS)]
    [UnsupportedOSPlatform("windows")]
    public void VerifyFFMpegExists_VerifiesEveryBinaryPath_NotOnlyTheFirst()
    {
        FFMpegHelper.VerifyFFMpegExists(GlobalFFOptions.Current);

        using var binaryFolder = new TemporaryBinaryFolder("ffmpeg");

        var exception = Assert.ThrowsExactly<FFMpegException>(() => FFMpegHelper.VerifyFFMpegExists(binaryFolder.Options));
        Assert.AreEqual(FFMpegExceptionType.Operation, exception.Type);
        StringAssert.Contains(exception.Message, binaryFolder.BinaryPath);
    }

    [OsSpecificTestMethod(OsPlatforms.Linux | OsPlatforms.MacOS)]
    [UnsupportedOSPlatform("windows")]
    public void VerifyFFProbeExists_VerifiesEveryBinaryPath_NotOnlyTheFirst()
    {
        FFProbeHelper.VerifyFFProbeExists(GlobalFFOptions.Current);

        using var binaryFolder = new TemporaryBinaryFolder("ffprobe");

        var exception = Assert.ThrowsExactly<FFProbeException>(() => FFProbeHelper.VerifyFFProbeExists(binaryFolder.Options));
        Assert.AreEqual(FFMpegExceptionType.Operation, exception.Type);
        StringAssert.Contains(exception.Message, binaryFolder.BinaryPath);
    }
}
