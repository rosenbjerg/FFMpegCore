namespace FFMpegCore.Arguments
{
    /// <summary>
    /// Remove metadata argument
    /// </summary>
    public class RemoveMetadataArgument : IArgument
    {
        public string Text => "-map_metadata -1";
    }
}
