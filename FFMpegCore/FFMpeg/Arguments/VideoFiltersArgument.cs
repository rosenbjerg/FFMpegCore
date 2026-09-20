using System.Drawing;
using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

public class VideoFiltersArgument : IArgument
{
    public readonly VideoFilterOptions Options;

    public VideoFiltersArgument(VideoFilterOptions options)
    {
        Options = options;
    }

    public string Text => GetText();

    private string GetText()
    {
        if (!Options.Arguments.Any())
        {
            throw new FFMpegArgumentException("No video-filter arguments provided");
        }

        var arguments = Options.Arguments
            .Where(arg => !string.IsNullOrEmpty(arg.Value))
            .Select(arg =>
            {
                var escapedValue = arg.Value.Replace(",", "\\,");
                return string.IsNullOrEmpty(arg.Key) ? escapedValue : $"{arg.Key}={escapedValue}";
            });

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
    public List<IVideoFilterArgument> Arguments { get; } = new();

    /// <summary>scale</summary>
    public VideoFilterOptions Scale(VideoSize videoSize)
    {
        return WithArgument(new ScaleArgument(videoSize));
    }

    /// <summary>scale</summary>
    public VideoFilterOptions Scale(int width, int height)
    {
        return WithArgument(new ScaleArgument(width, height));
    }

    /// <summary>scale</summary>
    public VideoFilterOptions Scale(Size size)
    {
        return WithArgument(new ScaleArgument(size));
    }

    /// <summary>crop</summary>
    public VideoFilterOptions Crop(Size size, int left = 0, int top = 0)
    {
        return WithArgument(new CropArgument(size, left, top));
    }

    /// <summary>crop</summary>
    public VideoFilterOptions Crop(int width, int height, int left = 0, int top = 0)
    {
        return WithArgument(new CropArgument(width, height, left, top));
    }

    /// <summary>transpose</summary>
    public VideoFilterOptions Transpose(Transposition transposition)
    {
        return WithArgument(new TransposeArgument(transposition));
    }

    /// <summary>hflip</summary>
    public VideoFilterOptions HorizontalFlip()
    {
        return WithArgument(FlipArgument.Horizontal);
    }

    /// <summary>vflip</summary>
    public VideoFilterOptions VerticalFlip()
    {
        return WithArgument(FlipArgument.Vertical);
    }

    /// <summary>drawtext</summary>
    public VideoFilterOptions DrawText(DrawTextOptions drawTextOptions)
    {
        return WithArgument(new DrawTextArgument(drawTextOptions));
    }

    /// <summary>subtitles</summary>
    public VideoFilterOptions HardBurnSubtitle(SubtitleHardBurnOptions subtitleHardBurnOptions)
    {
        return WithArgument(new SubtitleHardBurnArgument(subtitleHardBurnOptions));
    }

    /// <summary>blackdetect</summary>
    public VideoFilterOptions BlackDetect(double minimumDuration = 2.0, double pictureBlackRatioThreshold = 0.98, double pixelBlackThreshold = 0.1)
    {
        return WithArgument(new BlackDetectArgument(minimumDuration, pictureBlackRatioThreshold, pixelBlackThreshold));
    }

    /// <summary>blackframe</summary>
    public VideoFilterOptions BlackFrame(int amount = 98, int threshold = 32)
    {
        return WithArgument(new BlackFrameArgument(amount, threshold));
    }

    /// <summary>pad</summary>
    public VideoFilterOptions Pad(PadOptions padOptions)
    {
        return WithArgument(new PadArgument(padOptions));
    }

    private VideoFilterOptions WithArgument(IVideoFilterArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
