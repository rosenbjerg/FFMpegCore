using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

internal class OutputTeeArgument : IOutputArgument
{
    private readonly FFMpegMultiOutputOptions _options;

    public OutputTeeArgument(FFMpegMultiOutputOptions options)
    {
        if (options.Outputs.Count == 0)
        {
            throw new ArgumentException("Atleast one output must be specified.", nameof(options));
        }

        _options = options;
    }

    public string Text
    {
        get
        {
            var overwrite = _options.Outputs.SelectMany(o => o.Arguments).OfType<OutputArgument>().Any(o => o.Overwrite);
            return $"-f tee \"{string.Join("|", _options.Outputs.Select(MapOptions))}\"{(overwrite ? " -y" : string.Empty)}";
        }
    }

    private IEnumerable<IOutputArgument> Targets => _options.Outputs.Select(TargetOf);

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(Targets.Select(target => target.During(cancellationToken)));
    }

    public void Post()
    {
        foreach (var target in Targets)
        {
            target.Post();
        }
    }

    public void Pre(FFOptions options)
    {
        foreach (var target in Targets)
        {
            target.Pre(options);
        }
    }

    private static IOutputArgument TargetOf(FFMpegOutputOptions option)
    {
        return option.Arguments.OfType<IOutputArgument>().Single();
    }

    private static string MapOptions(FFMpegOutputOptions option)
    {
        var output = TargetOf(option);
        var options = option.Arguments.Where(argument => argument != output).Select(MapArgument).ToList();
        if (output is OutputPipeArgument pipe && pipe.Reader.GetStreamArguments() is { Length: > 0 } streamArguments)
        {
            options.Add(MapArgument(new CustomArgument(streamArguments)));
        }

        var optionPrefix = options.Count > 0 ? $"[{string.Join(":", options)}]" : string.Empty;
        var target = output switch
        {
            OutputArgument file => file.Path,
            PipeArgument pipeTarget => pipeTarget.PipePath,
            _ => output.Text.Trim('"')
        };
        return $"{optionPrefix}{EscapeTarget(target)}";
    }

    // The tee muxer tokenises slave specs itself: backslash escapes, single quotes group, | separates slaves
    private static string EscapeTarget(string target)
    {
        return target.Replace("\\", "\\\\").Replace("'", "\\'").Replace("|", "\\|");
    }

    private static string MapArgument(IArgument argument)
    {
        if (argument is MapStreamArgument map)
        {
            return map.Text.Replace("-map ", "select=\\'") + "\\'";
        }

        if (argument is BitstreamFilterArgument bitstreamFilter)
        {
            var specifier = bitstreamFilter.StreamType.Specifier().TrimStart(':');
            return $"bsfs{(specifier.Length > 0 ? "/" + specifier : string.Empty)}={bitstreamFilter.Filter}";
        }

        return argument.Text.TrimStart('-').Replace(' ', '=');
    }
}
