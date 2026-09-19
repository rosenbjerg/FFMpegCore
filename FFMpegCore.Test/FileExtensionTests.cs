using FFMpegCore.Enums;

namespace FFMpegCore.Test;

[TestClass]
public class FileExtensionTests
{
    [TestMethod]
    [DataRow("libx264", CodecType.Video, ".mp4")]
    [DataRow("libvpx", CodecType.Video, ".webm")]
    [DataRow("libtheora", CodecType.Video, ".ogv")]
    [DataRow("mpegts", CodecType.Video, ".ts")]
    [DataRow("png", CodecType.Video, ".png")]
    [DataRow("mjpeg", CodecType.Video, ".jpg")]
    [DataRow("bmp", CodecType.Video, ".bmp")]
    [DataRow("webp", CodecType.Video, ".webp")]
    public void Extension_MapsKnownCodecs(string codecName, CodecType type, string expected)
    {
        Assert.AreEqual(expected, new Codec(codecName, type).Extension());
    }

    [TestMethod]
    public void Extension_MatchesBuiltInCodecs()
    {
        Assert.AreEqual(FileExtension.Mp4, VideoCodec.LibX264.Extension());
        Assert.AreEqual(FileExtension.WebM, VideoCodec.LibVpx.Extension());
        Assert.AreEqual(FileExtension.Ogv, VideoCodec.LibTheora.Extension());
        Assert.AreEqual(FileExtension.Image.Jpg, VideoCodec.Image.Jpg.Extension());
    }

    [TestMethod]
    public void Extension_ThrowsForUnknownCodec()
    {
        Assert.ThrowsExactly<Exception>(() => new Codec("libx265", CodecType.Video).Extension());
    }
}
