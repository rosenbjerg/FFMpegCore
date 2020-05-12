using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FFMpegCore
{
    public class FFProbeAnalysis
    {
        [JsonPropertyName("streams")]
        public List<Stream> Streams { get; set; } = null!;
    }
}
