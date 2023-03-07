using System.Drawing;

namespace FFMpegCore.Arguments
{
    public class GifPalettArgument : IArgument
    {
        private readonly int _streamIndex;

        private readonly int _fps;

        private readonly Size? _size;

        public GifPalettArgument(int streamIndex, int fps, Size? size)
        {
            _streamIndex = streamIndex;
            _fps = fps;
            _size = size;
        }

        private string ScaleText => _size.HasValue ? $"scale=w={_size.Value.Width}:h={_size.Value.Height}," : string.Empty;

        public string Text => $"-filter_complex \"[{_streamIndex}:v] fps={_fps},{ScaleText}split [a][b];[a] palettegen=max_colors=32 [p];[b][p] paletteuse=dither=bayer\"";
    }
}
