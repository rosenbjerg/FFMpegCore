using System.Text;

namespace FFMpegCore;

public class FFMetadataBuilder
{
    private readonly List<ChapterData> _chapters = new();
    private readonly Dictionary<string, string> _tags = new();

    public static FFMetadataBuilder Empty()
    {
        return new FFMetadataBuilder();
    }

    public FFMetadataBuilder WithTag(string key, string value)
    {
        _tags[key] = value;
        return this;
    }

    public FFMetadataBuilder WithTag(string key, params string[] values)
    {
        return WithTag(key, string.Join("; ", values));
    }

    public FFMetadataBuilder WithTag(string key, IEnumerable<string> values)
    {
        return WithTag(key, string.Join("; ", values));
    }

    public FFMetadataBuilder WithMajorBrand(string value)
    {
        return WithTag("major_brand", value);
    }

    public FFMetadataBuilder WithMinorVersion(string value)
    {
        return WithTag("minor_version", value);
    }

    public FFMetadataBuilder WithCompatibleBrands(string value)
    {
        return WithTag("compatible_brands", value);
    }

    public FFMetadataBuilder WithCopyright(string value)
    {
        return WithTag("copyright", value);
    }

    public FFMetadataBuilder WithTitle(string value)
    {
        return WithTag("title", value);
    }

    public FFMetadataBuilder WithArtists(params string[] values)
    {
        return WithTag("artist", values);
    }

    public FFMetadataBuilder WithArtists(IEnumerable<string> values)
    {
        return WithTag("artist", values);
    }

    public FFMetadataBuilder WithComposers(params string[] values)
    {
        return WithTag("composer", values);
    }

    public FFMetadataBuilder WithComposers(IEnumerable<string> values)
    {
        return WithTag("composer", values);
    }

    public FFMetadataBuilder WithAlbumArtists(params string[] values)
    {
        return WithTag("album_artist", values);
    }

    public FFMetadataBuilder WithAlbumArtists(IEnumerable<string> values)
    {
        return WithTag("album_artist", values);
    }

    public FFMetadataBuilder WithAlbum(string value)
    {
        return WithTag("album", value);
    }

    public FFMetadataBuilder WithDate(string value)
    {
        return WithTag("date", value);
    }

    public FFMetadataBuilder WithGenres(params string[] values)
    {
        return WithTag("genre", values);
    }

    public FFMetadataBuilder WithGenres(IEnumerable<string> values)
    {
        return WithTag("genre", values);
    }

    public FFMetadataBuilder WithComments(params string[] values)
    {
        return WithTag("comment", values);
    }

    public FFMetadataBuilder WithComments(IEnumerable<string> values)
    {
        return WithTag("comment", values);
    }

    public FFMetadataBuilder WithEncoder(string value)
    {
        return WithTag("encoder", value);
    }

    public FFMetadataBuilder WithChapter(ChapterData chapter)
    {
        _chapters.Add(chapter);
        return this;
    }

    public FFMetadataBuilder WithChapter(string title, TimeSpan start, TimeSpan end)
    {
        return WithChapter(new ChapterData(title, start, end));
    }

    public FFMetadataBuilder WithChapter(string title, TimeSpan duration)
    {
        var start = _chapters.LastOrDefault()?.End ?? TimeSpan.Zero;
        return WithChapter(title, start, start + duration);
    }

    public FFMetadataBuilder WithChapter(string title, long durationMs)
    {
        return WithChapter(title, TimeSpan.FromMilliseconds(durationMs));
    }

    public FFMetadataBuilder WithChapter(string title, double durationSeconds)
    {
        return WithChapter(title, TimeSpan.FromMilliseconds(Convert.ToInt64(durationSeconds * 1000)));
    }

    public FFMetadataBuilder WithChapters<T>(IEnumerable<T> items, Func<T, (TimeSpan duration, string title)> chapterSelector)
    {
        foreach (var item in items)
        {
            var (duration, title) = chapterSelector(item);
            WithChapter(title, duration);
        }

        return this;
    }

    public string GetMetadataFileContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine(";FFMETADATA1");

        foreach (var tag in _tags)
        {
            sb.AppendLine($"{tag.Key}={tag.Value}");
        }

        for (var i = 0; i < _chapters.Count; i++)
        {
            var chapter = _chapters[i];
            var title = string.IsNullOrEmpty(chapter.Title) ? $"Chapter {i + 1}" : chapter.Title;

            sb.AppendLine("[CHAPTER]");
            sb.AppendLine("TIMEBASE=1/1000");
            sb.AppendLine($"START={(long)chapter.Start.TotalMilliseconds}");
            sb.AppendLine($"END={(long)chapter.End.TotalMilliseconds}");
            sb.AppendLine($"title={title}");
        }

        return sb.ToString();
    }
}
