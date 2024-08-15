namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents segment_wrap parameter
    /// </summary>
    public class SegmentWrapArgument : ISegmentArgument
    {
        public readonly int Limit;
        /// <summary>
        /// Represents segment_wrap parameter
        /// </summary>
        /// <param name="limit">limit value after which segment index will wrap around</param>
        public SegmentWrapArgument(int limit)
        {
            Limit = limit;
        }
        public string Key { get; } = "segment_wrap";
        public string Value => Limit <= 0 ? string.Empty : $"-segment_wrap {Limit}";
    }
}
