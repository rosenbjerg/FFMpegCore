namespace FFMpegCore.Extensions.Downloader.Enums;

[Flags]
public enum FFMpegBinaries : ushort
{
    FFMpeg = 1,
    FFProbe = 2,
    FFPlay = 4
}
