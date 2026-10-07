using FFMpegCore.Exceptions;

namespace FFMpegCore.Test;

[TestClass]
public class ExceptionTests
{
    [TestMethod]
    public void FFMpegException_CarriesType()
    {
        var inner = new InvalidOperationException("inner");

        var withInner = new FFMpegException(FFMpegExceptionType.Process, "message", inner);
        var minimal = new FFMpegException(FFMpegExceptionType.File, "message");

        Assert.AreEqual((FFMpegExceptionType.Process, "message", inner), (withInner.Type, withInner.Message, withInner.InnerException));
        Assert.AreEqual((FFMpegExceptionType.File, "message"), (minimal.Type, minimal.Message));
        Assert.IsNull(minimal.InnerException);
    }

    [TestMethod]
    public void FFMpegStreamFormatException_IsAnFFMpegException()
    {
        var exception = new FFMpegStreamFormatException(FFMpegExceptionType.Operation, "format", new Exception("inner"));

        Assert.IsInstanceOfType<FFMpegException>(exception);
        Assert.AreEqual(FFMpegExceptionType.Operation, exception.Type);
        Assert.AreEqual("inner", exception.InnerException?.Message);
    }

    [TestMethod]
    public void FFProbeExceptions_AreFFMpegExceptions()
    {
        var inner = new Exception("inner");
        var probe = new FFProbeException(FFMpegExceptionType.File, "probe", inner);
        var process = new FFProbeProcessException(1, new[] { "line1", "line2" }, inner);
        var formatNull = new FormatNullException();

        Assert.IsInstanceOfType<FFMpegException>(probe);
        Assert.AreEqual((FFMpegExceptionType.File, "probe", inner), (probe.Type, probe.Message, probe.InnerException));
        Assert.IsInstanceOfType<FFProbeException>(process);
        Assert.AreEqual((FFMpegExceptionType.Process, 1), (process.Type, process.ExitCode));
        CollectionAssert.AreEqual(new[] { "line1", "line2" }, process.StandardError.ToArray());
        Assert.IsInstanceOfType<FFProbeException>(formatNull);
        Assert.AreEqual("Format not specified", formatNull.Message);
    }
}
