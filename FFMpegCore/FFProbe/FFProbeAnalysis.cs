using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class FFProbeAnalysis
    {
        [JsonPropertyName("streams")]
        public List<FFProbeStream> Streams { get; set; } = null!;
        
        [JsonPropertyName("format")]
        public Format Format { get; set; } = null!;
        
        [JsonIgnore]
        public IReadOnlyList<string> ErrorData { get; set; }
    }
    
    public class FFProbeStream : ITagsContainer, IDispositionContainer
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }
        
        [JsonPropertyName("avg_frame_rate")]
        public string AvgFrameRate { get; set; } = null!;
        
        [JsonPropertyName("bits_per_raw_sample")]
        public string BitsPerRawSample { get; set; } = null!;
        
        [JsonPropertyName("bit_rate")]
        public string BitRate { get; set; } = null!;
        
        [JsonPropertyName("channels")]
        public int? Channels { get; set; }
        
        [JsonPropertyName("channel_layout")]
        public string ChannelLayout { get; set; } = null!;

        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; } = null!;
        
        [JsonPropertyName("codec_name")]
        public string CodecName { get; set; } = null!;
        
        [JsonPropertyName("codec_long_name")]
        public string CodecLongName { get; set; } = null!;

        [JsonPropertyName("codec_tag")]
        public string CodecTag { get; set; } = null!;

        [JsonPropertyName("codec_tag_string")]
        public string CodecTagString { get; set; } = null!;

        [JsonPropertyName("display_aspect_ratio")]
        public string DisplayAspectRatio { get; set; } = null!;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = null!;

        [JsonPropertyName("profile")]
        public string Profile { get; set; } = null!;

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("r_frame_rate")]
        public string FrameRate { get; set; } = null!;
        
        [JsonPropertyName("pix_fmt")]
        public string PixelFormat { get; set; } = null!;
        
        [JsonPropertyName("sample_rate")]
        public string SampleRate { get; set; } = null!;

        [JsonPropertyName("disposition")]
        public Dictionary<string, int> Disposition { get; set; } = null!;

        [JsonPropertyName("tags")]
        public Dictionary<string, string> Tags { get; set; } = null!;
    }

    public class Format : ITagsContainer
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; } = null!;

        [JsonPropertyName("nb_streams")]
        public int NbStreams { get; set; }

        [JsonPropertyName("nb_programs")]
        public int NbPrograms { get; set; }

        [JsonPropertyName("format_name")]
        public string FormatName { get; set; } = null!;

        [JsonPropertyName("format_long_name")]
        public string FormatLongName { get; set; } = null!;

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } = null!;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = null!;

        [JsonPropertyName("size")]
        public string Size { get; set; } = null!;

        [JsonPropertyName("bit_rate")]
        public string BitRate { get; set; } = null!;

        [JsonPropertyName("probe_score")]
        public int ProbeScore { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string> Tags { get; set; } = null!;
    }

    public interface IDispositionContainer
    {
        Dictionary<string, int> Disposition { get; set; }
    }

    public interface ITagsContainer
    {
        Dictionary<string, string> Tags { get; set; }
    }

    public static class TagExtensions
    {
        private static string? TryGetTagValue(ITagsContainer tagsContainer, string key)
        {
            if (tagsContainer.Tags != null && tagsContainer.Tags.TryGetValue(key, out var tagValue))
                return tagValue;
            return null;
        }
        
        public static string? GetLanguage(this ITagsContainer tagsContainer) => TryGetTagValue(tagsContainer, "language");
        public static string? GetCreationTime(this ITagsContainer tagsContainer) => TryGetTagValue(tagsContainer, "creation_time ");
        public static string? GetRotate(this ITagsContainer tagsContainer) => TryGetTagValue(tagsContainer, "rotate");
        public static string? GetDuration(this ITagsContainer tagsContainer) => TryGetTagValue(tagsContainer, "duration");
    }

    public static class DispositionExtensions
    {
        private static int? TryGetDispositionValue(IDispositionContainer dispositionContainer, string key)
        {
            if (dispositionContainer.Disposition != null && dispositionContainer.Disposition.TryGetValue(key, out var dispositionValue))
                return dispositionValue;
            return null;
        }

        public static int? GetDefault(this IDispositionContainer tagsContainer) => TryGetDispositionValue(tagsContainer, "default");
        public static int? GetForced(this IDispositionContainer tagsContainer) => TryGetDispositionValue(tagsContainer, "forced");
    }
}
