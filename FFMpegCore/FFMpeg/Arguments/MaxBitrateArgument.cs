namespace FFMpegCore.Arguments;

public class MaxBitrateArgument : IArgument
{
    public readonly int KilobitsPerSecond;

    public MaxBitrateArgument(int kilobitsPerSecond)
    {
        KilobitsPerSecond = kilobitsPerSecond;
    }

    public string Text => $"-maxrate {KilobitsPerSecond}k";
}
