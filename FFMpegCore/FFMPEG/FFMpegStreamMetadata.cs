using Newtonsoft.Json;
using System.Collections.Generic;

namespace FFMpegCore.FFMPEG
{
    internal class Stream
    {
        [JsonProperty("index")]
        internal int Index { get; set; }

        [JsonProperty("codec_name")]
        internal string CodecName { get; set; }

        [JsonProperty("bit_rate")]
        internal string BitRate { get; set; }

        [JsonProperty("profile")]
        internal string Profile { get; set; }

        [JsonProperty("codec_type")]
        internal string CodecType { get; set; }

        [JsonProperty("width")]
        internal int Width { get; set; }

        [JsonProperty("height")]
        internal int Height { get; set; }

        [JsonProperty("duration")]
        internal string Duration { get; set; }

        [JsonProperty("r_frame_rate")]
        internal string FrameRate { get; set; }

        [JsonProperty("tags")]
        internal Tags Tags { get; set; }
    }

    internal class Tags
    {
        [JsonProperty("DURATION")]
        internal string Duration { get; set; }
    }

    internal class FFMpegStreamMetadata
    {
        [JsonProperty("streams")]
        internal List<Stream> Streams { get; set; }
    }
}
