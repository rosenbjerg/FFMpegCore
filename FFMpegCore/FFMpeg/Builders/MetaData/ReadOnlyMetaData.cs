namespace FFMpegCore.Builders.MetaData
{
    public class ReadOnlyMetaData : IReadOnlyMetaData
    {
        public IReadOnlyDictionary<string, string> Entries { get; private set; }
        public IReadOnlyList<ChapterData> Chapters { get; private set; }

        public ReadOnlyMetaData(MetaData metaData)
        {
            Entries = new Dictionary<string, string>(metaData.Entries);
            Chapters = metaData.Chapters
                .Select(x => new ChapterData
                (
                    start: x.Start,
                    end: x.End,
                    title: x.Title
                ))
                .ToList()
                .AsReadOnly();
        }
    }
}
