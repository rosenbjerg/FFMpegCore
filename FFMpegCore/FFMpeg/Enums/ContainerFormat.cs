using System.Text.RegularExpressions;

namespace FFMpegCore.Enums;

public class ContainerFormat
{
    private static readonly Regex FormatRegex = new(@"([D ])([E ])\s+([a-z0-9_,]+)\s+(.+)");

    internal ContainerFormat(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public bool DemuxingSupported { get; private set; }
    public bool MuxingSupported { get; private set; }
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    ///     The file extension this container is normally written with, honouring <see cref="FFOptions.ExtensionOverrides" />.
    /// </summary>
    /// <param name="ffOptions">Options to read the overrides from, defaulting to the global options.</param>
    public string GetExtension(FFOptions? ffOptions = null)
    {
        var overrides = (ffOptions ?? GlobalFFOptions.Current).ExtensionOverrides;
        return overrides.TryGetValue(Name, out var overridden) ? overridden : "." + Name;
    }

    internal static IEnumerable<ContainerFormat> Parse(string line)
    {
        var match = FormatRegex.Match(line);
        if (!match.Success)
        {
            return Enumerable.Empty<ContainerFormat>();
        }

        return match.Groups[3].Value.Split(',').Select(name => new ContainerFormat(name)
        {
            DemuxingSupported = match.Groups[1].Value != " ",
            MuxingSupported = match.Groups[2].Value != " ",
            Description = match.Groups[4].Value
        });
    }

    internal void Merge(ContainerFormat other)
    {
        DemuxingSupported |= other.DemuxingSupported;
        MuxingSupported |= other.MuxingSupported;
    }
}
