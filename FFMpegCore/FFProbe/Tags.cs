using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class Tags
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; } = null!;
        
        [JsonPropertyName("language")]
        public string Language { get; set; } = null!;
    }
}