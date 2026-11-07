namespace FFMpegCore.Test;

[TestClass]
public class TagExtensionsTests
{
    [TestMethod]
    public void TagGetters_ReadKnownKeys()
    {
        var stream = new FFProbeStream
        {
            Tags = new Dictionary<string, string>
            {
                { "language", "eng" },
                { "creation_time", "2024-01-01T00:00:00Z" },
                { "rotate", "90" },
                { "duration", "00:00:03.000" }
            }
        };

        Assert.AreEqual("eng", stream.GetLanguage());
        Assert.AreEqual("2024-01-01T00:00:00Z", stream.GetCreationTime());
        Assert.AreEqual("90", stream.GetRotate());
        Assert.AreEqual("00:00:03.000", stream.GetDuration());
    }

    [TestMethod]
    public void TagGetters_ReturnNull_WhenMissingOrNoTags()
    {
        var withoutTags = new FFProbeStream { Tags = null };
        var withOtherTags = new FFProbeStream { Tags = new Dictionary<string, string> { { "title", "x" } } };

        Assert.IsNull(withoutTags.GetLanguage());
        Assert.IsNull(withOtherTags.GetLanguage());
        Assert.IsNull(withOtherTags.GetCreationTime());
        Assert.IsNull(withOtherTags.GetRotate());
        Assert.IsNull(withOtherTags.GetDuration());
    }
}
