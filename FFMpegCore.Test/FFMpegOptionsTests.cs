using System.Text.Json;

namespace FFMpegCore.Test;

[TestClass]
public class FFMpegOptionsTests
{
    [TestMethod]
    public void Options_Initialized()
    {
        Assert.IsNotNull(GlobalFFOptions.Current);
    }

    [TestMethod]
    public void Options_Defaults_Configured()
    {
        Assert.AreEqual("", new FFOptions().BinaryFolder);
    }

    [TestMethod]
    public void Options_Loaded_From_File()
    {
        Assert.AreEqual(
            GlobalFFOptions.Current.BinaryFolder,
            JsonSerializer.Deserialize<FFOptions>(File.ReadAllText("ffmpeg.config.json")).BinaryFolder
        );
    }

    [TestMethod]
    [DoNotParallelize]
    public void Options_Set_Programmatically()
    {
        try
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "Whatever" });
            Assert.AreEqual("Whatever", GlobalFFOptions.Current.BinaryFolder);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }
}
