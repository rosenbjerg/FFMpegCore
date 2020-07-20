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
    }
    
    public class FFProbeStream
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

    public class Tags
    {
        [JsonPropertyName("DURATION")]
        public string Duration { get; set; } = null!;
        
        [JsonPropertyName("language")]
        public string Language { get; set; } = null!;
        
        [JsonPropertyName("encoder")]
        public string Encoder { get; set; } = null!;
    }
    public class Format
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
        public Tags Tags { get; set; } = null!;
    }
}
