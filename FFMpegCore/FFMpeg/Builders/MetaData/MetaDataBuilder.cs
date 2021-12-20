using System;
using System.Collections.Generic;
using System.Linq;

namespace FFMpegCore.Builders.MetaData
{
    public class MetaDataBuilder
    {
        private MetaData _metaData = new MetaData();

        public MetaDataBuilder WithEntry(string key, string value)
        {
            _metaData.Entries[key] = value;
            return this;
        }

        public MetaDataBuilder AddChapter(ChapterData chapterData)
        {
            _metaData.Chapters.Add(chapterData);
            return this;
        }

        public MetaDataBuilder AddChapters<T>(IEnumerable<T> values, Func<T, (TimeSpan duration, string title)> chapterGetter)
        {
            foreach (T value in values)
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
            title = String.IsNullOrEmpty(title) ? $"Chapter {_metaData.Chapters.Count + 1}" : title;

            _metaData.Chapters.Add(new ChapterData
            (
                start: start,
                end: end,
                title: title
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
        public MetaDataBuilder WithArtist(string value) => WithEntry("artist", value);

        //composer=J. K. Rowling
        public MetaDataBuilder WithComposer(string value) => WithEntry("composer", value);

        //album_artist=Dennis E. Taylor
        public MetaDataBuilder WithAlbumArtist(string value) => WithEntry("album_artist", value);

        //album=Alle diese Welten: Bobiverse 3
        public MetaDataBuilder WithAlbum(string value) => WithEntry("album", value);

        //date=2019
        public MetaDataBuilder WithDate(string value) => WithEntry("date", value);

        //genre=Hörbuch
        public MetaDataBuilder WithGenre(string value) => WithEntry("genre", value);

        //comment=Chapter 200
        public MetaDataBuilder WithComment(string value) => WithEntry("comment", value);

        //encoder=Lavf58.47.100
        public MetaDataBuilder WithEncoder(string value) => WithEntry("encoder", value);

        public ReadOnlyMetaData Build()
        {
            return new ReadOnlyMetaData(_metaData);
        }
    }
}