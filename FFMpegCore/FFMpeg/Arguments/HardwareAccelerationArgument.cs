using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class HardwareAccelerationArgument : IArgument
{
    public readonly HardwareAccelerationDevice Device;

    public HardwareAccelerationArgument(HardwareAccelerationDevice device)
    {
        Device = device;
    }

    public string Text => $"-hwaccel {Device}";
}
