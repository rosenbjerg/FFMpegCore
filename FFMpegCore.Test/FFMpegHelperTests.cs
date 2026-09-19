using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;

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
}
