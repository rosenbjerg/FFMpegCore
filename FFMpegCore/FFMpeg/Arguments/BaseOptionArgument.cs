namespace FFMpegCore.Arguments;

/// <summary>
/// <see href="https://ffmpeg.org/ffmpeg.html#Options" />
/// Base class for option arguments.
/// Options which do not take arguments are boolean options, and set the corresponding value to true.
/// They can be set to false by prefixing the option name with "no". For example using "-nofoo" will set the boolean option with name "foo" to false. 
/// </summary>
/// <param name="value"></param>
public abstract class BaseOptionArgument(bool value) : IArgument
{
    protected abstract string ArgumentName { get; }
    public string Text => $"-{(value ? "" : "no")}{ArgumentName}";
}
