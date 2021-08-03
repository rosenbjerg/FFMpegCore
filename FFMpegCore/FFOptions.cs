using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FFMpegCore
{
    public class FFOptions
    {
        /// <summary>
        /// Folder container ffmpeg and ffprobe binaries. Leave empty if ffmpeg and ffprobe are present in PATH
        /// </summary>
        public string BinaryFolder { get; set; } = string.Empty;

        /// <summary>
        /// Folder used for temporary files necessary for static methods on FFMpeg class
        /// </summary>
        public string TemporaryFilesFolder { get; set; } = Path.GetTempPath();
        
        /// <summary>
        /// Encoding used for parsing stdout/stderr on ffmpeg and ffprobe processes
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> ExtensionOverrides { get; set; } = new Dictionary<string, string>
        {
            { "mpegts", ".ts" },
        };
        
        /// <summary>
        /// Whether to cache calls to get ffmpeg codec, pixel- and container-formats
        /// </summary>
        public bool UseCache { get; set; } = true;
    }
}