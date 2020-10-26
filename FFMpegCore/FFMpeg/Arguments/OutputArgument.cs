using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents output parameter
    /// </summary>
    public class OutputArgument : IOutputArgument
    {
        public readonly string Path;
        public readonly bool Overwrite;

        public OutputArgument(string path, bool overwrite = true)
        {
            Path = path;
            Overwrite = overwrite;
        }

        public void Pre()
        {
            if (!Overwrite && File.Exists(Path))
                throw new FFMpegException(FFMpegExceptionType.File, "Output file already exists and overwrite is disabled");
        }
        public Task During(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Post()
        {
        }

        public OutputArgument(FileInfo value) : this(value.FullName) { }

        public OutputArgument(Uri value) : this(value.AbsolutePath) { }

        public string Text => $"\"{Path}\"{(Overwrite ? " -y" : string.Empty)}";
    }
}
