using System.Reflection;
using FFMpegCore.Arguments;

namespace FFMpegCore.Test;

[TestClass]
public class FFMpegArgumentProcessorTest
{
    [TestCleanup]
    public void TestInitialize()

    {
        // After testing reset global configuration to null, to be not wrong for other test relying on configuration
        typeof(GlobalFFOptions).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(GlobalFFOptions.Current, null);
    }

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

        try
        {
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalWorkingDir });

            var processor = CreateArgumentProcessor();
            var options = processor.GetConfiguredOptions(null);

            Assert.AreEqual(globalWorkingDir, options.WorkingDirectory);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = string.Empty });
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
        GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalConfig, TemporaryFilesFolder = globalConfig, BinaryFolder = globalConfig });

        try
        {
            var processor = CreateArgumentProcessor();

            var sessionTempDir = "./CurrentRunWorkingDir";
            processor.Configure(options => options.TemporaryFilesFolder = sessionTempDir);

            var overrideOptions = new FFOptions { WorkingDirectory = "override" };
            var options = processor.GetConfiguredOptions(overrideOptions);

            Assert.AreSame(options, overrideOptions);
            Assert.AreEqual(sessionTempDir, options.TemporaryFilesFolder);
            Assert.AreNotEqual(globalConfig, options.BinaryFolder);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = string.Empty });
        }
    }

    [TestMethod]
    public void Options_Global_And_Session_Options_Can_Differ()
    {
        var globalWorkingDir = "Whatever";
        GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalWorkingDir });

        var processor1 = CreateArgumentProcessor();
        var sessionWorkingDir = "./CurrentRunWorkingDir";
        processor1.Configure(options => options.WorkingDirectory = sessionWorkingDir);
        var options1 = processor1.GetConfiguredOptions(null);
        Assert.AreEqual(sessionWorkingDir, options1.WorkingDirectory);

        var processor2 = CreateArgumentProcessor();
        var options2 = processor2.GetConfiguredOptions(null);
        Assert.AreEqual(globalWorkingDir, options2.WorkingDirectory);
    }

    [TestMethod]
    public void Concat_Escape()
    {
        var arg = new DemuxConcatArgument([@"Heaven's River\05 - Investigation.m4b"]);
        CollectionAssert.AreEquivalent(new[] { @"file 'Heaven'\''s River\05 - Investigation.m4b'" }, arg.Values.ToArray());
    }

    [TestMethod]
    public void Audible_Aaxc_Test()
    {
        var arg = new AudibleEncryptionKeyArgument("123", "456");
        Assert.AreEqual("-audible_key 123 -audible_iv 456", arg.Text);
    }

    [TestMethod]
    public void Audible_Aax_Test()
    {
        var arg = new AudibleEncryptionKeyArgument("62689101");
        Assert.AreEqual("-activation_bytes 62689101", arg.Text);
    }
}
