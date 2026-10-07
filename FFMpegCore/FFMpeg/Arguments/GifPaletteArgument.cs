using System.Drawing;

namespace FFMpegCore.Arguments;

internal class GifPaletteArgument : IArgument
{
    private readonly double _fps;
    private readonly Size? _size;
    private readonly int _streamIndex;

    public GifPaletteArgument(int streamIndex, double fps, Size? size)
    {
        _streamIndex = streamIndex;
        _fps = fps;
        _size = size;
    }

    private string ScaleText => _size.HasValue ? $"scale=w={_size.Value.Width}:h={_size.Value.Height}," : string.Empty;

    public string Text =>
        $"-filter_complex \"[0:{_streamIndex}] fps={_fps},{ScaleText}split [a][b];[a] palettegen=max_colors=32 [p];[b][p] paletteuse=dither=bayer\"";
}
