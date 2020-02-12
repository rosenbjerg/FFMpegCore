namespace FFMpegCore.FFMPEG.Argument
{
    public interface IArgumentBuilder
    {
        string BuildArguments(ArgumentContainer container);
    }
}
