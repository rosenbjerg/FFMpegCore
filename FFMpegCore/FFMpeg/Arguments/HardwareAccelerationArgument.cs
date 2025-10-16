using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class HardwareAccelerationArgument : IArgument
{
    public HardwareAccelerationArgument(HardwareAccelerationDevice hardwareAccelerationDevice)
    {
        HardwareAccelerationDevice = hardwareAccelerationDevice;
    }

    public HardwareAccelerationDevice HardwareAccelerationDevice { get; }

    public string Text => $"-hwaccel {HardwareAccelerationDevice.ToString().ToLower()}";
}
