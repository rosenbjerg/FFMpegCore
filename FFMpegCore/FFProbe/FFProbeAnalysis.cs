using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FFMpegCore;

public class FFProbeAnalysis
{
    [JsonConstructor]
    internal FFProbeAnalysis() { }

    [JsonInclude]
    [JsonPropertyName("streams")]
    public IReadOnlyList<FFProbeStream> Streams { get; internal set; } = [];

    [JsonInclude]
    [JsonPropertyName("format")]
    public FFProbeFormat Format { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("chapters")]
    public IReadOnlyList<FFProbeChapter> Chapters { get; internal set; } = [];

    [JsonExtensionData][JsonInclude] internal Dictionary<string, JsonElement>? ExtensionDataStore { get; set; }

    [JsonIgnore] public IReadOnlyDictionary<string, JsonElement> ExtensionData => ExtensionDataStore ?? new Dictionary<string, JsonElement>();
}

public class FFProbeStream : ITagsContainer
{
    [JsonConstructor]
    internal FFProbeStream() { }

    [JsonInclude]
    [JsonPropertyName("index")]
    public int Index { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("avg_frame_rate")]
    public string AvgFrameRate { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("bits_per_raw_sample")]
    public string BitsPerRawSample { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("bits_per_sample")]
    public int BitsPerSample { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("bit_rate")]
    public string BitRate { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("channels")]
    public int? Channels { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("channel_layout")]
    public string ChannelLayout { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("codec_type")]
    public string CodecType { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("codec_name")]
    public string CodecName { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("codec_long_name")]
    public string CodecLongName { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("codec_tag")]
    public string CodecTag { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("codec_tag_string")]
    public string CodecTagString { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("display_aspect_ratio")]
    public string DisplayAspectRatio { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("sample_aspect_ratio")]
    public string SampleAspectRatio { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("start_time")]
    public string StartTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("duration")]
    public string Duration { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("profile")]
    public string Profile { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("width")]
    public int? Width { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("height")]
    public int? Height { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("r_frame_rate")]
    public string FrameRate { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("pix_fmt")]
    public string PixelFormat { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("level")]
    public int Level { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("field_order")]
    public string FieldOrder { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("sample_rate")]
    public string SampleRate { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("side_data_list")]
    public IReadOnlyList<IReadOnlyDictionary<string, JsonValue>>? SideData { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("color_range")]
    public string ColorRange { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("color_space")]
    public string ColorSpace { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("color_transfer")]
    public string ColorTransfer { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("color_primaries")]
    public string ColorPrimaries { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("disposition")]
    public IReadOnlyDictionary<string, int>? Disposition { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("tags")]
    public IReadOnlyDictionary<string, string>? Tags { get; internal set; }

    [JsonExtensionData][JsonInclude] internal Dictionary<string, JsonElement>? ExtensionDataStore { get; set; }

    [JsonIgnore] public IReadOnlyDictionary<string, JsonElement> ExtensionData => ExtensionDataStore ?? new Dictionary<string, JsonElement>();
}

public class FFProbeFormat : ITagsContainer
{
    [JsonConstructor]
    internal FFProbeFormat() { }

    [JsonInclude]
    [JsonPropertyName("filename")]
    public string Filename { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("nb_streams")]
    public int NbStreams { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("nb_programs")]
    public int NbPrograms { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("format_name")]
    public string FormatName { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("format_long_name")]
    public string FormatLongName { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("start_time")]
    public string StartTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("duration")]
    public string Duration { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("size")]
    public string Size { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("bit_rate")]
    public string? BitRate { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("probe_score")]
    public int ProbeScore { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("tags")]
    public IReadOnlyDictionary<string, string>? Tags { get; internal set; }

    [JsonExtensionData][JsonInclude] internal Dictionary<string, JsonElement>? ExtensionDataStore { get; set; }

    [JsonIgnore] public IReadOnlyDictionary<string, JsonElement> ExtensionData => ExtensionDataStore ?? new Dictionary<string, JsonElement>();
}

public class FFProbeChapter : ITagsContainer
{
    [JsonConstructor]
    internal FFProbeChapter() { }

    [JsonInclude]
    [JsonPropertyName("id")]
    public long Id { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("time_base")]
    public string TimeBase { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("start")]
    public long Start { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("start_time")]
    public string StartTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("end")]
    public long End { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("end_time")]
    public string EndTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("tags")]
    public IReadOnlyDictionary<string, string>? Tags { get; internal set; }

    [JsonExtensionData][JsonInclude] internal Dictionary<string, JsonElement>? ExtensionDataStore { get; set; }

    [JsonIgnore] public IReadOnlyDictionary<string, JsonElement> ExtensionData => ExtensionDataStore ?? new Dictionary<string, JsonElement>();
}

public interface ITagsContainer
{
    IReadOnlyDictionary<string, string>? Tags { get; }
}

public static class TagExtensions
{
    private static string? TryGetTagValue(ITagsContainer tagsContainer, string key)
    {
        return tagsContainer.Tags?
            .FirstOrDefault(tag => string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase))
            .Value;
    }

    public static string? GetCreationTime(this ITagsContainer tagsContainer)
    {
        return TryGetTagValue(tagsContainer, "creation_time");
    }

    internal static string? GetLanguage(this ITagsContainer tagsContainer)
    {
        return TryGetTagValue(tagsContainer, "language");
    }

    internal static string? GetRotate(this ITagsContainer tagsContainer)
    {
        return TryGetTagValue(tagsContainer, "rotate");
    }

    internal static string? GetDuration(this ITagsContainer tagsContainer)
    {
        return TryGetTagValue(tagsContainer, "duration");
    }
}
