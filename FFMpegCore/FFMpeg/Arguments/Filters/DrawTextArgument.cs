using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class DrawTextArgument : IVideoFilterArgument
{
    public readonly DrawTextOptions Options;

    public DrawTextArgument(string text, Action<DrawTextOptions>? configure = null)
    {
        Options = new DrawTextOptions(text);
        configure?.Invoke(Options);
    }

    public string Key { get; } = "drawtext";
    public string Value => Options.TextInternal;
}

public class DrawTextOptions
{
    private readonly Dictionary<string, string> _parameters = new();
    public readonly string Text;

    internal DrawTextOptions(string text)
    {
        Text = text;
    }

    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    internal string TextInternal => string.Join(":", new[] { new KeyValuePair<string, string>("text", StringExtensions.ToQuotedFilterValue(Text)) }
        .Concat(_parameters)
        .Select(pair => $"{pair.Key}={pair.Value}"));

    public DrawTextOptions WithFontFile(string fontFile)
    {
        _parameters.Add("fontfile", StringExtensions.ToQuotedFilterValue(fontFile));
        return this;
    }

    public DrawTextOptions WithParameter(string key, string value)
    {
        _parameters.Add(key, StringExtensions.EncloseIfContainsSpace(value));
        return this;
    }
}
