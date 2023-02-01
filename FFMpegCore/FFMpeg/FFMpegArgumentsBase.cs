using FFMpegCore.Arguments;

namespace FFMpegCore
{
    public abstract class FFMpegArgumentsBase
    {
        internal readonly List<IArgument> Arguments = new();
    }
}
