using FFMpegCore.Exceptions;

namespace FFMpegCore.Test;

[TestClass]
public class ExceptionTests
{
    [TestMethod]
    public void FFMpegException_CarriesTypeAndErrorOutput()
    {
        var inner = new InvalidOperationException("inner");

        var withInner = new FFMpegException(FFMpegExceptionType.Process, "message", inner, "stderr");
        var withOutput = new FFMpegException(FFMpegExceptionType.Conversion, "message", "stderr");
        var minimal = new FFMpegException(FFMpegExceptionType.File, "message");

        Assert.AreEqual((FFMpegExceptionType.Process, "message", inner, "stderr"), (withInner.Type, withInner.Message, withInner.InnerException, withInner.FFMpegErrorOutput));
        Assert.AreEqual((FFMpegExceptionType.Conversion, "message", "stderr"), (withOutput.Type, withOutput.Message, withOutput.FFMpegErrorOutput));
        Assert.IsNull(withOutput.InnerException);
        Assert.AreEqual((FFMpegExceptionType.File, "message", string.Empty), (minimal.Type, minimal.Message, minimal.FFMpegErrorOutput));
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
    public void FFMpegArgumentException_WrapsMessageAndInner()
    {
        var inner = new Exception("inner");

        var argument = new FFMpegArgumentException("argument", inner);
        var argumentDefault = new FFMpegArgumentException();

        Assert.AreEqual(("argument", inner), (argument.Message, argument.InnerException));
        Assert.IsNull(argumentDefault.InnerException);
    }

    [TestMethod]
    public void FFProbeExceptions_AreFFMpegExceptions()
    {
        var inner = new Exception("inner");
        var probe = new FFProbeException(FFMpegExceptionType.File, "probe", inner);
        var process = new FFProbeProcessException("process", new[] { "line1", "line2" }, inner);
        var formatNull = new FormatNullException();

        Assert.IsInstanceOfType<FFMpegException>(probe);
        Assert.AreEqual((FFMpegExceptionType.File, "probe", inner, string.Empty), (probe.Type, probe.Message, probe.InnerException, probe.FFMpegErrorOutput));
        Assert.IsInstanceOfType<FFProbeException>(process);
        Assert.AreEqual((FFMpegExceptionType.Process, "line1\nline2"), (process.Type, process.FFMpegErrorOutput));
        CollectionAssert.AreEqual(new[] { "line1", "line2" }, process.ErrorOutput.ToArray());
        Assert.IsInstanceOfType<FFProbeException>(formatNull);
        Assert.AreEqual("Format not specified", formatNull.Message);
    }
}
