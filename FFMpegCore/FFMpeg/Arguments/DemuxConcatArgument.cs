namespace FFMpegCore.Arguments;

public class DemuxConcatArgument : IInputArgument
{
    private readonly string[] _values;
    private FFOptions? _options;
    private string? _tempFileName;

    public DemuxConcatArgument(IEnumerable<string> values)
    {
        _values = values.ToArray();
    }

    public IEnumerable<string> Values => _values.Select(value => $"file '{Escape(Resolve(value, (_options ?? GlobalFFOptions.Current).WorkingDirectory))}'");

    private string TempFileName => _tempFileName ??= TempFileNameIn(GlobalFFOptions.Current);

    public string Text => $"-f concat -safe 0 -i \"{TempFileName}\"";

    public void Pre(FFOptions options)
    {
        _options = options;
        _tempFileName = TempFileNameIn(options);
        File.WriteAllLines(_tempFileName, Values);
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
        File.Delete(TempFileName);
    }

    private static string TempFileNameIn(FFOptions options)
    {
        return Path.Combine(options.TemporaryFilesFolder, $"concat_{Guid.NewGuid()}.txt");
    }

    private static string Escape(string value)
    {
        return value.Replace("'", @"'\''");
    }

    // The concat demuxer resolves relative entries against the list file, which lives in TemporaryFilesFolder
    private static string Resolve(string value, string workingDirectory)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _)
            ? value
            : Path.GetFullPath(Path.Combine(workingDirectory, value));
    }
}
