namespace FFMpegCore.Test;

public class TemporaryFile : IDisposable
{
    private readonly string _path;

    public TemporaryFile(string filename)
    {
        _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}-{filename}");
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    public static implicit operator string(TemporaryFile temporaryFile)
    {
        return temporaryFile._path;
    }
}
