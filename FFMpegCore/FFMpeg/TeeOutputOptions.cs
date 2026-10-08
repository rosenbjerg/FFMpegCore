using FFMpegCore.Arguments;
using FFMpegCore.Pipes;

namespace FFMpegCore;

public class TeeOutputOptions
{
    internal readonly List<TeeTargetOptions> Targets = new();

    internal TeeOutputOptions() { }

    public TeeOutputOptions OutputToFile(string file, Action<TeeTargetOptions> addArguments)
    {
        return OutputToFile(file, true, addArguments);
    }

    public TeeOutputOptions OutputToFile(string file, bool overwrite = true, Action<TeeTargetOptions>? addArguments = null)
    {
        return AddTarget(new OutputArgument(file, overwrite), addArguments);
    }

    public TeeOutputOptions OutputToUrl(string uri, Action<TeeTargetOptions>? addArguments = null)
    {
        return AddTarget(new OutputUrlArgument(uri), addArguments);
    }

    public TeeOutputOptions OutputToUrl(Uri uri, Action<TeeTargetOptions>? addArguments = null)
    {
        return AddTarget(new OutputUrlArgument(uri.AbsoluteUri), addArguments);
    }

    public TeeOutputOptions OutputToPipe(IPipeSink reader, Action<TeeTargetOptions>? addArguments = null)
    {
        return AddTarget(new OutputPipeArgument(reader), addArguments);
    }

    private TeeOutputOptions AddTarget(IOutputArgument target, Action<TeeTargetOptions>? addArguments)
    {
        var options = new TeeTargetOptions(target);
        addArguments?.Invoke(options);
        Targets.Add(options);
        return this;
    }
}
