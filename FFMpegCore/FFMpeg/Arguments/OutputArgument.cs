using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents output parameter
/// </summary>
public class OutputArgument : IOutputArgument
{
    public readonly bool Overwrite;
    public readonly string Path;

    public OutputArgument(string path, bool overwrite = true)
    {
        Path = path;
        Overwrite = overwrite;
    }

    public OutputArgument(FileInfo value) : this(value.FullName) { }

    public OutputArgument(Uri value) : this(value.AbsolutePath) { }

    public void Pre()
    {
        if (!Overwrite && File.Exists(Path))
        {
            throw new FFMpegException(FFMpegExceptionType.File, "Output file already exists and overwrite is disabled");
        }
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
    }

    public string Text => $"\"{Path}\"{(Overwrite ? " -y" : string.Empty)}";
}
