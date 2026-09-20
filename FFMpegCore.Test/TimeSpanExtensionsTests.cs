using FFMpegCore.Extend;

namespace FFMpegCore.Test;

[TestClass]
public class TimeSpanExtensionsTests
{
    [TestMethod]
    [DataRow(0, 0, 0, 0, 0, "00:00:00.000")]
    [DataRow(0, 1, 2, 3, 4, "01:02:03.004")]
    [DataRow(0, 23, 59, 59, 999, "23:59:59.999")]
    [DataRow(1, 0, 0, 0, 0, "24:00:00.000")]
    [DataRow(2, 5, 30, 15, 500, "53:30:15.500")]
    public void ToLongString_FoldsDaysIntoHours(int days, int hours, int minutes, int seconds, int milliseconds, string expected)
    {
        var timeSpan = new TimeSpan(days, hours, minutes, seconds, milliseconds);

        Assert.AreEqual(expected, timeSpan.ToLongString());
    }
}
