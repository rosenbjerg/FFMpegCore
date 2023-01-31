namespace FFMpegCore.Arguments
{
    internal class BlackFrameArgument : IVideoFilterArgument
    {
        public string Key => "blackframe";

        public string Value { get; }

        public BlackFrameArgument(int amount = 98, int threshold = 32)
        {
            Value = $"amount={amount}:threshold={threshold}";
        }
    }
}
