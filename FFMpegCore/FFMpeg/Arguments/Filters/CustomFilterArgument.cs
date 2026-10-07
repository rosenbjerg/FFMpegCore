namespace FFMpegCore.Arguments;

internal class CustomFilterArgument : IVideoFilterArgument, IAudioFilterArgument
{
    public CustomFilterArgument(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }
    public string Value { get; }
}
