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

    internal string TextInternal => string.Join(":", new[] { ("text", Text) }.Concat(Parameters).Select(FormatArgumentPair));

    public DrawTextOptions WithFontFile(string fontFile)
    {
        return WithParameter("fontfile", fontFile);
    }

    public DrawTextOptions WithParameter(string key, string value)
    {
        Parameters.Add((key, value));
        return this;
    }

    private static string FormatArgumentPair((string key, string value) pair)
    {
        return $"{pair.key}={EncloseIfContainsSpace(pair.value)}";
    }

    private static string EncloseIfContainsSpace(string input)
    {
        return input.Contains(" ") ? $"'{input}'" : input;
    }
}
