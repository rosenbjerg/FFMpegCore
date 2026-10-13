using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFMpegCore;

public class FFProbePacket
{
    [JsonConstructor]
    internal FFProbePacket() { }

    [JsonInclude]
    [JsonPropertyName("codec_type")]
    public string CodecType { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("stream_index")]
    public int StreamIndex { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pts")]
    public long Pts { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pts_time")]
    public string PtsTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("dts")]
    public long Dts { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("dts_time")]
    public string DtsTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("duration")]
    public int Duration { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("duration_time")]
    public string DurationTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("size")]
    public int Size { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pos")]
    public long Pos { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("flags")]
    public string Flags { get; internal set; } = null!;

    [JsonExtensionData][JsonInclude] internal Dictionary<string, JsonElement>? ExtensionDataStore { get; set; }

    [JsonIgnore] public IReadOnlyDictionary<string, JsonElement> ExtensionData => ExtensionDataStore ?? new Dictionary<string, JsonElement>();
}

public class FFProbePackets
{
    [JsonConstructor]
    internal FFProbePackets() { }

    [JsonInclude]
    [JsonPropertyName("packets")]
    public IReadOnlyList<FFProbePacket> Packets { get; internal set; } = [];
}
