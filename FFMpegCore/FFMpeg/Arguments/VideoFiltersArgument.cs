using System.Drawing;
using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class VideoFiltersArgument : IArgument
{
    public readonly VideoFilterOptions Options;

    public VideoFiltersArgument(VideoFilterOptions options)
    {
        if (!options.Arguments.Any(arg => FilterRenderer.HasText(arg.Key, arg.Value)))
        {
            throw new ArgumentException("No video filters were added", nameof(options));
        }

        Options = options;
    }

    public string Text => GetText();

    private string GetText()
    {
        var arguments = Options.Arguments
            .Where(arg => FilterRenderer.HasText(arg.Key, arg.Value))
            .Select(arg => FilterRenderer.Render(arg.Key, arg.Value, true))
            .ToArray();

        return $"-vf \"{string.Join(", ", arguments)}\"";
    }
}

public interface IVideoFilterArgument
{
    string Key { get; }
    string Value { get; }
}

public class VideoFilterOptions
{
    private readonly List<IVideoFilterArgument> _arguments = new();

    public IReadOnlyList<IVideoFilterArgument> Arguments => _arguments;

    /// <summary>scale</summary>
    public VideoFilterOptions Scale(VideoSize videoSize)
    {
        return WithFilter(new ScaleArgument(videoSize));
    }

    /// <summary>scale</summary>
    public VideoFilterOptions Scale(int width, int height)
    {
        return WithFilter(new ScaleArgument(width, height));
    }

    /// <summary>scale</summary>
    public VideoFilterOptions Scale(Size size)
    {
        return WithFilter(new ScaleArgument(size));
    }

    /// <summary>crop</summary>
    public VideoFilterOptions Crop(Size size, int left = 0, int top = 0)
    {
        return WithFilter(new CropArgument(size, left, top));
    }

    /// <summary>crop</summary>
    public VideoFilterOptions Crop(int width, int height, int left = 0, int top = 0)
    {
        return WithFilter(new CropArgument(width, height, left, top));
    }

    /// <summary>transpose</summary>
    public VideoFilterOptions Transpose(Transposition transposition)
    {
        return WithFilter(new TransposeArgument(transposition));
    }

    /// <summary>hflip</summary>
    public VideoFilterOptions HorizontalFlip()
    {
        return WithFilter(FlipArgument.Horizontal);
    }

    /// <summary>vflip</summary>
    public VideoFilterOptions VerticalFlip()
    {
        return WithFilter(FlipArgument.Vertical);
    }

    /// <summary>drawtext</summary>
    public VideoFilterOptions DrawText(string text, Action<DrawTextOptions>? configure = null)
    {
        return WithFilter(new DrawTextArgument(text, configure));
    }

    /// <summary>subtitles</summary>
    public VideoFilterOptions BurnSubtitles(string subtitlePath, Action<SubtitleBurnOptions>? configure = null)
    {
        return WithFilter(new SubtitleBurnArgument(subtitlePath, configure));
    }

    /// <summary>blackdetect</summary>
    public VideoFilterOptions BlackDetect(double minimumDuration = 2.0, double pictureBlackRatioThreshold = 0.98, double pixelBlackThreshold = 0.1)
    {
        return WithFilter(new BlackDetectArgument(minimumDuration, pictureBlackRatioThreshold, pixelBlackThreshold));
    }

    /// <summary>blackframe</summary>
    public VideoFilterOptions BlackFrame(int amount = 98, int threshold = 32)
    {
        return WithFilter(new BlackFrameArgument(amount, threshold));
    }

    /// <summary>fps</summary>
    public VideoFilterOptions Fps(double frameRate, FpsRounding? round = null)
    {
        return WithFilter(new FpsArgument(frameRate, round));
    }

    /// <summary>tile</summary>
    public VideoFilterOptions Tile(int columns, int rows, int margin = 0, int padding = 0, string? color = null)
    {
        return WithFilter(new TileArgument(columns, rows, margin, padding, color));
    }

    /// <summary>setpts</summary>
    public VideoFilterOptions Speed(double multiplier)
    {
        return WithFilter(new VideoSpeedArgument(multiplier));
    }

    /// <summary>fade</summary>
    public VideoFilterOptions Fade(FadeDirection direction, TimeSpan start, TimeSpan duration, string? color = null)
    {
        return WithFilter(new VideoFadeArgument(direction, start, duration, color));
    }

    /// <summary>pad</summary>
    public VideoFilterOptions Pad(string? width = null, string? height = null, Action<PadOptions>? configure = null)
    {
        return WithFilter(new PadArgument(width, height, configure));
    }

    public VideoFilterOptions WithFilter(IVideoFilterArgument filter)
    {
        _arguments.Add(filter);
        return this;
    }

    public VideoFilterOptions WithCustomFilter(string key, string value = "")
    {
        return WithFilter(new CustomFilterArgument(key, value));
    }
}
