namespace FFMpegCore.Builders.MetaData;

public class MetaData : IReadOnlyMetaData
{
    public MetaData()
    {
        Entries = new Dictionary<string, string>();
        Chapters = new List<ChapterData>();
    }

    public MetaData(MetaData cloneSource)
    {
        Entries = new Dictionary<string, string>(cloneSource.Entries);
        Chapters = cloneSource.Chapters
            .Select(x => new ChapterData
            (
                start: x.Start,
                end: x.End,
                title: x.Title
            ))
            .ToList();
    }

    public Dictionary<string, string> Entries { get; }
    public List<ChapterData> Chapters { get; }

    IReadOnlyList<ChapterData> IReadOnlyMetaData.Chapters => Chapters;
    IReadOnlyDictionary<string, string> IReadOnlyMetaData.Entries => Entries;
}
