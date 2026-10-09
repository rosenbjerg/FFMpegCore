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
    public readonly List<(string key, string value)> Parameters = new();
    public readonly string Text;

    internal DrawTextOptions(string text)
    {
        Text = text;
    }

    internal string TextInternal => string.Join(":", new[] { (key: "text", value: StringExtensions.ToQuotedFilterValue(Text)) }.Concat(Parameters)
        .Select(pair => $"{pair.key}={pair.value}"));

    public DrawTextOptions WithFontFile(string fontFile)
    {
        Parameters.Add(("fontfile", StringExtensions.ToQuotedFilterValue(fontFile)));
        return this;
    }

    public DrawTextOptions WithParameter(string key, string value)
    {
        Parameters.Add((key, StringExtensions.EncloseIfContainsSpace(value)));
        return this;
    }
}
