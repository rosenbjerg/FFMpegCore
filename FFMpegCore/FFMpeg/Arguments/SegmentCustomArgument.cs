namespace FFMpegCore.Arguments
{
    public class SegmentCustomArgument : ISegmentArgument
    {
        public readonly string Argument;

        public SegmentCustomArgument(string argument)
        {
            Argument = argument;
        }
        public string Key => "custom";
        public string Value => Argument ?? string.Empty;
    }
}
