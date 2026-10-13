namespace FFMpegCore.Arguments;

/// <summary>
///     Represents input parameter
/// </summary>
public class InputArgument : IInputArgument
{
    public readonly string FilePath;
    public readonly bool VerifyExists;

    public InputArgument(string filePath, bool verifyExists)
    {
        VerifyExists = verifyExists;
        FilePath = filePath;
    }

    public void Pre(FFOptions options)
    {
        if (VerifyExists && !File.Exists(options.ResolvePath(FilePath)))
        {
            throw new FileNotFoundException("Input file not found", FilePath);
        }
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post() { }

    public string Text => $"-i \"{FilePath}\"";
}
