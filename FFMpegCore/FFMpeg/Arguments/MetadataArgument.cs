namespace FFMpegCore.Arguments;

public class MetadataArgument : IInputArgument
{
    private readonly string _metadataContent;
    private bool _written;
    private string? _tempFileName;

    public MetadataArgument(string metadataContent)
    {
        _metadataContent = metadataContent;
    }

    public string Text => $"-i \"{TempFileName}\"";

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private string TempFileName => _tempFileName ??= TempFileNameIn(GlobalFFOptions.Current);

    public void Pre(FFOptions options)
    {
        _tempFileName = TempFileNameIn(options);
        File.WriteAllText(_tempFileName, _metadataContent);
        _written = true;
    }

    public void Post()
    {
        if (_written)
        {
            File.Delete(TempFileName);
            _written = false;
        }
    }

    private static string TempFileNameIn(FFOptions options)
    {
        return Path.Combine(options.TemporaryFilesFolder, $"metadata_{Guid.NewGuid()}.txt");
    }
}
