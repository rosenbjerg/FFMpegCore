using System.Drawing;
using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class SubtitleHardBurnArgument : IVideoFilterArgument
{
    private readonly SubtitleHardBurnOptions _subtitleHardBurnOptions;

    public SubtitleHardBurnArgument(string subtitlePath, Action<SubtitleHardBurnOptions>? configure = null)
    {
        _subtitleHardBurnOptions = new SubtitleHardBurnOptions(subtitlePath);
        configure?.Invoke(_subtitleHardBurnOptions);
    }

    public string Key => "subtitles";

    public string Value => _subtitleHardBurnOptions.TextInternal;
}

public class SubtitleHardBurnOptions
{
    private readonly string _subtitle;

    public readonly Dictionary<string, string> Parameters = new();

    internal SubtitleHardBurnOptions(string subtitle)
    {
        _subtitle = subtitle;
    }

    internal string TextInternal => string
        .Join(":", new[] { StringExtensions.EncloseInQuotes(StringExtensions.ToFFmpegLibavfilterPath(_subtitle)) }
            .Concat(Parameters.Select(parameter => parameter.FormatArgumentPair(true))));

    public SubtitleHardBurnOptions WithOriginalSize(int width, int height)
    {
        return WithParameter("original_size", $"{width}x{height}");
    }

    public SubtitleHardBurnOptions WithOriginalSize(Size size)
    {
        return WithOriginalSize(size.Width, size.Height);
    }

    public SubtitleHardBurnOptions WithSubtitleIndex(int index)
    {
        return WithParameter("stream_index", index.ToString());
    }

    public SubtitleHardBurnOptions WithCharacterEncoding(string encoding)
    {
        return WithParameter("charenc", encoding);
    }

    public SubtitleHardBurnOptions WithStyle(Action<StyleOptions> configure)
    {
        var styleOptions = new StyleOptions();
        configure(styleOptions);
        return WithParameter("force_style", styleOptions.TextInternal);
    }

    public SubtitleHardBurnOptions WithParameter(string key, string value)
    {
        Parameters.Add(key, value);
        return this;
    }
}

public class StyleOptions
{
    public readonly Dictionary<string, string> Parameters = new();

    internal StyleOptions() { }

    internal string TextInternal => string.Join(",", Parameters.Select(parameter => parameter.FormatArgumentPair(false)));

    public StyleOptions WithParameter(string key, string value)
    {
        Parameters.Add(key, value);
        return this;
    }
}
