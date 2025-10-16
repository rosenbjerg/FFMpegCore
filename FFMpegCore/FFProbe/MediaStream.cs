﻿using FFMpegCore.Enums;

namespace FFMpegCore;

public abstract class MediaStream : ITagsContainer
{
    public int Index { get; set; }
    public string CodecName { get; set; } = null!;
    public string CodecLongName { get; set; } = null!;
    public string CodecTagString { get; set; } = null!;
    public string CodecTag { get; set; } = null!;
    public long BitRate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string? Language { get; set; }
    public Dictionary<string, bool>? Disposition { get; set; }
    public int? BitDepth { get; set; }
    public Dictionary<string, string>? Tags { get; set; }

    public Codec GetCodecInfo()
    {
        return FFMpeg.GetCodec(CodecName);
    }
}
