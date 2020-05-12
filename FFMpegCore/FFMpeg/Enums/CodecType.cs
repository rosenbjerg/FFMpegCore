using System;

namespace FFMpegCore.Enums
{
    [Flags]
    public enum CodecType
    {
        Unknown = 0,
        Video = 1 << 1,
        Audio = 1 << 2,
        Subtitle = 1 << 3,
        Data = 1 << 4,
    }
}