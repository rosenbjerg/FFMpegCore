namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Remove metadata argument
    /// </summary>
    public class RemoveMetadataArgument : Argument
    {
        public RemoveMetadataArgument()
        {
        }

        public override string GetStringValue()
        {
            return $"-map_metadata -1 ";
        }
    }
}