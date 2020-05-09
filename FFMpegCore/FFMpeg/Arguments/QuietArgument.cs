namespace FFMpegCore.Arguments
{
    public class QuietArgument : IArgument
    {
        public string Text => "-hide_banner -loglevel warning";
    }
}