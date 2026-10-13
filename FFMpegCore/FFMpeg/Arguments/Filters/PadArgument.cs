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
    private readonly Dictionary<string, string> _parameters = new();

    internal PadOptions(string? width, string? height)
    {
        if (width != null)
        {
            _parameters.Add("width", width);
        }

        if (height != null)
        {
            _parameters.Add("height", height);
        }
    }

    public IReadOnlyDictionary<string, string> Parameters => _parameters;

    internal string TextInternal => string.Join(":", _parameters.Select(parameter => parameter.FormatArgumentPair(true)));

    public PadOptions WithAspectRatio(string aspectRatio)
    {
        return WithParameter("aspect", aspectRatio);
    }

    public PadOptions WithParameter(string key, string value)
    {
        _parameters.Add(key, value);
        return this;
    }
}
