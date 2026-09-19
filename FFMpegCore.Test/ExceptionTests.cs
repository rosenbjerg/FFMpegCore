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
    public void FFOptionsException_And_FFMpegArgumentException_WrapMessageAndInner()
    {
        var inner = new Exception("inner");

        var options = new FFOptionsException("options", inner);
        var argument = new FFMpegArgumentException("argument", inner);
        var argumentDefault = new FFMpegArgumentException();

        Assert.AreEqual(("options", inner), (options.Message, options.InnerException));
        Assert.AreEqual(("argument", inner), (argument.Message, argument.InnerException));
        Assert.IsNull(argumentDefault.InnerException);
    }

    [TestMethod]
    public void FFProbeExceptions_FormAHierarchy()
    {
        var inner = new Exception("inner");
        var probe = new FFProbeException("probe", inner);
        var process = new FFProbeProcessException("process", new[] { "line1", "line2" }, inner);
        var formatNull = new FormatNullException();

        Assert.AreEqual(("probe", inner), (probe.Message, probe.InnerException));
        Assert.IsInstanceOfType<FFProbeException>(process);
        Assert.IsInstanceOfType<FFProbeException>(formatNull);
        CollectionAssert.AreEqual(new[] { "line1", "line2" }, process.ProcessErrors.ToArray());
        Assert.AreEqual("Format not specified", formatNull.Message);
    }
}
