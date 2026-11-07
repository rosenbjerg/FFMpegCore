using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class SpeedPresetArgument : IArgument
{
    public readonly EncoderPreset Preset;

    public SpeedPresetArgument(EncoderPreset preset)
    {
        Preset = preset;
    }

    public string Text => $"-preset {Preset}";
}
