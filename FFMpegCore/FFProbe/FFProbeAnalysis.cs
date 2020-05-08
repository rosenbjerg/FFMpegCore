using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFMpegCore.FFMPEG
{
    public class FFProbeAnalysis
    {
        [JsonPropertyName("streams")]
        public List<Stream> Streams { get; set; }
    }
    
    public class Stream
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }
        
        [JsonPropertyName("avg_frame_rate")]
        public string AvgFrameRate { get; set; }
        
        [JsonPropertyName("bits_per_raw_sample")]
        public string BitsPerRawSample { get; set; }
        
        [JsonPropertyName("bit_rate")]
        public string BitRate { get; set; }
        
        [JsonPropertyName("channels")]
        public int? Channels { get; set; }
        
        [JsonPropertyName("channel_layout")]
        public string ChannelLayout { get; set; }

        [JsonPropertyName("codec_type")]
        public string CodecType { get; set; }
        
        [JsonPropertyName("codec_name")]
        public string CodecName { get; set; }
        
        [JsonPropertyName("codec_long_name")]
        public string CodecLongName { get; set; }
        
        [JsonPropertyName("codec_tag_string")]
        public string CodecTagString { get; set; }

        [JsonPropertyName("display_aspect_ratio")]
        public string DisplayAspectRatio { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("profile")]
        public string Profile { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("r_frame_rate")]
        public string FrameRate { get; set; }
        
        [JsonPropertyName("pix_fmt")]
        public string PixelFormat { get; set; }
        
        [JsonPropertyName("sample_rate")]
        public string SampleRate { get; set; }

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; }
    }

    public class Tags
    {
        [JsonPropertyName("DURATION")]
        public string Duration { get; set; }
    }
}
