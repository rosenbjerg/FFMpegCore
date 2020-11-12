using System.Collections.Generic;
using FFMpegCore.Arguments;

namespace FFMpegCore
{
    public abstract class FFMpegOptionsBase
    {
        internal readonly List<IArgument> Arguments = new List<IArgument>();
    }
}