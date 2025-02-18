using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using FFMpegCore.Extensions.Downloader.Enums;

namespace FFMpegCore.Extensions.Downloader.Models;

internal record BinaryInfo
{
    [JsonPropertyName("windows-64")] public DownloadInfo? Windows64 { get; set; }

    [JsonPropertyName("windows-32")] public DownloadInfo? Windows32 { get; set; }

    [JsonPropertyName("linux-32")] public DownloadInfo? Linux32 { get; set; }

    [JsonPropertyName("linux-64")] public DownloadInfo? Linux64 { get; set; }

    [JsonPropertyName("linux-armhf")] public DownloadInfo? LinuxArmhf { get; set; }

    [JsonPropertyName("linux-armel")] public DownloadInfo? LinuxArmel { get; set; }

    [JsonPropertyName("linux-arm64")] public DownloadInfo? LinuxArm64 { get; set; }

    [JsonPropertyName("osx-64")] public DownloadInfo? Osx64 { get; set; }

    /// <summary>
    ///     Automatically get the compatible download info for current os and architecture
    /// </summary>
    /// <param name="platformOverride"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public DownloadInfo? GetCompatibleDownloadInfo(SupportedPlatforms? platformOverride = null)
    {
        if (platformOverride is not null)
        {
            return platformOverride switch
            {
                SupportedPlatforms.Windows64 => Windows64,
                SupportedPlatforms.Windows32 => Windows32,
                SupportedPlatforms.Linux64 => Linux64,
                SupportedPlatforms.Linux32 => Linux32,
                SupportedPlatforms.LinuxArmhf => LinuxArmhf,
                SupportedPlatforms.LinuxArmel => LinuxArmel,
                SupportedPlatforms.LinuxArm64 => LinuxArm64,
                SupportedPlatforms.Osx64 => Osx64,
                _ => throw new ArgumentOutOfRangeException(nameof(platformOverride), platformOverride, null)
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.OSArchitecture == Architecture.X64 ? Windows64 : Windows32;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.X86 => Linux32,
                Architecture.X64 => Linux64,
                Architecture.Arm => LinuxArmhf,
                Architecture.Arm64 => LinuxArm64,
                _ => LinuxArmel
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Osx64;
        }

        throw new PlatformNotSupportedException("Unsupported OS or Architecture");
    }
}
