using FFMpegCore.Extend;

namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents seek parameter
    /// </summary>
    public class SeekArgument : IArgument
    {
        public readonly TimeSpan? SeekTo;

        public SeekArgument(TimeSpan? seekTo)
        {
            SeekTo = seekTo;
        }

        public string Text => SeekTo.HasValue ? $"-ss {SeekTo.Value.ToLongString()}" : string.Empty;
    }
}
