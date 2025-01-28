using System.Text.Json.Serialization;

namespace FFMpegCore.Extensions.Downloader.Models;

internal record DownloadInfo
{
    [JsonPropertyName("ffmpeg")] 
    public string? FFMpeg { get; set; }

    [JsonPropertyName("ffprobe")] 
    public string? FFProbe { get; set; }

    [JsonPropertyName("ffplay")] 
    public string? FFPlay { get; set; }
}
