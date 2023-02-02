using FFMpegCore.Enums;

namespace FFMpegCore.Arguments
{
    public class HardwareAccelerationArgument : IArgument
    {
        public HardwareAccelerationDevice HardwareAccelerationDevice { get; }

        public HardwareAccelerationArgument(HardwareAccelerationDevice hardwareAccelerationDevice)
        {
            HardwareAccelerationDevice = hardwareAccelerationDevice;
        }

        public string Text => $"-hwaccel {HardwareAccelerationDevice.ToString().ToLower()}";
    }
}
