using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FFMpegCore
{
    public class FFMpegOptions
    {
        private static readonly string ConfigFile = Path.Combine(".", "ffmpeg.config.json");
        private static readonly string DefaultRoot = Path.Combine(".", "FFMPEG", "bin");
        private static readonly string DefaultTemp = Path.Combine(Path.GetTempPath(), "FFMpegCore");
        private static readonly Dictionary<string, string> DefaultExtensionsOverrides = new Dictionary<string, string>
        {
            { "mpegts", ".ts" },
        };

        public static FFMpegOptions Options { get; private set; } = new FFMpegOptions();

        public static void Configure(Action<FFMpegOptions> optionsAction)
        {
            optionsAction?.Invoke(Options);
        }

        public static void Configure(FFMpegOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        static FFMpegOptions()
        {
            if (File.Exists(ConfigFile))
            {
                Options = JsonSerializer.Deserialize<FFMpegOptions>(File.ReadAllText(ConfigFile));
                foreach (var kv in DefaultExtensionsOverrides)
                    if (!Options.ExtensionOverrides.ContainsKey(kv.Key)) Options.ExtensionOverrides.Add(kv.Key, kv.Value);
            }
        }

        public string RootDirectory { get; set; } = DefaultRoot;
        public string TempDirectory { get; set; } = DefaultTemp;

        public string FFmpegBinary() => FFBinary("FFMpeg");

        public string FFProbeBinary() => FFBinary("FFProbe");

        public Dictionary<string, string> ExtensionOverrides { get; private set; } = new Dictionary<string, string>();

        public bool UseCache { get; set; } = true;

        private static string FFBinary(string name)
        {
            var ffName = name.ToLowerInvariant();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ffName += ".exe";

            var target = Environment.Is64BitProcess ? "x64" : "x86";
            if (Directory.Exists(Path.Combine(Options.RootDirectory, target)))
                ffName = Path.Combine(target, ffName);

            return Path.Combine(Options.RootDirectory, ffName);
        }
    }
}
