using System.Text;

namespace FFMpegCore;

public class FFMetadataBuilder
{
    private Dictionary<string, string> Tags { get; } = new();
    private List<FFMetadataChapter> Chapters { get; } = [];

    public static FFMetadataBuilder Empty()
    {
        return new FFMetadataBuilder();
    }

    public FFMetadataBuilder WithTag(string key, string value)
    {
        Tags.Add(key, value);
        return this;
    }

    public FFMetadataBuilder WithChapter(string title, long durationMs)
    {
        Chapters.Add(new FFMetadataChapter(title, durationMs));
        return this;
    }

    public FFMetadataBuilder WithChapter(string title, double durationSeconds)
    {
        Chapters.Add(new FFMetadataChapter(title, Convert.ToInt64(durationSeconds * 1000)));
        return this;
    }

    public string GetMetadataFileContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine(";FFMETADATA1");

        foreach (var tag in Tags)
        {
            sb.AppendLine($"{tag.Key}={tag.Value}");
        }

        long totalDurationMs = 0;
        foreach (var chapter in Chapters)
        {
            sb.AppendLine("[CHAPTER]");
            sb.AppendLine("TIMEBASE=1/1000");
            sb.AppendLine($"START={totalDurationMs}");
            sb.AppendLine($"END={totalDurationMs + chapter.DurationMs}");
            sb.AppendLine($"title={chapter.Title}");
            totalDurationMs += chapter.DurationMs;
        }

        return sb.ToString();
    }


    private class FFMetadataChapter(string title, long durationMs)
    {
        public string Title { get; } = title;
        public long DurationMs { get; } = durationMs;
    }
}
