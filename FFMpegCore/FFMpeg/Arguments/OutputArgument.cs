using System;
using System.IO;
using FFMpegCore.FFMPEG.Exceptions;

namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Represents output parameter
    /// </summary>
    public class OutputArgument : IOutputArgument
    {
        public readonly string Path;
        public readonly bool Overwrite;

        public OutputArgument(string path, bool overwrite = false)
        {
            Path = path;
            Overwrite = overwrite;
        }

        public void Pre()
        {
            if (!Overwrite && File.Exists(Path))
                throw new FFMpegException(FFMpegExceptionType.File, "Output file already exists and overwrite is disabled");
        }
        public void Post()
        {
            if (!File.Exists(Path))
                throw new FFMpegException(FFMpegExceptionType.File, "Output file was not created");
        }

        public OutputArgument(FileInfo value) : this(value.FullName) { }

        public OutputArgument(Uri value) : this(value.AbsolutePath) { }

        public string Text => $"\"{Path}\"{(Overwrite ? " -y" : string.Empty)}";
    }
}
