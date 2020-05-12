using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class Stream
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

        [JsonPropertyName("tags")]
        public Tags Tags { get; set; } = null!;
    }
}