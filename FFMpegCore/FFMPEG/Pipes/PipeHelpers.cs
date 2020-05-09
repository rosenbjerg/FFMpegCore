using System;
using System.Runtime.InteropServices;

namespace FFMpegCore.FFMPEG.Pipes
{
    static class PipeHelpers
    {
        public static string GetUnqiuePipeName() => "FFMpegCore_Pipe_" + Guid.NewGuid();

        public static string GetPipePath(string pipeName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $@"\\.\pipe\{pipeName}";
            else
                return $"/tmp/CoreFxPipe_{pipeName}"; // dotnet uses unix sockets on unix, for more see https://github.com/dotnet/runtime/issues/24390
        }
    }
}
