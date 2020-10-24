using FFMpegCore.Arguments;

namespace FFMpegCore
{
    public sealed class FFMpegGlobalOptions : FFMpegOptionsBase
    {
        internal FFMpegGlobalOptions() { }
        
        public FFMpegGlobalOptions WithVerbosityLevel(VerbosityLevel verbosityLevel = VerbosityLevel.Error) => WithOption(new VerbosityLevelArgument(verbosityLevel));
        
        protected FFMpegGlobalOptions WithOption(IArgument argument)
        {
            Arguments.Add(argument);
            return this;
        }
        
    }
}