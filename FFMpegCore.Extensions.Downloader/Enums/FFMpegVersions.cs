using System.ComponentModel;

namespace FFMpegCore.Extensions.Downloader.Enums;

public enum FFMpegVersions : ushort
{
    [Description("https://ffbinaries.com/api/v1/version/latest")]
    Latest,
    [Description("https://ffbinaries.com/api/v1/version/6.1")]
    V6_1,
    [Description("https://ffbinaries.com/api/v1/version/5.1")]
    V5_1,
    [Description("https://ffbinaries.com/api/v1/version/4.4.1")]
    V4_4_1,
    [Description("https://ffbinaries.com/api/v1/version/4.2.1")]
    V4_2_1,
    [Description("https://ffbinaries.com/api/v1/version/4.2")]
    V4_2,
    [Description("https://ffbinaries.com/api/v1/version/4.1")]
    V4_1,
    [Description("https://ffbinaries.com/api/v1/version/4.0")]
    V4_0,
    [Description("https://ffbinaries.com/api/v1/version/3.4")]
    V3_4,
    [Description("https://ffbinaries.com/api/v1/version/3.3")]
    V3_3,
    [Description("https://ffbinaries.com/api/v1/version/3.2")]
    V3_2
}
