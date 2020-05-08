namespace FFMpegCore.FFMPEG.Argument
{
    public class QuietArgument : IArgument
    {
        public string Text => "-hide_banner -loglevel warning";
    }
}