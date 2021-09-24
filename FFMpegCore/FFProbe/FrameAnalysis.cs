using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class Frame
    {
        public string media_type { get; set; }
        public int stream_index { get; set; }
        public int key_frame { get; set; }
        public long pkt_pts { get; set; }
        public string pkt_pts_time { get; set; }
        public long pkt_dts { get; set; }
        public string pkt_dts_time { get; set; }
        public long best_effort_timestamp { get; set; }
        public string best_effort_timestamp_time { get; set; }
        public int pkt_duration { get; set; }
        public string pkt_duration_time { get; set; }
        public long pkt_pos { get; set; }
        public int pkt_size { get; set; }
        public long width { get; set; }
        public long height { get; set; }
        public string pix_fmt { get; set; }
        public string pict_type { get; set; }
        public long coded_picture_number { get; set; }
        public long display_picture_number { get; set; }
        public int interlaced_frame { get; set; }
        public int top_field_first { get; set; }
        public int repeat_pict { get; set; }
        public string chroma_location { get; set; }
    }

    public class FFProbeFrames
    {
        public List<Frame> frames { get; set; }
    }
}
