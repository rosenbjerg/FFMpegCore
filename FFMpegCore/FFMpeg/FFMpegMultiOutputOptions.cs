using FFMpegCore.Arguments;
using FFMpegCore.Helpers;
using FFMpegCore.Pipes;

namespace FFMpegCore;

public class FFMpegMultiOutputOptions
{
    internal readonly List<FFMpegOutputOptions> Outputs = new();

    public IEnumerable<IArgument> Arguments => Outputs.SelectMany(o => o.Arguments);

    public FFMpegMultiOutputOptions OutputToFile(string file, Action<FFMpegOutputOptions> addArguments)
    {
        return OutputToFile(file, true, addArguments);
    }

    public FFMpegMultiOutputOptions OutputToFile(string file, bool overwrite = true, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return AddOutput(new OutputArgument(file, overwrite), addArguments);
    }

    public FFMpegMultiOutputOptions OutputToUrl(string uri, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return AddOutput(new OutputUrlArgument(uri), addArguments);
    }

    public FFMpegMultiOutputOptions OutputToUrl(Uri uri, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return AddOutput(new OutputUrlArgument(uri.ToFFmpegUrl()), addArguments);
    }

    public FFMpegMultiOutputOptions OutputToPipe(IPipeSink reader, Action<FFMpegOutputOptions>? addArguments = null)
    {
        return AddOutput(new OutputPipeArgument(reader), addArguments);
    }

    public FFMpegMultiOutputOptions OutputToNull(Action<FFMpegOutputOptions>? addArguments = null)
    {
        return AddOutput(new NullOutputArgument(), addArguments);
    }

    public FFMpegMultiOutputOptions AddOutput(IOutputArgument argument, Action<FFMpegOutputOptions>? addArguments = null)
    {
        var args = new FFMpegOutputOptions();
        addArguments?.Invoke(args);
        args.Arguments.Add(argument);
        Outputs.Add(args);
        return this;
    }
}
