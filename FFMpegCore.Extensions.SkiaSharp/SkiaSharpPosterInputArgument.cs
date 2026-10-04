using FFMpegCore.Arguments;
using SkiaSharp;

namespace FFMpegCore.Extensions.SkiaSharp;

internal class SkiaSharpPosterInputArgument : IInputArgument
{
    private readonly SKBitmap _poster;
    private string? _path;

    public SkiaSharpPosterInputArgument(SKBitmap poster)
    {
        _poster = poster;
    }

    private string PosterPath => _path ??= PathIn(GlobalFFOptions.Current);

    public string Text => $"-i \"{PosterPath}\"";

    public void Pre(FFOptions options)
    {
        _path = PathIn(options);
        using var fileStream = File.OpenWrite(_path);
        _poster.Encode(fileStream, SKEncodedImageFormat.Png, default);
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
        File.Delete(PosterPath);
    }

    private static string PathIn(FFOptions options)
    {
        return Path.Combine(options.TemporaryFilesFolder, $"poster_{Guid.NewGuid()}.png");
    }
}
