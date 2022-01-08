using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class FFProbePacketAnalysis
    {
        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; }

        [JsonPropertyName("stream_index")]
        public int StreamIndex { get; set; }

        [JsonPropertyName("pts")]
        public long Pts { get; set; }
        
        [JsonPropertyName("pts_time")]
        public string PtsTime { get; set; }

        [JsonPropertyName("dts")]
        public long Dts { get; set; }
        
        [JsonPropertyName("dts_time")]
        public string DtsTime { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }
        
        [JsonPropertyName("duration_time")]
        public string DurationTime { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("pos")]
        public long Pos { get; set; }

        [JsonPropertyName("flags")]
        public string Flags { get; set; }
    }

    public class FFProbePackets
    {
        [JsonPropertyName("packets")]
        public List<FFProbePacketAnalysis> Packets { get; set; }
    }
}
