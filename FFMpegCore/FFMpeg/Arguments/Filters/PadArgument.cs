using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class PadArgument : IVideoFilterArgument
{
    private readonly PadOptions _options;

    public PadArgument(string? width = null, string? height = null, Action<PadOptions>? configure = null)
    {
        _options = new PadOptions(width, height);
        configure?.Invoke(_options);
        if (!_options.Parameters.ContainsKey("width") && !_options.Parameters.ContainsKey("height") && !_options.Parameters.ContainsKey("aspect"))
        {
            throw new ArgumentException("Pad needs a width, a height or an aspect ratio");
        }
    }

    public string Key => "pad";
    public string Value => _options.TextInternal;
}

public class PadOptions
{
    public readonly Dictionary<string, string> Parameters = new();

    internal PadOptions(string? width, string? height)
    {
        if (width != null)
        {
            Parameters.Add("width", width);
        }

        if (height != null)
        {
            Parameters.Add("height", height);
        }
    }

    internal string TextInternal => string.Join(":", Parameters.Select(parameter => parameter.FormatArgumentPair(true)));

    public PadOptions WithAspectRatio(string aspectRatio)
    {
        return WithParameter("aspect", aspectRatio);
    }

    public PadOptions WithParameter(string key, string value)
    {
        Parameters.Add(key, value);
        return this;
    }
}
