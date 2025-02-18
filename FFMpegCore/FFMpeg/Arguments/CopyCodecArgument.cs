namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents a copy codec parameter
    /// </summary>
    public class CopyCodecArgument : IArgument
    {
        public string Text => $"-codec copy";
    }
}
