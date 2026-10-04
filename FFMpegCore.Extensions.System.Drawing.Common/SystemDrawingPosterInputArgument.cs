using System.Drawing;
using FFMpegCore.Arguments;

namespace FFMpegCore.Extensions.System.Drawing.Common;

internal class SystemDrawingPosterInputArgument : IInputArgument
{
    private readonly Image _poster;
    private string? _path;

    public SystemDrawingPosterInputArgument(Image poster)
    {
        _poster = poster;
    }

    private string PosterPath => _path ??= PathIn(GlobalFFOptions.Current);

    public string Text => $"-i \"{PosterPath}\"";

    public void Pre(FFOptions options)
    {
        _path = PathIn(options);
        _poster.Save(_path);
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
