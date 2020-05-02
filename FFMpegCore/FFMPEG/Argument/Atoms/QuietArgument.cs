namespace FFMpegCore.FFMPEG.Argument
{
    public class QuietArgument : Argument
    {
        public override string GetStringValue()
        {
            return "-hide_banner -loglevel warning";
        }
    }
}