namespace FFMpegCore.FFMPEG.Argument
{
    /// <summary>
    /// Remove metadata argument
    /// </summary>
    public class RemoveMetadataArgument : Argument
    {
        public RemoveMetadataArgument() { }

        /// <inheritdoc/>
        public override string GetStringValue()
        {
            return $"-map_metadata -1";
        }
    }
}