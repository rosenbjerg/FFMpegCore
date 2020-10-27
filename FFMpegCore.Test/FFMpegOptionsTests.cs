using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;

namespace FFMpegCore.Test
{
    [TestClass]
    public class FFMpegOptionsTest
    {
        [TestMethod]
        public void Options_Initialized()
        {
            Assert.IsNotNull(FFMpegOptions.Options);
        }

        [TestMethod]
        public void Options_Defaults_Configured()
        {
            Assert.AreEqual(new FFMpegOptions().RootDirectory, $"");
        }

        [TestMethod]
        public void Options_Loaded_From_File()
        {
            Assert.AreEqual(
                FFMpegOptions.Options.RootDirectory, 
                JsonConvert.DeserializeObject<FFMpegOptions>(File.ReadAllText("ffmpeg.config.json")).RootDirectory
            );
        }

        [TestMethod]
        public void Options_Set_Programmatically()
        {
            var original = FFMpegOptions.Options; 
            try
            {
                FFMpegOptions.Configure(new FFMpegOptions { RootDirectory = "Whatever" });
                Assert.AreEqual(
                    FFMpegOptions.Options.RootDirectory,
                    "Whatever"
                );
            }
            finally
            {
                FFMpegOptions.Configure(original);
            }
        }
    }
}
