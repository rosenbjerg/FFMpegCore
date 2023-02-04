using FFMpegCore.Arguments;

namespace FFMpegCore
{
    public sealed class FFMpegGlobalArguments : FFMpegArgumentsBase
    {
        internal FFMpegGlobalArguments() { }

        public FFMpegGlobalArguments WithVerbosityLevel(VerbosityLevel verbosityLevel = VerbosityLevel.Error) => WithOption(new VerbosityLevelArgument(verbosityLevel));

        private FFMpegGlobalArguments WithOption(IArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
    }
}
