using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FFMpegCore.Pipes
{
    static class PipeHelpers
    {
        static readonly string PipePrefix = Path.Combine(Path.GetTempPath(), "CoreFxPipe_");
        
        public static string GetUnqiuePipeName() => $"FFMpegCore_{Guid.NewGuid("N").Substring(0, 5)}";

        public static string GetPipePath(string pipeName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $@"\\.\pipe\{pipeName}";
            else
                return $"unix:{PipePrefix}{pipeName}"; // dotnet uses unix sockets on unix, for more see https://github.com/dotnet/runtime/issues/24390
        }
    }
}
