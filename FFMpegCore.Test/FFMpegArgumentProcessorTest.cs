using FFMpegCore.Arguments;

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
}
