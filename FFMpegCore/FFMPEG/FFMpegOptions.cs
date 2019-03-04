using Newtonsoft.Json;
using System.IO;

namespace FFMpegCore.FFMPEG
{
    public class FFMpegOptions
    {
        private static string _ConfigFile = ".\\ffmpeg.config.json";
        private static string _DefaultRoot = ".\\FFMPEG\\bin";

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
    }
}
