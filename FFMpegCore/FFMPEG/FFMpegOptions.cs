using FFMpegCore.FFMPEG.Exceptions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FFMpegCore.FFMPEG
{
    public class FFMpegOptions
    {
        private static string _ConfigFile = Path.Combine(".", "ffmpeg.config.json");
        private static string _DefaultRoot = Path.Combine(".", "FFMPEG", "bin");

        public static FFMpegOptions Options { get; private set; } = new FFMpegOptions();

        public static void Configure(FFMpegOptions options)
        {
            Options = options;
        }

        static FFMpegOptions()
        {
            if (File.Exists(_ConfigFile))
            {
                Options = JsonConvert.DeserializeObject<FFMpegOptions>(File.ReadAllText(_ConfigFile));
            }
        }

        public string RootDirectory { get; set; } = _DefaultRoot;

        public string FFmpegBinary
        {
            get
            {
                var target = Environment.Is64BitProcess ? "x64" : "x86";
                var progName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg";

                if (Directory.Exists(Path.Combine(Options.RootDirectory, target)))
                {
                    progName = Path.Combine(target, progName);
                }

                var path = Path.Combine(Options.RootDirectory, progName);

                if (!File.Exists(path))
                    throw new FFMpegException(FFMpegExceptionType.Dependency,
                        $"FFMpeg cannot be found @ {path}");

                return path;
            }
        }

        public string FFProbeBinary
        {
            get
            {
                var target = Environment.Is64BitProcess ? "x64" : "x86";
                var progName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffprobe.exe" : "ffprobe";

                if (Directory.Exists(Path.Combine(Options.RootDirectory, target)))
                {
                    progName = Path.Combine(target, progName);
                }

                var path = Path.Combine(Options.RootDirectory, progName);

                if (!File.Exists(path))
                    throw new FFMpegException(FFMpegExceptionType.Dependency,
                        $"FFProbe cannot be found @ {path}");

                return path;
            }
        }
    }
}
