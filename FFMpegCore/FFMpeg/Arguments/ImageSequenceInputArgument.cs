namespace FFMpegCore.Arguments;

public class ImageSequenceInputArgument : IInputArgument
{
    private readonly string _extension;
    private readonly string[] _images;
    private readonly string _tempFolder = Path.Combine(GlobalFFOptions.Current.TemporaryFilesFolder, Guid.NewGuid().ToString());

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

    public string Text => $"-i \"{Path.Combine(_tempFolder, $"%09d{_extension}")}\"";

    public void Pre()
    {
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
        Directory.Delete(_tempFolder, true);
    }
}
