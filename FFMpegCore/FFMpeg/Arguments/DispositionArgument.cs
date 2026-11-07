using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class DispositionArgument : IArgument
{
    public readonly StreamDisposition Disposition;
    public readonly int? StreamIndex;
    public readonly StreamType StreamType;

    public DispositionArgument(StreamDisposition disposition, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        Disposition = disposition;
        StreamType = streamType;
        StreamIndex = streamIndex;
    }

    public string Text => $"-disposition{StreamType.Specifier()}{(StreamIndex == null ? "" : $":{StreamIndex}")} {Disposition}";
}
