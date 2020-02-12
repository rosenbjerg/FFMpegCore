using FFMpegCore.FFMPEG.Exceptions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FFMpegCore.FFMPEG
{
    public class FFMpegOptions
    {
        private static readonly string ConfigFile = Path.Combine(".", "ffmpeg.config.json");
        private static readonly string DefaultRoot = Path.Combine(".", "FFMPEG", "bin");

        public static FFMpegOptions Options { get; private set; } = new FFMpegOptions();

        public static void Configure(FFMpegOptions options)
        {
            Options = options;
        }

        static FFMpegOptions()
        {
            if (File.Exists(ConfigFile))
            {
                Options = JsonConvert.DeserializeObject<FFMpegOptions>(File.ReadAllText(ConfigFile));
            }
        }

        public string RootDirectory { get; set; } = DefaultRoot;

        public string FFmpegBinary => FFBinary("FFMpeg");

        public string FFProbeBinary => FFBinary("FFProbe");

        private static string FFBinary(string name)
        {
            var ffName = name.ToLowerInvariant();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ffName += ".exe";

            var target = Environment.Is64BitProcess ? "x64" : "x86";
            if (Directory.Exists(Path.Combine(Options.RootDirectory, target)))
            {
                ffName = Path.Combine(target, ffName);
            }

            var path = Path.Combine(Options.RootDirectory, ffName);

            if (!File.Exists(path))
                throw new FFMpegException(FFMpegExceptionType.Dependency,
                    $"{name} cannot be found @ {path}");

            return path;
        }
    }
}
