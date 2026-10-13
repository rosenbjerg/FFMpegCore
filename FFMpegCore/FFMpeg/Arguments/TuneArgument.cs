using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class TuneArgument : IArgument
{
    public readonly EncoderTune Tune;

    public TuneArgument(EncoderTune tune)
    {
        Tune = tune;
    }

    public string Text => $"-tune {Tune}";
}
