namespace FFMpegCore.Arguments;

public class ImageSequenceInputArgument : IInputArgument
{
    private readonly string _extension;
    private readonly string[] _images;
    private string? _tempFolder;

    public ImageSequenceInputArgument(IEnumerable<string> images)
    {
        _images = images.ToArray();
        var extensions = _images.Select(image => Path.GetExtension(image).ToLowerInvariant()).Distinct().ToArray();
        if (extensions.Length != 1)
        {
            throw new ArgumentException("All images must have the same extension", nameof(images));
        }

        _extension = extensions[0];
    }

    private string TempFolder => _tempFolder ??= TempFolderIn(GlobalFFOptions.Current);

    public string Text => $"-i \"{Path.Combine(TempFolder, $"%09d{_extension}")}\"";

    public void Pre(FFOptions options)
    {
        _tempFolder = TempFolderIn(options);
        Directory.CreateDirectory(_tempFolder);
        for (var index = 0; index < _images.Length; index++)
        {
            File.Copy(_images[index], Path.Combine(_tempFolder, $"{index:D9}{_extension}"));
        }
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
        Directory.Delete(TempFolder, true);
    }

    private static string TempFolderIn(FFOptions options)
    {
        return Path.Combine(options.TemporaryFilesFolder, Guid.NewGuid().ToString());
    }
}
