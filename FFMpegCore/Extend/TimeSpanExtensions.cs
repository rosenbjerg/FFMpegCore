namespace FFMpegCore.Extend;

public static class TimeSpanExtensions
{
    public static string ToLongString(this TimeSpan timeSpan)
    {
        var hours = timeSpan.Hours;
        if (timeSpan.Days > 0)
        {
            hours += timeSpan.Days * 24;
        }

        return $"{hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
    }
}
