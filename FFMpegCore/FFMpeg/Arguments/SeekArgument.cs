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

        public string Text
        {
            get
            {
                if (SeekTo.HasValue)
                {
                    var hours = SeekTo.Value.Hours;
                    if (SeekTo.Value.Days > 0)
                    {
                        hours += SeekTo.Value.Days * 24;
                    }

                    return $"-ss {hours.ToString("00")}:{SeekTo.Value.Minutes.ToString("00")}:{SeekTo.Value.Seconds.ToString("00")}.{SeekTo.Value.Milliseconds.ToString("000")}";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
