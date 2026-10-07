using System.Drawing;
using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class SubtitleBurnArgument : IVideoFilterArgument
{
    private readonly SubtitleBurnOptions _options;

    public SubtitleBurnArgument(string subtitlePath, Action<SubtitleBurnOptions>? configure = null)
    {
        _options = new SubtitleBurnOptions(subtitlePath);
        configure?.Invoke(_options);
    }

    public string Key => "subtitles";

    public string Value => _options.TextInternal;
}

public class SubtitleBurnOptions
{
    private readonly string _subtitle;

    public readonly Dictionary<string, string> Parameters = new();

    internal SubtitleBurnOptions(string subtitle)
    {
        _subtitle = subtitle;
    }

    internal string TextInternal => string
        .Join(":", new[] { StringExtensions.EncloseInQuotes(StringExtensions.ToFFmpegLibavfilterPath(_subtitle)) }
            .Concat(Parameters.Select(parameter => parameter.FormatArgumentPair(true))));

    public SubtitleBurnOptions WithOriginalSize(int width, int height)
    {
        return WithParameter("original_size", $"{width}x{height}");
    }

    public SubtitleBurnOptions WithOriginalSize(Size size)
    {
        return WithOriginalSize(size.Width, size.Height);
    }

    public SubtitleBurnOptions WithSubtitleIndex(int index)
    {
        return WithParameter("stream_index", index.ToString());
    }

    public SubtitleBurnOptions WithCharacterEncoding(string encoding)
    {
        return WithParameter("charenc", encoding);
    }

    public SubtitleBurnOptions WithStyle(Action<StyleOptions> configure)
    {
        var styleOptions = new StyleOptions();
        configure(styleOptions);
        return WithParameter("force_style", styleOptions.TextInternal);
    }

    public SubtitleBurnOptions WithParameter(string key, string value)
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
