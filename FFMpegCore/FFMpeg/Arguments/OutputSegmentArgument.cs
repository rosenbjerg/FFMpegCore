using FFMpegCore.Exceptions;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents output parameter
/// </summary>
public class OutputSegmentArgument : IOutputArgument
{
    public readonly SegmentArgumentOptions Options;
    public readonly bool Overwrite;
    public readonly string SegmentPattern;

    public OutputSegmentArgument(SegmentArgument segmentArgument)
    {
        SegmentPattern = segmentArgument.SegmentPattern;
        Overwrite = segmentArgument.Overwrite;
        var segmentArgumentobj = new SegmentArgumentOptions();
        segmentArgument.Options?.Invoke(segmentArgumentobj);
        Options = segmentArgumentobj;
    }

    public void Pre()
    {
        if (int.TryParse(Options.Arguments.FirstOrDefault(x => x.Key == "segment_time").Value, out var result) && result < 1)
        {
            throw new FFMpegException(FFMpegExceptionType.Process, "Parameter SegmentTime cannot be negative or equal to zero");
        }

        if (Options.Arguments.FirstOrDefault(x => x.Key == "segment_time").Value == "0")
        {
            throw new FFMpegException(FFMpegExceptionType.Process, "Parameter SegmentWrap cannot equal to zero");
        }
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
    }

    public string Text => GetText();

    private string GetText()
    {
        var arguments = Options.Arguments
            .Where(arg => !string.IsNullOrWhiteSpace(arg.Value) && !string.IsNullOrWhiteSpace(arg.Key))
            .Select(arg =>
            {
                return arg.Value;
            });

        return $"-f segment {string.Join(" ", arguments)} \"{SegmentPattern}\"{(Overwrite ? " -y" : string.Empty)}";
    }
}

public interface ISegmentArgument
{
    string Key { get; }
    string Value { get; }
}

public class SegmentArgumentOptions
{
    public List<ISegmentArgument> Arguments { get; } = new();

    public SegmentArgumentOptions ResetTimeStamps(bool resetTimestamps = true)
    {
        return WithArgument(new SegmentResetTimeStampsArgument(resetTimestamps));
    }

    public SegmentArgumentOptions Strftime(bool enable = false)
    {
        return WithArgument(new SegmentStrftimeArgument(enable));
    }

    public SegmentArgumentOptions Time(int time = 60)
    {
        return WithArgument(new SegmentTimeArgument(time));
    }

    public SegmentArgumentOptions Wrap(int limit = -1)
    {
        return WithArgument(new SegmentWrapArgument(limit));
    }

    public SegmentArgumentOptions WithCustomArgument(string argument)
    {
        return WithArgument(new SegmentCustomArgument(argument));
    }

    private SegmentArgumentOptions WithArgument(ISegmentArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}

public class SegmentArgument
{
    public readonly Action<SegmentArgumentOptions> Options;
    public readonly bool Overwrite;
    public readonly string SegmentPattern;

    public SegmentArgument(string segmentPattern, bool overwrite, Action<SegmentArgumentOptions> options)
    {
        SegmentPattern = segmentPattern;
        Overwrite = overwrite;
        Options = options;
    }
}
