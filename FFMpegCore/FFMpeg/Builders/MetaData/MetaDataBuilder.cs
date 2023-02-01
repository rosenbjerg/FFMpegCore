namespace FFMpegCore.Builders.MetaData
{
    public class MetaDataBuilder
    {
        private readonly MetaData _metaData = new();

        public MetaDataBuilder WithEntry(string key, string entry)
        {
            if (_metaData.Entries.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                entry = string.Concat(value, "; ", entry);
            }

            _metaData.Entries[key] = entry;
            return this;
        }

        public MetaDataBuilder WithEntry(string key, params string[] values)
            => WithEntry(key, string.Join("; ", values));

        public MetaDataBuilder WithEntry(string key, IEnumerable<string> values)
            => WithEntry(key, string.Join("; ", values));

        public MetaDataBuilder AddChapter(ChapterData chapterData)
        {
            _metaData.Chapters.Add(chapterData);
            return this;
        }

        public MetaDataBuilder AddChapters<T>(IEnumerable<T> values, Func<T, (TimeSpan duration, string title)> chapterGetter)
        {
            foreach (var value in values)
            {
                var (duration, title) = chapterGetter(value);
                AddChapter(duration, title);
            }

            return this;
        }

        public MetaDataBuilder AddChapter(TimeSpan duration, string? title = null)
        {
            var start = _metaData.Chapters.LastOrDefault()?.End ?? TimeSpan.Zero;
            var end = start + duration;
            title = string.IsNullOrEmpty(title) ? $"Chapter {_metaData.Chapters.Count + 1}" : title;

            _metaData.Chapters.Add(new ChapterData
            (
                start: start,
                end: end,
                title: title ?? string.Empty
            ));

            return this;
        }

        //major_brand=M4A
        public MetaDataBuilder WithMajorBrand(string value) => WithEntry("major_brand", value);

        //minor_version=512
        public MetaDataBuilder WithMinorVersion(string value) => WithEntry("minor_version", value);

        //compatible_brands=M4A isomiso2
        public MetaDataBuilder WithCompatibleBrands(string value) => WithEntry("compatible_brands", value);

        //copyright=©2017 / 2019 Dennis E. Taylor / Random House Audio / Wilhelm Heyne Verlag. Übersetzung von Urban Hofstetter (P)2019 Random House Audio
        public MetaDataBuilder WithCopyright(string value) => WithEntry("copyright", value);

        //title=Alle diese Welten: Bobiverse 3
        public MetaDataBuilder WithTitle(string value) => WithEntry("title", value);

        //artist=Dennis E. Taylor
        public MetaDataBuilder WithArtists(params string[] value) => WithEntry("artist", value);
        public MetaDataBuilder WithArtists(IEnumerable<string> value) => WithEntry("artist", value);

        //composer=J. K. Rowling
        public MetaDataBuilder WithComposers(params string[] value) => WithEntry("composer", value);
        public MetaDataBuilder WithComposers(IEnumerable<string> value) => WithEntry("composer", value);

        //album_artist=Dennis E. Taylor
        public MetaDataBuilder WithAlbumArtists(params string[] value) => WithEntry("album_artist", value);
        public MetaDataBuilder WithAlbumArtists(IEnumerable<string> value) => WithEntry("album_artist", value);

        //album=Alle diese Welten: Bobiverse 3
        public MetaDataBuilder WithAlbum(string value) => WithEntry("album", value);

        //date=2019
        public MetaDataBuilder WithDate(string value) => WithEntry("date", value);

        //genre=Hörbuch
        public MetaDataBuilder WithGenres(params string[] value) => WithEntry("genre", value);
        public MetaDataBuilder WithGenres(IEnumerable<string> value) => WithEntry("genre", value);

        //comment=Chapter 200
        public MetaDataBuilder WithComments(params string[] value) => WithEntry("comment", value);
        public MetaDataBuilder WithComments(IEnumerable<string> value) => WithEntry("comment", value);

        //encoder=Lavf58.47.100
        public MetaDataBuilder WithEncoder(string value) => WithEntry("encoder", value);

        public ReadOnlyMetaData Build() => new(_metaData);
    }
}
