namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Represents overwrite parameter
    /// If output file should be overwritten if exists
    /// </summary>
    public class OverwriteArgument : IArgument
    {
        public string Text => "-y";
    }
}
