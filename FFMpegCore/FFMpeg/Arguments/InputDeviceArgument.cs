namespace FFMpegCore.Arguments;

/// <summary>
///     Represents an input device parameter
/// </summary>
public class InputDeviceArgument : IInputArgument
{
    public InputDeviceArgument(string device)
    {
        Text = device;
    }

    public Task During(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Pre(FFOptions options) { }

    public void Post() { }

    public string Text => $"-i {field}";
}
