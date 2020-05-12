using System;
using System.IO;
using FFMpegCore.Models;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents output parameter
    /// </summary>
    public class OutputArgument : IOutputArgument
    {
        public readonly string Path;
        public readonly bool Overwrite;
        public readonly bool VerifyOutputExists;

        public OutputArgument(string path, bool overwrite = false, bool verifyOutputExists = true)
        {
            Path = path;
            Overwrite = overwrite;
            VerifyOutputExists = verifyOutputExists;
        }

        public void Pre()
        {
            if (!Overwrite && File.Exists(Path))
                throw new FFMpegException(FFMpegExceptionType.File, "Output file already exists and overwrite is disabled");
        }
        public void Post()
        {
            if (VerifyOutputExists && !File.Exists(Path))
                throw new FFMpegException(FFMpegExceptionType.File, "Output file was not created");
        }

        public OutputArgument(FileInfo value) : this(value.FullName) { }

        public OutputArgument(Uri value) : this(value.AbsolutePath) { }

        public string Text => $"\"{Path}\"{(Overwrite ? " -y" : string.Empty)}";
    }
}
