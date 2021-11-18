using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Reflection;

namespace FFMpegCore.Test
{
    [TestClass]
    public class FFMpegArgumentProcessorTest
    {
        [TestCleanup]
        public void TestInitialize()

        {
            // After testing reset global configuration to null, to be not wrong for other test relying on configuration
            typeof(GlobalFFOptions).GetField("_current", BindingFlags.NonPublic | BindingFlags.Static).SetValue(GlobalFFOptions.Current, null);
        }

        private static FFMpegArgumentProcessor CreateArgumentProcessor() => FFMpegArguments
                        .FromFileInput("")
                        .OutputToFile("");


        [TestMethod]
        public void Processor_GlobalOptions_GetUsed()
        {
            var globalWorkingDir = "Whatever";
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalWorkingDir });

            var processor = CreateArgumentProcessor();
            var options2 = processor.GetConfiguredOptions(null);
            options2.WorkingDirectory.Should().Be(globalWorkingDir);
        }

        [TestMethod]
        public void Processor_SessionOptions_GetUsed()
        {
            var sessionWorkingDir = "./CurrentRunWorkingDir";

            var processor = CreateArgumentProcessor();
            processor.Configure(options => options.WorkingDirectory = sessionWorkingDir);
            var options = processor.GetConfiguredOptions(null);

            options.WorkingDirectory.Should().Be(sessionWorkingDir);
        }


        [TestMethod]
        public void Processor_Options_CanBeOverridden_And_Configured()
        {
            var globalConfig = "Whatever";
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalConfig, TemporaryFilesFolder = globalConfig, BinaryFolder = globalConfig });


            var processor = CreateArgumentProcessor();

            var sessionTempDir = "./CurrentRunWorkingDir";
            processor.Configure(options => options.TemporaryFilesFolder = sessionTempDir);

            var overrideOptions = new FFOptions() { WorkingDirectory = "override" };
            var options = processor.GetConfiguredOptions(overrideOptions);

            options.Should().BeEquivalentTo(overrideOptions);
            options.TemporaryFilesFolder.Should().BeEquivalentTo(sessionTempDir);
            options.BinaryFolder.Should().NotBeEquivalentTo(globalConfig);
        }


        [TestMethod]
        public void Options_Global_And_Session_Options_Can_Differ()
        {
            FFMpegArgumentProcessor CreateArgumentProcessor() => FFMpegArguments
                .FromFileInput("")
                .OutputToFile("");

            var globalWorkingDir = "Whatever";
            GlobalFFOptions.Configure(new FFOptions { WorkingDirectory = globalWorkingDir });

            var processor1 = CreateArgumentProcessor();
            var sessionWorkingDir = "./CurrentRunWorkingDir";
            processor1.Configure(options => options.WorkingDirectory = sessionWorkingDir);
            var options1 = processor1.GetConfiguredOptions(null);
            options1.WorkingDirectory.Should().Be(sessionWorkingDir);


            var processor2 = CreateArgumentProcessor();
            var options2 = processor2.GetConfiguredOptions(null);
            options2.WorkingDirectory.Should().Be(globalWorkingDir);
        }
    }
}