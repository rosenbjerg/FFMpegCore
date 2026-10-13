namespace FFMpegCore;

public class ChapterData
{
    public ChapterData(string title, TimeSpan start, TimeSpan end)
    {
        Title = title;
        Start = start;
        End = end;
    }

    public string Title { get; }
    public TimeSpan Start { get; }
    public TimeSpan End { get; }

    public TimeSpan Duration => End - Start;
}
