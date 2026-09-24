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

    /// <summary>scale</summary>
    public ComplexFilterChain Scale(int width, int height)
    {
        return WithFilter(new ScaleArgument(width, height));
    }

    /// <summary>scale</summary>
    public ComplexFilterChain Scale(VideoSize videoSize)
    {
        return WithFilter(new ScaleArgument(videoSize));
    }

    /// <summary>crop</summary>
    public ComplexFilterChain Crop(int width, int height, int left = 0, int top = 0)
    {
        return WithFilter(new CropArgument(width, height, left, top));
    }

    /// <summary>pad</summary>
    public ComplexFilterChain Pad(PadOptions padOptions)
    {
        return WithFilter(new PadArgument(padOptions));
    }

    /// <summary>transpose</summary>
    public ComplexFilterChain Transpose(Transposition transposition)
    {
        return WithFilter(new TransposeArgument(transposition));
    }

    /// <summary>hflip</summary>
    public ComplexFilterChain HorizontalFlip()
    {
        return WithFilter(FlipArgument.Horizontal);
    }

    /// <summary>vflip</summary>
    public ComplexFilterChain VerticalFlip()
    {
        return WithFilter(FlipArgument.Vertical);
    }

    /// <summary>drawtext</summary>
    public ComplexFilterChain DrawText(DrawTextOptions drawTextOptions)
    {
        return WithFilter(new DrawTextArgument(drawTextOptions));
    }

    /// <summary>subtitles</summary>
    public ComplexFilterChain HardBurnSubtitle(SubtitleHardBurnOptions subtitleHardBurnOptions)
    {
        return WithFilter(new SubtitleHardBurnArgument(subtitleHardBurnOptions));
    }

    /// <summary>fps</summary>
    public ComplexFilterChain Fps(double frameRate, string? round = null)
    {
        return WithFilter(new FpsArgument(frameRate, round));
    }

    /// <summary>tile</summary>
    public ComplexFilterChain Tile(int columns, int rows, int margin = 0, int padding = 0, string? color = null)
    {
        return WithFilter(new TileArgument(columns, rows, margin, padding, color));
    }

    /// <summary>setpts</summary>
    public ComplexFilterChain Speed(double multiplier)
    {
        return WithFilter(new VideoSpeedArgument(multiplier));
    }

    /// <summary>fade</summary>
    public ComplexFilterChain Fade(FadeDirection direction, TimeSpan start, TimeSpan duration, string? color = null)
    {
        return WithFilter(new VideoFadeArgument(direction, start, duration, color));
    }

    /// <summary>atempo, chained when the multiplier is outside the 0.5x-100x a single atempo spans</summary>
    public ComplexFilterChain AudioSpeed(double multiplier)
    {
        return AudioSpeedArgument.StepsFor(multiplier)
            .Aggregate(this, (chain, step) => chain.WithFilter(new AudioSpeedArgument(step)));
    }

    /// <summary>afade</summary>
    public ComplexFilterChain AudioFade(FadeDirection direction, TimeSpan start, TimeSpan duration, string? curve = null)
    {
        return WithFilter(new AudioFadeArgument(direction, start, duration, curve));
    }

    /// <summary>loudnorm</summary>
    public ComplexFilterChain Loudnorm(double integratedLoudness = -24, double loudnessRange = 7, double truePeak = -2, bool dualMono = false)
    {
        return WithFilter(new LoudnormArgument(integratedLoudness, loudnessRange, truePeak, dualMono));
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
