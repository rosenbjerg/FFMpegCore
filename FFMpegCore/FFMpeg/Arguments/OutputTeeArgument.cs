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

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Post()
    {
    }

    public void Pre()
    {
    }

    private static string MapOptions(FFMpegArgumentOptions option)
    {
        var optionPrefix = string.Empty;
        if (option.Arguments.Count > 1)
        {
            var options = option.Arguments.Take(option.Arguments.Count - 1);
            optionPrefix = $"[{string.Join(":", options.Select(MapArgument))}]";
        }

        var output = option.Arguments.OfType<IOutputArgument>().Single();
        var target = output is OutputArgument file ? file.Path : output.Text.Trim('"');
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

        if (argument is BitStreamFilterArgument bitstreamFilter)
        {
            return bitstreamFilter.Text.Replace("-bsf:", "bsfs/").Replace(' ', '=');
        }

        return argument.Text.TrimStart('-').Replace(' ', '=');
    }
}
