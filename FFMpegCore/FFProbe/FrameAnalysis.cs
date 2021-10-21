using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class FFProbeFrameAnalysis
    {
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; }
        
        [JsonPropertyName("stream_index")]
        public int StreamIndex { get; set; }
        
        [JsonPropertyName("key_frame")]
        public int KeyFrame { get; set; }
        
        [JsonPropertyName("pkt_pts")]
        public long PacketPts { get; set; }
        
        [JsonPropertyName("pkt_pts_time")]
        public string PacketPtsTime { get; set; }
        
        [JsonPropertyName("pkt_dts")]
        public long PacketDts { get; set; }
        
        [JsonPropertyName("pkt_dts_time")]
        public string PacketDtsTime { get; set; }
        
        [JsonPropertyName("best_effort_timestamp")]
        public long BestEffortTimestamp { get; set; }
        
        [JsonPropertyName("best_effort_timestamp_time")]
        public string BestEffortTimestampTime { get; set; }
        
        [JsonPropertyName("pkt_duration")]
        public int PacketDuration { get; set; }
        
        [JsonPropertyName("pkt_duration_time")]
        public string PacketDurationTime { get; set; }
        
        [JsonPropertyName("pkt_pos")]
        public long PacketPos { get; set; }
        
        [JsonPropertyName("pkt_size")]
        public int PacketSize { get; set; }
        
        [JsonPropertyName("width")]
        public long Width { get; set; }
        
        [JsonPropertyName("height")]
        public long Height { get; set; }
        
        [JsonPropertyName("pix_fmt")]
        public string PixelFormat { get; set; }
        
        [JsonPropertyName("pict_type")]
        public string PictureType { get; set; }
        
        [JsonPropertyName("coded_picture_number")]
        public long CodedPictureNumber { get; set; }
        
        [JsonPropertyName("display_picture_number")]
        public long DisplayPictureNumber { get; set; }
        
        [JsonPropertyName("interlaced_frame")]
        public int InterlacedFrame { get; set; }
        
        [JsonPropertyName("top_field_first")]
        public int TopFieldFirst { get; set; }
        
        [JsonPropertyName("repeat_pict")]
        public int RepeatPicture { get; set; }
        
        [JsonPropertyName("chroma_location")]
        public string ChromaLocation { get; set; }
    }

    public class FFProbeFrames
    {
        [JsonPropertyName("frames")]
        public List<FFProbeFrameAnalysis> Frames { get; set; }
    }
}
