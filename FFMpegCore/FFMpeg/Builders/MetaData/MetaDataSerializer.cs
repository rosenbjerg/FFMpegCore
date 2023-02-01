using System.Text;

namespace FFMpegCore.Builders.MetaData
{
    public class MetaDataSerializer
    {
        public static readonly MetaDataSerializer Instance = new();

        public string Serialize(IReadOnlyMetaData metaData)
        {
            var sb = new StringBuilder()
                .AppendLine(";FFMETADATA1");

            foreach (var value in metaData.Entries)
            {
                sb.AppendLine($"{value.Key}={value.Value}");
            }

            var chapterNumber = 0;
            foreach (var chapter in metaData.Chapters ?? Enumerable.Empty<ChapterData>())
            {
                chapterNumber++;
                var title = string.IsNullOrEmpty(chapter.Title) ? $"Chapter {chapterNumber}" : chapter.Title;

                sb
                    .AppendLine("[CHAPTER]")
                    .AppendLine($"TIMEBASE=1/1000")
                    .AppendLine($"START={(int)chapter.Start.TotalMilliseconds}")
                    .AppendLine($"END={(int)chapter.End.TotalMilliseconds}")
                    .AppendLine($"title={title}")
                    ;
            }

            return sb.ToString();
        }
    }
}
