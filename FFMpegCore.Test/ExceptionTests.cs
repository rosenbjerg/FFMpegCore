using FFMpegCore.Exceptions;

namespace FFMpegCore.Test;

[TestClass]
public class ExceptionTests
{
    [TestMethod]
    public void FFMpegException_CarriesMessageAndInner()
    {
        var inner = new InvalidOperationException("inner");

        var withInner = new FFMpegException("message", inner);
        var minimal = new FFMpegException("message");

        Assert.AreEqual(("message", inner), (withInner.Message, withInner.InnerException));
        Assert.AreEqual("message", minimal.Message);
        Assert.IsNull(minimal.InnerException);
    }

    [TestMethod]
    public void FFMpegStreamFormatException_IsAnFFMpegException()
    {
        var exception = new FFMpegStreamFormatException("format", new Exception("inner"));

        Assert.IsInstanceOfType<FFMpegException>(exception);
        Assert.AreEqual("inner", exception.InnerException?.Message);
    }

    [TestMethod]
    public void FFProbeExceptions_AreFFMpegExceptions()
    {
        var inner = new Exception("inner");
        var probe = new FFProbeException("probe", inner);
        var process = new FFProbeProcessException(1, ["line1", "line2"], inner);
        var formatNull = new FormatNullException();

        Assert.IsInstanceOfType<FFMpegException>(probe);
        Assert.AreEqual(("probe", inner), (probe.Message, probe.InnerException));
        Assert.IsInstanceOfType<FFProbeException>(process);
        Assert.AreEqual(1, process.ExitCode);
        CollectionAssert.AreEqual(new[] { "line1", "line2" }, process.StandardError.ToArray());
        Assert.IsInstanceOfType<FFProbeException>(formatNull);
        Assert.AreEqual("Format not specified", formatNull.Message);
    }
}
