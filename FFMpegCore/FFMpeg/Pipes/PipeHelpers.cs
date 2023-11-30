using System.Runtime.InteropServices;

namespace FFMpegCore.Pipes
{
    internal static class PipeHelpers
    {
        public static string GetUnqiuePipeName() => $"FFMpegCore_{Guid.NewGuid().ToString("N").Substring(16, 16)}";

        public static string GetPipePath(string pipeName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $@"\\.\pipe\{pipeName}";
            }

            return $"unix:{Path.Combine(Path.GetTempPath(), $"CoreFxPipe_{pipeName}")}";
        }
    }
}
