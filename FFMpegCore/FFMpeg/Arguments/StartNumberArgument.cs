using FFMpegCore.Helpers;

namespace FFMpegCore.Arguments;

public class StartNumberArgument : IArgument
{
    public readonly int StartNumber;

    public StartNumberArgument(int startNumber)
    {
        StartNumber = startNumber;
    }

    public string Text => $"-start_number {StartNumber.ToInvariantString()}";
}
