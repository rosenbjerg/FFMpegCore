using System.Runtime.InteropServices;

namespace FFMpegCore.Test.Utilities;

public static class OperatingSystemUtils
{
    public static bool NotWindows()
    {
        return !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}