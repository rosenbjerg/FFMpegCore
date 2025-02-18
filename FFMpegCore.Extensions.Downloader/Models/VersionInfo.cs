using System.Text.Json.Serialization;

namespace FFMpegCore.Extensions.Downloader.Models;

internal record VersionInfo
{
    [JsonPropertyName("version")] public string? Version { get; set; }

    [JsonPropertyName("permalink")] public string? Permalink { get; set; }

    [JsonPropertyName("bin")] public BinaryInfo? BinaryInfo { get; set; }
}
