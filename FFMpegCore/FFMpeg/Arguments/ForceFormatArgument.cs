using FFMpegCore.Enums;

namespace FFMpegCore.Arguments;

/// <summary>
///     Represents force format parameter
/// </summary>
public class ForceFormatArgument : IArgument
{
    public ForceFormatArgument(string format)
    {
        Text = format;
    }

    public ForceFormatArgument(ContainerFormat format)
    {
        Text = format.Name;
    }

    public string Text => $"-f {field}";
}
