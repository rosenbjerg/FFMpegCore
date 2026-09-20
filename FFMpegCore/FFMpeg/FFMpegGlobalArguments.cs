using FFMpegCore.Arguments;

namespace FFMpegCore;

public sealed class FFMpegGlobalArguments : FFMpegArgumentsBase
{
    internal FFMpegGlobalArguments() { }

    public FFMpegGlobalArguments WithVerbosityLevel(VerbosityLevel verbosityLevel = VerbosityLevel.Error)
    {
        return WithArgument(new VerbosityLevelArgument(verbosityLevel));
    }

    public FFMpegGlobalArguments WithArgument(IArgument argument)
    {
        Arguments.Add(argument);
        return this;
    }
}
