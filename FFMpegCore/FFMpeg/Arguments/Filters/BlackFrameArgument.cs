namespace FFMpegCore.Arguments;

internal class BlackFrameArgument : IVideoFilterArgument
{
    public BlackFrameArgument(int amount = 98, int threshold = 32)
    {
        Value = $"amount={amount}:threshold={threshold}";
    }

    public string Key => "blackframe";

    public string Value { get; }
}
