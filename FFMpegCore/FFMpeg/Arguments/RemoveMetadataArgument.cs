namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Remove metadata argument
    /// </summary>
    public class RemoveMetadataArgument : IArgument
    {
        public string Text => "-map_metadata -1";
    }
}