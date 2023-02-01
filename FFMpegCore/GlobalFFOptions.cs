using System.Runtime.InteropServices;
using System.Text.Json;

namespace FFMpegCore
{
    public static class GlobalFFOptions
    {
        private const string ConfigFile = "ffmpeg.config.json";
        private static FFOptions? _current;

        public static FFOptions Current => _current ??= LoadFFOptions();

        public static void Configure(Action<FFOptions> optionsAction) => optionsAction.Invoke(Current);

        public static void Configure(FFOptions ffOptions)
        {
            _current = ffOptions ?? throw new ArgumentNullException(nameof(ffOptions));
        }

        public static string GetFFMpegBinaryPath(FFOptions? ffOptions = null) => GetFFBinaryPath("FFMpeg", ffOptions ?? Current);

        public static string GetFFProbeBinaryPath(FFOptions? ffOptions = null) => GetFFBinaryPath("FFProbe", ffOptions ?? Current);

        private static string GetFFBinaryPath(string name, FFOptions ffOptions)
        {
            var ffName = name.ToLowerInvariant();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ffName += ".exe";
            }

            var target = Environment.Is64BitProcess ? "x64" : "x86";
            if (Directory.Exists(Path.Combine(ffOptions.BinaryFolder, target)))
            {
                ffName = Path.Combine(target, ffName);
            }

            return Path.Combine(ffOptions.BinaryFolder, ffName);
        }

        private static FFOptions LoadFFOptions()
        {
            return File.Exists(ConfigFile)
                ? JsonSerializer.Deserialize<FFOptions>(File.ReadAllText(ConfigFile))!
                : new FFOptions();
        }
    }
}
