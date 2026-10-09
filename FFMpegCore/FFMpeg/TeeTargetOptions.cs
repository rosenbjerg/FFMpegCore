using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace FFMpegCore;

public class TeeTargetOptions
{
    internal readonly List<(string Key, string Value)> Options = new();

    internal TeeTargetOptions(IOutputArgument target)
    {
        Target = target;
    }

    internal IOutputArgument Target { get; }

    /// <summary>f</summary>
    public TeeTargetOptions ForceFormat(ContainerFormat format)
    {
        return WithMuxerOption("f", format.Name);
    }

    /// <summary>f</summary>
    public TeeTargetOptions ForceFormat(string format)
    {
        return WithMuxerOption("f", format);
    }

    /// <summary>select, choosing which of the encoded streams this target receives</summary>
    public TeeTargetOptions WithSelect(StreamType streamType, int? streamIndex = null)
    {
        var specifier = $"{streamType.Specifier().TrimStart(':')}{(streamIndex == null ? "" : $":{streamIndex}")}".TrimStart(':');
        return WithMuxerOption("select", $"\\'{specifier}\\'");
    }

    /// <summary>bsfs, or bsfs/v / bsfs/a / bsfs/s for one stream type</summary>
    public TeeTargetOptions WithBitstreamFilter(StreamType streamType, BitstreamFilter filter)
    {
        var specifier = streamType.Specifier().TrimStart(':');
        return WithMuxerOption(specifier.Length > 0 ? $"bsfs/{specifier}" : "bsfs", filter.Value);
    }

    /// <summary>movflags=faststart</summary>
    public TeeTargetOptions WithFastStart()
    {
        return WithMovFlags(MovFlags.FastStart);
    }

    /// <summary>movflags; repeated calls add to the same option</summary>
    public TeeTargetOptions WithMovFlags(MovFlags flags)
    {
        var index = Options.FindIndex(option => option.Key == "movflags");
        if (index < 0)
        {
            return WithMuxerOption("movflags", flags.Value);
        }

        Options[index] = ("movflags", $"{Options[index].Value}+{flags.Value}");
        return this;
    }

    /// <summary>onfail</summary>
    public TeeTargetOptions WithOnFail(TeeOnFail onFail)
    {
        return WithMuxerOption("onfail", onFail.Value);
    }

    /// <summary>Any option of this target's muxer, such as hls_time</summary>
    public TeeTargetOptions WithMuxerOption(string key, string value)
    {
        Options.Add((key, value));
        return this;
    }
}
