namespace FFMpegCore.Arguments;

internal class OutputTeeArgument : IOutputArgument
{
    private readonly TeeOutputOptions _options;

    public OutputTeeArgument(TeeOutputOptions options)
    {
        if (options.Targets.Count == 0)
        {
            throw new ArgumentException("At least one output must be specified.", nameof(options));
        }

        _options = options;
    }

    public string Text
    {
        get
        {
            var overwrite = Targets.OfType<OutputArgument>().Any(o => o.Overwrite);
            return $"-f tee \"{string.Join("|", _options.Targets.Select(MapTarget))}\"{(overwrite ? " -y" : string.Empty)}";
        }
    }

    private IEnumerable<IOutputArgument> Targets => _options.Targets.Select(target => target.Target);

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

    private static string MapTarget(TeeTargetOptions target)
    {
        var options = target.Options.Select(option => $"{option.Key}={option.Value}").ToList();
        if (target.Target is OutputPipeArgument pipe && pipe.Reader.GetStreamArguments() is { Length: > 0 } streamArguments)
        {
            options.Add(streamArguments.TrimStart('-').Replace(' ', '='));
        }

        var optionPrefix = options.Count > 0 ? $"[{string.Join(":", options)}]" : string.Empty;
        var path = target.Target switch
        {
            OutputArgument file => file.Path,
            OutputUrlArgument url => url.Url,
            PipeArgument pipeTarget => pipeTarget.PipePath,
            _ => target.Target.Text.Trim('"')
        };
        return $"{optionPrefix}{EscapeTarget(path)}";
    }

    // The tee muxer tokenises slave specs itself: backslash escapes, single quotes group, | separates slaves
    private static string EscapeTarget(string target)
    {
        return target.Replace("\\", "\\\\").Replace("'", "\\'").Replace("|", "\\|");
    }
}
