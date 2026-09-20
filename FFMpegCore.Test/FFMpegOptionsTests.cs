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

    [TestMethod]
    [DoNotParallelize]
    public void Options_Configure_WithAction_MutatesCurrent()
    {
        try
        {
            GlobalFFOptions.Configure(options => options.WorkingDirectory = "Whatever");
            Assert.AreEqual("Whatever", GlobalFFOptions.Current.WorkingDirectory);
        }
        finally
        {
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    [TestMethod]
    public void BinaryPath_FallsBackToBareName_WhenNothingOnDisk()
    {
        var options = new FFOptions { BinaryFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) };
        var expected = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

        Assert.AreEqual(expected, GlobalFFOptions.GetFFMpegBinaryPath(options));
        Assert.AreEqual(expected.Replace("ffmpeg", "ffprobe"), GlobalFFOptions.GetFFProbeBinaryPath(options));
    }

    [TestMethod]
    public void BinaryPath_PrefersArchitectureSubfolder_OverBinaryFolder()
    {
        var binaryFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var archFolder = Path.Combine(binaryFolder, Environment.Is64BitProcess ? "x64" : "x86");
        var binaryName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        Directory.CreateDirectory(archFolder);
        try
        {
            File.WriteAllText(Path.Combine(binaryFolder, binaryName), string.Empty);
            Assert.AreEqual(Path.Combine(binaryFolder, binaryName), GlobalFFOptions.GetFFMpegBinaryPath(new FFOptions { BinaryFolder = binaryFolder }));

            File.WriteAllText(Path.Combine(archFolder, binaryName), string.Empty);
            Assert.AreEqual(Path.Combine(archFolder, binaryName), GlobalFFOptions.GetFFMpegBinaryPath(new FFOptions { BinaryFolder = binaryFolder }));
        }
        finally
        {
            Directory.Delete(binaryFolder, true);
        }
    }

    [TestMethod]
    public void Options_Clone_IsIndependentCopy()
    {
        var original = new FFOptions { BinaryFolder = "bin", WorkingDirectory = "work", LogLevel = Enums.FFMpegLogLevel.Debug };

        var clone = original.Clone();
        clone.BinaryFolder = "other";

        Assert.AreEqual("bin", original.BinaryFolder);
        Assert.AreEqual(("other", "work", Enums.FFMpegLogLevel.Debug), (clone.BinaryFolder, clone.WorkingDirectory, clone.LogLevel));
        Assert.AreNotSame(original, ((ICloneable)original).Clone());
    }

    [TestMethod]
    public void Options_Encoding_RoundTripsThroughWebName()
    {
        var options = new FFOptions { Encoding = System.Text.Encoding.UTF8 };

        Assert.AreEqual("utf-8", options.EncodingWebName);
        Assert.AreEqual(System.Text.Encoding.UTF8.WebName, options.Encoding.WebName);

        options.Encoding = null;
        Assert.AreEqual(System.Text.Encoding.Default.WebName, options.EncodingWebName);
    }
}
