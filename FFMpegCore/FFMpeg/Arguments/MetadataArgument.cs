namespace FFMpegCore.Arguments;

public class MetadataArgument : IInputArgument, IDynamicArgument
{
    private readonly string _metadataContent;
    private string? _tempFileName;

    public MetadataArgument(string metadataContent)
    {
        _metadataContent = metadataContent;
    }

    public string GetText(IEnumerable<IArgument>? arguments)
    {
        arguments ??= Enumerable.Empty<IArgument>();

        var index = arguments
            .TakeWhile(x => x != this)
            .OfType<IInputArgument>()
            .Count();

        return $"-i \"{TempFileName}\" -map_metadata {index}";
    }

    public string Text => GetText(null);

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private string TempFileName => _tempFileName ??= TempFileNameIn(GlobalFFOptions.Current);

    public void Pre(FFOptions options)
    {
        _tempFileName = TempFileNameIn(options);
        File.WriteAllText(_tempFileName, _metadataContent);
    }

    public void Post()
    {
        File.Delete(TempFileName);
    }

    private static string TempFileNameIn(FFOptions options)
    {
        return Path.Combine(options.TemporaryFilesFolder, $"metadata_{Guid.NewGuid()}.txt");
    }
}
