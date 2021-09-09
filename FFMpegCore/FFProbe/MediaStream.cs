using FFMpegCore.Enums;

using System;
using System.Collections.Generic;

namespace FFMpegCore
{
    public class MediaStream
    {
        public int Index { get; internal set; }
        public string CodecName { get; internal set; } = null!;
        public string CodecLongName { get; internal set; } = null!;
        public string CodecTagString { get; set; } = null!;
        public string CodecTag { get; set; } = null!;
        public int BitRate { get; internal set; }
        public TimeSpan Duration { get; internal set; }
        public string? Language { get; internal set; }
        public Dictionary<string, bool>? Disposition { get; internal set; }
        public Dictionary<string, string>? Tags { get; internal set; }
        
        public Codec GetCodecInfo() => FFMpeg.GetCodec(CodecName);
    }
}