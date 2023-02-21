using FFMpegCore.Extend;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents seek parameter
    /// </summary>
    public class EndSeekArgument : IArgument
    {
        public readonly TimeSpan? SeekTo;

        public EndSeekArgument(TimeSpan? seekTo)
        {
            SeekTo = seekTo;
        }

        public string Text => SeekTo.HasValue ? $"-to {SeekTo.Value.ToLongString()}" : string.Empty;
    }
}
