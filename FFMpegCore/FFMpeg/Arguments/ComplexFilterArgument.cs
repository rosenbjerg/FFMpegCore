using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class ComplexFilterArgument : IArgument
{
    public readonly ComplexFilterGraph Graph;

    public ComplexFilterArgument(ComplexFilterGraph graph)
    {
        if (graph.Chains.Count == 0)
        {
            throw new ArgumentException("No complex-filter chains were added", nameof(graph));
        }

        if (graph.Chains.Any(chain => !chain.HasFilters))
        {
            throw new ArgumentException("A complex-filter chain needs at least one filter", nameof(graph));
        }

        Graph = graph;
    }

    public string Text => $"-filter_complex \"{Graph.GetText()}\"";
}

/// <summary>
///     A <c>-filter_complex</c> graph: one or more chains, each taking labelled inputs, applying filters in order and
///     labelling what it produces. Chains are joined with <c>;</c>, filters within a chain with <c>,</c>.
/// </summary>
public class ComplexFilterGraph
{
    internal readonly List<ComplexFilterChain> Chains = new();

    /// <summary>Starts a chain reading a stream of an input file.</summary>
    /// <param name="inputFileIndex">Index of the input, in the order the inputs were added.</param>
    /// <param name="streamType">Which kind of stream to take.</param>
    /// <param name="streamIndex">Which stream of that kind, or null for all of them.</param>
    public ComplexFilterChain From(int inputFileIndex, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        return NewChain().From(inputFileIndex, streamType, streamIndex);
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
        return string.Join(";", Chains.Select(chain => chain.GetText()));
    }
}

/// <summary>
///     One chain of a <see cref="ComplexFilterGraph" /> graph. Add inputs with <c>From</c>, then filters, then close
///     it with <see cref="As" />, which hands the graph back so another chain can be started.
/// </summary>
public class ComplexFilterChain
{
    private readonly List<(string Key, string Value)> _filters = new();
    private readonly ComplexFilterGraph _graph;
    private readonly List<string> _inputs = new();
    private readonly List<string> _outputs = new();

    internal ComplexFilterChain(ComplexFilterGraph graph)
    {
        _graph = graph;
    }

    /// <summary>Reads another stream of an input file into this chain.</summary>
    public ComplexFilterChain From(int inputFileIndex, StreamType streamType = StreamType.All, int? streamIndex = null)
    {
        _inputs.Add($"{inputFileIndex}{streamType.Specifier()}{(streamIndex == null ? "" : $":{streamIndex}")}");
        return this;
    }

    /// <summary>Reads a label an earlier chain produced into this chain.</summary>
    public ComplexFilterChain From(string label)
    {
        _inputs.Add(label.Trim('[', ']'));
        return this;
    }

    /// <summary>Closes the chain, labelling what it produces, and returns the graph.</summary>
    public ComplexFilterGraph As(params string[] labels)
    {
        _outputs.AddRange(labels.Select(label => label.Trim('[', ']')));
        return _graph;
    }

    public ComplexFilterChain Video(Action<VideoFilterOptions> filters)
    {
        var options = new VideoFilterOptions();
        filters(options);
        return options.Arguments.Aggregate(this, (chain, filter) => chain.WithFilter(filter));
    }

    public ComplexFilterChain Audio(Action<AudioFilterOptions> filters)
    {
        var options = new AudioFilterOptions();
        filters(options);
        return options.Arguments.Aggregate(this, (chain, filter) => chain.WithFilter(filter));
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

    /// <summary>concat; <paramref name="segments" /> defaults to this chain's inputs divided by the streams per segment</summary>
    public ComplexFilterChain Concat(int? segments = null, int videoStreams = 1, int audioStreams = 0)
    {
        var streamsPerSegment = videoStreams + audioStreams;
        if (segments == null && (streamsPerSegment == 0 || _inputs.Count % streamsPerSegment != 0))
        {
            throw new ArgumentException(
                $"The chain's {_inputs.Count} inputs do not divide into segments of {videoStreams} video and {audioStreams} audio streams",
                nameof(segments));
        }

        return WithCustomFilter("concat", $"n={segments ?? _inputs.Count / streamsPerSegment}:v={videoStreams}:a={audioStreams}");
    }

    /// <summary>overlay</summary>
    public ComplexFilterChain Overlay(string x = "0", string y = "0", OverlayEofAction? eofAction = null, bool shortest = false)
    {
        return WithFilter(new OverlayArgument(x, y, eofAction, shortest));
    }

    /// <summary>amix; <paramref name="inputs" /> defaults to this chain's inputs</summary>
    public ComplexFilterChain AudioMix(int? inputs = null, AudioMixDuration? duration = null)
    {
        return WithCustomFilter("amix", $"inputs={inputs ?? _inputs.Count}:duration={duration ?? AudioMixDuration.Longest}");
    }

    internal bool HasFilters => _filters.Any(filter => FilterRenderer.HasText(filter.Key, filter.Value));

    internal string GetText()
    {
        var rendered = _filters
            .Where(filter => FilterRenderer.HasText(filter.Key, filter.Value))
            .Select(filter => FilterRenderer.Render(filter.Key, filter.Value, false))
            .ToArray();

        var pads = string.Concat(_inputs.Select(input => $"[{input}]"));
        var outputs = string.Concat(_outputs.Select(output => $"[{output}]"));
        return $"{pads}{string.Join(",", rendered)}{outputs}";
    }
}
