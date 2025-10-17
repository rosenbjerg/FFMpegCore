﻿using Newtonsoft.Json;

namespace FFMpegCore.Test;

[TestClass]
public class FFMpegOptionsTest
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
            JsonConvert.DeserializeObject<FFOptions>(File.ReadAllText("ffmpeg.config.json")).BinaryFolder
        );
    }

    [TestMethod]
    public void ZZZ_Options_Set_Programmatically()
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
