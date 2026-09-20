using FFMpegCore.Enums;
using FFMpegCore.Exceptions;
using FFMpegCore.Test.Resources;

namespace FFMpegCore.Test;

[TestClass]
public class FFMpegArgumentProcessorTest
{
    private static FFMpegArgumentProcessor CreateArgumentProcessor()
    {
        return FFMpegArguments
            .FromFileInput("")
            .OutputToFile("");
    }

    [TestMethod]
    [DoNotParallelize]
    public void Processor_GlobalOptions_GetUsed()
    {
        var globalWorkingDir = "Whatever";
        var processor = CreateArgumentProcessor();

        try
        {
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalWorkingDir });

            var options = processor.GetConfiguredOptions(null);

            Assert.AreEqual(globalWorkingDir, options.WorkingDirectory);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    [TestMethod]
    public void Processor_SessionOptions_GetUsed()
    {
        var sessionWorkingDir = "./CurrentRunWorkingDir";

        var processor = CreateArgumentProcessor();
        processor.Configure(options => options.WorkingDirectory = sessionWorkingDir);
        var options = processor.GetConfiguredOptions(null);

        Assert.AreEqual(sessionWorkingDir, options.WorkingDirectory);
    }

    [TestMethod]
    [DoNotParallelize]
    public void Processor_Options_CanBeOverridden_And_Configured()
    {
        var globalConfig = "Whatever";

        try
        {
            var processor = CreateArgumentProcessor();

            var sessionTempDir = "./CurrentRunWorkingDir";
            processor.Configure(options => options.TemporaryFilesFolder = sessionTempDir);

            var overrideOptions = new FFOptions { WorkingDirectory = "override" };

            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalConfig, TemporaryFilesFolder = globalConfig, BinaryFolder = globalConfig });
            var options = processor.GetConfiguredOptions(overrideOptions);

            Assert.AreEqual(options.WorkingDirectory, overrideOptions.WorkingDirectory);
            Assert.AreEqual(options.TemporaryFilesFolder, overrideOptions.TemporaryFilesFolder);
            Assert.AreEqual(options.BinaryFolder, overrideOptions.BinaryFolder);

            Assert.AreEqual(sessionTempDir, options.TemporaryFilesFolder);
            Assert.AreNotEqual(globalConfig, options.BinaryFolder);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    [TestMethod]
    [DoNotParallelize]
    public void Options_Global_And_Session_Options_Can_Differ()
    {
        var globalWorkingDir = "Whatever";

        try
        {
            var processor1 = CreateArgumentProcessor();
            var sessionWorkingDir = "./CurrentRunWorkingDir";
            processor1.Configure(options => options.WorkingDirectory = sessionWorkingDir);
            var options1 = processor1.GetConfiguredOptions(null);
            Assert.AreEqual(sessionWorkingDir, options1.WorkingDirectory);

            var processor2 = CreateArgumentProcessor();
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalWorkingDir });
            var options2 = processor2.GetConfiguredOptions(null);
            Assert.AreEqual(globalWorkingDir, options2.WorkingDirectory);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    private static FFMpegArgumentProcessor CreateCopyProcessor(string output)
    {
        return FFMpegArguments
            .FromFileInput(TestResources.Mp4Video)
            .OutputToFile(output, true, options => options.CopyChannel());
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Processor_WithLogLevel_ControlsStderrVolume()
    {
        using var output = new TemporaryFile("out.mp4");
        var quietLines = new List<string>();
        var infoLines = new List<string>();

        CreateCopyProcessor(output).WithLogLevel(FFMpegLogLevel.Quiet).NotifyOnError(quietLines.Add).ProcessSynchronously();
        CreateCopyProcessor(output).WithLogLevel(FFMpegLogLevel.Info).NotifyOnError(infoLines.Add).ProcessSynchronously();

        Assert.IsEmpty(quietLines);
        Assert.IsNotEmpty(infoLines);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Processor_LogLevel_FromOptions_IsNotStickyAcrossRuns()
    {
        using var output = new TemporaryFile("out.mp4");
        var lines = new List<string>();
        var processor = CreateCopyProcessor(output).NotifyOnError(lines.Add);

        processor.ProcessSynchronously(true, new FFOptions { LogLevel = FFMpegLogLevel.Quiet });
        Assert.IsEmpty(lines);

        processor.ProcessSynchronously(true, new FFOptions { LogLevel = FFMpegLogLevel.Info });
        Assert.IsNotEmpty(lines);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Processor_ExplicitLogLevel_OverridesOptions()
    {
        using var output = new TemporaryFile("out.mp4");
        var lines = new List<string>();

        CreateCopyProcessor(output)
            .WithLogLevel(FFMpegLogLevel.Quiet)
            .NotifyOnError(lines.Add)
            .ProcessSynchronously(true, new FFOptions { LogLevel = FFMpegLogLevel.Info });

        Assert.IsEmpty(lines);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Processor_NotifyOnProgress_ReportsAtQuietLogLevel()
    {
        using var output = new TemporaryFile("out.mp4");
        var times = new List<TimeSpan>();

        CreateCopyProcessor(output)
            .WithLogLevel(FFMpegLogLevel.Quiet)
            .NotifyOnProgress(times.Add)
            .ProcessSynchronously();

        Assert.IsNotEmpty(times);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public async Task Processor_NotifyOnOutput_ReceivesStdout()
    {
        using var output = new TemporaryFile("out.mp4");
        var lines = new List<string>();

        var success = await FFMpegArguments
            .FromFileInput(TestResources.Mp4Video, true, options => options.WithCustomArgument("-progress pipe:1"))
            .OutputToFile(output, true, options => options.CopyChannel())
            .NotifyOnOutput(lines.Add)
            .CancellableThrough(TestContext.CancellationToken)
            .ProcessAsynchronously();

        Assert.IsTrue(success);
        Assert.Contains("progress=end", lines);
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Processor_MissingInput_ThrowsBeforeStartingFFMpeg()
    {
        using var output = new TemporaryFile("out.mp4");
        var missing = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp4");

        Assert.ThrowsExactly<FileNotFoundException>(() => FFMpegArguments.FromFileInput(missing).OutputToFile(output).ProcessSynchronously());
        Assert.ThrowsExactly<FileNotFoundException>(() => FFMpegArguments.FromFileInput(new[] { TestResources.Mp4Video, missing }).OutputToFile(output).ProcessSynchronously());
    }

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Processor_ExistingOutput_ThrowsWhenOverwriteDisabled()
    {
        using var output = new TemporaryFile("out.mp4");
        File.WriteAllText(output, string.Empty);

        var exception = Assert.ThrowsExactly<FFMpegException>(() =>
            FFMpegArguments.FromFileInput(TestResources.Mp4Video).OutputToFile(output, false).ProcessSynchronously());

        Assert.AreEqual(FFMpegExceptionType.File, exception.Type);
    }

    public TestContext TestContext { get; set; }
}
