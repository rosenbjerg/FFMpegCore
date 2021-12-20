using System.Collections.Generic;

namespace FFMpegCore.Builders.MetaData
{

    public interface IReadOnlyMetaData
    {
        IReadOnlyList<ChapterData> Chapters { get; }
        IReadOnlyDictionary<string, string> Entries { get; }
    }
}