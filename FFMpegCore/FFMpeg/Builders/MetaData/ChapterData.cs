using System;

namespace FFMpegCore.Builders.MetaData
{
    public class ChapterData
    {
        public string Title { get; private set; }
        public TimeSpan Start { get; private set; }
        public TimeSpan End { get; private set; }

        public ChapterData(string title, TimeSpan start, TimeSpan end)
        {
            Title = title;
            Start = start;
            End = end;
        }
    }
}