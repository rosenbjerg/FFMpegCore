using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class FpsModeArgument : IArgument
{
    public readonly FpsMode Mode;

    public FpsModeArgument(FpsMode mode)
    {
        Mode = mode;
    }

    public string Text => $"-fps_mode {Mode}";
}
