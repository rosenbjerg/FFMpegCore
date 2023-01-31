using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FFMpegCore.Pipes
{
    static class PipeHelpers
    {
        public static string GetUnqiuePipeName() => $"FFMpegCore_{Guid.NewGuid().ToString("N").Substring(0, 5)}";

        public static string GetPipePath(string pipeName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $@"\\.\pipe\{pipeName}";
            return $"unix:{Path.Combine(Path.GetTempPath(), $"CoreFxPipe_{pipeName}")}";
        }
    }
}
