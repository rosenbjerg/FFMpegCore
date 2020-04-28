namespace FFMpegCore.FFMPEG.Argument
{
    public class RawArgument : Argument<string>
    {
        public RawArgument(string argument) : base(argument)
        {
        }

        public override string GetStringValue()
        {
            return Value ?? string.Empty;
        }
    }
}