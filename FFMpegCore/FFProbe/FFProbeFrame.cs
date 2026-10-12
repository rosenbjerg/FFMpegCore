using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FFMpegCore;

public class FFProbeFrame
{
    [JsonConstructor]
    internal FFProbeFrame() { }

    [JsonInclude]
    [JsonPropertyName("media_type")]
    public string MediaType { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("stream_index")]
    public int StreamIndex { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("key_frame")]
    public int KeyFrame { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pkt_pts")]
    public long PacketPts { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pkt_pts_time")]
    public string PacketPtsTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("pkt_dts")]
    public long PacketDts { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pkt_dts_time")]
    public string PacketDtsTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("best_effort_timestamp")]
    public long BestEffortTimestamp { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("best_effort_timestamp_time")]
    public string BestEffortTimestampTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("pkt_duration")]
    public int PacketDuration { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pkt_duration_time")]
    public string PacketDurationTime { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("pkt_pos")]
    public long PacketPos { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pkt_size")]
    public int PacketSize { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("width")]
    public long Width { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("height")]
    public long Height { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("pix_fmt")]
    public string PixelFormat { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("pict_type")]
    public string PictureType { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("coded_picture_number")]
    public long CodedPictureNumber { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("display_picture_number")]
    public long DisplayPictureNumber { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("interlaced_frame")]
    public int InterlacedFrame { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("top_field_first")]
    public int TopFieldFirst { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("repeat_pict")]
    public int RepeatPicture { get; internal set; }

    [JsonInclude]
    [JsonPropertyName("chroma_location")]
    public string ChromaLocation { get; internal set; } = null!;

    [JsonInclude]
    [JsonPropertyName("side_data_list")]
    public IReadOnlyList<IReadOnlyDictionary<string, JsonNode>>? SideData { get; internal set; }

    [JsonExtensionData][JsonInclude] internal Dictionary<string, JsonElement>? ExtensionDataStore { get; set; }

    [JsonIgnore] public IReadOnlyDictionary<string, JsonElement> ExtensionData => ExtensionDataStore ?? new Dictionary<string, JsonElement>();
}

public class FFProbeFrames
{
    [JsonConstructor]
    internal FFProbeFrames() { }

    [JsonInclude]
    [JsonPropertyName("frames")]
    public IReadOnlyList<FFProbeFrame> Frames { get; internal set; } = [];
}
