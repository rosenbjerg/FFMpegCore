namespace FFMpegCore.Arguments;

/// <summary>
///     Represents parameter of concat argument
///     Used for creating video from multiple images or videos
/// </summary>
public class DemuxConcatArgument : IInputArgument
{
    private readonly string _tempFileName = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, $"concat_{Guid.NewGuid()}.txt");
    public readonly IEnumerable<string> Values;

    public DemuxConcatArgument(IEnumerable<string> values)
    {
        Values = values.Select(value => $"file '{Escape(value)}'");
    }

    public void Pre()
    {
        File.WriteAllLines(_tempFileName, Values);
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
        File.Delete(_tempFileName);
    }

    public string Text => $"-f concat -safe 0 -i \"{_tempFileName}\"";

    /// <summary>
    ///     Thanks slhck
    ///     https://superuser.com/a/787651/1089628
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string Escape(string value)
    {
        return value.Replace("'", @"'\''");
    }
}
