using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

public class MovFlagsArgument : IArgument
{
    public readonly MovFlags Flags;

    public MovFlagsArgument(MovFlags flags)
    {
        Flags = flags;
    }

    public string Text => $"-movflags {Flags}";
}
