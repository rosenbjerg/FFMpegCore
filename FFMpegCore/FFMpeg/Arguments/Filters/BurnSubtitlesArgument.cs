using System.Drawing;
using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class BurnSubtitlesArgument : IVideoFilterArgument
{
    private readonly BurnSubtitlesOptions _options;

    public BurnSubtitlesArgument(string subtitlePath, Action<BurnSubtitlesOptions>? configure = null)
    {
        _options = new BurnSubtitlesOptions(subtitlePath);
        configure?.Invoke(_options);
    }

    public string Key => "subtitles";

    public string Value => _options.TextInternal;
}

public class BurnSubtitlesOptions
{
    private readonly Dictionary<string, string> _parameters = new();

    internal BurnSubtitlesOptions(string subtitle)
    {
        TextInternal = subtitle;
    }

    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    internal string TextInternal => string
        .Join(":", new[] { StringExtensions.EncloseInQuotes(StringExtensions.ToFFMpegLibavfilterPath(field)) }
            .Concat(_parameters.Select(parameter => parameter.FormatArgumentPair(true))));

    public BurnSubtitlesOptions WithOriginalSize(int width, int height)
    {
        return WithParameter("original_size", $"{width}x{height}");
    }

    public BurnSubtitlesOptions WithOriginalSize(Size size)
    {
        return WithOriginalSize(size.Width, size.Height);
    }

    public BurnSubtitlesOptions WithSubtitleIndex(int index)
    {
        return WithParameter("stream_index", index.ToString());
    }

    public BurnSubtitlesOptions WithCharacterEncoding(string encoding)
    {
        return WithParameter("charenc", encoding);
    }

    public BurnSubtitlesOptions WithStyle(Action<StyleOptions> configure)
    {
        var styleOptions = new StyleOptions();
        configure(styleOptions);
        return WithParameter("force_style", styleOptions.TextInternal);
    }

    public BurnSubtitlesOptions WithParameter(string key, string value)
    {
        _parameters.Add(key, value);
        return this;
    }
}

public class StyleOptions
{
    private readonly Dictionary<string, string> _parameters = new();

    internal StyleOptions() { }

    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    internal string TextInternal => string.Join(",", _parameters.Select(parameter => parameter.FormatArgumentPair(false)));

    public StyleOptions WithParameter(string key, string value)
    {
        _parameters.Add(key, value);
        return this;
    }
}
