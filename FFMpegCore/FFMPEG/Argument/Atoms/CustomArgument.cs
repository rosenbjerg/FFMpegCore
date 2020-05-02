namespace FFMpegCore.FFMPEG.Argument
{
    public class CustomArgument : Argument<string>
    {
        public CustomArgument(string argument) : base(argument)
        {
        }

        public override string GetStringValue()
        {
            return Value ?? string.Empty;
        }
    }
}