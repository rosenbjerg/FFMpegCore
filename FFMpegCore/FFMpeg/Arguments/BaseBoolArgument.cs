namespace FFMpegCore.Arguments;

/// <summary>
/// Base class for boolean arguments with value <c>0</c> or <c>1</c>.
/// </summary>
/// <param name="value"></param>
public abstract class BaseBoolArgument(bool value) : IArgument
{
    protected abstract string ArgumentName { get; }
    public string Text => $"-{ArgumentName} {(value ? '1' : '0')}";
}
