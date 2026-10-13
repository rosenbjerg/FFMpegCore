using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class QualityScaleArgument : IArgument
{
    public readonly int Quality;
    public readonly StreamType StreamType;

    public QualityScaleArgument(StreamType streamType, int quality)
    {
        StreamType = streamType;
        Quality = quality;
    }

    public string Text => $"-q{StreamType.Specifier()} {Quality}";
}
