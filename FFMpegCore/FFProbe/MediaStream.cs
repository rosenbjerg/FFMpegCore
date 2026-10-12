using System.Text.Json.Nodes;
using FFMpegCore.Enums;

namespace FFMpegCore;

public abstract class MediaStream : ITagsContainer
{
    internal MediaStream() { }

    public int Index { get; internal set; }
    public string CodecName { get; internal set; } = null!;
    public string CodecLongName { get; internal set; } = null!;
    public string CodecTagString { get; internal set; } = null!;
    public string CodecTag { get; internal set; } = null!;
    public long BitRate { get; internal set; }
    public TimeSpan StartTime { get; internal set; }
    public TimeSpan Duration { get; internal set; }
    public string? Language { get; internal set; }
    public IReadOnlyDictionary<string, bool>? Disposition { get; internal set; }
    public int? BitDepth { get; internal set; }
    public IReadOnlyDictionary<string, string>? Tags { get; internal set; }
    public IReadOnlyList<IReadOnlyDictionary<string, JsonValue>>? SideData { get; internal set; }

    public Codec GetCodecInfo(FFOptions? ffOptions = null)
    {
        return FFMpeg.GetCodec(CodecName, ffOptions);
    }
}
