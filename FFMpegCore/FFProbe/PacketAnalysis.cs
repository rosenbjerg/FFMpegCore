using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class FFProbePacketAnalysis
    {
        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; } = null!;

        [JsonPropertyName("stream_index")]
        public int StreamIndex { get; set; }

        [JsonPropertyName("pts")]
        public long Pts { get; set; }
        
        [JsonPropertyName("pts_time")]
        public string PtsTime { get; set; } = null!;

        [JsonPropertyName("dts")]
        public long Dts { get; set; }
        
        [JsonPropertyName("dts_time")]
        public string DtsTime { get; set; } = null!;

        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        
        [JsonPropertyName("duration_time")]
        public string DurationTime { get; set; } = null!;

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("pos")]
        public long Pos { get; set; }

        [JsonPropertyName("flags")]
        public string Flags { get; set; } = null!;
    }

    public class FFProbePackets
    {
        [JsonPropertyName("packets")]
        public List<FFProbePacketAnalysis> Packets { get; set; } = null!;
    }
}
