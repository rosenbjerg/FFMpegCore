using FFMpegCore.Enums;
using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

public class ComplexFilterArgument : IArgument
{
    public readonly FFMpegComplexFilterOptions Options;

    public ComplexFilterArgument(FFMpegComplexFilterOptions options)
    {
        Options = options;
    }

    public string Text => $"-filter_complex \"{Options.GetText()}\"";
}

/// <summary>
///     A <c>-filter_complex</c> graph: one or more chains, each taking labelled inputs, applying filters in order and
///     labelling what it produces. Chains are joined with <c>;</c>, filters within a chain with <c>,</c>.
/// </summary>
public class FFMpegComplexFilterOptions
{
    internal readonly List<ComplexFilterChain> Chains = new();

    /// <summary>Starts a chain reading a stream of an input file.</summary>
    /// <param name="inputIndex">Index of the input, in the order the inputs were added.</param>
    /// <param name="streamType">Which kind of stream to take.</param>
    /// <param name="streamIndex">Which stream of that kind, or null for all of them.</param>
    public ComplexFilterChain From(int inputIndex, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        return NewChain().From(inputIndex, streamType, streamIndex);
    }

    /// <summary>Starts a chain reading a label an earlier chain produced.</summary>
    public ComplexFilterChain From(string label)
    {
        return NewChain().From(label);
    }

    private ComplexFilterChain NewChain()
    {
        var chain = new ComplexFilterChain(this);
        Chains.Add(chain);
        return chain;
    }

    internal string GetText()
    {
        if (Chains.Count == 0)
        {
            throw new FFMpegArgumentException("No complex-filter chains provided");
        }

        return string.Join(";", Chains.Select(chain => chain.GetText()));
    }
}

/// <summary>
///     One chain of a <see cref="FFMpegComplexFilterOptions" /> graph. Add inputs with <c>From</c>, then filters, then close
///     it with <see cref="As" />, which hands the graph back so another chain can be started.
/// </summary>
public class ComplexFilterChain
{
    private readonly List<(string Key, string Value)> _filters = new();
    private readonly FFMpegComplexFilterOptions _graph;
    private readonly List<string> _inputs = new();
    private readonly List<string> _outputs = new();

    internal ComplexFilterChain(FFMpegComplexFilterOptions graph)
    {
        _graph = graph;
    }

    /// <summary>Reads another stream of an input file into this chain.</summary>
    public ComplexFilterChain From(int inputIndex, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        _inputs.Add($"{inputIndex}{streamType.Specifier()}{(streamIndex == null ? "" : $":{streamIndex}")}");
        return this;
    }

    /// <summary>Reads a label an earlier chain produced into this chain.</summary>
    public ComplexFilterChain From(string label)
    {
        _inputs.Add(label.Trim('[', ']'));
        return this;
    }

    /// <summary>Closes the chain, labelling what it produces, and returns the graph.</summary>
    public FFMpegComplexFilterOptions As(params string[] labels)
    {
        _outputs.AddRange(labels.Select(label => label.Trim('[', ']')));
        return _graph;
    }

    public ComplexFilterChain WithFilter(IVideoFilterArgument filter)
    {
        _filters.Add((filter.Key, filter.Value));
        return this;
    }

    public ComplexFilterChain WithFilter(IAudioFilterArgument filter)
    {
        _filters.Add((filter.Key, filter.Value));
        return this;
    }

    public ComplexFilterChain WithCustomFilter(string key, string value = "")
    {
        _filters.Add((key, value));
        return this;
    }

    /// <summary>concat</summary>
    public ComplexFilterChain Concat(int segments, int videoStreams = 1, int audioStreams = 0)
    {
        return WithCustomFilter("concat", $"n={segments}:v={videoStreams}:a={audioStreams}");
    }

    /// <summary>overlay</summary>
    public ComplexFilterChain Overlay(string x = "0", string y = "0", string? eofAction = null, bool shortest = false)
    {
        return WithFilter(new OverlayArgument(x, y, eofAction, shortest));
    }

    /// <summary>amix</summary>
    public ComplexFilterChain AudioMix(int inputs, string duration = "longest")
    {
        return WithCustomFilter("amix", $"inputs={inputs}:duration={duration}");
    }

    internal string GetText()
    {
        if (_filters.Count == 0)
        {
            throw new FFMpegArgumentException("A complex-filter chain needs at least one filter");
        }

        var pads = string.Concat(_inputs.Select(input => $"[{input}]"));
        var filters = string.Join(",", _filters.Select(Render));
        var outputs = string.Concat(_outputs.Select(output => $"[{output}]"));
        return $"{pads}{filters}{outputs}";

        static string Render((string Key, string Value) filter)
        {
            if (string.IsNullOrEmpty(filter.Key))
            {
                return filter.Value;
            }

            return string.IsNullOrEmpty(filter.Value) ? filter.Key : $"{filter.Key}={filter.Value}";
        }
    }
}
